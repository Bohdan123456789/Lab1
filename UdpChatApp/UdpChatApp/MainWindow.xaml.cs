// MainWindow.xaml.cs
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows; // Додано для MessageBox
using System.Windows.Controls;
using System.Windows.Media; // Для FontFamily
using System.IO; // Додано для File, якщо раніше не було

namespace UdpChatApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UdpClient udpClient;
        private Thread receiveThread;
        private bool isListening = false;
        private ChatSettings appSettings; // Об'єкт для зберігання налаштувань

        public MainWindow()
        {
            InitializeComponent();
            LoadApplicationSettings(); // Завантажуємо налаштування при старті
            ApplySettingsToUI(); // Застосовуємо налаштування до UI

            // При закритті вікна переконайтеся, що потоки зупинені
            this.Closing += MainWindow_Closing;
        }

        // Завантаження налаштувань додатку
        private void LoadApplicationSettings()
        {
            appSettings = ChatSettings.Load(); // Завантажуємо налаштування з файлу (або отримуємо за замовчуванням)
        }

        // Застосування налаштувань до UI головного вікна
        private void ApplySettingsToUI()
        {
            chatTextBox.FontFamily = new FontFamily(appSettings.ChatFontFamily);
            chatTextBox.FontSize = appSettings.ChatFontSize;
        }

        // Обробник для відкриття вікна налаштувань
        private void OpenChatSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(appSettings); // Передаємо поточні налаштування
            bool? result = settingsWindow.ShowDialog(); // Відкриваємо модально

            if (result == true)
            {
                // Якщо налаштування були збережені, перезавантажуємо та застосовуємо їх
                LoadApplicationSettings();
                ApplySettingsToUI();
                LogMessage("Налаштування оновлено.", true); // Записуємо в лог, що налаштування оновлено
            }
        }

        // Обробник кнопки "Вхід"
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string userName = userNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("Будь ласка, введіть ваше ім'я.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Ініціалізуємо UDP клієнт, прив'язуючись до вказаного порту
                udpClient = new UdpClient(appSettings.Port);
                isListening = true;

                // Запускаємо потік для прийому повідомлень
                receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true; // Дозволяє потоку завершитися при закритті додатку
                receiveThread.Start();

                LogMessage($"Ви увійшли як {userName} на порт {appSettings.Port}. Очікування повідомлень...", true);

                // Оновлення стану кнопок
                loginButton.IsEnabled = false;
                logoutButton.IsEnabled = true;
                userNameTextBox.IsReadOnly = true;
                messageTextBox.IsEnabled = true;
                sendButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка підключення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                LogMessage($"Помилка підключення: {ex.Message}", true);
            }
        }

        // Обробник кнопки "Вихід"
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            DisconnectChat();
        }

        // Обробник кнопки "Отправить"
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = messageTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            string userName = userNameTextBox.Text.Trim();
            string fullMessage = $"{userName}: {message}";

            try
            {
                // Відправляємо повідомлення на IP-адресу та порт, вказані в налаштуваннях
                byte[] data = Encoding.UTF8.GetBytes(fullMessage);
                IPEndPoint targetEndPoint = new IPEndPoint(IPAddress.Parse(appSettings.IpAddress), appSettings.Port);
                await udpClient.SendAsync(data, data.Length, targetEndPoint);

                // Додаємо власне повідомлення до логу чату
                LogMessage($"Ви: {message}", false); // Не додаємо ім'я користувача, бо воно вже відображається як "Ви"

                messageTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка відправки повідомлення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                LogMessage($"Помилка відправки: {ex.Message}", true);
            }
        }

        // Потік для прийому повідомлень
        private void ReceiveMessages()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (isListening)
            {
                try
                {
                    // Очікування повідомлення
                    byte[] receivedBytes = udpClient.Receive(ref remoteEndPoint);
                    string receivedMessage = Encoding.UTF8.GetString(receivedBytes);

                    // Оновлення UI через Dispatcher (для безпечного доступу з іншого потоку)
                    Dispatcher.Invoke(() =>
                    {
                        string senderInfo = remoteEndPoint.Address.ToString();
                        // Якщо відправник - це ми самі, ми вже додали повідомлення до чату
                        // Якщо ні, додаємо його до чату.
                        if (!remoteEndPoint.Address.Equals(IPAddress.Parse(appSettings.IpAddress)) || receivedMessage.StartsWith($"{userNameTextBox.Text.Trim()}:"))
                        {
                            // Перевірка, чи повідомлення не є дублюючим (тобто, від нас самих)
                            // Ця перевірка спрощена, і може бути покращена для більш складних сценаріїв.
                            if (!receivedMessage.Contains($"Ви: ") && !receivedMessage.StartsWith($"{userNameTextBox.Text.Trim()}:"))
                            {
                                LogMessage($"{senderInfo}: {receivedMessage}", false);
                            }
                            else if (receivedMessage.StartsWith($"{userNameTextBox.Text.Trim()}:"))
                            {
                                // Якщо повідомлення від нас самих, і ми його ще не записали
                                LogMessage(receivedMessage, false);
                            }
                        }
                    });
                }
                catch (SocketException ex)
                {
                    // Обробка помилки, наприклад, при закритті сокету
                    if (isListening) // Тільки якщо ми все ще слухаємо
                    {
                        Dispatcher.Invoke(() => LogMessage($"Помилка прийому: {ex.Message}", true));
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Сокет був закритий, це нормально при виході
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => LogMessage($"Загальна помилка прийому: {ex.Message}", true));
                }
            }
        }

        // Метод для додавання повідомлень у chatTextBox та лог-файл
        private void LogMessage(string message, bool isSystemMessage)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string formattedMessage = $"[{timestamp}] {message}";

            chatTextBox.AppendText(formattedMessage + Environment.NewLine);
            chatTextBox.ScrollToEnd(); // Автоматична прокрутка до кінця

            // Логування у файл, якщо увімкнено
            if (appSettings.EnableChatLogging)
            {
                try
                {
                    File.AppendAllText(appSettings.ChatLogFilePath, formattedMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    // Запобігаємо збою додатку через помилки логування
                    MessageBox.Show($"Помилка логування у файл: {ex.Message}", "Помилка логування", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        // Метод для відключення від чату
        private void DisconnectChat()
        {
            if (udpClient != null)
            {
                isListening = false;
                udpClient.Close(); // Закриття сокету
                udpClient.Dispose(); // Звільнення ресурсів
                udpClient = null;
            }

            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Join(100); // Чекаємо трохи, щоб потік завершився
                if (receiveThread.IsAlive)
                {
                    receiveThread.Abort(); // Якщо потік все ще живий, примусово перериваємо
                }
                receiveThread = null;
            }

            LogMessage("Ви відключилися від чату.", true);

            // Оновлення стану кнопок
            loginButton.IsEnabled = true;
            logoutButton.IsEnabled = false;
            userNameTextBox.IsReadOnly = false;
            messageTextBox.IsEnabled = false;
            sendButton.IsEnabled = false;
        }

        // Обробник закриття головного вікна
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DisconnectChat(); // Відключитися при закритті вікна
        }

        // Обробник для кнопки "Вихід" з меню "Файл"
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
