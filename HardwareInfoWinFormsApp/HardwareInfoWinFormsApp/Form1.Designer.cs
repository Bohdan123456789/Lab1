// Form1.Designer.cs (Auto-generated code for UI layout)
namespace HardwareInfoWinFormsApp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.headerPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.refreshButton = new System.Windows.Forms.Button(); // Переміщено кнопку перед заголовком
            this.titleLabel = new System.Windows.Forms.Label();
            this.TabPage = new System.Windows.Forms.TabControl(); // Назва залишена TabPage, але бажано перейменувати
            this.processorTabPage = new System.Windows.Forms.TabPage();
            this.videoCardTabPage = new System.Windows.Forms.TabPage();
            this.dvdDriveTabPage = new System.Windows.Forms.TabPage();
            this.diskDriveTabPage = new System.Windows.Forms.TabPage();
            this.motherboardTabPage = new System.Windows.Forms.TabPage();
            this.networkAdapterTabPage = new System.Windows.Forms.TabPage();
            this.biosTabPage = new System.Windows.Forms.TabPage();
            this.ramTabPage = new System.Windows.Forms.TabPage();
            this.osTabPage = new System.Windows.Forms.TabPage();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tableLayoutPanel1.SuspendLayout();
            this.headerPanel.SuspendLayout();
            this.TabPage.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            // tableLayoutPanel1
            //
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.headerPanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.TabPage, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.statusStrip1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            // Коректне налаштування RowStyles:
            // 60px для верхньої панелі (кнопка + заголовок)
            // 100% для TabPage (вкладки з інформацією)
            // 25px для statusStrip
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F)); // Header Panel
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)); // TabControl
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F)); // Status Strip
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 450); // Розмір форми встановлюється в Form1.cs
            this.tableLayoutPanel1.TabIndex = 0;
            //
            // headerPanel
            //
            this.headerPanel.Controls.Add(this.refreshButton); // Кнопка перша
            this.headerPanel.Controls.Add(this.titleLabel); // Потім заголовок
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerPanel.Location = new System.Drawing.Point(3, 3);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Padding = new System.Windows.Forms.Padding(10);
            this.headerPanel.Size = new System.Drawing.Size(794, 54);
            this.headerPanel.TabIndex = 0;
            //
            // refreshButton
            //
            this.refreshButton.AutoSize = true;
            this.refreshButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.refreshButton.Location = new System.Drawing.Point(13, 13);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.refreshButton.Size = new System.Drawing.Size(210, 41);
            this.refreshButton.TabIndex = 1;
            this.refreshButton.Text = "Оновити інформацію";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click); // !!! ВАЖЛИВО: Прив'язка обробника події
            //
            // titleLabel
            //
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(229, 10);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(527, 30);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Детальна інформація про апаратне забезпечення";
            //
            // TabPage (TabControl)
            // Назва "TabPage" конфліктує з класом System.Windows.Forms.TabPage, але збережено як у вашому коді.
            // Краще було б TabControl, наприклад, "tabControlInfo".
            this.TabPage.AllowDrop = true;
            this.TabPage.Controls.Add(this.processorTabPage);
            this.TabPage.Controls.Add(this.videoCardTabPage);
            this.TabPage.Controls.Add(this.dvdDriveTabPage);
            this.TabPage.Controls.Add(this.diskDriveTabPage);
            this.TabPage.Controls.Add(this.motherboardTabPage);
            this.TabPage.Controls.Add(this.networkAdapterTabPage);
            this.TabPage.Controls.Add(this.biosTabPage);
            this.TabPage.Controls.Add(this.ramTabPage);
            this.TabPage.Controls.Add(this.osTabPage);
            this.TabPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabPage.Location = new System.Drawing.Point(3, 63);
            this.TabPage.Name = "TabPage"; // Збережено як "TabPage"
            this.TabPage.SelectedIndex = 0;
            this.TabPage.Size = new System.Drawing.Size(794, 94); // Цей розмір буде перевизначений RowStyles.Add(SizeType.Percent, 100F)
            this.TabPage.TabIndex = 1;
            //
            // processorTabPage
            //
            this.processorTabPage.AutoScroll = true;
            this.processorTabPage.Location = new System.Drawing.Point(4, 22);
            this.processorTabPage.Name = "processorTabPage";
            this.processorTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.processorTabPage.Size = new System.Drawing.Size(786, 68);
            this.processorTabPage.TabIndex = 0;
            this.processorTabPage.Text = "Процесор";
            this.processorTabPage.UseVisualStyleBackColor = true;
            //
            // videoCardTabPage
            //
            this.videoCardTabPage.AutoScroll = true;
            this.videoCardTabPage.Location = new System.Drawing.Point(4, 22);
            this.videoCardTabPage.Name = "videoCardTabPage";
            this.videoCardTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.videoCardTabPage.Size = new System.Drawing.Size(786, 68);
            this.videoCardTabPage.TabIndex = 1;
            this.videoCardTabPage.Text = "Відеокарта";
            this.videoCardTabPage.UseVisualStyleBackColor = true;
            //
            // dvdDriveTabPage
            //
            this.dvdDriveTabPage.AutoScroll = true;
            this.dvdDriveTabPage.Location = new System.Drawing.Point(4, 22);
            this.dvdDriveTabPage.Name = "dvdDriveTabPage";
            this.dvdDriveTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.dvdDriveTabPage.Size = new System.Drawing.Size(786, 68);
            this.dvdDriveTabPage.TabIndex = 2;
            this.dvdDriveTabPage.Text = "Оптичний привід";
            this.dvdDriveTabPage.UseVisualStyleBackColor = true;
            //
            // diskDriveTabPage
            //
            this.diskDriveTabPage.AutoScroll = true;
            this.diskDriveTabPage.Location = new System.Drawing.Point(4, 22);
            this.diskDriveTabPage.Name = "diskDriveTabPage";
            this.diskDriveTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.diskDriveTabPage.Size = new System.Drawing.Size(786, 68);
            this.diskDriveTabPage.TabIndex = 3;
            this.diskDriveTabPage.Text = "Жорсткий диск";
            this.diskDriveTabPage.UseVisualStyleBackColor = true;
            //
            // motherboardTabPage
            //
            this.motherboardTabPage.AutoScroll = true;
            this.motherboardTabPage.Location = new System.Drawing.Point(4, 22);
            this.motherboardTabPage.Name = "motherboardTabPage";
            this.motherboardTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.motherboardTabPage.Size = new System.Drawing.Size(786, 68);
            this.motherboardTabPage.TabIndex = 4;
            this.motherboardTabPage.Text = "Материнська плата";
            this.motherboardTabPage.UseVisualStyleBackColor = true;
            //
            // networkAdapterTabPage
            //
            this.networkAdapterTabPage.AutoScroll = true;
            this.networkAdapterTabPage.Location = new System.Drawing.Point(4, 22);
            this.networkAdapterTabPage.Name = "networkAdapterTabPage";
            this.networkAdapterTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.networkAdapterTabPage.Size = new System.Drawing.Size(786, 68);
            this.networkAdapterTabPage.TabIndex = 5;
            this.networkAdapterTabPage.Text = "Мережеве обладнання";
            this.networkAdapterTabPage.UseVisualStyleBackColor = true;
            //
            // biosTabPage
            //
            this.biosTabPage.AutoScroll = true;
            this.biosTabPage.Location = new System.Drawing.Point(4, 22);
            this.biosTabPage.Name = "biosTabPage";
            this.biosTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.biosTabPage.Size = new System.Drawing.Size(786, 68);
            this.biosTabPage.TabIndex = 6;
            this.biosTabPage.Text = "BIOS";
            this.biosTabPage.UseVisualStyleBackColor = true;
            //
            // ramTabPage
            //
            this.ramTabPage.AutoScroll = true;
            this.ramTabPage.Location = new System.Drawing.Point(4, 22);
            this.ramTabPage.Name = "ramTabPage";
            this.ramTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ramTabPage.Size = new System.Drawing.Size(786, 68);
            this.ramTabPage.TabIndex = 7;
            this.ramTabPage.Text = "RAM";
            this.ramTabPage.UseVisualStyleBackColor = true;
            //
            // osTabPage
            //
            this.osTabPage.AutoScroll = true;
            this.osTabPage.Location = new System.Drawing.Point(4, 22);
            this.osTabPage.Name = "osTabPage";
            this.osTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.osTabPage.Size = new System.Drawing.Size(786, 68);
            this.osTabPage.TabIndex = 8;
            this.osTabPage.Text = "Операційна система";
            this.osTabPage.UseVisualStyleBackColor = true;
            //
            // statusStrip1
            //
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 160); // Цей Location буде перевизначений Dock.Fill в tableLayoutPanel1
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 290); // Цей розмір буде перевизначений RowStyles.Add(SizeType.Absolute, 25F)
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            //
            // statusLabel
            //
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(310, 285); // Цей розмір також буде автоматично керуватися Dock.Fill
            this.statusLabel.Text = "Натисніть \'Оновити інформацію\', щоб отримати дані...";
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450); // Розмір форми встановлюється в Form1.cs
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Form1"; // Заголовок форми встановлюється в Form1.cs
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.TabPage.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel headerPanel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.TabControl TabPage; // Назва TabControl
        private System.Windows.Forms.TabPage processorTabPage;
        private System.Windows.Forms.TabPage videoCardTabPage;
        private System.Windows.Forms.TabPage dvdDriveTabPage;
        private System.Windows.Forms.TabPage diskDriveTabPage;
        private System.Windows.Forms.TabPage motherboardTabPage;
        private System.Windows.Forms.TabPage networkAdapterTabPage;
        private System.Windows.Forms.TabPage biosTabPage;
        private System.Windows.Forms.TabPage ramTabPage;
        private System.Windows.Forms.TabPage osTabPage;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    }
}
