// SettingsForm.Designer.cs (для ChatClientWF)
namespace ChatClientWF
{
    partial class SettingsForm
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
            this.lblIpAddress = new System.Windows.Forms.Label();
            this.txtIpAddress = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblChatFontFamily = new System.Windows.Forms.Label();
            this.cmbChatFontFamily = new System.Windows.Forms.ComboBox();
            this.lblChatFontSize = new System.Windows.Forms.Label();
            this.cmbChatFontSize = new System.Windows.Forms.ComboBox();
            this.chkEnableChatLogging = new System.Windows.Forms.CheckBox();
            this.lblChatLogFilePath = new System.Windows.Forms.Label();
            this.txtChatLogFilePath = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblIpAddress
            //
            this.lblIpAddress.AutoSize = true;
            this.lblIpAddress.Location = new System.Drawing.Point(25, 25);
            this.lblIpAddress.Name = "lblIpAddress";
            this.lblIpAddress.Size = new System.Drawing.Size(76, 17);
            this.lblIpAddress.TabIndex = 0;
            this.lblIpAddress.Text = "IP-адреса:";
            //
            // txtIpAddress
            //
            this.txtIpAddress.Location = new System.Drawing.Point(160, 22);
            this.txtIpAddress.Name = "txtIpAddress";
            this.txtIpAddress.Size = new System.Drawing.Size(200, 22);
            this.txtIpAddress.TabIndex = 1;
            //
            // lblPort
            //
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(25, 60);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(46, 17);
            this.lblPort.TabIndex = 2;
            this.lblPort.Text = "Порт:";
            //
            // txtPort
            //
            this.txtPort.Location = new System.Drawing.Point(160, 57);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(200, 22);
            this.txtPort.TabIndex = 3;
            //
            // lblChatFontFamily
            //
            this.lblChatFontFamily.AutoSize = true;
            this.lblChatFontFamily.Location = new System.Drawing.Point(25, 95);
            this.lblChatFontFamily.Name = "lblChatFontFamily";
            this.lblChatFontFamily.Size = new System.Drawing.Size(95, 17);
            this.lblChatFontFamily.TabIndex = 4;
            this.lblChatFontFamily.Text = "Шрифт чату:";
            //
            // cmbChatFontFamily
            //
            this.cmbChatFontFamily.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbChatFontFamily.FormattingEnabled = true;
            this.cmbChatFontFamily.Location = new System.Drawing.Point(160, 92);
            this.cmbChatFontFamily.Name = "cmbChatFontFamily";
            this.cmbChatFontFamily.Size = new System.Drawing.Size(200, 24);
            this.cmbChatFontFamily.TabIndex = 5;
            //
            // lblChatFontSize
            //
            this.lblChatFontSize.AutoSize = true;
            this.lblChatFontSize.Location = new System.Drawing.Point(25, 130);
            this.lblChatFontSize.Name = "lblChatFontSize";
            this.lblChatFontSize.Size = new System.Drawing.Size(107, 17);
            this.lblChatFontSize.TabIndex = 6;
            this.lblChatFontSize.Text = "Розмір шрифту:";
            //
            // cmbChatFontSize
            //
            this.cmbChatFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbChatFontSize.FormattingEnabled = true;
            this.cmbChatFontSize.Location = new System.Drawing.Point(160, 127);
            this.cmbChatFontSize.Name = "cmbChatFontSize";
            this.cmbChatFontSize.Size = new System.Drawing.Size(200, 24);
            this.cmbChatFontSize.TabIndex = 7;
            //
            // chkEnableChatLogging
            //
            this.chkEnableChatLogging.AutoSize = true;
            this.chkEnableChatLogging.Location = new System.Drawing.Point(25, 170);
            this.chkEnableChatLogging.Name = "chkEnableChatLogging";
            this.chkEnableChatLogging.Size = new System.Drawing.Size(193, 21);
            this.chkEnableChatLogging.TabIndex = 8;
            this.chkEnableChatLogging.Text = "Увімкнути логування чату";
            this.chkEnableChatLogging.UseVisualStyleBackColor = true;
            //
            // lblChatLogFilePath
            //
            this.lblChatLogFilePath.AutoSize = true;
            this.lblChatLogFilePath.Location = new System.Drawing.Point(25, 205);
            this.lblChatLogFilePath.Name = "lblChatLogFilePath";
            this.lblChatLogFilePath.Size = new System.Drawing.Size(129, 17);
            this.lblChatLogFilePath.TabIndex = 9;
            this.lblChatLogFilePath.Text = "Шлях до файлу логу:";
            //
            // txtChatLogFilePath
            //
            this.txtChatLogFilePath.Location = new System.Drawing.Point(160, 202);
            this.txtChatLogFilePath.Name = "txtChatLogFilePath";
            this.txtChatLogFilePath.Size = new System.Drawing.Size(200, 22);
            this.txtChatLogFilePath.TabIndex = 10;
            //
            // btnSave
            //
            this.btnSave.Location = new System.Drawing.Point(200, 250);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.TabIndex = 11;
            this.btnSave.Text = "Зберегти";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            //
            // btnCancel
            //
            this.btnCancel.Location = new System.Drawing.Point(285, 250);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Скасувати";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // SettingsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtChatLogFilePath);
            this.Controls.Add(this.lblChatLogFilePath);
            this.Controls.Add(this.chkEnableChatLogging);
            this.Controls.Add(this.cmbChatFontSize);
            this.Controls.Add(this.lblChatFontSize);
            this.Controls.Add(this.cmbChatFontFamily);
            this.Controls.Add(this.lblChatFontFamily);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.txtIpAddress);
            this.Controls.Add(this.lblIpAddress);
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Налаштування чату";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblIpAddress;
        private System.Windows.Forms.TextBox txtIpAddress;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblChatFontFamily;
        private System.Windows.Forms.ComboBox cmbChatFontFamily;
        private System.Windows.Forms.Label lblChatFontSize;
        private System.Windows.Forms.ComboBox cmbChatFontSize;
        private System.Windows.Forms.CheckBox chkEnableChatLogging;
        private System.Windows.Forms.Label lblChatLogFilePath;
        private System.Windows.Forms.TextBox txtChatLogFilePath;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}
