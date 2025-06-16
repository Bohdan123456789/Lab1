// SettingsWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FileManagerApp
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private const string SettingsFilePath = "settings.txt"; // Шлях до файлу налаштувань

        public SettingsWindow()
        {
            // InitializeComponent() є обов'язковим викликом, який генерується автоматично
            // і ініціалізує елементи XAML. Якщо ця помилка виникає, це свідчить про проблему
            // з файлом .designer.cs або його включенням до проекту.
            // Переконайтеся, що файл SettingsWindow.xaml.cs є "підпорядкованим" файлом для SettingsWindow.xaml
            InitializeComponent();
            LoadSettings(); // Завантажуємо налаштування при ініціалізації вікна
        }

        // Завантаження налаштувань з файлу
        private void LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    foreach (string line in File.ReadLines(SettingsFilePath))
                    {
                        string[] parts = line.Split(new char[] { '=' }, 2); // Розділити на ключ і значення
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();

                            switch (key)
                            {
                                case "DefaultFolder":
                                    txtDefaultFolder.Text = value;
                                    break;
                                case "DefaultFontSize":
                                    // Перевірка на коректність значення перед парсингом
                                    if (double.TryParse(value, out double fontSize))
                                    {
                                        txtDefaultFontSize.Text = value;
                                    }
                                    else
                                    {
                                        txtDefaultFontSize.Text = "12"; // Значення за замовчуванням у разі помилки
                                    }
                                    break;
                                case "EnableDarkMode":
                                    if (bool.TryParse(value, out bool darkMode))
                                    {
                                        chkEnableDarkMode.IsChecked = darkMode;
                                    }
                                    else
                                    {
                                        chkEnableDarkMode.IsChecked = false; // Значення за замовчуванням
                                    }
                                    break;
                                case "HideSystemFiles":
                                    if (bool.TryParse(value, out bool hideSystemFiles))
                                    {
                                        chkHideSystemFiles.IsChecked = hideSystemFiles;
                                    }
                                    else
                                    {
                                        chkHideSystemFiles.IsChecked = true; // Значення за замовчуванням
                                    }
                                    break;
                                    // Додайте інші налаштування тут
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка завантаження налаштувань: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Встановити значення за замовчуванням, якщо файл налаштувань не існує
                txtDefaultFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                txtDefaultFontSize.Text = "12";
                chkEnableDarkMode.IsChecked = false;
                chkHideSystemFiles.IsChecked = true;
            }
        }

        // Збереження налаштувань у файл
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(SettingsFilePath))
                {
                    writer.WriteLine($"DefaultFolder={txtDefaultFolder.Text}");
                    writer.WriteLine($"DefaultFontSize={txtDefaultFontSize.Text}");
                    writer.WriteLine($"EnableDarkMode={chkEnableDarkMode.IsChecked}");
                    writer.WriteLine($"HideSystemFiles={chkHideSystemFiles.IsChecked}");
                    // Додайте інші налаштування тут
                }
                MessageBox.Show("Налаштування успішно збережено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true; // Повернути true, якщо збереження успішне
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження налаштувань: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обробник кнопки "Скасувати"
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Повернути false, якщо скасовано
            this.Close();
        }
    }
}
