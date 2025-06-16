// MainWindow.xaml.cs
using System;
using System.Data;
using System.Data.OleDb; // Для роботи з базами даних Access (mdb/accdb)
using System.Windows;
using System.Windows.Controls;
using System.IO; // Для Path.Combine та File.Exists
using System.Windows.Media; // Для Brush (зміна кольору тексту статусу)
using System.ComponentModel; // Для CancelEventArgs

namespace PreschoolManagementApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OleDbConnection connection;
        private string connectionString;

        public MainWindow()
        {
            InitializeComponent();
            // Встановлюємо шлях до бази даних у текстове поле.
            // Припускаємо, що файл знаходиться поруч з виконуваним файлом програми.
            txtDbPath.Text = "PreschoolDB.accdb";
            UpdateConnectionStatus(false); // Початковий статус: не підключено
        }

        // Оновлення статусу підключення у UI
        private void UpdateConnectionStatus(bool isConnected, string message = "")
        {
            if (isConnected)
            {
                txtConnectionStatus.Text = "Статус: Підключено до БД! " + message;
                txtConnectionStatus.Foreground = Brushes.Green;
            }
            else
            {
                txtConnectionStatus.Text = "Статус: Не підключено. " + message;
                txtConnectionStatus.Foreground = Brushes.Red;
            }
        }

        // Обробник натискання кнопки "Підключитися до БД"
        private void ConnectToDb_Click(object sender, RoutedEventArgs e)
        {
            string dbFileName = txtDbPath.Text;
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFileName);

            if (!File.Exists(fullPath))
            {
                UpdateConnectionStatus(false, $"Помилка: Файл бази даних '{dbFileName}' не знайдено за шляхом: {fullPath}");
                return;
            }

            // Рядок підключення для бази даних Access (.accdb)
            // Примітка: Для 64-бітної ОС може знадобитися встановити "Microsoft Access Database Engine 2010 Redistributable" або "2016 Redistributable"
            // Це для OleDb.
            connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={fullPath};Persist Security Info=False;";

            connection = new OleDbConnection(connectionString);

            try
            {
                connection.Open();
                UpdateConnectionStatus(true, "Підключення успішне.");
            }
            catch (Exception ex)
            {
                UpdateConnectionStatus(false, $"Помилка підключення: {ex.Message}");
                MessageBox.Show($"Не вдалося підключитися до бази даних.\n\nПереконайтесь, що встановлено 'Microsoft Access Database Engine 2010 (або 2016) Redistributable' (64-bit).\n\nДеталі: {ex.Message}", "Помилка підключення", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Допоміжний метод для перевірки підключення до БД
        private bool IsDbConnected()
        {
            if (connection == null || connection.State != ConnectionState.Open)
            {
                MessageBox.Show("Будь ласка, спочатку підключіться до бази даних на вкладці 'Підключення'.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                UpdateConnectionStatus(false, "Відсутнє підключення.");
                return false;
            }
            return true;
        }

        // Обробник натискання кнопки "Додати Дитину"
        private void AddChild_Click(object sender, RoutedEventArgs e)
        {
            if (!IsDbConnected()) return;

            if (string.IsNullOrWhiteSpace(txtChildFirstName.Text) || string.IsNullOrWhiteSpace(txtChildLastName.Text) || !dpChildDOB.SelectedDate.HasValue || string.IsNullOrWhiteSpace(txtChildGroupId.Text))
            {
                txtChildStatus.Text = "Будь ласка, заповніть всі поля для дитини.";
                txtChildStatus.Foreground = Brushes.Red;
                return;
            }

            // Генеруємо новий ChildID (або ви можете використовувати автоінкремент в Access, якщо він налаштований)
            // Якщо ChildID в Access - це AutoIncrement, то вам не потрібно його вказувати в INSERT.
            // Припустимо, що у нас AutoIncrement.
            // Якщо не AutoIncrement, потрібно генерувати унікальний ID, наприклад, guid або max(ChildID)+1
            // int newChildId = GetNextId("Children", "ChildID"); // Якщо ID не автоінкрементний

            string firstName = txtChildFirstName.Text;
            string lastName = txtChildLastName.Text;
            string dob = dpChildDOB.SelectedDate.Value.ToString("yyyy-MM-dd"); // Формат дати
            if (!int.TryParse(txtChildGroupId.Text, out int groupId))
            {
                txtChildStatus.Text = "ID Групи має бути числом.";
                txtChildStatus.Foreground = Brushes.Red;
                return;
            }

            string query = "INSERT INTO Children (FirstName, LastName, DateOfBirth, GroupID) VALUES (@FirstName, @LastName, @DateOfBirth, @GroupID)";

            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("@FirstName", firstName);
                command.Parameters.AddWithValue("@LastName", lastName);
                command.Parameters.AddWithValue("@DateOfBirth", dob);
                command.Parameters.AddWithValue("@GroupID", groupId);

                try
                {
                    command.ExecuteNonQuery();
                    txtChildStatus.Text = "Дитину успішно додано!";
                    txtChildStatus.Foreground = Brushes.Green;
                    // Очистити поля
                    txtChildFirstName.Clear();
                    txtChildLastName.Clear();
                    dpChildDOB.SelectedDate = null;
                    txtChildGroupId.Clear();
                }
                catch (Exception ex)
                {
                    txtChildStatus.Text = $"Помилка додавання дитини: {ex.Message}";
                    txtChildStatus.Foreground = Brushes.Red;
                }
            }
        }

        // Обробник натискання кнопки "Додати Групу"
        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!IsDbConnected()) return;

            if (string.IsNullOrWhiteSpace(txtGroupName.Text) || string.IsNullOrWhiteSpace(txtGroupTeacherId.Text))
            {
                txtGroupStatus.Text = "Будь ласка, заповніть всі поля для групи.";
                txtGroupStatus.Foreground = Brushes.Red;
                return;
            }

            string groupName = txtGroupName.Text;
            if (!int.TryParse(txtGroupTeacherId.Text, out int teacherId))
            {
                txtGroupStatus.Text = "ID Вихователя має бути числом.";
                txtGroupStatus.Foreground = Brushes.Red;
                return;
            }

            // int newGroupId = GetNextId("Groups", "GroupID"); // Якщо ID не автоінкрементний

            string query = "INSERT INTO Groups (GroupName, TeacherID) VALUES (@GroupName, @TeacherID)";

            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("@GroupName", groupName);
                command.Parameters.AddWithValue("@TeacherID", teacherId);

                try
                {
                    command.ExecuteNonQuery();
                    txtGroupStatus.Text = "Групу успішно додано!";
                    txtGroupStatus.Foreground = Brushes.Green;
                    txtGroupName.Clear();
                    txtGroupTeacherId.Clear();
                }
                catch (Exception ex)
                {
                    txtGroupStatus.Text = $"Помилка додавання групи: {ex.Message}";
                    txtGroupStatus.Foreground = Brushes.Red;
                }
            }
        }

        // Обробник натискання кнопки "Додати Вихователя"
        private void AddTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (!IsDbConnected()) return;

            if (string.IsNullOrWhiteSpace(txtTeacherFirstName.Text) || string.IsNullOrWhiteSpace(txtTeacherLastName.Text))
            {
                txtTeacherStatus.Text = "Будь ласка, заповніть ім'я та прізвище вихователя.";
                txtTeacherStatus.Foreground = Brushes.Red;
                return;
            }

            string firstName = txtTeacherFirstName.Text;
            string lastName = txtTeacherLastName.Text;
            string phone = txtTeacherPhone.Text;
            string email = txtTeacherEmail.Text;

            // int newTeacherId = GetNextId("Teachers", "TeacherID"); // Якщо ID не автоінкрементний

            string query = "INSERT INTO Teachers (FirstName, LastName, Phone, Email) VALUES (@FirstName, @LastName, @Phone, @Email)";

            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("@FirstName", firstName);
                command.Parameters.AddWithValue("@LastName", lastName);
                command.Parameters.AddWithValue("@Phone", phone);
                command.Parameters.AddWithValue("@Email", email);

                try
                {
                    command.ExecuteNonQuery();
                    txtTeacherStatus.Text = "Вихователя успішно додано!";
                    txtTeacherStatus.Foreground = Brushes.Green;
                    txtTeacherFirstName.Clear();
                    txtTeacherLastName.Clear();
                    txtTeacherPhone.Clear();
                    txtTeacherEmail.Clear();
                }
                catch (Exception ex)
                {
                    txtTeacherStatus.Text = $"Помилка додавання вихователя: {ex.Message}";
                    txtTeacherStatus.Foreground = Brushes.Red;
                }
            }
        }

        // Обробник зміни вибору у ComboBox звітів
        private void CmbReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Цей метод можна використовувати для попереднього завантаження даних
            // або для динамічного оновлення інших елементів UI, якщо потрібно.
            // Наразі основна логіка генерується по кнопці "Побудувати Звіт".
        }

        // Обробник натискання кнопки "Побудувати Звіт"
        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (!IsDbConnected()) return;

            if (cmbReports.SelectedItem is ComboBoxItem selectedItem)
            {
                string reportType = selectedItem.Content.ToString();
                DataTable dataTable = new DataTable();
                string query = "";

                try
                {
                    if (reportType == "Всі діти з назвами груп")
                    {
                        query = "SELECT C.ChildID, C.FirstName, C.LastName, C.DateOfBirth, G.GroupName " +
                                "FROM Children AS C INNER JOIN Groups AS G ON C.GroupID = G.GroupID;";
                    }
                    else if (reportType == "Всі групи з вихователями")
                    {
                        query = "SELECT G.GroupID, G.GroupName, T.FirstName AS TeacherFirstName, T.LastName AS TeacherLastName " +
                                "FROM Groups AS G INNER JOIN Teachers AS T ON G.TeacherID = T.TeacherID;";
                    }
                    else
                    {
                        MessageBox.Show("Будь ласка, виберіть тип звіту.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                        dataGridReports.ItemsSource = dataTable.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при генерації звіту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть тип звіту.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Обробник натискання кнопки "Шукати" (пошук дітей за прізвищем)
        private void SearchChildren_Click(object sender, RoutedEventArgs e)
        {
            if (!IsDbConnected()) return;

            string searchLastName = txtSearchLastName.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchLastName))
            {
                MessageBox.Show("Будь ласка, введіть прізвище для пошуку.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataTable dataTable = new DataTable();
            // Використовуємо LIKE для пошуку за частиною прізвища, додаючи '*' для OleDb
            string query = "SELECT ChildID, FirstName, LastName, DateOfBirth, GroupID FROM Children WHERE LastName LIKE @LastName;";

            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                // Для OleDb з Access, wildcard символ для LIKE це '*', а не '%'.
                command.Parameters.AddWithValue("@LastName", "*" + searchLastName + "*");

                try
                {
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                        dataGridSearchResults.ItemsSource = dataTable.DefaultView;
                    }

                    if (dataTable.Rows.Count == 0)
                    {
                        MessageBox.Show($"Дітей з прізвищем '{searchLastName}' не знайдено.", "Пошук", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при пошуку: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Закриття підключення до БД при закритті вікна
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
                UpdateConnectionStatus(false, "Підключення до БД закрито.");
            }
        }
    }
}
