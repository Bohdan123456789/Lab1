// Form1.cs (Main logic)
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing; // Для Font, Color, FontStyle
using System.Linq;
using System.Management; // Для WMI
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms; // Для базових контролів Windows Forms

namespace HardwareInfoWinFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = "Інформація про обладнання ПК"; // Встановлення заголовка форми
            this.Size = new Size(900, 700); // Встановлення початкового розміру форми
            statusLabel.Text = "Натисніть 'Оновити інформацію', щоб отримати дані...";
        }

        // Допоміжний метод для отримання інформації за допомогою WMI
        private async Task<List<Dictionary<string, string>>> GetWmiInfo(string className, params string[] properties)
        {
            List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();
            await Task.Run(() => // Виконуємо запит WMI в окремому потоці
            {
                try
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT * FROM {className}");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        Dictionary<string, string> itemProperties = new Dictionary<string, string>();
                        foreach (string propName in properties)
                        {
                            try
                            {
                                if (obj[propName] != null)
                                {
                                    itemProperties[propName] = obj[propName].ToString().Trim();
                                }
                                else
                                {
                                    itemProperties[propName] = "N/A"; // Not Available
                                }
                            }
                            catch (Exception)
                            {
                                itemProperties[propName] = "N/A"; // Помилка доступу до конкретної властивості
                            }
                        }
                        results.Add(itemProperties);
                    }
                }
                catch (ManagementException ex)
                {
                    // Оновлення UI з фонового потоку (вимагає Invoke)
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show($"Помилка WMI для класу '{className}': {ex.Message}\nМожливо, потрібні права адміністратора.", "Помилка WMI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
                catch (Exception ex)
                {
                    // Оновлення UI з фонового потоку (вимагає Invoke)
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show($"Загальна помилка отримання інформації для класу '{className}': {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
            });
            return results;
        }

        // Допоміжний метод для відображення інформації в Panel (або TabPage)
        private void DisplayInfoInPanel(Control panel, string title, List<Dictionary<string, string>> data)
        {
            panel.Controls.Clear(); // Очищаємо попередній вміст

            Label titleLabel = new Label
            {
                Text = title,
                // Виправлення: Використання FontFamily замість String для Font конструктора
                // Для FontStyle.SemiBold використовуємо FontStyle.Bold
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10) // Розташування заголовка
            };
            panel.Controls.Add(titleLabel);

            if (data.Count == 0)
            {
                Label noInfoLabel = new Label
                {
                    Text = "Інформація недоступна або не знайдена.",
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    AutoSize = true,
                    Location = new Point(10, titleLabel.Bottom + 5)
                };
                panel.Controls.Add(noInfoLabel);
                return;
            }

            int yOffset = titleLabel.Bottom + 10; // Початковий відступ для першого елемента

            for (int i = 0; i < data.Count; i++)
            {
                if (data.Count > 1)
                {
                    Label itemHeaderLabel = new Label
                    {
                        Text = $"----- Елемент {i + 1} -----",
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        AutoSize = true,
                        Location = new Point(10, yOffset),
                    };
                    panel.Controls.Add(itemHeaderLabel);
                    yOffset = itemHeaderLabel.Bottom + 5;
                }

                foreach (var prop in data[i])
                {
                    Label propNameLabel = new Label
                    {
                        Text = $"{prop.Key}:",
                        // Виправлення: Для FontStyle.SemiBold використовуємо FontStyle.Bold
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        AutoSize = true,
                        Location = new Point(10, yOffset),
                    };
                    panel.Controls.Add(propNameLabel);
                    yOffset = propNameLabel.Bottom + 2;

                    Label propValueLabel = new Label
                    {
                        Text = prop.Value,
                        Font = new Font("Segoe UI", 9, FontStyle.Regular),
                        AutoSize = true,
                        Location = new Point(20, yOffset), // Невеликий відступ для значення
                        MaximumSize = new Size(panel.Width - 40, 0), // Дозволяє перенос рядків
                    };
                    panel.Controls.Add(propValueLabel);
                    yOffset = propValueLabel.Bottom + 5;
                }
            }
        }

        // Обробник натискання кнопки "Оновити інформацію"
        private async void refreshButton_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "Отримання інформації про обладнання. Будь ласка, зачекайте...";
            statusLabel.ForeColor = Color.Blue;

            // Процесор
            var processors = await GetWmiInfo("Win32_Processor", "Name", "Manufacturer", "Description", "NumberOfCores", "NumberOfLogicalProcessors", "CurrentClockSpeed");
            DisplayInfoInPanel(processorTabPage, "Інформація про процесор:", processors);

            // Відеокарта
            var videoCards = await GetWmiInfo("Win32_VideoController", "Name", "VideoProcessor", "DriverVersion", "AdapterRAM", "CurrentHorizontalResolution", "CurrentVerticalResolution");
            DisplayInfoInPanel(videoCardTabPage, "Інформація про відеокарту:", videoCards);

            // Оптичний привід
            var dvdDrives = await GetWmiInfo("Win32_CDROMDrive", "Name", "Drive", "MediaType");
            DisplayInfoInPanel(dvdDriveTabPage, "Інформація про оптичні приводи:", dvdDrives);

            // Жорсткий диск
            var diskDrives = await GetWmiInfo("Win32_DiskDrive", "Caption", "Size", "MediaType", "SerialNumber");
            DisplayInfoInPanel(diskDriveTabPage, "Інформація про жорсткі диски:", diskDrives);

            // Материнська плата
            var motherboards = await GetWmiInfo("Win32_BaseBoard", "Product", "Manufacturer", "SerialNumber", "Version");
            DisplayInfoInPanel(motherboardTabPage, "Інформація про материнську плату:", motherboards);

            // Мережеве обладнання
            var networkAdapters = await GetWmiInfo("Win32_NetworkAdapter", "Name", "MACAddress", "AdapterType", "Speed", "Manufacturer", "NetConnectionStatus");
            DisplayInfoInPanel(networkAdapterTabPage, "Інформація про мережеве обладнання:", networkAdapters);

            // BIOS
            var biosInfo = await GetWmiInfo("Win32_BIOS", "Name", "Manufacturer", "Version", "SerialNumber", "SMBIOSBIOSVersion", "ReleaseDate");
            DisplayInfoInPanel(biosTabPage, "Інформація про BIOS:", biosInfo);

            // RAM (Фізична пам'ять)
            var physicalMemory = await GetWmiInfo("Win32_PhysicalMemory", "Capacity", "Manufacturer", "Speed", "PartNumber");
            DisplayInfoInPanel(ramTabPage, "Інформація про оперативну пам'ять (RAM):", physicalMemory);

            // Операційна система
            var osInfo = await GetWmiInfo("Win32_OperatingSystem", "Caption", "OSArchitecture", "Version", "BuildNumber", "RegisteredUser", "SerialNumber");
            DisplayInfoInPanel(osTabPage, "Інформація про операційну систему:", osInfo);

            statusLabel.Text = "Оновлення інформації завершено!";
            statusLabel.ForeColor = Color.DarkGreen;
        }
    }
}
