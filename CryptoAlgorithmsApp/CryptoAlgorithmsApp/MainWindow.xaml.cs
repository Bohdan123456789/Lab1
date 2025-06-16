// MainWindow.xaml.cs
using System;
using System.Security.Cryptography; // Для реальних криптографічних алгоритмів (якщо потрібні)
using System.Text; // Для кодування/декодування рядків
using System.Threading.Tasks; // Для асинхронних операцій
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // Для кольорів Foreground, якщо потрібно

namespace CryptoAlgorithmsApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            txtOverallStatus.Text = "Готово до запуску...";
        }

        // Метод 1: Імітація блокового алгоритму SKIPJACK (асинхронно)
        private async Task SimulateSkipjack(string input, TextBlock statusBlock, TextBlock resultBlock)
        {
            statusBlock.Text = "Виконання...";
            statusBlock.Foreground = Brushes.Orange;
            resultBlock.Text = string.Empty;

            try
            {
                // Імітація тривалої операції
                await Task.Delay(2000); // Затримка на 2 секунди

                // Дуже спрощена "імітація" шифрування
                // У реальному житті тут був би складний алгоритм SKIPJACK
                string encryptedText = $"SKIPJACK_Encrypted({input.Length}): {Convert.ToBase64String(Encoding.UTF8.GetBytes(input + "_SALT"))}";

                resultBlock.Text = encryptedText;
                statusBlock.Text = "Завершено";
                statusBlock.Foreground = Brushes.Green;
            }
            catch (Exception ex)
            {
                resultBlock.Text = $"Помилка: {ex.Message}";
                statusBlock.Text = "Помилка";
                statusBlock.Foreground = Brushes.Red;
            }
        }

        // Метод 2: Імітація алгоритму хешування Snefru (асинхронно)
        private async Task SimulateSnefru(string input, TextBlock statusBlock, TextBlock resultBlock)
        {
            statusBlock.Text = "Виконання...";
            statusBlock.Foreground = Brushes.Orange;
            resultBlock.Text = string.Empty;

            try
            {
                // Імітація тривалої операції
                await Task.Delay(1500); // Затримка на 1.5 секунди

                // Дуже спрощена "імітація" хешування
                // У реальному житті тут був би складний алгоритм Snefru
                // Для простоти, використовуємо SHA256 як placeholder
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2")); // Перетворення в шістнадцятковий формат
                    }
                    resultBlock.Text = $"SNEFRU_HASH({input.Length}): {builder.ToString()} (спрощ.)";
                }

                statusBlock.Text = "Завершено";
                statusBlock.Foreground = Brushes.Green;
            }
            catch (Exception ex)
            {
                resultBlock.Text = $"Помилка: {ex.Message}";
                statusBlock.Text = "Помилка";
                statusBlock.Foreground = Brushes.Red;
            }
        }

        // Метод 3: Імітація PKZIP шифрування/генерації випадкових чисел (асинхронно)
        private async Task SimulatePkzip(string input, TextBlock statusBlock, TextBlock resultBlock)
        {
            statusBlock.Text = "Виконання...";
            statusBlock.Foreground = Brushes.Orange;
            resultBlock.Text = string.Empty;

            try
            {
                // Імітація тривалої операції
                await Task.Delay(2500); // Затримка на 2.5 секунди

                // Дуже спрощена "імітація" PKZIP шифрування та генерації
                // У реальному житті PKZIP не є криптографічним алгоритмом сам по собі,
                // але використовує алгоритми шифрування та генерації.
                Random rand = new Random();
                string generatedData = string.Join("", Enumerable.Range(0, 16).Select(_ => ((char)rand.Next(33, 126)).ToString()));

                resultBlock.Text = $"PKZIP_Simulated({input.Length}): Encrypted({input.Substring(0, Math.Min(input.Length, 5))}...){generatedData}";

                statusBlock.Text = "Завершено";
                statusBlock.Foreground = Brushes.Green;
            }
            catch (Exception ex)
            {
                resultBlock.Text = $"Помилка: {ex.Message}";
                statusBlock.Text = "Помилка";
                statusBlock.Foreground = Brushes.Red;
            }
        }

        // Обробник натискання кнопки "Виконати SKIPJACK"
        private async void RunSkipjack_Click(object sender, RoutedEventArgs e)
        {
            string input = txtSkipjackInput.Text;
            await SimulateSkipjack(input, txtSkipjackStatus, txtSkipjackResult);
        }

        // Обробник натискання кнопки "Виконати Snefru"
        private async void RunSnefru_Click(object sender, RoutedEventArgs e)
        {
            string input = txtSnefruInput.Text;
            await SimulateSnefru(input, txtSnefruStatus, txtSnefruResult);
        }

        // Обробник натискання кнопки "Виконати PKZIP"
        private async void RunPkzip_Click(object sender, RoutedEventArgs e)
        {
            string input = txtPkzipInput.Text;
            await SimulatePkzip(input, txtPkzipStatus, txtPkzipResult);
        }

        // Обробник натискання кнопки "Виконати всі методи асинхронно"
        private async void RunAll_Click(object sender, RoutedEventArgs e)
        {
            txtOverallStatus.Text = "Запуск всіх алгоритмів...";
            txtOverallStatus.Foreground = Brushes.Blue;

            // Скинути статус окремих блоків перед запуском всіх
            txtSkipjackStatus.Text = "Очікування...";
            txtSkipjackStatus.Foreground = Brushes.Gray;
            txtSkipjackResult.Text = string.Empty;

            txtSnefruStatus.Text = "Очікування...";
            txtSnefruStatus.Foreground = Brushes.Gray;
            txtSnefruResult.Text = string.Empty;

            txtPkzipStatus.Text = "Очікування...";
            txtPkzipStatus.Foreground = Brushes.Gray;
            txtPkzipResult.Text = string.Empty;


            // Запускаємо всі три методи асинхронно і чекаємо їх завершення
            try
            {
                await Task.WhenAll(
                    SimulateSkipjack(txtSkipjackInput.Text, txtSkipjackStatus, txtSkipjackResult),
                    SimulateSnefru(txtSnefruInput.Text, txtSnefruStatus, txtSnefruResult),
                    SimulatePkzip(txtPkzipInput.Text, txtPkzipStatus, txtPkzipResult)
                );

                txtOverallStatus.Text = "Всі алгоритми завершено!";
                txtOverallStatus.Foreground = Brushes.DarkGreen;
            }
            catch (Exception ex)
            {
                txtOverallStatus.Text = $"Помилка під час виконання всіх алгоритмів: {ex.Message}";
                txtOverallStatus.Foreground = Brushes.Red;
            }
        }
    }
}
