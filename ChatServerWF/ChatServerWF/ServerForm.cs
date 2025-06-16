// ServerForm.cs (для ChatServerWF)
using System;
using System.Threading;
using System.Windows.Forms;

namespace ChatServerWF
{
    public partial class ServerForm : Form
    {
        private ServerObject server; // Об'єкт сервера
        private Thread listenThread; // Потік для прослуховування підключень

        public ServerForm()
        {
            InitializeComponent();
            this.FormClosing += ServerForm_FormClosing; // Обробник закриття форми
            LogMessage("Сервер готовий до запуску.");
        }

        // Метод для безпечного оновлення RichTextBox з різних потоків
        private void LogMessage(string message)
        {
            if (this.chatLogRichTextBox.InvokeRequired)
            {
                this.chatLogRichTextBox.Invoke(new Action(() => LogMessage(message)));
            }
            else
            {
                chatLogRichTextBox.AppendText(message + Environment.NewLine);
                chatLogRichTextBox.ScrollToCaret(); // Прокрутка до кінця
            }
        }

        // Обробник кнопки "Запустити сервер"
        private void btnStartServer_Click(object sender, EventArgs e)
        {
            if (server == null)
            {
                // Створюємо екземпляр ServerObject, передаючи метод для логування
                server = new ServerObject(LogMessage);
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.IsBackground = true;
                listenThread.Start();

                btnStartServer.Enabled = false;
                btnStopServer.Enabled = true;
            }
            else
            {
                LogMessage("Сервер вже запущено.");
            }
        }

        // Обробник кнопки "Зупинити сервер"
        private void btnStopServer_Click(object sender, EventArgs e)
        {
            if (server != null)
            {
                server.Disconnect(); // Відключаємо всіх клієнтів та зупиняємо слухача
                if (listenThread != null && listenThread.IsAlive)
                {
                    try
                    {
                        listenThread.Interrupt(); // Намагаємося перервати потік
                        listenThread.Join(100); // Чекаємо короткий час
                    }
                    catch (ThreadStateException) { /* Потік вже зупинено */ }
                    catch (ThreadInterruptedException) { /* Потік був перерваний */ }
                }
                server = null;
                listenThread = null;

                btnStartServer.Enabled = true;
                btnStopServer.Enabled = false;
            }
            else
            {
                LogMessage("Сервер не запущено.");
            }
        }

        // Обробник закриття форми сервера
        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (server != null)
            {
                server.Disconnect(); // Забезпечуємо коректне відключення при закритті форми
            }
            if (listenThread != null && listenThread.IsAlive)
            {
                try
                {
                    listenThread.Interrupt();
                    listenThread.Join(500); // Даємо час потоку на завершення
                }
                catch (Exception) { /* Ігноруємо помилки при завершенні потоку */ }
            }
        }
    }
}
