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
			this.components = new System.ComponentModel.Container();
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
			this.button2 = new System.Windows.Forms.Button();
			this.chkFreeProAccount = new System.Windows.Forms.CheckBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.chkIdolMine = new System.Windows.Forms.CheckBox();
			this.chkMineBitcoins = new System.Windows.Forms.CheckBox();
			this.btnRunTest = new System.Windows.Forms.Button();
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
			this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.mnuDeleteFriend = new System.Windows.Forms.ToolStripMenuItem();
			this.tcOptions.SuspendLayout();
			this.tpFriends.SuspendLayout();
			this.tpGames.SuspendLayout();
			this.tpBitCoin.SuspendLayout();
			this.tpAddFriend.SuspendLayout();
			this.tpSettings.SuspendLayout();
			this.contextMenu.SuspendLayout();
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
			this.tcOptions.Size = new System.Drawing.Size(248, 658);
			this.tcOptions.TabIndex = 1;
			// 
			// tpFriends
			// 
			this.tpFriends.Controls.Add(this.lvContacts);
			this.tpFriends.Location = new System.Drawing.Point(4, 22);
			this.tpFriends.Name = "tpFriends";
			this.tpFriends.Padding = new System.Windows.Forms.Padding(3);
			this.tpFriends.Size = new System.Drawing.Size(240, 632);
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
			this.lvContacts.Size = new System.Drawing.Size(234, 626);
			this.lvContacts.TabIndex = 6;
			this.lvContacts.UseCompatibleStateImageBehavior = false;
			this.lvContacts.View = System.Windows.Forms.View.Details;
			this.lvContacts.SelectedIndexChanged += new System.EventHandler(this.lvContacts_SelectedIndexChanged);
			this.lvContacts.Click += new System.EventHandler(this.lvContacts_Click);
			this.lvContacts.DoubleClick += new System.EventHandler(this.lvContacts_DoubleClick);
			this.lvContacts.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lvContacts_MouseClick);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "UserName";
			this.columnHeader1.Width = 118;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Status";
			this.columnHeader2.Width = 111;
			// 
			// tpGames
			// 
			this.tpGames.Controls.Add(this.lvGames);
			this.tpGames.Location = new System.Drawing.Point(4, 22);
			this.tpGames.Name = "tpGames";
			this.tpGames.Padding = new System.Windows.Forms.Padding(3);
			this.tpGames.Size = new System.Drawing.Size(240, 632);
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
			this.tpBitCoin.Controls.Add(this.button2);
			this.tpBitCoin.Controls.Add(this.chkFreeProAccount);
			this.tpBitCoin.Controls.Add(this.label7);
			this.tpBitCoin.Controls.Add(this.label5);
			this.tpBitCoin.Controls.Add(this.label4);
			this.tpBitCoin.Controls.Add(this.chkIdolMine);
			this.tpBitCoin.Controls.Add(this.chkMineBitcoins);
			this.tpBitCoin.Controls.Add(this.btnRunTest);
			this.tpBitCoin.Location = new System.Drawing.Point(4, 22);
			this.tpBitCoin.Name = "tpBitCoin";
			this.tpBitCoin.Size = new System.Drawing.Size(240, 632);
			this.tpBitCoin.TabIndex = 2;
			this.tpBitCoin.Text = "BitCoin";
			this.tpBitCoin.UseVisualStyleBackColor = true;
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(69, 256);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 8;
			this.button2.Text = "Start";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// chkFreeProAccount
			// 
			this.chkFreeProAccount.AutoSize = true;
			this.chkFreeProAccount.Location = new System.Drawing.Point(54, 182);
			this.chkFreeProAccount.Name = "chkFreeProAccount";
			this.chkFreeProAccount.Size = new System.Drawing.Size(109, 17);
			this.chkFreeProAccount.TabIndex = 7;
			this.chkFreeProAccount.Text = "Free Pro Account";
			this.chkFreeProAccount.UseVisualStyleBackColor = true;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(51, 213);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(80, 13);
			this.label7.TabIndex = 6;
			this.label7.Text = "Points Mined: 0";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(51, 156);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(125, 13);
			this.label5.TabIndex = 4;
			this.label5.Text = "Average Hash Rate: 369";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(51, 128);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(67, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Nvidia 650M";
			// 
			// chkIdolMine
			// 
			this.chkIdolMine.AutoSize = true;
			this.chkIdolMine.Checked = true;
			this.chkIdolMine.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkIdolMine.Location = new System.Drawing.Point(54, 73);
			this.chkIdolMine.Name = "chkIdolMine";
			this.chkIdolMine.Size = new System.Drawing.Size(122, 17);
			this.chkIdolMine.TabIndex = 2;
			this.chkIdolMine.Text = "Only Mine when Idol";
			this.chkIdolMine.UseVisualStyleBackColor = true;
			// 
			// chkMineBitcoins
			// 
			this.chkMineBitcoins.AutoSize = true;
			this.chkMineBitcoins.Location = new System.Drawing.Point(55, 50);
			this.chkMineBitcoins.Name = "chkMineBitcoins";
			this.chkMineBitcoins.Size = new System.Drawing.Size(89, 17);
			this.chkMineBitcoins.TabIndex = 1;
			this.chkMineBitcoins.Text = "Mine Bitcoins";
			this.chkMineBitcoins.UseVisualStyleBackColor = true;
			// 
			// btnRunTest
			// 
			this.btnRunTest.Location = new System.Drawing.Point(54, 21);
			this.btnRunTest.Name = "btnRunTest";
			this.btnRunTest.Size = new System.Drawing.Size(119, 23);
			this.btnRunTest.TabIndex = 0;
			this.btnRunTest.Text = "Run 30 Second Test";
			this.btnRunTest.UseVisualStyleBackColor = true;
			// 
			// tpAddFriend
			// 
			this.tpAddFriend.Controls.Add(this.label1);
			this.tpAddFriend.Controls.Add(this.btnAddFriend);
			this.tpAddFriend.Controls.Add(this.txtFriendName);
			this.tpAddFriend.Location = new System.Drawing.Point(4, 22);
			this.tpAddFriend.Name = "tpAddFriend";
			this.tpAddFriend.Size = new System.Drawing.Size(240, 632);
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
			this.tpSettings.Size = new System.Drawing.Size(240, 632);
			this.tpSettings.TabIndex = 4;
			this.tpSettings.Text = "Settings";
			this.tpSettings.UseVisualStyleBackColor = true;
			// 
			// checkBox2
			// 
			this.checkBox2.AutoSize = true;
			this.checkBox2.Location = new System.Drawing.Point(54, 137);
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
			this.checkBox1.Location = new System.Drawing.Point(54, 114);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(119, 17);
			this.checkBox1.TabIndex = 4;
			this.checkBox1.Text = "Sync Game Configs";
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(124, 62);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(73, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Ctrl + Shift + h";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(51, 62);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(67, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Toggle HUD";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(54, 16);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(121, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "Scan for Games";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// btnLogOut
			// 
			this.btnLogOut.Location = new System.Drawing.Point(78, 601);
			this.btnLogOut.Name = "btnLogOut";
			this.btnLogOut.Size = new System.Drawing.Size(75, 23);
			this.btnLogOut.TabIndex = 0;
			this.btnLogOut.Text = "Log Out";
			this.btnLogOut.UseVisualStyleBackColor = true;
			this.btnLogOut.Click += new System.EventHandler(this.btnLogOut_Click);
			// 
			// contextMenu
			// 
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuDeleteFriend});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(144, 26);
			// 
			// mnuDeleteFriend
			// 
			this.mnuDeleteFriend.Name = "mnuDeleteFriend";
			this.mnuDeleteFriend.Size = new System.Drawing.Size(143, 22);
			this.mnuDeleteFriend.Text = "Delete Friend";
			this.mnuDeleteFriend.Click += new System.EventHandler(this.mnuDeleteFriend_Click);
			// 
			// frmHome
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(248, 658);
			this.Controls.Add(this.tcOptions);
			this.MaximizeBox = false;
			this.Name = "frmHome";
			this.Text = "Home";
			this.Load += new System.EventHandler(this.frmHome_Load);
			this.tcOptions.ResumeLayout(false);
			this.tpFriends.ResumeLayout(false);
			this.tpGames.ResumeLayout(false);
			this.tpBitCoin.ResumeLayout(false);
			this.tpBitCoin.PerformLayout();
			this.tpAddFriend.ResumeLayout(false);
			this.tpAddFriend.PerformLayout();
			this.tpSettings.ResumeLayout(false);
			this.tpSettings.PerformLayout();
			this.contextMenu.ResumeLayout(false);
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
		private System.Windows.Forms.ContextMenuStrip contextMenu;
		private System.Windows.Forms.ToolStripMenuItem mnuDeleteFriend;
		private System.Windows.Forms.CheckBox chkFreeProAccount;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox chkIdolMine;
		private System.Windows.Forms.CheckBox chkMineBitcoins;
		private System.Windows.Forms.Button btnRunTest;
		private System.Windows.Forms.Button button2;
	}
}