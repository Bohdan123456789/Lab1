// ServerObject.cs (для ChatServerWF)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;

namespace ChatServerWF
{
    public class ServerObject
    {
        static TcpListener tcpListener; // сервер для прослуховування
        List<ClientObject> clients = new List<ClientObject>(); // всі підключення
        protected internal Action<string> ServerLogCallback; // Делегат для оновлення UI форми

        public ServerObject(Action<string> logCallback = null) // Конструктор для отримання callback
        {
            ServerLogCallback = logCallback;
        }

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
            LogMessageToUI($"Клієнт підключився: {clientObject.UserName ?? clientObject.Id}");
        }

        protected internal void RemoveConnection(string id)
        {
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
            {
                clients.Remove(client);
                LogMessageToUI($"Клієнт відключився: {client.UserName ?? client.Id}");
            }
        }

        // прослуховування вхідних повідомлень
        public void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888); // Порт за замовчуванням
                tcpListener.Start();
                LogMessageToUI("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.IsBackground = true; // Дозволяє завершитися при закритті сервера
                    clientThread.Start();
                }
            }
            catch (SocketException ex)
            {
                LogMessageToUI($"Помилка сокету: {ex.Message}");
            }
            catch (ThreadAbortException)
            {
                LogMessageToUI("Потік прослуховування сервера зупинено.");
            }
            catch (Exception ex)
            {
                LogMessageToUI($"Загальна помилка сервера: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        // трансляція повідомлення підключеним клієнтам
        protected internal void BroadcastMessage(string message, string excludeClientId = null)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            foreach (ClientObject client in clients.ToList()) // Використовуємо ToList() для безпечної ітерації
            {
                if (client.Id != excludeClientId)
                {
                    try
                    {
                        client.Stream.Write(data, 0, data.Length); // передача даних
                    }
                    catch (IOException)
                    {
                        LogMessageToUI($"Клієнт {client.UserName ?? client.Id} відключився під час трансляції.");
                        RemoveConnection(client.Id);
                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        LogMessageToUI($"Помилка відправки повідомлення клієнту {client.UserName ?? client.Id}: {ex.Message}");
                    }
                }
            }
        }

        // відключення всіх клієнтів
        protected internal void Disconnect()
        {
            if (tcpListener != null)
            {
                tcpListener.Stop(); // зупинка сервера
                LogMessageToUI("Сервер зупинено.");
            }

            foreach (ClientObject client in clients.ToList())
            {
                client.Close();
            }
            clients.Clear();
        }

        // Метод для логування повідомлень у UI форми
        private void LogMessageToUI(string message)
        {
            ServerLogCallback?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}
