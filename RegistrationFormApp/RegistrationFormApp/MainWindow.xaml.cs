// MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents; // Для RichTextBox, TextRange, FlowDocument
using Microsoft.Win32; // Для OpenFileDialog, SaveFileDialog
using System.Text.RegularExpressions; // Для пошуку та заміни
using System.Windows.Media; // Для Brushes

namespace RegistrationFormApp // Важливо: Це ім'я проекту, яке ви повинні використовувати!
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Словник для зберігання шаблонів. Ключ - назва шаблону, значення - вміст шаблону.
        // Шаблони містять плейсхолдери у форматі {{FIELD_NAME}}
        private Dictionary<string, string> templates = new Dictionary<string, string>();

        public MainWindow()
        {
            InitializeComponent(); // Цей метод ініціалізує компоненти UI, визначені в XAML
            InitializeTemplates();
        }

        // Ініціалізація та завантаження шаблонів
        private void InitializeTemplates()
        {
            // Шаблон 1: Реєстрація учасника
            templates.Add("Шаблон 1: Реєстрація учасника",
                "Шановний(а) {{ПІБ}},\n\n" +
                "Дякуємо за Вашу реєстрацію на подію!\n\n" +
                "Ми підтверджуємо Вашу участь.\n\n" +
                "Дата реєстрації: {{Дата}}\n" +
                "Контактний Email: {{Email}}\n" +
                "Телефон: {{Телефон}}\n\n" +
                "З повагою,\nКоманда організаторів.");

            // Шаблон 2: Замовлення послуги
            templates.Add("Шаблон 2: Замовлення послуги",
                "Бланк замовлення послуги\n\n" +
                "Дата: {{Дата}}\n" +
                "Компанія: {{Назва компанії}}\n" +
                "ПІБ контактної особи: {{ПІБ}}\n" +
                "Email: {{Email}}\n" +
                "Телефон: {{Телефон}}\n\n" +
                "Опис запиту:\n{{Опис запиту}}\n\n" +
                "Дякуємо за Ваше звернення!");

            // Шаблон 3: Зворотній зв'язок
            templates.Add("Шаблон 3: Зворотній зв'язок",
                "Форма зворотнього зв'язку\n\n" +
                "Дата надсилання: {{Дата}}\n" +
                "Від: {{ПІБ}}\n" +
                "Email: {{Email}}\n" +
                "Телефон: {{Телефон}}\n" +
                "Компанія (якщо є): {{Назва компанії}}\n\n" +
                "Текст повідомлення:\n{{Опис запиту}}\n\n" +
                "Ми цінуємо Ваш відгук!");

            // Заповнити ComboBox доступними шаблонами
            foreach (var templateName in templates.Keys)
            {
                cmbTemplates.Items.Add(new ComboBoxItem { Content = templateName });
            }

            // Вибрати перший шаблон за замовчуванням
            if (cmbTemplates.Items.Count > 0)
            {
                cmbTemplates.SelectedIndex = 0;
            }
        }

        // Обробник зміни вибраного шаблону в ComboBox
        private void CmbTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTemplates.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedTemplateName = selectedItem.Content.ToString();
                if (templates.TryGetValue(selectedTemplateName, out string templateContent))
                {
                    rtbDocument.Document.Blocks.Clear(); // Очистити існуючий вміст

                    // Встановлюємо RichTextBox в режим "лише для читання" для попереднього перегляду шаблону
                    rtbDocument.IsReadOnly = true;

                    Paragraph paragraph = new Paragraph();
                    // Регулярний вираз для знаходження плейсхолдерів типу {{FIELD_NAME}}
                    // Змінено: захоплюємо текст всередині дужок
                    Regex regex = new Regex(@"\{\{(.*?)\}\}");
                    int lastIndex = 0;

                    foreach (Match match in regex.Matches(templateContent))
                    {
                        // Додати звичайний текст, що передує плейсхолдеру
                        if (match.Index > lastIndex)
                        {
                            paragraph.Inlines.Add(new Run(templateContent.Substring(lastIndex, match.Index - lastIndex)));
                        }

                        // Додати плейсхолдер зі стилем "примарного" тексту, використовуючи захоплений текст (без дужок)
                        Run placeholderRun = new Run(match.Groups[1].Value); // Використовуємо захоплену групу 1
                        placeholderRun.Foreground = Brushes.LightGray; // Світло-сірий колір
                        placeholderRun.FontStyle = FontStyles.Italic;  // Курсив
                        paragraph.Inlines.Add(placeholderRun);

                        lastIndex = match.Index + match.Length;
                    }

                    // Додати будь-який текст, що залишився після останнього плейсхолдера
                    if (lastIndex < templateContent.Length)
                    {
                        paragraph.Inlines.Add(new Run(templateContent.Substring(lastIndex)));
                    }

                    rtbDocument.Document.Blocks.Add(paragraph);
                }
            }
        }

        // Обробник натискання кнопки "Створити Документ"
        private void CreateDocument_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTemplates.SelectedItem is ComboBoxItem selectedItem && templates.TryGetValue(selectedItem.Content.ToString(), out string templateContent))
            {
                // Для заповнення беремо оригінальний шаблон, а не поточний текст з RichTextBox
                // Це гарантує, що ми замінюємо саме {{...}} плейсхолдери
                string filledContent = templateContent;

                // Заміна плейсхолдерів на дані з полів введення
                filledContent = filledContent.Replace("{{ПІБ}}", txtFullName.Text);
                filledContent = filledContent.Replace("{{Дата}}", dpDate.SelectedDate?.ToString("yyyy-MM-dd") ?? "не вказано");
                filledContent = filledContent.Replace("{{Email}}", txtEmail.Text);
                filledContent = filledContent.Replace("{{Телефон}}", txtPhone.Text);
                filledContent = filledContent.Replace("{{Назва компанії}}", txtCompany.Text);
                filledContent = filledContent.Replace("{{Опис запиту}}", txtDescription.Text);

                // Очистити RichTextBox та вставити заповнений шаблон
                rtbDocument.Document.Blocks.Clear();
                rtbDocument.AppendText(filledContent);

                // Зробити RichTextBox редагованим після створення документа
                rtbDocument.IsReadOnly = false;

                MessageBox.Show("Документ успішно створено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть шаблон.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Обробник натискання кнопки "Зберегти Документ"
        private void SaveDocument_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "RTF файли (*.rtf)|*.rtf|Текстові файли (*.txt)|*.txt|Всі файли (*.*)|*.*";
            saveFileDialog.FileName = "Бланк_Реєстрації_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    TextRange textRange = new TextRange(rtbDocument.Document.ContentStart, rtbDocument.Document.ContentEnd);
                    using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        if (saveFileDialog.FilterIndex == 1) // RTF
                        {
                            textRange.Save(fs, DataFormats.Rtf);
                        }
                        else // Text
                        {
                            textRange.Save(fs, DataFormats.Text);
                        }
                    }
                    MessageBox.Show("Документ успішно збережено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при збереженні документа: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обробник натискання кнопки "Знайти"
        private void Find_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearch.Text;
            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Будь ласка, введіть текст для пошуку.", "Пошук", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Очистити попереднє виділення
            rtbDocument.Selection.Text = string.Empty;

            TextPointer current = rtbDocument.Document.ContentStart;
            // Перетворення FlowDocument на звичайний текст для пошуку
            TextRange docTextRange = new TextRange(rtbDocument.Document.ContentStart, rtbDocument.Document.ContentEnd);
            string documentText = docTextRange.Text;

            int startIndex = documentText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
            if (startIndex >= 0)
            {
                // Знайти позицію початку та кінця знайденого тексту у RichTextBox
                TextPointer start = docTextRange.Start.GetPositionAtOffset(startIndex);
                TextPointer end = docTextRange.Start.GetPositionAtOffset(startIndex + searchText.Length);

                rtbDocument.Selection.Select(start, end);
                rtbDocument.Focus(); // Встановити фокус на RichTextBox
            }
            else
            {
                MessageBox.Show("Текст не знайдено.", "Пошук", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Обробник натискання кнопки "Замінити"
        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearch.Text;
            // У цьому простому прикладі "Замінити" буде замінювати лише виділений текст.
            // Для повного функціоналу "Замінити все" або "Замінити наступний"
            // потрібна додаткова логіка.

            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Будь ласка, введіть текст для пошуку та заміни.", "Заміна", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Припустимо, що користувач виділив текст, який хоче замінити
            // Або ми могли б додати додаткове поле для тексту заміни
            string replaceWithText = "ЗАМІНЕНО"; // Це можна зробити через окремий TextBox або InputBox

            if (!rtbDocument.Selection.IsEmpty && rtbDocument.Selection.Text.Equals(searchText, StringComparison.OrdinalIgnoreCase))
            {
                // У WPF RichTextBox, просто встановіть Text виділення
                rtbDocument.Selection.Text = replaceWithText;
                MessageBox.Show("Текст успішно замінено.", "Заміна", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Якщо нічого не виділено або виділений текст не співпадає з пошуковим
                MessageBox.Show("Будь ласка, виділіть текст для заміни або переконайтесь, що виділений текст відповідає пошуковому.", "Заміна", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
