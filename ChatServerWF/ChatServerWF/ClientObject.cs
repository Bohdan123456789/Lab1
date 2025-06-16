// ClientObject.cs (для ChatServerWF)
using System;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.IO;

namespace ChatServerWF
{
    public class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        public string UserName { get; private set; } // Змінено на публічну властивість
        TcpClient client;
        ServerObject server; // об’єкт серверу

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                // отримуємо ім’я користувача
                string message = GetMessage();
                UserName = message; // Присвоюємо публічній властивості
                message = UserName + " вошел в чат";
                // відсилаемо повідомлення про вхід в чат користувача
                server.BroadcastMessage(message, this.Id);
                Console.WriteLine(message); // Все ще використовуємо Console.WriteLine для логування сервера

                // в нескінченому циклі отримуємо повідомлення клієнта
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        message = String.Format("{0}: {1}", UserName, message);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                    }
                    catch (IOException) // Обробка розриву з'єднання клієнтом
                    {
                        message = String.Format("{0}: покинув чат (збій з'єднання)", UserName);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Помилка отримання повідомлення від {UserName}: {ex.Message}");
                        message = String.Format("{0}: покинув чат (помилка отримання)", UserName);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Помилка обробки клієнта {UserName}: {e.Message}");
            }
            finally
            {
                // звільняємо ресурси
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // читання вхідного повідомлення та перетворення в строку
        private string GetMessage()
        {
            byte[] data = new byte[256]; // Збільшений буфер для більших повідомлень
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);
            return builder.ToString();
        }

        // закриття підключення
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
