// ProcessInfoDialog.xaml.cs
using System;
using System.Diagnostics;
using System.Windows;
using System.IO; // Для Path.GetFileName

namespace ProcessManagerApp
{
    /// <summary>
    /// Interaction logic for ProcessInfoDialog.xaml
    /// </summary>
    public partial class ProcessInfoDialog : Window
    {
        private Process _process;

        public ProcessInfoDialog(Process process)
        {
            InitializeComponent();
            _process = process;
            LoadProcessDetails();
        }

        private void LoadProcessDetails()
        {
            try
            {
                _process.Refresh(); // Оновлюємо дані процесу
                if (_process.HasExited)
                {
                    MessageBox.Show("Процес вже завершився.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                    return;
                }

                // Загальна інформація
                ProcessNameTextBlock.Text = $"Назва: {_process.ProcessName}";
                ProcessIdTextBlock.Text = $"ID: {_process.Id}";

                try
                {
                    ProcessPathTextBlock.Text = $"Шлях: {_process.MainModule.FileName}";
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    ProcessPathTextBlock.Text = "Шлях: Доступ відмовлено або процес не має основного модуля.";
                }
                catch (InvalidOperationException)
                {
                    ProcessPathTextBlock.Text = "Шлях: Процес вже завершився.";
                }

                ProcessMemoryTextBlock.Text = $"Використання пам'яті: {_process.WorkingSet64 / (1024.0 * 1024.0):F2} МБ";

                try
                {
                    ProcessStartTimeTextBlock.Text = $"Час запуску: {_process.StartTime.ToString()}";
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    ProcessStartTimeTextBlock.Text = "Час запуску: Доступ відмовлено.";
                }
                catch (InvalidOperationException)
                {
                    ProcessStartTimeTextBlock.Text = "Час запуску: Процес вже завершився.";
                }


                // Потоки
                ThreadsListView.ItemsSource = _process.Threads;

                // Модулі
                // Доступ до модулів може викликати Win32Exception для деяких процесів
                try
                {
                    ModulesListView.ItemsSource = _process.Modules;
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    MessageBox.Show($"Відмовлено в доступі до модулів процесу: {ex.Message}\nСпробуйте запустити додаток з правами адміністратора.", "Помилка доступу", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (InvalidOperationException)
                {
                    // Процес вже завершився під час спроби доступу до модулів
                }
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Процес вже завершився і не може бути відображений.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Відмовлено в доступі до деталей процесу: {ex.Message}\nСпробуйте запустити додаток з правами адміністратора.", "Помилка доступу", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося завантажити деталі процесу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        // Обробник кнопки "Закрити"
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
