// MainWindow.xaml.cs
using System;
using System.Windows;
using System.Windows.Media; // Для Brush, якщо хочете змінювати колір фону/тексту програмно

namespace ExpressionCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            txtErrorMessage.Visibility = Visibility.Collapsed; // Приховати попередні помилки
            txtXResult.Text = string.Empty; // Очистити попередні результати
            txtYResult.Text = string.Empty;

            if (double.TryParse(txtTValue.Text, out double t))
            {
                // Обчислення виразів
                // x = t^2 * sin(t)
                double x = Math.Pow(t, 2) * Math.Sin(t);

                // y = t * cos^2(t)  (cos^2(t) = (cos(t))^2)
                double y = t * Math.Pow(Math.Cos(t), 2);

                // Відображення результатів
                txtXResult.Text = x.ToString("F4"); // "F4" форматує число до 4 знаків після коми
                txtYResult.Text = y.ToString("F4");
            }
            else
            {
                // Відображення повідомлення про помилку, якщо введення не є числом
                txtErrorMessage.Text = "Будь ласка, введіть дійсне числове значення для 't'.";
                txtErrorMessage.Visibility = Visibility.Visible;
            }
        }
    }
}
