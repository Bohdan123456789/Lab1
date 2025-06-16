// ClientForm.cs (для ChatClientWF)
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO; // Для File
using System.Drawing; // Для Font

namespace ChatClientWF
{
    public partial class ClientForm : Form
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private Thread receiveThread;
        private bool isConnected = false;
        private ChatSettings appSettings; // Об'єкт для зберігання налаштувань
        private string currentUserName;

        public ClientForm()
        {
            InitializeComponent();
            LoadApplicationSettings(); // Завантажуємо налаштування при старті
            ApplySettingsToUI(); // Застосовуємо налаштування до UI

            // При закритті форми переконайтеся, що підключення закрите
            this.FormClosing += ClientForm_FormClosing;
        }

        // Завантаження налаштувань додатку
        private void LoadApplicationSettings()
        {
            appSettings = ChatSettings.Load(); // Завантажуємо налаштування з файлу (або отримуємо за замовчуванням)
        }

        // Застосування налаштувань до UI головної форми
        private void ApplySettingsToUI()
        {
            // Застосовуємо шрифт та розмір до RichTextBox (chatTextBox)
            try
            {
                chatTextBox.Font = new Font(appSettings.ChatFontFamily, appSettings.ChatFontSize);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка застосування шрифту: {ex.Message}. Використовується шрифт за замовчуванням.", "Помилка шрифту", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                chatTextBox.Font = new Font("Microsoft Sans Serif", 10.0f); // Запасний варіант
            }
            // Можливо, встановити userNameTextBox.Text = appSettings.DefaultUserName, якщо таке поле буде
        }

        // Обробник для відкриття вікна налаштувань
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm(appSettings);
            DialogResult result = settingsForm.ShowDialog(); // Відкриваємо модально

            if (result == DialogResult.OK)
            {
                // Якщо налаштування були збережені, перезавантажуємо та застосовуємо їх
                LoadApplicationSettings();
                ApplySettingsToUI();
                LogMessage("Налаштування оновлено. Перепідключіться, щоб застосувати деякі зміни (IP, порт).", true);
                // Після зміни IP/Port потрібно перепідключитися. Можливо, варто автоматично відключати клієнта.
                if (isConnected)
                {
                    DisconnectChat();
                }
            }
        }

        // Обробник кнопки "Вхід"
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            currentUserName = txtUserName.Text.Trim();
            if (string.IsNullOrEmpty(currentUserName))
            {
                MessageBox.Show("Будь ласка, введіть ваше ім'я.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                tcpClient = new TcpClient();
                LogMessage($"Підключення до {appSettings.IpAddress}:{appSettings.Port}...", true);
                await tcpClient.ConnectAsync(appSettings.IpAddress, appSettings.Port);
                stream = tcpClient.GetStream();
                isConnected = true;

                // Відправити ім'я користувача на сервер
                byte[] data = Encoding.Unicode.GetBytes(currentUserName);
                await stream.WriteAsync(data, 0, data.Length);

                // Запускаємо потік для прийому повідомлень
                receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true; // Дозволяє потоку завершитися при закритті додатку
                receiveThread.Start();

                LogMessage($"Ви увійшли як {currentUserName}.", true);

                // Оновлення стану кнопок
                btnLogin.Enabled = false;
                btnLogout.Enabled = true;
                txtUserName.ReadOnly = true;
                txtMessage.Enabled = true;
                btnSend.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка підключення: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"Помилка підключення: {ex.Message}", true);
                DisconnectChat(); // Скидаємо стан, якщо підключення не вдалося
            }
        }

        // Обробник кнопки "Вихід"
        private void btnLogout_Click(object sender, EventArgs e)
        {
            DisconnectChat();
        }

        // Обробник кнопки "Отправить"
        private async void btnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(message) || !isConnected) return;

            try
            {
                // Відправляємо повідомлення на сервер
                byte[] data = Encoding.Unicode.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);

                // Додаємо власне повідомлення до логу чату (сервер поверне його, але ми додамо для негайного відображення)
                LogMessage($"{currentUserName}: {message}", false); // Відображаємо, як сервер би це зробив

                txtMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка відправки повідомлення: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"Помилка відправки: {ex.Message}", true);
                DisconnectChat(); // Відключаємо, якщо відправка не вдалася
            }
        }

        // Обробник натискання клавіші в полі введення повідомлення (для надсилання по Enter)
        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Запобігти додаванню нового рядка в TextBox
                btnSend_Click(sender, e);
            }
        }

        // Потік для прийому повідомлень від сервера
        private void ReceiveMessages()
        {
            byte[] data = new byte[256]; // Буфер для отриманих даних
            while (isConnected)
            {
                try
                {
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable); // Читаємо, поки є доступні дані

                    string message = builder.ToString();

                    // Оновлення UI через Invoke (для безпечного доступу з іншого потоку)
                    this.Invoke((MethodInvoker)delegate
                    {
                        LogMessage(message, false);
                    });
                }
                catch (IOException ex)
                {
                    if (isConnected)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            LogMessage($"Підключення до сервера втрачено: {ex.Message}", true);
                            DisconnectChat();
                        });
                    }
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        LogMessage($"Помилка прийому повідомлення: {ex.Message}", true);
                        DisconnectChat();
                    });
                    break;
                }
            }
        }

        // Метод для додавання повідомлень у chatTextBox та лог-файл
        private void LogMessage(string message, bool isSystemMessage)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string formattedMessage = $"[{timestamp}] {message}";

            // Додаємо текст до RichTextBox
            chatTextBox.AppendText(formattedMessage + Environment.NewLine);
            chatTextBox.ScrollToCaret(); // Автоматична прокрутка до кінця

            // Логування у файл, якщо увімкнуто
            if (appSettings.EnableChatLogging)
            {
                try
                {
                    // Перевіряємо та створюємо директорію, якщо вона не існує
                    string logDirectory = Path.GetDirectoryName(appSettings.ChatLogFilePath);
                    if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                    {
                        Directory.CreateDirectory(logDirectory);
                    }
                    File.AppendAllText(appSettings.ChatLogFilePath, formattedMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка логування у файл: {ex.Message}", "Помилка логування", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    appSettings.EnableChatLogging = false; // Вимкнути логування, щоб уникнути подальших помилок
                }
            }
        }

        // Метод для відключення від чату
        private void DisconnectChat()
        {
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
            }

            isConnected = false;

            if (receiveThread != null && receiveThread.IsAlive)
            {
                try
                {
                    receiveThread.Interrupt(); // Намагаємося перервати потік
                    receiveThread.Join(100); // Чекаємо трохи
                }
                catch (ThreadStateException) { /* Потік вже зупинено */ }
                catch (ThreadInterruptedException) { /* Потік був перерваний */ }
                receiveThread = null;
            }

            LogMessage("Ви відключилися від чату.", true);

            // Оновлення стану кнопок
            btnLogin.Enabled = true;
            btnLogout.Enabled = false;
            txtUserName.ReadOnly = false;
            txtMessage.Enabled = false;
            btnSend.Enabled = false;
        }

        // Обробник закриття головної форми
        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisconnectChat(); // Відключитися при закритті форми
        }

        // Обробник для кнопки "Вихід" з меню
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
