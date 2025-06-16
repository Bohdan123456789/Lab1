// MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media; // For Brushes
using System.Security.AccessControl; // For accessing security attributes
using System.Security.Principal; // For SecurityIdentifier
using System.Threading.Tasks; // For async operations
using System.Collections; // Added for System.Collections.IEnumerable

namespace FileManagerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // ObservableCollection для прив'язки дисків до TreeView
        public ObservableCollection<DriveItem> Drives { get; set; }

        // Зберігаємо історію навігації
        private Stack<string> navigationHistory = new Stack<string>();

        public MainWindow()
        {
            InitializeComponent();
            Drives = new ObservableCollection<DriveItem>();
            this.DataContext = Drives; // Встановлюємо DataContext для прив'язки TreeView
            LoadDrives();
        }

        // Завантаження доступних дисків
        private void LoadDrives()
        {
            Drives.Clear();
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                try
                {
                    if (drive.IsReady) // Перевіряємо, чи диск готовий до використання
                    {
                        Drives.Add(new DriveItem(drive));
                    }
                }
                catch (Exception ex)
                {
                    // Ігнорувати диски, до яких немає доступу або вони не готові
                    Console.WriteLine($"Error loading drive {drive.Name}: {ex.Message}");
                }
            }
        }

        // Обробник зміни виділеного елемента в TreeView (диски/папки)
        private void TreeViewFolders_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            string newPath = string.Empty;
            if (e.NewValue is DriveItem selectedDrive)
            {
                newPath = selectedDrive.FullPath;
            }
            else if (e.NewValue is DirectoryItem selectedDirectory)
            {
                newPath = selectedDirectory.FullPath;
            }

            if (!string.IsNullOrEmpty(newPath))
            {
                txtCurrentPath.Text = newPath;
                LoadDirectoryContents(e.NewValue as FileSystemObjectInfo, newPath);
                UpdatePropertiesDisplay(newPath);

                // Додаємо до історії лише якщо це нова навігація, а не просто виділення
                if (navigationHistory.Count == 0 || navigationHistory.Peek() != newPath)
                {
                    navigationHistory.Push(newPath);
                }
            }
            else
            {
                // Якщо виділено щось інше (наприклад, файл), або виділення скасовано
                txtCurrentPath.Text = "";
                listViewFiles.ItemsSource = null;
                ClearPropertiesDisplay();
                ClearFileContentPreview();
            }
        }

        // Завантаження вмісту каталогу (підкаталоги та файли)
        private void LoadDirectoryContents(FileSystemObjectInfo parentItem, string path)
        {
            // Для DirectoryItem, якщо це заглушка, видаляємо її
            if (parentItem is DirectoryItem dirItem && dirItem.Children.Any() && dirItem.Children[0] is DummyDirectoryItem)
            {
                dirItem.Children.Clear();
            }

            ObservableCollection<DirectoryItem> subdirectories = new ObservableCollection<DirectoryItem>();
            ObservableCollection<FileItem> files = new ObservableCollection<FileItem>();

            try
            {
                // Завантаження підкаталогів
                foreach (string dir in Directory.EnumerateDirectories(path))
                {
                    try
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(dir);
                        if (!dirInfo.Attributes.HasFlag(FileAttributes.Hidden) && !dirInfo.Attributes.HasFlag(FileAttributes.System))
                        {
                            subdirectories.Add(new DirectoryItem(dirInfo));
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Ігнорувати папки, до яких немає доступу
                        Console.WriteLine($"Access denied to directory: {dir}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error enumerating directory {dir}: {ex.Message}");
                    }
                }

                if (parentItem is DriveItem driveParent)
                {
                    driveParent.Children.Clear();
                    foreach (var subDir in subdirectories.OrderBy(d => d.Name))
                    {
                        driveParent.Children.Add(subDir);
                    }
                }
                else if (parentItem is DirectoryItem directoryParent)
                {
                    directoryParent.Children.Clear();
                    foreach (var subDir in subdirectories.OrderBy(d => d.Name))
                    {
                        directoryParent.Children.Add(subDir);
                    }
                    directoryParent.IsLoaded = true; // Позначаємо, що вміст завантажено
                }


                // Завантаження файлів
                foreach (string file in Directory.EnumerateFiles(path))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        files.Add(new FileItem(fileInfo));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Ігнорувати файли, до яких немає доступу
                        Console.WriteLine($"Access denied to file: {file}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error enumerating file {file}: {ex.Message}");
                    }
                }
                listViewFiles.ItemsSource = files.OrderBy(f => f.Name); // Сортуємо за назвою
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Відмовлено в доступі до каталогу: " + path, "Помилка доступу", MessageBoxButton.OK, MessageBoxImage.Warning);
                listViewFiles.ItemsSource = null;
                if (parentItem is DirectoryItem currentDirItem && !currentDirItem.IsLoaded) // Changed dirItem to currentDirItem to avoid conflict
                {
                    currentDirItem.Children.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження вмісту каталогу '{path}': {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                listViewFiles.ItemsSource = null;
                if (parentItem is DirectoryItem currentDirItem && !currentDirItem.IsLoaded) // Changed dirItem to currentDirItem to avoid conflict
                {
                    currentDirItem.Children.Clear();
                }
            }
        }

        // Навігація "Назад"
        private void NavigateBack_Click(object sender, RoutedEventArgs e)
        {
            if (navigationHistory.Count > 1) // Перевіряємо, чи є куди повертатися
            {
                navigationHistory.Pop(); // Видаляємо поточний шлях
                string previousPath = navigationHistory.Peek(); // Отримуємо попередній шлях

                SelectTreeViewItem(previousPath);
            }
            else if (navigationHistory.Count == 1) // Якщо залишився лише один елемент (корінь)
            {
                // Повертаємося до списку дисків
                LoadDrives();
                txtCurrentPath.Text = "";
                listViewFiles.ItemsSource = null;
                ClearPropertiesDisplay();
                ClearFileContentPreview();
                navigationHistory.Clear();
            }
        }

        // Допоміжний метод для виділення елемента в TreeView за шляхом
        private void SelectTreeViewItem(string path)
        {
            // Скидаємо виділення, щоб викликати SelectedItemChanged, навіть якщо елемент вже виділений
            // treeViewFolders.SelectedItem = null; // Цей рядок викликав би помилку CS0200, якщо б він був тут

            foreach (var drive in Drives)
            {
                if (drive.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    TreeViewItem driveTreeViewItem = treeViewFolders.ItemContainerGenerator.ContainerFromItem(drive) as TreeViewItem;
                    if (driveTreeViewItem != null)
                    {
                        driveTreeViewItem.IsSelected = true; // Corrected: set IsSelected on the TreeViewItem
                        driveTreeViewItem.BringIntoView();
                        driveTreeViewItem.Focus(); // Ensure focus
                    }
                    // If driveTreeViewItem is null, the item might not be materialized.
                    // For now, we rely on LoadDrives to ensure top-level items are there.
                    return;
                }
                if (path.StartsWith(drive.FullPath, StringComparison.OrdinalIgnoreCase))
                {
                    // Розгортаємо диск і шукаємо всередині
                    TreeViewItem driveTreeViewItem = treeViewFolders.ItemContainerGenerator.ContainerFromItem(drive) as TreeViewItem;
                    if (driveTreeViewItem != null)
                    {
                        driveTreeViewItem.IsExpanded = true;
                        driveTreeViewItem.UpdateLayout(); // Force UI update to generate children
                        // Continue searching in children that are now materialized
                        if (SelectDirectoryItem(driveTreeViewItem.Items, path)) return;
                    }
                    else
                    {
                        // If TreeViewItem for drive is not materialized, rely on underlying data model
                        if (SelectDirectoryItem(drive.Children, path)) return;
                    }
                }
            }
        }

        private bool SelectDirectoryItem(IEnumerable items, string path) // Changed type to IEnumerable
        {
            foreach (var item in items)
            {
                if (item is DirectoryItem dir) // Use 'dir' to refer to DirectoryItem
                {
                    if (dir.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase))
                    {
                        TreeViewItem dirTreeViewItem = treeViewFolders.ItemContainerGenerator.ContainerFromItem(dir) as TreeViewItem;
                        if (dirTreeViewItem != null)
                        {
                            dirTreeViewItem.IsSelected = true; // Corrected: set IsSelected on the TreeViewItem
                            dirTreeViewItem.BringIntoView();
                            dirTreeViewItem.Focus(); // Ensure focus
                        }
                        else
                        {
                            // Item might not be materialized. Expand parents if needed.
                            // This part of the logic is complex for deep paths.
                            // For a simple fix, we'll try to find its parent and expand it.
                            // This would ideally be a recursive function that ensures all parent TreeViewItems are expanded.
                            // This is a simplification:
                            string parentPath = Path.GetDirectoryName(dir.FullPath);
                            if (parentPath != null && parentPath != dir.FullPath) // Avoid infinite loop for root
                            {
                                SelectTreeViewItem(parentPath); // Try to select parent, which might expand the path
                                // After selecting parent, try to re-select this item again
                                TreeViewItem recheckedDirTreeViewItem = treeViewFolders.ItemContainerGenerator.ContainerFromItem(dir) as TreeViewItem;
                                if (recheckedDirTreeViewItem != null)
                                {
                                    recheckedDirTreeViewItem.IsSelected = true;
                                    recheckedDirTreeViewItem.BringIntoView();
                                    recheckedDirTreeViewItem.Focus();
                                }
                            }
                        }
                        return true;
                    }
                    if (path.StartsWith(dir.FullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        TreeViewItem dirTreeViewItem = treeViewFolders.ItemContainerGenerator.ContainerFromItem(dir) as TreeViewItem;
                        if (dirTreeViewItem != null)
                        {
                            dirTreeViewItem.IsExpanded = true;
                            dirTreeViewItem.UpdateLayout(); // Force UI update
                            // Continue searching in children that are now materialized
                            if (SelectDirectoryItem(dirTreeViewItem.Items, path)) return true;
                        }
                        else
                        {
                            // If TreeViewItem for dir is not materialized, rely on underlying data model
                            if (SelectDirectoryItem(dir.Children, path)) return true;
                        }
                    }
                }
            }
            return false;
        }


        // Навігація "Вгору"
        private void NavigateUp_Click(object sender, RoutedEventArgs e)
        {
            string currentPath = txtCurrentPath.Text;
            if (string.IsNullOrEmpty(currentPath)) return;

            try
            {
                DirectoryInfo currentDir = new DirectoryInfo(currentPath);
                if (currentDir.Parent != null)
                {
                    string parentPath = currentDir.Parent.FullName;
                    // Додаємо поточний шлях до історії перед переходом вгору, якщо його ще немає
                    if (navigationHistory.Count == 0 || navigationHistory.Peek() != currentPath)
                    {
                        navigationHistory.Push(currentPath);
                    }
                    SelectTreeViewItem(parentPath);
                }
                else // Якщо це кореневий каталог диска, повернутися до списку дисків
                {
                    LoadDrives();
                    txtCurrentPath.Text = "";
                    listViewFiles.ItemsSource = null;
                    ClearPropertiesDisplay();
                    ClearFileContentPreview();
                    navigationHistory.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка навігації вгору: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Фільтрація файлів за назвою
        private void FilterFiles_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filterText = txtFileNameFilter.Text.ToLower();
            string currentPath = txtCurrentPath.Text;

            if (string.IsNullOrEmpty(currentPath))
            {
                listViewFiles.ItemsSource = null; // Нічого не показуємо, якщо немає шляху
                return;
            }

            try
            {
                var allFiles = new ObservableCollection<FileItem>();
                foreach (string file in Directory.EnumerateFiles(currentPath))
                {
                    try
                    {
                        allFiles.Add(new FileItem(new FileInfo(file)));
                    }
                    catch (UnauthorizedAccessException) { /* ignore */ }
                    catch (Exception) { /* ignore */ }
                }

                if (string.IsNullOrEmpty(filterText))
                {
                    listViewFiles.ItemsSource = allFiles.OrderBy(f => f.Name);
                }
                else
                {
                    var filteredFiles = allFiles.Where(f => f.Name.ToLower().Contains(filterText)).ToList();
                    listViewFiles.ItemsSource = new ObservableCollection<FileItem>(filteredFiles).OrderBy(f => f.Name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка фільтрації файлів: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                listViewFiles.ItemsSource = null;
            }
        }

        // Фільтрація каталогів за назвою (ТІЛЬКИ для поточного рівня TreeView)
        private void FilterDirectories_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filterText = txtDirectoryNameFilter.Text.ToLower();

            // Перезавантажуємо вміст поточного каталогу, щоб застосувати фільтр
            FileSystemObjectInfo currentSelected = treeViewFolders.SelectedItem as FileSystemObjectInfo;
            if (currentSelected == null)
            {
                // Якщо нічого не вибрано, фільтруємо диски
                LoadDrives(); // Перезавантажуємо диски
                if (!string.IsNullOrEmpty(filterText))
                {
                    var filteredDrives = Drives.Where(d => d.Name.ToLower().Contains(filterText)).ToList();
                    Drives.Clear();
                    foreach (var drive in filteredDrives) Drives.Add(drive);
                }
                return;
            }

            // Якщо вибрано DriveItem або DirectoryItem
            ObservableCollection<DirectoryItem> currentChildren = null;
            if (currentSelected is DriveItem driveItem)
            {
                LoadDirectoryContents(driveItem, driveItem.FullPath); // Перезавантажуємо, щоб отримати повний список
                currentChildren = driveItem.Children;
            }
            else if (currentSelected is DirectoryItem directoryItem)
            {
                LoadDirectoryContents(directoryItem, directoryItem.FullPath); // Перезавантажуємо
                currentChildren = directoryItem.Children;
            }

            if (currentChildren != null)
            {
                if (string.IsNullOrEmpty(filterText))
                {
                    // Якщо фільтр порожній, переконайтесь, що показані всі (перезавантажили вище)
                }
                else
                {
                    var filteredDirs = currentChildren.Where(d => d.Name.ToLower().Contains(filterText)).ToList();
                    currentChildren.Clear(); // Очищаємо поточну колекцію
                    foreach (var dir in filteredDirs)
                    {
                        currentChildren.Add(dir); // Додаємо відфільтровані елементи
                    }
                }
            }
        }


        // Оновлення панелі властивостей
        private void UpdatePropertiesDisplay(string path)
        {
            ClearPropertiesDisplay(); // Очистити попередні значення

            try
            {
                FileAttributes attributes = File.GetAttributes(path);

                if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    // Це папка
                    DirectoryInfo dirInfo = new DirectoryInfo(path);
                    txtPropName.Text = dirInfo.Name;
                    txtPropFullPath.Text = dirInfo.FullName;
                    txtPropType.Text = "Папка";
                    txtPropCreationTime.Text = dirInfo.CreationTime.ToString();
                    txtPropLastWriteTime.Text = dirInfo.LastWriteTime.ToString();
                    txtPropSize.Text = "N/A"; // Розмір папок не завжди легко отримати без рекурсивного підрахунку

                    txtPropSecurityAttributes.Text = GetDirectorySecurity(dirInfo);
                }
                else
                {
                    // Це файл
                    FileInfo fileInfo = new FileInfo(path);
                    txtPropName.Text = fileInfo.Name;
                    txtPropFullPath.Text = fileInfo.FullName;
                    txtPropType.Text = fileInfo.Extension;
                    txtPropSize.Text = fileInfo.Length.ToString();
                    txtPropCreationTime.Text = fileInfo.CreationTime.ToString();
                    txtPropLastWriteTime.Text = fileInfo.LastWriteTime.ToString();

                    txtPropSecurityAttributes.Text = GetFileSecurity(fileInfo);

                    // Попередній перегляд вмісту
                    UpdateFileContentPreview(fileInfo.FullName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при отриманні властивостей: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                ClearPropertiesDisplay();
                ClearFileContentPreview();
            }
        }

        // Очищення полів властивостей
        private void ClearPropertiesDisplay()
        {
            txtPropName.Text = "";
            txtPropFullPath.Text = "";
            txtPropType.Text = "";
            txtPropSize.Text = "";
            txtPropCreationTime.Text = "";
            txtPropLastWriteTime.Text = "";
            txtPropSecurityAttributes.Text = "";
        }

        // Оновлення попереднього перегляду файлу (текст або зображення)
        private void UpdateFileContentPreview(string filePath)
        {
            txtFileContentPreview.Clear();
            imgFilePreview.Source = null;

            string extension = Path.GetExtension(filePath).ToLower();

            // Перегляд тексту
            if (IsTextFile(extension))
            {
                try
                {
                    txtFileContentPreview.Text = File.ReadAllText(filePath);
                }
                catch (Exception ex)
                {
                    txtFileContentPreview.Text = $"Не вдалося прочитати текстовий файл: {ex.Message}";
                }
            }
            // Перегляд зображення
            else if (IsImageFile(extension))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Завантажити в пам'ять, щоб файл не блокувався
                    bitmap.EndInit();
                    imgFilePreview.Source = bitmap;
                }
                catch (Exception ex)
                {
                    // Показати placeholder або повідомлення про помилку
                    MessageBox.Show($"Не вдалося завантажити зображення: {ex.Message}", "Помилка зображення", MessageBoxButton.OK, MessageBoxImage.Error);
                    imgFilePreview.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/error_image.png")); // Заглушка, якщо зображення не завантажилось
                }
            }
            else
            {
                txtFileContentPreview.Text = "Попередній перегляд недоступний для цього типу файлу.";
            }
        }

        // Допоміжний метод: Перевірка, чи це текстовий файл
        private bool IsTextFile(string extension)
        {
            return new[] { ".txt", ".log", ".csv", ".xml", ".json", ".html", ".css", ".js", ".cs", ".xaml", ".ini", ".inf" }.Contains(extension);
        }

        // Допоміжний метод: Перевірка, чи це файл зображення
        private bool IsImageFile(string extension)
        {
            return new[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tiff", ".ico" }.Contains(extension);
        }

        // Очищення попереднього перегляду файлу
        private void ClearFileContentPreview()
        {
            txtFileContentPreview.Clear();
            imgFilePreview.Source = null;
        }

        // Обробник зміни виділеного елемента в ListView (файли)
        private void ListViewFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewFiles.SelectedItem is FileItem selectedFile)
            {
                UpdatePropertiesDisplay(selectedFile.FullPath);
            }
            else
            {
                ClearFileContentPreview();
            }
        }

        // Обробник подвійного кліку на файлі в ListView
        private void ListViewFiles_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (listViewFiles.SelectedItem is FileItem selectedFile)
            {
                // Відкрити файл за замовчуванням у системі
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(selectedFile.FullPath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося відкрити файл: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        // Отримання атрибутів безпеки для файлу
        private string GetFileSecurity(FileInfo fileInfo)
        {
            try
            {
                FileSecurity fs = fileInfo.GetAccessControl();
                string securityInfo = "";

                foreach (FileSystemAccessRule rule in fs.GetAccessRules(true, true, typeof(NTAccount)))
                {
                    securityInfo += $"Ім'я: {rule.IdentityReference.Value}\n";
                    securityInfo += $"  Дозволити: {rule.FileSystemRights}\n";
                    securityInfo += $"  Тип: {rule.AccessControlType}\n";
                    securityInfo += "\n";
                }
                return string.IsNullOrEmpty(securityInfo) ? "Немає даних про безпеку або відмовлено в доступі." : securityInfo;
            }
            catch (UnauthorizedAccessException)
            {
                return "Відмовлено в доступі до атрибутів безпеки.";
            }
            catch (Exception ex)
            {
                return $"Помилка отримання атрибутів безпеки: {ex.Message}";
            }
        }

        // Отримання атрибутів безпеки для каталогу
        private string GetDirectorySecurity(DirectoryInfo dirInfo)
        {
            try
            {
                DirectorySecurity ds = dirInfo.GetAccessControl();
                string securityInfo = "";

                foreach (FileSystemAccessRule rule in ds.GetAccessRules(true, true, typeof(NTAccount)))
                {
                    securityInfo += $"Ім'я: {rule.IdentityReference.Value}\n";
                    securityInfo += $"  Дозволити: {rule.FileSystemRights}\n";
                    securityInfo += $"  Тип: {rule.AccessControlType}\n";
                    securityInfo += "\n";
                }
                return string.IsNullOrEmpty(securityInfo) ? "Немає даних про безпеку або відмовлено в доступі." : securityInfo;
            }
            catch (UnauthorizedAccessException)
            {
                return "Відмовлено в доступі до атрибутів безпеки.";
            }
            catch (Exception ex)
            {
                return $"Помилка отримання атрибутів безпеки: {ex.Message}";
            }
        }
    }
}
