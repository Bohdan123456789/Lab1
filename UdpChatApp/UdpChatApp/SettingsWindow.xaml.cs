// SettingsWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // Для FontFamily

namespace UdpChatApp
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private ChatSettings _currentSettings; // Поточні налаштування, з якими працює вікно

        public SettingsWindow(ChatSettings settings) // Цей конструктор потрібен!
        {
            InitializeComponent();
            _currentSettings = settings; // Приймаємо поточні налаштування з головного вікна
            LoadSettingsToUI(); // Завантажуємо їх в UI
            InitializeFontControls(); // Ініціалізуємо ComboBox'и шрифтів
        }

        // Ініціалізація вибору шрифтів та розмірів
        private void InitializeFontControls()
        {
            // Заповнення ComboBox шрифтами
            foreach (FontFamily fontFamily in Fonts.SystemFontFamilies.OrderBy(f => f.Source))
            {
                cmbChatFontFamily.Items.Add(fontFamily.Source); // Додаємо назву шрифту
            }
            cmbChatFontFamily.SelectedItem = _currentSettings.ChatFontFamily;
            if (cmbChatFontFamily.SelectedItem == null && cmbChatFontFamily.Items.Count > 0)
            {
                cmbChatFontFamily.SelectedIndex = 0;
            }

            // Заповнення ComboBox розмірами шрифтів
            for (int i = 8; i <= 72; i += 2)
            {
                cmbChatFontSize.Items.Add((double)i);
            }
            cmbChatFontSize.SelectedItem = _currentSettings.ChatFontSize;
            if (cmbChatFontSize.SelectedItem == null && cmbChatFontSize.Items.Count > 0)
            {
                cmbChatFontSize.SelectedIndex = 0;
            }
        }


        // Завантаження налаштувань з об'єкта ChatSettings в UI
        private void LoadSettingsToUI()
        {
            txtIpAddress.Text = _currentSettings.IpAddress;
            txtPort.Text = _currentSettings.Port.ToString();
            // Шрифти будуть завантажені в InitializeFontControls
            chkEnableChatLogging.IsChecked = _currentSettings.EnableChatLogging;
            txtChatLogFilePath.Text = _currentSettings.ChatLogFilePath;
        }

        // Збереження налаштувань з UI в об'єкт ChatSettings
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentSettings.IpAddress = txtIpAddress.Text;

                if (int.TryParse(txtPort.Text, out int port))
                {
                    _currentSettings.Port = port;
                }
                else
                {
                    MessageBox.Show("Будь ласка, введіть дійсний номер порту.", "Помилка введення", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _currentSettings.ChatFontFamily = cmbChatFontFamily.SelectedItem?.ToString() ?? "Inter";
                if (double.TryParse(cmbChatFontSize.SelectedItem?.ToString(), out double fontSize))
                {
                    _currentSettings.ChatFontSize = fontSize;
                }
                else
                {
                    MessageBox.Show("Будь ласка, виберіть дійсний розмір шрифту.", "Помилка введення", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _currentSettings.EnableChatLogging = chkEnableChatLogging.IsChecked ?? false;
                _currentSettings.ChatLogFilePath = txtChatLogFilePath.Text;

                _currentSettings.Save(); // Зберігаємо налаштування у файл
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
