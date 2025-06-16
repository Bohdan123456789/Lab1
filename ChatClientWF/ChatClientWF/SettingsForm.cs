// SettingsForm.cs (для ChatClientWF)
using System;
using System.Linq;
using System.Drawing; // Для Font
using System.Windows.Forms;

namespace ChatClientWF
{
    public partial class SettingsForm : Form
    {
        private ChatSettings _currentSettings; // Поточні налаштування

        public SettingsForm(ChatSettings settings)
        {
            InitializeComponent();
            _currentSettings = settings;
            LoadSettingsToUI();
            InitializeFontControls();
        }

        private void InitializeFontControls()
        {
            // Заповнення ComboBox шрифтами
            foreach (FontFamily fontFamily in FontFamily.Families.OrderBy(f => f.Name))
            {
                cmbChatFontFamily.Items.Add(fontFamily.Name);
            }
            cmbChatFontFamily.Text = _currentSettings.ChatFontFamily; // Встановлюємо вибраний шрифт

            // Заповнення ComboBox розмірами шрифтів
            for (int i = 8; i <= 72; i += 2)
            {
                cmbChatFontSize.Items.Add(i);
            }
            cmbChatFontSize.Text = _currentSettings.ChatFontSize.ToString(); // Встановлюємо вибраний розмір
        }

        private void LoadSettingsToUI()
        {
            txtIpAddress.Text = _currentSettings.IpAddress;
            txtPort.Text = _currentSettings.Port.ToString();
            chkEnableChatLogging.Checked = _currentSettings.EnableChatLogging;
            txtChatLogFilePath.Text = _currentSettings.ChatLogFilePath;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                _currentSettings.IpAddress = txtIpAddress.Text;

                if (int.TryParse(txtPort.Text, out int port))
                {
                    _currentSettings.Port = port;
                }
                else
                {
                    MessageBox.Show("Будь ласка, введіть дійсний номер порту.", "Помилка введення", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _currentSettings.ChatFontFamily = cmbChatFontFamily.Text;
                if (float.TryParse(cmbChatFontSize.Text, out float fontSize))
                {
                    _currentSettings.ChatFontSize = fontSize;
                }
                else
                {
                    MessageBox.Show("Будь ласка, виберіть дійсний розмір шрифту.", "Помилка введення", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _currentSettings.EnableChatLogging = chkEnableChatLogging.Checked;
                _currentSettings.ChatLogFilePath = txtChatLogFilePath.Text;

                _currentSettings.Save(); // Зберігаємо налаштування у файл
                MessageBox.Show("Налаштування успішно збережено.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK; // Повернути OK, якщо збереження успішне
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження налаштувань: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel; // Повернути Cancel
            this.Close();
        }
    }
}
