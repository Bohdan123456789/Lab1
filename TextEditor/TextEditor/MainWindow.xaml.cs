using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Globalization;
using System.Windows.Media.Imaging;


// MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq; // Цей using потрібен для розширень Linq, таких як OrderBy
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents; // Для RichTextBox, TextRange, FlowDocument, Paragraph, Inline
using System.Windows.Input; // Для EditingCommands
using System.Windows.Media; // Для FontFamily, BitmapImage
using System.Windows.Media.Imaging; // Для BitmapImage
using Microsoft.Win32; // Для OpenFileDialog, SaveFileDialog

namespace TextEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeFontControls();
            CreateNewDocumentTab(); // Створити перший документ при запуску
        }

        // Ініціалізація вибору шрифтів та розмірів
        private void InitializeFontControls()
        {
            // Заповнення ComboBox шрифтами
            foreach (FontFamily fontFamily in Fonts.SystemFontFamilies.OrderBy(f => f.Source))
            {
                cmbFontFamily.Items.Add(fontFamily);
            }
            // Встановити Inter як шрифт за замовчуванням. Якщо його немає, вибрати перший доступний.
            if (cmbFontFamily.Items.OfType<FontFamily>().Any(f => f.Source == "Inter"))
            {
                cmbFontFamily.SelectedItem = new FontFamily("Inter");
            }
            else if (cmbFontFamily.Items.Count > 0)
            {
                cmbFontFamily.SelectedIndex = 0;
            }


            // Заповнення ComboBox розмірами шрифтів
            for (int i = 8; i <= 72; i += 2)
            {
                cmbFontSize.Items.Add((double)i);
            }
            cmbFontSize.SelectedItem = 12.0; // Розмір шрифту за замовчуванням
        }

        // Створення нової вкладки для документа
        private void CreateNewDocumentTab()
        {
            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Margin = new Thickness(5);
            richTextBox.AcceptsReturn = true; // Дозволити перехід на новий рядок
            richTextBox.AcceptsTab = true; // Дозволити використання Tab
            richTextBox.SpellCheck.IsEnabled = true; // Увімкнути перевірку орфографії
            richTextBox.SelectionChanged += RichTextBox_SelectionChanged; // Додати обробник для оновлення панелі інструментів
            richTextBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            richTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            // Встановлення шрифту за замовчуванням для нового RichTextBox
            // Це застосовується до всього RichTextBox, коли він порожній
            if (cmbFontFamily.SelectedItem is FontFamily defaultFontFamily)
            {
                richTextBox.FontFamily = defaultFontFamily;
            }
            if (cmbFontSize.SelectedItem is double defaultFontSize)
            {
                richTextBox.FontSize = defaultFontSize;
            }


            TabItem newTab = new TabItem();
            newTab.Header = "Новий Документ";
            newTab.Content = richTextBox;
            tabControlDocuments.Items.Add(newTab);
            tabControlDocuments.SelectedItem = newTab; // Зробити нову вкладку активною
        }

        // Отримання поточного RichTextBox
        private RichTextBox GetCurrentRichTextBox()
        {
            TabItem currentTab = tabControlDocuments.SelectedItem as TabItem;
            if (currentTab != null)
            {
                return currentTab.Content as RichTextBox;
            }
            return null;
        }

        // Обробник зміни вибору в вкладках

        
private void TabControlDocuments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RichTextBox currentRichTextBox = GetCurrentRichTextBox();
            if (currentRichTextBox != null)
            {
                // Оновити стан панелі інструментів при зміні активної вкладки
                UpdateFormattingToolbar(currentRichTextBox);
            }
        }

        // Обробники подій для файлового меню
        private void New_Click(object sender, RoutedEventArgs e)
        {
            CreateNewDocumentTab();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "RTF файли (*.rtf)|*.rtf|Текстові файли (*.txt)|*.txt|Всі файли (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                CreateNewDocumentTab(); // Створити нову вкладку для відкритого файлу
                TabItem currentTab = tabControlDocuments.SelectedItem as TabItem;
                if (currentTab != null)
                {
                    RichTextBox richTextBox = currentTab.Content as RichTextBox;
                    if (richTextBox != null)
                    {
                        TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                        using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                        {
                            string fileExtension = Path.GetExtension(openFileDialog.FileName).ToLower();
                            if (fileExtension == ".rtf")
                            {
                                textRange.Load(fs, DataFormats.Rtf);
                            }
                            else // За замовчуванням відкриваємо як простий текст
                            {
                                textRange.Load(fs, DataFormats.Text);
                            }
                        }
                        currentTab.Header = Path.GetFileName(openFileDialog.FileName);
                    }
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // У повноцінному редакторі тут мала б бути логіка
            // перевірки, чи файл вже збережений (тобто, чи має шлях),
            // і якщо так, то збереження за існуючим шляхом без діалогу.
            // Наразі просто викликаємо SaveAs_Click.
            SaveAs_Click(sender, e);
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "RTF файли (*.rtf)|*.rtf|Текстові файли (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                TabItem currentTab = tabControlDocuments.SelectedItem as TabItem;
                if (currentTab != null)
                {
                    RichTextBox richTextBox = currentTab.Content as RichTextBox;
                    if (richTextBox != null)
                    {
                        TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
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
                        currentTab.Header = Path.GetFileName(saveFileDialog.FileName); // Оновити заголовок вкладки
                    }
                }
            }
        }

        
private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Обробники подій для форматування тексту
        private void CmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontFamily.SelectedItem is FontFamily selectedFontFamily)
            {
                ApplyCharacterFormatting(selectedFontFamily, null, null, null, null);
            }
        }

        private void CmbFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontSize.SelectedItem is double selectedFontSize)
            {
                ApplyCharacterFormatting(null, selectedFontSize, null, null, null);
            }
        }

        private void ToggleBold_Click(object sender, RoutedEventArgs e)
        {
            RichTextBox currentRichTextBox = GetCurrentRichTextBox();
            if (currentRichTextBox != null)
            {
                EditingCommands.ToggleBold.Execute(null, currentRichTextBox);
            }
        }

        private void ToggleItalic_Click(object sender, RoutedEventArgs e)
        {
            RichTextBox currentRichTextBox = GetCurrentRichTextBox();
            if (currentRichTextBox != null)
            {
                EditingCommands.ToggleItalic.Execute(null, currentRichTextBox);
            }
        }

        private void ToggleUnderline_Click(object sender, RoutedEventArgs e)
        {
            RichTextBox currentRichTextBox = GetCurrentRichTextBox();
            if (currentRichTextBox != null)
            {
                EditingCommands.ToggleUnderline.Execute(null, currentRichTextBox);
            }
        }

        private void AlignLeft_Click(object sender, RoutedEventArgs e)
        {
            RichTextBox currentRichTextBox = GetCurrentRichTextBox();
            if (currentRichTextBox != null)
            {
                ApplyParagraphAlignment(TextAlignment.Left, currentRichTextBox);
            }
        }

        private void AlignCenter_Click(object sender, RoutedEventArgs e)
        {
            RichTextBox currentRichTextBox = GetCurrentRichTextBox();
            if (currentRichTextBox != null)
            {
                ApplyParagraphAlignment(TextAlignment.Center, currentRichTextBox);
            }
        }

        private void AlignRight_Click(object sender, RoutedEventArgs e)
        {
            RichTextBox currentRichTextBox = GetCurrentRichTextBox();
            if (currentRichTextBox != null)
            {
                ApplyParagraphAlignment(TextAlignment.Right, currentRichTextBox);
            }
        }

        private void AlignJustify_Click(object sender, RoutedEventArgs e)
        {
            RichTextBox currentRichTextBox = GetCurrentRichTextBox();
            if (currentRichTextBox != null)
            {
                ApplyParagraphAlignment(TextAlignment.Justify, currentRichTextBox);
            }
        }

        // Новий допоміжний метод для застосування вирівнювання до абзаців
        private void ApplyParagraphAlignment(TextAlignment alignment, RichTextBox richTextBox)
        {
            if (richTextBox.Selection.IsEmpty)
            {
                // Якщо нічого не виділено, застосувати до поточного абзацу
                Paragraph currentParagraph = richTextBox.CaretPosition.Paragraph;
                if (currentParagraph != null)
                {
                    currentParagraph.TextAlignment = alignment;
                }
            }
            else
            {
                // Застосувати до всіх абзаців у виділенні
                // Використовуємо Blocks замість SiblingBlocksAfterSelf для надійності
                // та перевіряємо, чи блок є абзацом і чи він входить у виділення.
                TextPointer start = richTextBox.Selection.Start;
                TextPointer end = richTextBox.Selection.End;

                
foreach (Block block in richTextBox.Document.Blocks)
                {
                    if (block is Paragraph paragraph)
                    {
                        // Перевіряємо, чи поточний абзац перетинається з виділенням
                        if (paragraph.ContentStart.CompareTo(end) < 0 && paragraph.ContentEnd.CompareTo(start) > 0)
                        {
                            paragraph.TextAlignment = alignment;
                        }
                    }
                }
            }
        }


        // Метод для застосування форматування на рівні символів до виділеного тексту або до поточного RichTextBox
        private void ApplyCharacterFormatting(FontFamily fontFamily, double? fontSize, FontWeight? fontWeight, FontStyle? fontStyle, TextDecorationCollection textDecorations)
        {
            RichTextBox currentRichTextBox = GetCurrentRichTextBox();
            if (currentRichTextBox == null) return;

            TextRange selection = currentRichTextBox.Selection;

            if (selection.IsEmpty)
            {
                // Якщо нічого не виділено, застосувати до поточного положення курсору
                // Це змінить форматування для нового тексту, який буде введений
                currentRichTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily ?? currentRichTextBox.FontFamily);
                currentRichTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize ?? currentRichTextBox.FontSize);
                currentRichTextBox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, fontWeight ?? currentRichTextBox.FontWeight);
                currentRichTextBox.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, fontStyle ?? currentRichTextBox.FontStyle);
                currentRichTextBox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, textDecorations ?? Inline.TextDecorationsProperty.DefaultMetadata.DefaultValue);
            }
            else
            {
                // Застосувати до виділеного тексту
                if (fontFamily != null) selection.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
                if (fontSize.HasValue) selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize.Value);
                if (fontWeight.HasValue) selection.ApplyPropertyValue(TextElement.FontWeightProperty, fontWeight.Value);
                if (fontStyle.HasValue) selection.ApplyPropertyValue(TextElement.FontStyleProperty, fontStyle.Value);
                if (textDecorations != null) selection.ApplyPropertyValue(Inline.TextDecorationsProperty, textDecorations);
            }
        }

        // Оновлення стану панелі інструментів на основі виділення/позиції курсора
        private void RichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            RichTextBox richTextBox = sender as RichTextBox;
            if (richTextBox == null) return;

            // Оновлення шрифту та розміру
            object fontFamilyValue = richTextBox.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            if (fontFamilyValue != DependencyProperty.UnsetValue && fontFamilyValue is FontFamily currentFontFamily)
            {
                cmbFontFamily.SelectedItem = currentFontFamily;
            }
            else
            {
                // Якщо виділення не містить єдиного шрифту, або порожнє
                // Можемо встановити за замовчуванням або залишити як є.
                // Залишаємо як є, щоб не змінювати вибір ComboBox без потреби.
            }

            object fontSizeValue = richTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            if (fontSizeValue != DependencyProperty.UnsetValue && fontSizeValue is double currentFontSize)
            {
                cmbFontSize.SelectedItem = currentFontSize;
            }

            
// Оновлення кнопок Жирний/Курсив/Підкреслений
            btnBold.IsChecked = (FontWeight)richTextBox.Selection.GetPropertyValue(TextElement.FontWeightProperty) == FontWeights.Bold;
            btnItalic.IsChecked = (FontStyle)richTextBox.Selection.GetPropertyValue(TextElement.FontStyleProperty) == FontStyles.Italic;
            // Перевірка TextDecorations може бути трохи складнішою, оскільки може бути кілька декорацій
            btnUnderline.IsChecked = (TextDecorationCollection)richTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty) == TextDecorations.Underline;

            // Оновлення кнопок вирівнювання
            // Вирівнювання застосовується до Paragraph, а не до TextRange.
            // Тому отримуємо поточний параграф за позицією курсору.
            Paragraph currentParagraph = richTextBox.CaretPosition.Paragraph;
            if (currentParagraph != null)
            {
                TextAlignment currentAlignment = (TextAlignment)currentParagraph.GetValue(Paragraph.TextAlignmentProperty);
                rbAlignLeft.IsChecked = currentAlignment == TextAlignment.Left;
                rbAlignCenter.IsChecked = currentAlignment == TextAlignment.Center;
                rbAlignRight.IsChecked = currentAlignment == TextAlignment.Right;
                rbAlignJustify.IsChecked = currentAlignment == TextAlignment.Justify;
            }
            else
            {
                // Якщо параграф ще не існує (наприклад, пустий RichTextBox),
                // скидаємо вибір кнопок вирівнювання.
                rbAlignLeft.IsChecked = false;
                rbAlignCenter.IsChecked = false;
                rbAlignRight.IsChecked = false;
                rbAlignJustify.IsChecked = false;
            }
        }


        // Допоміжний метод для оновлення панелі інструментів
        // Цей метод викликається з TabControlDocuments_SelectionChanged
        // Він дублює логіку RichTextBox_SelectionChanged, але це необхідно
        // для коректного оновлення UI при перемиканні вкладок.
        private void UpdateFormattingToolbar(RichTextBox richTextBox)
        {
            // Оновлення шрифту та розміру
            object fontFamilyValue = richTextBox.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            if (fontFamilyValue != DependencyProperty.UnsetValue && fontFamilyValue is FontFamily currentFontFamily)
            {
                cmbFontFamily.SelectedItem = currentFontFamily;
            }

            object fontSizeValue = richTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            if (fontSizeValue != DependencyProperty.UnsetValue && fontSizeValue is double currentFontSize)
            {
                cmbFontSize.SelectedItem = currentFontSize;
            }

            // Оновлення кнопок Жирний/Курсив/Підкреслений
            btnBold.IsChecked = (FontWeight)richTextBox.Selection.GetPropertyValue(TextElement.FontWeightProperty) == FontWeights.Bold;
            btnItalic.IsChecked = (FontStyle)richTextBox.Selection.GetPropertyValue(TextElement.FontStyleProperty) == FontStyles.Italic;
            btnUnderline.IsChecked = (TextDecorationCollection)richTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty) == TextDecorations.Underline;

            // Оновлення кнопок вирівнювання
            Paragraph currentParagraph = richTextBox.CaretPosition.Paragraph;
            if (currentParagraph != null)
            {
                TextAlignment currentAlignment = (TextAlignment)currentParagraph.GetValue(Paragraph.TextAlignmentProperty);
                rbAlignLeft.IsChecked = currentAlignment == TextAlignment.Left;
                rbAlignCenter.IsChecked = currentAlignment == TextAlignment.Center;
                rbAlignRight.IsChecked = currentAlignment == TextAlignment.Right;

                
rbAlignJustify.IsChecked = currentAlignment == TextAlignment.Justify;
            }
            else
            {
                rbAlignLeft.IsChecked = false;
                rbAlignCenter.IsChecked = false;
                rbAlignRight.IsChecked = false;
                rbAlignJustify.IsChecked = false;
            }
        }


       
        // Обробник для вставки зображення
        private void InsertImage_Click(object sender, RoutedEventArgs e)
        {
            RichTextBox currentRichTextBox = GetCurrentRichTextBox();
            if (currentRichTextBox == null) return;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файли зображень (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|Всі файли (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.EndInit();

                    Image image = new Image();
                    image.Source = bitmap;
                    image.Stretch = Stretch.Uniform; // Зберігати співвідношення сторін
                    image.MaxWidth = currentRichTextBox.ActualWidth - 20; // Обмежити розмір зображення

                    // Створення InlineUIContainer для вставки зображення в RichTextBox
                    InlineUIContainer container = new InlineUIContainer(image);

                    // Вставити контейнер у поточну позицію курсору
                    TextPointer insertPosition = currentRichTextBox.CaretPosition;

                    Paragraph currentParagraph = insertPosition.Paragraph;
                    if (currentParagraph != null)
                    {
                        currentParagraph.Inlines.Add(container);
                        currentRichTextBox.CaretPosition = currentParagraph.ContentEnd; // Перемістити курсор після зображення
                    }
                    else
                    {
                        // Створити новий FlowDocument, якщо він порожній
                        if (currentRichTextBox.Document == null)
                        {
                            currentRichTextBox.Document = new FlowDocument();
                        }
                        // Створити новий параграф для зображення
                        Paragraph newParagraph = new Paragraph();
                        newParagraph.Inlines.Add(container);
                        currentRichTextBox.Document.Blocks.Add(newParagraph);
                        currentRichTextBox.CaretPosition = newParagraph.ContentEnd;
                    }

                    // Опціонально: додати перехід на новий рядок після зображення, якщо потрібно
                    currentRichTextBox.CaretPosition.InsertParagraphBreak();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при вставці зображення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обробник для зміни мови інтерфейсу (placeholder)
        private void ChangeLanguage_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null && menuItem.Tag is string languageCode)
            {
                // Це спрощений приклад. Для повної локалізації потрібно:
                // 1. Створити файли ресурсів (.resx) для кожної мови.
                // 2. Завантажувати відповідні ресурси та оновлювати всі елементи UI.
                // 3. Змінити CultureInfo поточного потоку.
                try
                {
                    CultureInfo culture = new CultureInfo(languageCode);
                    // Насправді тут потрібно було б перезавантажити UI або динамічно оновити всі текст
                    // Thread.CurrentThread.CurrentCulture = culture;
                    // Thread.CurrentThread.CurrentUICulture = culture;

                    MessageBox.Show($"Мова інтерфейсу змінена на: {culture.DisplayName}", "Зміна мови", MessageBoxButton.OK, MessageBoxImage.Information);

                   
// У реальному додатку ви б тут перевантажили вікно або оновили всі текстові елементи
                    // new MainWindow().Show();
                    // this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при зміні мови: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}