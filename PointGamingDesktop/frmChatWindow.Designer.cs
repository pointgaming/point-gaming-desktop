namespace PointGaming
{
	partial class frmChatWindow
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
			this.btnSend = new System.Windows.Forms.Button();
			this.txtChatText = new System.Windows.Forms.TextBox();
			this.txtChatBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// btnSend
			// 
			this.btnSend.Location = new System.Drawing.Point(462, 584);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(86, 53);
			this.btnSend.TabIndex = 5;
			this.btnSend.Text = "Send";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// txtChatText
			// 
			this.txtChatText.Location = new System.Drawing.Point(12, 584);
			this.txtChatText.Multiline = true;
			this.txtChatText.Name = "txtChatText";
			this.txtChatText.Size = new System.Drawing.Size(444, 53);
			this.txtChatText.TabIndex = 4;
			// 
			// txtChatBox
			// 
			this.txtChatBox.Location = new System.Drawing.Point(12, 12);
			this.txtChatBox.Multiline = true;
			this.txtChatBox.Name = "txtChatBox";
			this.txtChatBox.ReadOnly = true;
			this.txtChatBox.Size = new System.Drawing.Size(536, 566);
			this.txtChatBox.TabIndex = 3;
			// 
			// frmChatWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(560, 650);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.txtChatText);
			this.Controls.Add(this.txtChatBox);
			this.Name = "frmChatWindow";
			this.Text = "Chat";
			this.Load += new System.EventHandler(this.frmChatWindow_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.TextBox txtChatText;
		private System.Windows.Forms.TextBox txtChatBox;

	}
}