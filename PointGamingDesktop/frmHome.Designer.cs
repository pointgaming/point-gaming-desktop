namespace PointGaming
{
	partial class frmHome
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
			this.tcOptions = new System.Windows.Forms.TabControl();
			this.tpFriends = new System.Windows.Forms.TabPage();
			this.lvContacts = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tpGames = new System.Windows.Forms.TabPage();
			this.lvGames = new System.Windows.Forms.ListView();
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tpBitCoin = new System.Windows.Forms.TabPage();
			this.tpAddFriend = new System.Windows.Forms.TabPage();
			this.label1 = new System.Windows.Forms.Label();
			this.btnAddFriend = new System.Windows.Forms.Button();
			this.txtFriendName = new System.Windows.Forms.TextBox();
			this.tpSettings = new System.Windows.Forms.TabPage();
			this.checkBox2 = new System.Windows.Forms.CheckBox();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.btnLogOut = new System.Windows.Forms.Button();
			this.tcOptions.SuspendLayout();
			this.tpFriends.SuspendLayout();
			this.tpGames.SuspendLayout();
			this.tpAddFriend.SuspendLayout();
			this.tpSettings.SuspendLayout();
			this.SuspendLayout();
			// 
			// tcOptions
			// 
			this.tcOptions.Controls.Add(this.tpFriends);
			this.tcOptions.Controls.Add(this.tpGames);
			this.tcOptions.Controls.Add(this.tpBitCoin);
			this.tcOptions.Controls.Add(this.tpAddFriend);
			this.tcOptions.Controls.Add(this.tpSettings);
			this.tcOptions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tcOptions.Location = new System.Drawing.Point(0, 0);
			this.tcOptions.Name = "tcOptions";
			this.tcOptions.SelectedIndex = 0;
			this.tcOptions.Size = new System.Drawing.Size(240, 658);
			this.tcOptions.TabIndex = 1;
			// 
			// tpFriends
			// 
			this.tpFriends.Controls.Add(this.lvContacts);
			this.tpFriends.Location = new System.Drawing.Point(4, 22);
			this.tpFriends.Name = "tpFriends";
			this.tpFriends.Padding = new System.Windows.Forms.Padding(3);
			this.tpFriends.Size = new System.Drawing.Size(232, 632);
			this.tpFriends.TabIndex = 0;
			this.tpFriends.Text = "Friends";
			this.tpFriends.UseVisualStyleBackColor = true;
			// 
			// lvContacts
			// 
			this.lvContacts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.lvContacts.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvContacts.FullRowSelect = true;
			this.lvContacts.Location = new System.Drawing.Point(3, 3);
			this.lvContacts.MultiSelect = false;
			this.lvContacts.Name = "lvContacts";
			this.lvContacts.Size = new System.Drawing.Size(226, 626);
			this.lvContacts.TabIndex = 6;
			this.lvContacts.UseCompatibleStateImageBehavior = false;
			this.lvContacts.View = System.Windows.Forms.View.Details;
			this.lvContacts.SelectedIndexChanged += new System.EventHandler(this.lvContacts_SelectedIndexChanged);
			this.lvContacts.Click += new System.EventHandler(this.lvContacts_Click);
			this.lvContacts.DoubleClick += new System.EventHandler(this.lvContacts_DoubleClick);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Username";
			this.columnHeader1.Width = 125;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Status";
			this.columnHeader2.Width = 88;
			// 
			// tpGames
			// 
			this.tpGames.Controls.Add(this.lvGames);
			this.tpGames.Location = new System.Drawing.Point(4, 22);
			this.tpGames.Name = "tpGames";
			this.tpGames.Padding = new System.Windows.Forms.Padding(3);
			this.tpGames.Size = new System.Drawing.Size(232, 632);
			this.tpGames.TabIndex = 1;
			this.tpGames.Text = "Games";
			this.tpGames.UseVisualStyleBackColor = true;
			// 
			// lvGames
			// 
			this.lvGames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
			this.lvGames.FullRowSelect = true;
			this.lvGames.Location = new System.Drawing.Point(0, 3);
			this.lvGames.Name = "lvGames";
			this.lvGames.Size = new System.Drawing.Size(231, 629);
			this.lvGames.TabIndex = 0;
			this.lvGames.UseCompatibleStateImageBehavior = false;
			this.lvGames.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Game";
			this.columnHeader3.Width = 114;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Players";
			this.columnHeader4.Width = 115;
			// 
			// tpBitCoin
			// 
			this.tpBitCoin.Location = new System.Drawing.Point(4, 22);
			this.tpBitCoin.Name = "tpBitCoin";
			this.tpBitCoin.Size = new System.Drawing.Size(232, 632);
			this.tpBitCoin.TabIndex = 2;
			this.tpBitCoin.Text = "BitCoin";
			this.tpBitCoin.UseVisualStyleBackColor = true;
			// 
			// tpAddFriend
			// 
			this.tpAddFriend.Controls.Add(this.label1);
			this.tpAddFriend.Controls.Add(this.btnAddFriend);
			this.tpAddFriend.Controls.Add(this.txtFriendName);
			this.tpAddFriend.Location = new System.Drawing.Point(4, 22);
			this.tpAddFriend.Name = "tpAddFriend";
			this.tpAddFriend.Size = new System.Drawing.Size(232, 632);
			this.tpAddFriend.TabIndex = 3;
			this.tpAddFriend.Text = "   +";
			this.tpAddFriend.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(4, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(55, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Username";
			// 
			// btnAddFriend
			// 
			this.btnAddFriend.Location = new System.Drawing.Point(116, 47);
			this.btnAddFriend.Name = "btnAddFriend";
			this.btnAddFriend.Size = new System.Drawing.Size(75, 23);
			this.btnAddFriend.TabIndex = 1;
			this.btnAddFriend.Text = "Add Friend";
			this.btnAddFriend.UseVisualStyleBackColor = true;
			this.btnAddFriend.Click += new System.EventHandler(this.btnAddFriend_Click);
			// 
			// txtFriendName
			// 
			this.txtFriendName.Location = new System.Drawing.Point(65, 20);
			this.txtFriendName.Name = "txtFriendName";
			this.txtFriendName.Size = new System.Drawing.Size(127, 20);
			this.txtFriendName.TabIndex = 0;
			// 
			// tpSettings
			// 
			this.tpSettings.Controls.Add(this.checkBox2);
			this.tpSettings.Controls.Add(this.checkBox1);
			this.tpSettings.Controls.Add(this.label3);
			this.tpSettings.Controls.Add(this.label2);
			this.tpSettings.Controls.Add(this.button1);
			this.tpSettings.Controls.Add(this.btnLogOut);
			this.tpSettings.Location = new System.Drawing.Point(4, 22);
			this.tpSettings.Name = "tpSettings";
			this.tpSettings.Size = new System.Drawing.Size(232, 632);
			this.tpSettings.TabIndex = 4;
			this.tpSettings.Text = "Settings";
			this.tpSettings.UseVisualStyleBackColor = true;
			// 
			// checkBox2
			// 
			this.checkBox2.AutoSize = true;
			this.checkBox2.Location = new System.Drawing.Point(54, 113);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new System.Drawing.Size(117, 17);
			this.checkBox2.TabIndex = 5;
			this.checkBox2.Text = "Hide Game Configs";
			this.checkBox2.UseVisualStyleBackColor = true;
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Checked = true;
			this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBox1.Location = new System.Drawing.Point(54, 90);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(119, 17);
			this.checkBox1.TabIndex = 4;
			this.checkBox1.Text = "Sync Game Configs";
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(110, 62);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(73, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Ctrl + Shift + h";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(25, 62);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(67, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Toggle HUD";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(38, 13);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(121, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "Scan for Games";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// btnLogOut
			// 
			this.btnLogOut.Location = new System.Drawing.Point(66, 586);
			this.btnLogOut.Name = "btnLogOut";
			this.btnLogOut.Size = new System.Drawing.Size(75, 23);
			this.btnLogOut.TabIndex = 0;
			this.btnLogOut.Text = "Log Out";
			this.btnLogOut.UseVisualStyleBackColor = true;
			this.btnLogOut.Click += new System.EventHandler(this.btnLogOut_Click);
			// 
			// frmHome
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(240, 658);
			this.Controls.Add(this.tcOptions);
			this.MaximizeBox = false;
			this.Name = "frmHome";
			this.Text = "Home";
			this.Load += new System.EventHandler(this.frmHome_Load);
			this.tcOptions.ResumeLayout(false);
			this.tpFriends.ResumeLayout(false);
			this.tpGames.ResumeLayout(false);
			this.tpAddFriend.ResumeLayout(false);
			this.tpAddFriend.PerformLayout();
			this.tpSettings.ResumeLayout(false);
			this.tpSettings.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tcOptions;
		private System.Windows.Forms.TabPage tpFriends;
		private System.Windows.Forms.ListView lvContacts;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.TabPage tpGames;
		private System.Windows.Forms.ListView lvGames;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.TabPage tpBitCoin;
		private System.Windows.Forms.TabPage tpAddFriend;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnAddFriend;
		private System.Windows.Forms.TextBox txtFriendName;
		private System.Windows.Forms.TabPage tpSettings;
		private System.Windows.Forms.CheckBox checkBox2;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button btnLogOut;
	}
}