// Program.cs (для ChatServerWF)
using System;
using System.Windows.Forms; // Додано для Application

namespace ChatServerWF
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread] // Важливо для Windows Forms
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ServerForm()); // Запускаємо нашу форму сервера
        }
    }
}
