// ServerForm.Designer.cs (для ChatServerWF)
namespace ChatServerWF
{
    partial class ServerForm
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
            this.btnStartServer = new System.Windows.Forms.Button();
            this.btnStopServer = new System.Windows.Forms.Button();
            this.chatLogRichTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            //
            // btnStartServer
            //
            this.btnStartServer.Location = new System.Drawing.Point(12, 12);
            this.btnStartServer.Name = "btnStartServer";
            this.btnStartServer.Size = new System.Drawing.Size(150, 40);
            this.btnStartServer.TabIndex = 0;
            this.btnStartServer.Text = "Запустити сервер";
            this.btnStartServer.UseVisualStyleBackColor = true;
            this.btnStartServer.Click += new System.EventHandler(this.btnStartServer_Click);
            //
            // btnStopServer
            //
            this.btnStopServer.Enabled = false;
            this.btnStopServer.Location = new System.Drawing.Point(168, 12);
            this.btnStopServer.Name = "btnStopServer";
            this.btnStopServer.Size = new System.Drawing.Size(150, 40);
            this.btnStopServer.TabIndex = 1;
            this.btnStopServer.Text = "Зупинити сервер";
            this.btnStopServer.UseVisualStyleBackColor = true;
            this.btnStopServer.Click += new System.EventHandler(this.btnStopServer_Click);
            //
            // chatLogRichTextBox
            //
            this.chatLogRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chatLogRichTextBox.Location = new System.Drawing.Point(12, 58);
            this.chatLogRichTextBox.Name = "chatLogRichTextBox";
            this.chatLogRichTextBox.ReadOnly = true;
            this.chatLogRichTextBox.Size = new System.Drawing.Size(760, 480);
            this.chatLogRichTextBox.TabIndex = 2;
            this.chatLogRichTextBox.Text = "";
            //
            // ServerForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 550);
            this.Controls.Add(this.chatLogRichTextBox);
            this.Controls.Add(this.btnStopServer);
            this.Controls.Add(this.btnStartServer);
            this.Name = "ServerForm";
            this.Text = "TCP Чат Сервер";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.Button btnStopServer;
        private System.Windows.Forms.RichTextBox chatLogRichTextBox;
    }
}
