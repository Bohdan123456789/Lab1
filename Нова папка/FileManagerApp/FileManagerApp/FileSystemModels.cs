// FileSystemViewModels.cs
using System;
using System.Collections.ObjectModel; // Для ObservableCollection
using System.IO; // Для DriveInfo, DirectoryInfo, FileInfo
using System.Windows.Media; // Для ImageSource
using System.Windows.Media.Imaging; // Для BitmapImage
using System.ComponentModel; // Для INotifyPropertyChanged
using System.Security.AccessControl; // Для FileSecurity

namespace FileManagerApp
{
    // Базовий клас для всіх елементів файлової системи
    public abstract class FileSystemObjectInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string _fullPath;
        public string FullPath
        {
            get => _fullPath;
            set
            {
                if (_fullPath != value)
                {
                    _fullPath = value;
                    OnPropertyChanged(nameof(FullPath));
                }
            }
        }

        private ImageSource _iconSource;
        public ImageSource IconSource
        {
            get => _iconSource;
            set
            {
                if (_iconSource != value)
                {
                    _iconSource = value;
                    OnPropertyChanged(nameof(IconSource));
                }
            }
        }

        private DateTime _lastWriteTime;
        public DateTime LastWriteTime
        {
            get => _lastWriteTime;
            set
            {
                if (_lastWriteTime != value)
                {
                    _lastWriteTime = value;
                    OnPropertyChanged(nameof(LastWriteTime));
                }
            }
        }
    }

    // Клас для представлення диска у TreeView
    public class DriveItem : FileSystemObjectInfo
    {
        public ObservableCollection<DirectoryItem> Children { get; set; }

        public DriveItem(DriveInfo driveInfo)
        {
            Name = driveInfo.Name;
            FullPath = driveInfo.RootDirectory.FullName;
            LastWriteTime = driveInfo.RootDirectory.LastWriteTime; // Або інша відповідна дата
            IconSource = GetDriveIcon(); // Отримання іконки диска
            Children = new ObservableCollection<DirectoryItem>();
        }

        private ImageSource GetDriveIcon()
        {
            // Для Windows, іконки дисків можна отримати за допомогою System.Drawing.Icon.ExtractAssociatedIcon
            // Але System.Drawing не є частиною .NET Core / .NET 5+ WPF за замовчуванням.
            // Для крос-платформності або спрощення, використовуємо стандартні системні іконки або ресурс.
            // Можна використовувати Path.GetPathRoot для визначення типу диска (локальний, мережевий)
            // і повертати відповідну іконку.
            // Тут використовуємо заглушку.
            return new BitmapImage(new Uri("pack://application:,,,/Icons/drive.png")); // Припустимо, у вас є папка Icons з drive.png
        }
    }

    // Клас для представлення каталогу (папки) у TreeView
    public class DirectoryItem : FileSystemObjectInfo
    {
        public ObservableCollection<DirectoryItem> Children { get; set; }
        public ObservableCollection<FileItem> Files { get; set; } // Може бути використано для ListView

        // Властивість, яка вказує, чи були вже завантажені дочірні елементи
        public bool IsLoaded { get; set; }

        public DirectoryItem(DirectoryInfo directoryInfo)
        {
            Name = directoryInfo.Name;
            FullPath = directoryInfo.FullName;
            LastWriteTime = directoryInfo.LastWriteTime;
            IconSource = GetFolderIcon(); // Отримання іконки папки
            Children = new ObservableCollection<DirectoryItem>();
            Files = new ObservableCollection<FileItem>();
            IsLoaded = false;

            // Додаємо заглушку для розширення TreeView (якщо ще не завантажено)
            // Це дозволяє TreeView показувати '+' поруч з папкою
            if (HasSubdirectories(directoryInfo.FullName))
            {
                Children.Add(new DummyDirectoryItem());
            }
        }

        private ImageSource GetFolderIcon()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Icons/folder.png")); // Припустимо, у вас є папка Icons з folder.png
        }

        // Допоміжний метод для перевірки наявності підкаталогів
        private bool HasSubdirectories(string path)
        {
            try
            {
                return Directory.EnumerateDirectories(path).Any();
            }
            catch
            {
                return false; // Помилки доступу або інші проблеми
            }
        }
    }

    // Клас-заглушка для асинхронного завантаження TreeView
    public class DummyDirectoryItem : DirectoryItem
    {
        public DummyDirectoryItem() : base(new DirectoryInfo("Dummy")) // Передаємо фіктивний DirectoryInfo
        {
            Name = string.Empty; // Не відображати ім'я
            FullPath = string.Empty;
            IsLoaded = false;
        }
    }


    // Клас для представлення файлу у ListView
    public class FileItem : FileSystemObjectInfo
    {
        public long Size { get; set; }
        public string Type { get; set; } // Тип файлу (наприклад, ".txt", ".png")

        public FileItem(FileInfo fileInfo)
        {
            Name = fileInfo.Name;
            FullPath = fileInfo.FullName;
            LastWriteTime = fileInfo.LastWriteTime;
            Size = fileInfo.Length;
            Type = fileInfo.Extension; // Розширення файлу
            IconSource = GetFileIcon(fileInfo.Extension); // Отримання іконки файлу
        }

        private ImageSource GetFileIcon(string extension)
        {
            // Можна додати логіку для різних типів файлів
            // Наприклад: if (extension == ".txt") return new BitmapImage(...)
            // Для спрощення, використовуємо одну універсальну іконку.
            return new BitmapImage(new Uri("pack://application:,,,/Icons/file.png")); // Припустимо, у вас є папка Icons з file.png
        }
    }
}
