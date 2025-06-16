// ChatSettings.cs
using System;
using System.IO;
using System.Xml.Serialization; // Для серіалізації/десеріалізації налаштувань
using System.Windows; // Додано для MessageBox

namespace UdpChatApp
{
    // Клас для зберігання налаштувань чату
    [Serializable] // Позначаємо клас як серіалізований для збереження у файл
    public class ChatSettings
    {
        public string IpAddress { get; set; } = "127.0.0.1"; // IP-адреса за замовчуванням
        public int Port { get; set; } = 8888; // Порт за замовчуванням
        public string ChatFontFamily { get; set; } = "Inter"; // Сімейство шрифтів чату
        public double ChatFontSize { get; set; } = 14.0; // Розмір шрифту чату
        public bool EnableChatLogging { get; set; } = true; // Увімкнути/вимкнути логування
        public string ChatLogFilePath { get; set; } = "chat_log.txt"; // Шлях до файлу логу

        // Статичний метод для завантаження налаштувань
        public static ChatSettings Load()
        {
            try
            {
                // Використовуємо XML-серіалізацію для простого збереження об'єкта
                XmlSerializer serializer = new XmlSerializer(typeof(ChatSettings));
                using (FileStream fs = new FileStream("chat_settings.xml", FileMode.Open))
                {
                    return (ChatSettings)serializer.Deserialize(fs);
                }
            }
            catch (FileNotFoundException)
            {
                // Якщо файл не знайдено, повертаємо налаштування за замовчуванням
                return new ChatSettings();
            }
            catch (Exception ex)
            {
                // Для інших помилок, виводимо повідомлення та повертаємо налаштування за замовчуванням
                MessageBox.Show($"Помилка завантаження налаштувань: {ex.Message}. Використовуються налаштування за замовчуванням.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new ChatSettings();
            }
        }

        // Метод для збереження налаштувань
        public void Save()
        {
            try
            {
                // Виправлено: видалено дублюючий 'new'
                XmlSerializer serializer = new XmlSerializer(typeof(ChatSettings));
                using (FileStream fs = new FileStream("chat_settings.xml", FileMode.Create))
                {
                    serializer.Serialize(fs, this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження налаштувань: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
