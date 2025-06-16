// MainWindow.xaml.cs (для ProcessManagerApp)
using System;
using System.Collections.ObjectModel;
using System.Diagnostics; // Для роботи з процесами
using System.IO; // Для файлових операцій
using System.Linq; // Для LINQ-запитів
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading; // Для DispatcherTimer
using Microsoft.Win32; // Для SaveFileDialog
using System.Management; // <--- ДОДАНО: Для доступу до PerformanceCounter (для ЦП) та іншої інформації
// using System.Management; // Якщо у вас є кілька таких рядків, залиште лише один

namespace ProcessManagerApp
{
    // Клас для представлення процесу в ListView
    public class ProcessDisplayItem
    {
        public int Id { get; set; }
        public string ProcessName { get; set; }
        public double CpuUsage { get; set; } // Для реального CPU потрібен PerformanceCounter
        public double MemoryUsageMb { get; set; }
        public ProcessPriorityClass PriorityClass { get; set; }
        public int ThreadCount { get; set; }
        public Process Process { get; set; } // Зберігаємо посилання на об'єкт Process для подальших операцій
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<ProcessDisplayItem> _processes = new ObservableCollection<ProcessDisplayItem>();
        private DispatcherTimer _refreshTimer; // Таймер для автоматичного оновлення

        public MainWindow()
        {
            InitializeComponent();
            ProcessesListView.ItemsSource = _processes; // Прив'язуємо ListView до колекції

            InitializeRefreshTimer(); // Ініціалізуємо таймер
            RefreshProcesses(); // Початкове завантаження процесів
        }

        // Ініціалізація таймера для автоматичного оновлення
        private void InitializeRefreshTimer()
        {
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(5); // Оновлення кожні 5 секунд
            _refreshTimer.Tick += (sender, e) => RefreshProcesses();
            _refreshTimer.Start();
        }

        // Метод для оновлення списку процесів
        private void RefreshProcesses()
        {
            StatusTextBlock.Text = "Оновлення списку процесів...";
            _processes.Clear(); // Очищаємо поточний список

            try
            {
                Process[] allProcesses = Process.GetProcesses();
                foreach (Process p in allProcesses)
                {
                    try
                    {
                        p.Refresh(); // Оновлюємо дані процесу

                        double memoryUsageMb = p.WorkingSet64 / (1024.0 * 1024.0); // Пам'ять у МБ
                        double cpuUsage = 0; // Для точного CPU потрібен PerformanceCounter, що вимагає більше логіки

                        _processes.Add(new ProcessDisplayItem
                        {
                            Id = p.Id,
                            ProcessName = p.ProcessName,
                            CpuUsage = cpuUsage,
                            MemoryUsageMb = memoryUsageMb,
                            PriorityClass = p.PriorityClass,
                            ThreadCount = p.Threads.Count,
                            Process = p // Зберігаємо сам об'єкт Process
                        });
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        // Проігнорувати процеси, до яких немає доступу (наприклад, системні процеси)
                        // Або додати їх з мінімальною інформацією
                        _processes.Add(new ProcessDisplayItem
                        {
                            Id = p.Id,
                            ProcessName = p.ProcessName + " (Доступ відмовлено)",
                            CpuUsage = 0,
                            MemoryUsageMb = 0,
                            PriorityClass = ProcessPriorityClass.Normal, // За замовчуванням
                            ThreadCount = 0,
                            Process = p
                        });
                    }
                    catch (InvalidOperationException)
                    {
                        // Процес вже завершився
                    }
                    catch (Exception ex)
                    {
                        // Інші неочікувані помилки
                        StatusTextBlock.Text = $"Помилка при отриманні даних процесу: {ex.Message}";
                    }
                }
                StatusTextBlock.Text = $"Список оновлено. Знайдено процесів: {_processes.Count}";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Помилка завантаження процесів: {ex.Message}";
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка завантаження", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обробник натискання кнопки "Оновити список процесів"
        private void RefreshProcesses_Click(object sender, RoutedEventArgs e)
        {
            RefreshProcesses();
        }

        // Обробник натискання кнопки "Експортувати список"
        private void ExportProcesses_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстові файли (*.txt)|*.txt|Усі файли (*.*)|*.*";
            saveFileDialog.FileName = "process_list.txt";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var lines = new System.Collections.Generic.List<string>();
                    lines.Add("ID\tНазва процесу\tЦП (%)\tПам'ять (МБ)\tПриоритет\tКількість потоків"); // Заголовок

                    foreach (var p in _processes)
                    {
                        lines.Add($"{p.Id}\t{p.ProcessName}\t{p.CpuUsage:F2}\t{p.MemoryUsageMb:F2}\t{p.PriorityClass}\t{p.ThreadCount}");
                    }

                    File.WriteAllLines(saveFileDialog.FileName, lines);
                    StatusTextBlock.Text = $"Список процесів експортовано до {saveFileDialog.FileName}";
                }
                catch (Exception ex)
                {
                    StatusTextBlock.Text = $"Помилка експорту: {ex.Message}";
                    MessageBox.Show($"Помилка експорту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обробник пункту контекстного меню "Деталі процесу"
        private void ShowProcessDetails_Click(object sender, RoutedEventArgs e)
        {
            ProcessDisplayItem selectedProcessItem = ProcessesListView.SelectedItem as ProcessDisplayItem;
            if (selectedProcessItem != null && selectedProcessItem.Process != null)
            {
                try
                {
                    // Перевіряємо, чи процес все ще активний
                    selectedProcessItem.Process.Refresh();
                    if (!selectedProcessItem.Process.HasExited)
                    {
                        ProcessInfoDialog dialog = new ProcessInfoDialog(selectedProcessItem.Process);
                        dialog.Owner = this; // Встановлюємо батьківське вікно
                        dialog.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Цей процес вже завершився.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshProcesses(); // Оновити список, щоб видалити завершений процес
                    }
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Цей процес вже завершився.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshProcesses();
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    MessageBox.Show($"Відмовлено в доступі до деталей процесу: {ex.Message}\nСпробуйте запустити додаток з правами адміністратора.", "Помилка доступу", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося отримати деталі процесу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть процес.", "Вибір процесу", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Обробник пункту контекстного меню "Завершити процес"
        private void TerminateProcess_Click(object sender, RoutedEventArgs e)
        {
            ProcessDisplayItem selectedProcessItem = ProcessesListView.SelectedItem as ProcessDisplayItem;
            if (selectedProcessItem != null && selectedProcessItem.Process != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Ви впевнені, що хочете завершити процес '{selectedProcessItem.ProcessName}' (ID: {selectedProcessItem.Id})?",
                    "Підтвердження завершення процесу",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Перевіряємо, чи процес все ще активний
                        selectedProcessItem.Process.Refresh();
                        if (!selectedProcessItem.Process.HasExited)
                        {
                            selectedProcessItem.Process.Kill(); // Завершуємо процес
                            StatusTextBlock.Text = $"Процес '{selectedProcessItem.ProcessName}' (ID: {selectedProcessItem.Id}) завершено.";
                        }
                        else
                        {
                            StatusTextBlock.Text = $"Процес '{selectedProcessItem.ProcessName}' (ID: {selectedProcessItem.Id}) вже завершився.";
                        }
                        RefreshProcesses(); // Оновлюємо список після завершення
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        MessageBox.Show($"Відмовлено в доступі для завершення процесу: {ex.Message}\nСпробуйте запустити додаток з правами адміністратора.", "Помилка доступу", MessageBoxButton.OK, MessageBoxImage.Error);
                        StatusTextBlock.Text = "Помилка: Відмовлено в доступі до завершення процесу.";
                    }
                    catch (InvalidOperationException)
                    {
                        MessageBox.Show("Процес вже завершився.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshProcesses();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не вдалося завершити процес: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        StatusTextBlock.Text = "Помилка завершення процесу.";
                    }
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть процес для завершення.", "Вибір процесу", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
