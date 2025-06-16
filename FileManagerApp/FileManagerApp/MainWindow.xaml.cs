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
using System.IO.Compression; // For ZIP operations

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

        // Зберігає шлях до файлу, який зараз редагується в текстовому перегляді
        private string _editingFilePath = null;

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
            _editingFilePath = null; // Скинути шлях редагованого файлу
            txtFileContentPreview.IsReadOnly = true; // Переконатися, що попередній перегляд тексту лише для читання
            btnEditFile.IsEnabled = false; // Вимкнути кнопку редагування
            btnSaveEditedFile.IsEnabled = false; // Вимкнути кнопку збереження змін


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

                    // Якщо це папка, перемикаємо на вкладку властивостей
                    tabControlPreview.SelectedItem = tabControlPreview.Items.OfType<TabItem>().FirstOrDefault(ti => ti.Header.ToString() == "Властивості");
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

                    // Увімкнути кнопку "Редагувати", якщо це текстовий файл
                    if (IsTextFile(fileInfo.Extension))
                    {
                        btnEditFile.IsEnabled = true;
                    }
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
            _editingFilePath = null; // Скидаємо шлях редагованого файлу
            txtFileContentPreview.IsReadOnly = true; // Завжди починаємо з режиму лише для читання
            btnEditFile.IsEnabled = false; // Вимкнути кнопку редагування
            btnSaveEditedFile.IsEnabled = false; // Вимкнути кнопку збереження змін


            string extension = Path.GetExtension(filePath).ToLower();

            // Перегляд тексту
            if (IsTextFile(extension))
            {
                try
                {
                    txtFileContentPreview.Text = File.ReadAllText(filePath);
                    _editingFilePath = filePath; // Зберігаємо шлях для можливого редагування
                    btnEditFile.IsEnabled = true; // Увімкнути кнопку "Редагувати"
                    tabControlPreview.SelectedItem = tabControlPreview.Items.OfType<TabItem>().FirstOrDefault(ti => ti.Header.ToString() == "Перегляд тексту");

                }
                catch (Exception ex)
                {
                    txtFileContentPreview.Text = $"Не вдалося прочитати текстовий файл: {ex.Message}";
                    btnEditFile.IsEnabled = false;
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
                    tabControlPreview.SelectedItem = tabControlPreview.Items.OfType<TabItem>().FirstOrDefault(ti => ti.Header.ToString() == "Перегляд зображення");
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
                tabControlPreview.SelectedItem = tabControlPreview.Items.OfType<TabItem>().FirstOrDefault(ti => ti.Header.ToString() == "Перегляд тексту"); // Перемикаємо на текстовий перегляд
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
            _editingFilePath = null;
            txtFileContentPreview.IsReadOnly = true;
            btnEditFile.IsEnabled = false;
            btnSaveEditedFile.IsEnabled = false;
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
                // Якщо виділення файлу скасовано, очистити попередній перегляд
                ClearPropertiesDisplay(); // Очистити властивості
                ClearFileContentPreview(); // Очистити попередній перегляд
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

        // ====================================================================
        // НОВІ ФУНКЦІЇ ЗГІДНО З ВИМОГАМИ ЗАВДАННЯ
        // ====================================================================

        // Допоміжний метод для отримання шляху вибраного каталогу
        private string GetSelectedDirectoryPath()
        {
            if (treeViewFolders.SelectedItem is DirectoryItem selectedDir)
            {
                return selectedDir.FullPath;
            }
            else if (treeViewFolders.SelectedItem is DriveItem selectedDrive)
            {
                return selectedDrive.FullPath;
            }
            else
            {
                // Якщо нічого не вибрано, використовуємо поточний шлях з TextBox
                return txtCurrentPath.Text;
            }
        }

        // Допоміжний метод для оновлення поточного каталогу в TreeView та ListView
        private void RefreshCurrentDirectoryView()
        {
            string currentPath = txtCurrentPath.Text;
            if (!string.IsNullOrEmpty(currentPath))
            {
                // Намагаємося знайти і повторно вибрати поточний елемент TreeView, щоб оновити його вміст
                SelectTreeViewItem(currentPath);
            }
            else
            {
                LoadDrives(); // Якщо немає поточного шляху, перевантажуємо диски
            }
        }

        // Обробник для кнопки "Створити папку"
        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            string currentPath = GetSelectedDirectoryPath();
            if (string.IsNullOrEmpty(currentPath))
            {
                MessageBox.Show("Будь ласка, виберіть каталог, де створити нову папку.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string folderName = PromptForInput("Створити нову папку", "Введіть назву нової папки:");
            if (string.IsNullOrEmpty(folderName)) return;

            string newFolderPath = Path.Combine(currentPath, folderName);
            try
            {
                Directory.CreateDirectory(newFolderPath);
                MessageBox.Show($"Папку '{folderName}' успішно створено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshCurrentDirectoryView(); // Оновити відображення
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка створення папки: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обробник для кнопки "Створити файл"
        private void CreateFile_Click(object sender, RoutedEventArgs e)
        {
            string currentPath = GetSelectedDirectoryPath();
            if (string.IsNullOrEmpty(currentPath))
            {
                MessageBox.Show("Будь ласка, виберіть каталог, де створити новий файл.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string fileName = PromptForInput("Створити новий файл", "Введіть назву нового файлу (наприклад, file.txt):");
            if (string.IsNullOrEmpty(fileName)) return;

            string newFilePath = Path.Combine(currentPath, fileName);
            try
            {
                File.WriteAllText(newFilePath, string.Empty); // Створити порожній файл
                MessageBox.Show($"Файл '{fileName}' успішно створено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshCurrentDirectoryView(); // Оновити відображення
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка створення файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обробник для кнопки "Перейменувати"
        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            FileSystemObjectInfo selectedObject = treeViewFolders.SelectedItem as FileSystemObjectInfo;
            if (selectedObject == null)
            {
                selectedObject = listViewFiles.SelectedItem as FileItem;
            }

            if (selectedObject == null)
            {
                MessageBox.Show("Будь ласка, виберіть файл або папку для перейменування.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string currentFullName = selectedObject.FullPath;
            string currentName = selectedObject.Name;

            string newName = PromptForInput("Перейменувати", $"Введіть нову назву для '{currentName}':", currentName);
            if (string.IsNullOrEmpty(newName) || newName == currentName) return;

            string newFullName = Path.Combine(Path.GetDirectoryName(currentFullName), newName);

            try
            {
                if (Directory.Exists(currentFullName))
                {
                    Directory.Move(currentFullName, newFullName);
                    MessageBox.Show($"Папку '{currentName}' успішно перейменовано на '{newName}'.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (File.Exists(currentFullName))
                {
                    File.Move(currentFullName, newFullName);
                    MessageBox.Show($"Файл '{currentName}' успішно перейменовано на '{newName}'.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                RefreshCurrentDirectoryView(); // Оновити відображення
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка перейменування: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обробник для кнопки "Копіювати"
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            FileSystemObjectInfo selectedObject = treeViewFolders.SelectedItem as FileSystemObjectInfo;
            if (selectedObject == null)
            {
                selectedObject = listViewFiles.SelectedItem as FileItem;
            }

            if (selectedObject == null)
            {
                MessageBox.Show("Будь ласка, виберіть файл або папку для копіювання.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string sourcePath = selectedObject.FullPath;
            string destinationPath = PromptForInput("Копіювати", $"Введіть цільовий шлях для копіювання '{selectedObject.Name}':", txtCurrentPath.Text);
            if (string.IsNullOrEmpty(destinationPath)) return;

            try
            {
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }

                string destinationFullName = Path.Combine(destinationPath, selectedObject.Name);

                if (Directory.Exists(sourcePath))
                {
                    CopyDirectory(sourcePath, destinationFullName); // Рекурсивне копіювання папки
                    MessageBox.Show($"Папку '{selectedObject.Name}' успішно скопійовано.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destinationFullName, true); // Перезаписати, якщо існує
                    MessageBox.Show($"Файл '{selectedObject.Name}' успішно скопійовано.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                RefreshCurrentDirectoryView(); // Оновити відображення
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка копіювання: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Допоміжний метод для рекурсивного копіювання каталогу
        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                file.CopyTo(Path.Combine(destinationDir, file.Name), true); // true для перезапису
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                CopyDirectory(subDir.FullName, Path.Combine(destinationDir, subDir.Name));
            }
        }


        // Обробник для кнопки "Перемістити"
        private void Move_Click(object sender, RoutedEventArgs e)
        {
            FileSystemObjectInfo selectedObject = treeViewFolders.SelectedItem as FileSystemObjectInfo;
            if (selectedObject == null)
            {
                selectedObject = listViewFiles.SelectedItem as FileItem;
            }

            if (selectedObject == null)
            {
                MessageBox.Show("Будь ласка, виберіть файл або папку для переміщення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string sourcePath = selectedObject.FullPath;
            string destinationPath = PromptForInput("Перемістити", $"Введіть цільовий шлях для переміщення '{selectedObject.Name}':", txtCurrentPath.Text);
            if (string.IsNullOrEmpty(destinationPath)) return;

            try
            {
                string destinationFullName = Path.Combine(destinationPath, selectedObject.Name);

                if (Directory.Exists(sourcePath))
                {
                    Directory.Move(sourcePath, destinationFullName);
                    MessageBox.Show($"Папку '{selectedObject.Name}' успішно переміщено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (File.Exists(sourcePath))
                {
                    File.Move(sourcePath, destinationFullName);
                    MessageBox.Show($"Файл '{selectedObject.Name}' успішно переміщено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                RefreshCurrentDirectoryView(); // Оновити відображення
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка переміщення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обробник для кнопки "Видалити"
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            FileSystemObjectInfo selectedObject = treeViewFolders.SelectedItem as FileSystemObjectInfo;
            if (selectedObject == null)
            {
                selectedObject = listViewFiles.SelectedItem as FileItem;
            }

            if (selectedObject == null)
            {
                MessageBox.Show("Будь ласка, виберіть файл або папку для видалення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string pathToDelete = selectedObject.FullPath;
            MessageBoxResult result = MessageBox.Show($"Ви впевнені, що хочете видалити '{selectedObject.Name}'? Це дія незворотня.", "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (Directory.Exists(pathToDelete))
                    {
                        Directory.Delete(pathToDelete, true); // true для рекурсивного видалення
                        MessageBox.Show($"Папку '{selectedObject.Name}' успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (File.Exists(pathToDelete))
                    {
                        File.Delete(pathToDelete);
                        MessageBox.Show($"Файл '{selectedObject.Name}' успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    RefreshCurrentDirectoryView(); // Оновити відображення
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка видалення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обробник для кнопки "Редагувати" текстовий файл
        private void EditFile_Click(object sender, RoutedEventArgs e)
        {
            if (listViewFiles.SelectedItem is FileItem selectedFile && IsTextFile(selectedFile.Type))
            {
                _editingFilePath = selectedFile.FullPath;
                try
                {
                    txtFileContentPreview.Text = File.ReadAllText(_editingFilePath);
                    txtFileContentPreview.IsReadOnly = false; // Дозволити редагування
                    btnSaveEditedFile.IsEnabled = true; // Увімкнути кнопку "Зберегти зміни"
                    btnEditFile.IsEnabled = false; // Вимкнути кнопку "Редагувати"
                    tabControlPreview.SelectedItem = tabControlPreview.Items.OfType<TabItem>().FirstOrDefault(ti => ti.Header.ToString() == "Перегляд тексту");

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося відкрити файл для редагування: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtFileContentPreview.IsReadOnly = true;
                    btnSaveEditedFile.IsEnabled = false;
                    _editingFilePath = null;
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть текстовий файл для редагування.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Обробник для кнопки "Зберегти зміни" текстового файлу
        private void SaveEditedFile_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_editingFilePath) && !txtFileContentPreview.IsReadOnly)
            {
                try
                {
                    File.WriteAllText(_editingFilePath, txtFileContentPreview.Text);
                    MessageBox.Show("Зміни успішно збережено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtFileContentPreview.IsReadOnly = true; // Знову зробити лише для читання
                    btnSaveEditedFile.IsEnabled = false; // Вимкнути кнопку збереження
                    btnEditFile.IsEnabled = true; // Увімкнути кнопку редагування
                    RefreshCurrentDirectoryView(); // Оновити відображення (наприклад, час зміни)
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка збереження змін: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Немає активного файлу для збереження або він не був відкритий для редагування.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Обробник для кнопки "Архівувати (ZIP)"
        private void Archive_Click(object sender, RoutedEventArgs e)
        {
            FileSystemObjectInfo selectedObject = treeViewFolders.SelectedItem as FileSystemObjectInfo;
            if (selectedObject == null)
            {
                selectedObject = listViewFiles.SelectedItem as FileItem;
            }

            if (selectedObject == null)
            {
                MessageBox.Show("Будь ласка, виберіть файл або папку для архівації.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string sourcePath = selectedObject.FullPath;
            string parentDirectory = Path.GetDirectoryName(sourcePath);
            string zipFileName = Path.GetFileNameWithoutExtension(sourcePath) + ".zip";
            string destinationZipPath = Path.Combine(parentDirectory, zipFileName);

            try
            {
                if (File.Exists(destinationZipPath))
                {
                    var result = MessageBox.Show($"Файл '{zipFileName}' вже існує. Перезаписати?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No) return;
                    File.Delete(destinationZipPath); // Видалити існуючий файл для перезапису
                }

                if (Directory.Exists(sourcePath))
                {
                    ZipFile.CreateFromDirectory(sourcePath, destinationZipPath, CompressionLevel.Fastest, true);
                    MessageBox.Show($"Папку '{selectedObject.Name}' успішно заархівовано в '{zipFileName}'.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (File.Exists(sourcePath))
                {
                    // Для окремого файлу створюємо тимчасову папку, поміщаємо туди файл і архівуємо
                    string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempDir);
                    string tempFilePath = Path.Combine(tempDir, selectedObject.Name);
                    File.Copy(sourcePath, tempFilePath, true);

                    ZipFile.CreateFromDirectory(tempDir, destinationZipPath, CompressionLevel.Fastest, false); // false, щоб не включати кореневий каталог
                    Directory.Delete(tempDir, true); // Видалити тимчасову папку
                    MessageBox.Show($"Файл '{selectedObject.Name}' успішно заархівовано в '{zipFileName}'.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                RefreshCurrentDirectoryView(); // Оновити відображення
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка архівації: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обробник для кнопки "Розпакувати (ZIP)"
        private void Extract_Click(object sender, RoutedEventArgs e)
        {
            FileItem selectedFile = listViewFiles.SelectedItem as FileItem;

            if (selectedFile == null || !selectedFile.Type.Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Будь ласка, виберіть ZIP-файл для розпакування.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string sourceZipPath = selectedFile.FullPath;
            string parentDirectory = Path.GetDirectoryName(sourceZipPath);
            string defaultExtractPath = Path.Combine(parentDirectory, Path.GetFileNameWithoutExtension(sourceZipPath));

            string destinationExtractPath = PromptForInput("Розпакувати ZIP", $"Введіть цільову папку для розпакування '{selectedFile.Name}':", defaultExtractPath);
            if (string.IsNullOrEmpty(destinationExtractPath)) return;

            try
            {
                if (Directory.Exists(destinationExtractPath))
                {
                    var result = MessageBox.Show($"Папка '{Path.GetFileName(destinationExtractPath)}' вже існує. Перезаписати вміст?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No) return;
                    // Обережне видалення, щоб уникнути помилок, якщо папка не пуста
                    foreach (string file in Directory.EnumerateFiles(destinationExtractPath, "*", SearchOption.AllDirectories))
                    {
                        File.SetAttributes(file, FileAttributes.Normal); // Знімаємо атрибути лише для читання
                    }
                    Directory.Delete(destinationExtractPath, true);
                }

                ZipFile.ExtractToDirectory(sourceZipPath, destinationExtractPath);
                MessageBox.Show($"Файл '{selectedFile.Name}' успішно розпаковано.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshCurrentDirectoryView(); // Оновити відображення
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка розпакування: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Допоміжний метод для відображення діалогового вікна введення
        private string PromptForInput(string title, string message, string defaultValue = "")
        {
            // Створення нового вікна для введення
            Window promptWindow = new Window
            {
                Title = title,
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Topmost = true
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(10) };
            panel.Children.Add(new TextBlock { Text = message, Margin = new Thickness(0, 0, 0, 10), TextWrapping = TextWrapping.Wrap });

            TextBox inputTextBox = new TextBox { Text = defaultValue, Margin = new Thickness(0, 0, 0, 10), MinHeight = 25 };
            panel.Children.Add(inputTextBox);

            Button okButton = new Button { Content = "OK", Width = 75, Height = 25, Margin = new Thickness(5), Style = (Style)FindResource("CommonButtonStyle") };
            Button cancelButton = new Button { Content = "Відміна", Width = 75, Height = 25, Margin = new Thickness(5), Style = (Style)FindResource("CommonButtonStyle") };

            okButton.Click += (s, e) => { promptWindow.DialogResult = true; };
            cancelButton.Click += (s, e) => { promptWindow.DialogResult = false; };

            StackPanel buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            panel.Children.Add(buttonPanel);

            promptWindow.Content = panel;
            inputTextBox.Focus();
            inputTextBox.SelectAll(); // Виділити текст за замовчуванням

            if (promptWindow.ShowDialog() == true)
            {
                return inputTextBox.Text;
            }
            return null; // Користувач скасував
        }
    }
}
