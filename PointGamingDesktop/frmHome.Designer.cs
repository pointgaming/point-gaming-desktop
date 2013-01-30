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
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "Test",
            "Test"}, -1);
			System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Test");
			System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Test");
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.tcOptions = new System.Windows.Forms.TabControl();
			this.tpFriends = new System.Windows.Forms.TabPage();
			this.lvContacts = new System.Windows.Forms.ListView();
			this.tpGames = new System.Windows.Forms.TabPage();
			this.tpBitCoin = new System.Windows.Forms.TabPage();
			this.tpAddFriend = new System.Windows.Forms.TabPage();
			this.label1 = new System.Windows.Forms.Label();
			this.btnAddFriend = new System.Windows.Forms.Button();
			this.txtFriendName = new System.Windows.Forms.TextBox();
			this.tpSettings = new System.Windows.Forms.TabPage();
			this.pnlChat = new System.Windows.Forms.Panel();
			this.lvGames = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.tcOptions.SuspendLayout();
			this.tpFriends.SuspendLayout();
			this.tpGames.SuspendLayout();
			this.tpAddFriend.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.tcOptions);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.pnlChat);
			this.splitContainer1.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel2_Paint);
			this.splitContainer1.Size = new System.Drawing.Size(940, 660);
			this.splitContainer1.SplitterDistance = 234;
			this.splitContainer1.TabIndex = 1;
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
			this.tcOptions.Size = new System.Drawing.Size(232, 658);
			this.tcOptions.TabIndex = 0;
			this.tcOptions.SelectedIndexChanged += new System.EventHandler(this.tcOptions_SelectedIndexChanged);
			this.tcOptions.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tcOptions_Selecting);
			this.tcOptions.TabIndexChanged += new System.EventHandler(this.tcOptions_TabIndexChanged);
			// 
			// tpFriends
			// 
			this.tpFriends.Controls.Add(this.lvContacts);
			this.tpFriends.Location = new System.Drawing.Point(4, 22);
			this.tpFriends.Name = "tpFriends";
			this.tpFriends.Padding = new System.Windows.Forms.Padding(3);
			this.tpFriends.Size = new System.Drawing.Size(224, 632);
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
			this.lvContacts.Size = new System.Drawing.Size(218, 626);
			this.lvContacts.TabIndex = 6;
			this.lvContacts.UseCompatibleStateImageBehavior = false;
			this.lvContacts.View = System.Windows.Forms.View.Details;
			// 
			// tpGames
			// 
			this.tpGames.Controls.Add(this.lvGames);
			this.tpGames.Location = new System.Drawing.Point(4, 22);
			this.tpGames.Name = "tpGames";
			this.tpGames.Padding = new System.Windows.Forms.Padding(3);
			this.tpGames.Size = new System.Drawing.Size(224, 632);
			this.tpGames.TabIndex = 1;
			this.tpGames.Text = "Games";
			this.tpGames.UseVisualStyleBackColor = true;
			// 
			// tpBitCoin
			// 
			this.tpBitCoin.Location = new System.Drawing.Point(4, 22);
			this.tpBitCoin.Name = "tpBitCoin";
			this.tpBitCoin.Size = new System.Drawing.Size(224, 632);
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
			this.tpAddFriend.Size = new System.Drawing.Size(224, 632);
			this.tpAddFriend.TabIndex = 3;
			this.tpAddFriend.Text = "+";
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
			this.tpSettings.Location = new System.Drawing.Point(4, 22);
			this.tpSettings.Name = "tpSettings";
			this.tpSettings.Size = new System.Drawing.Size(224, 632);
			this.tpSettings.TabIndex = 4;
			this.tpSettings.Text = "Settings";
			this.tpSettings.UseVisualStyleBackColor = true;
			// 
			// pnlChat
			// 
			this.pnlChat.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlChat.Location = new System.Drawing.Point(0, 0);
			this.pnlChat.Name = "pnlChat";
			this.pnlChat.Size = new System.Drawing.Size(700, 658);
			this.pnlChat.TabIndex = 0;
			this.pnlChat.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlChat_Paint);
			// 
			// lvGames
			// 
			this.lvGames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
			this.lvGames.FullRowSelect = true;
			listViewItem1.Tag = "Test";
			this.lvGames.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3});
			this.lvGames.Location = new System.Drawing.Point(0, 3);
			this.lvGames.Name = "lvGames";
			this.lvGames.Size = new System.Drawing.Size(225, 629);
			this.lvGames.TabIndex = 0;
			this.lvGames.UseCompatibleStateImageBehavior = false;
			this.lvGames.View = System.Windows.Forms.View.Details;
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
			// frmHome
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(940, 660);
			this.Controls.Add(this.splitContainer1);
			this.Name = "frmHome";
			this.Text = "Point Gaming";
			this.Load += new System.EventHandler(this.frmHome_Load);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.tcOptions.ResumeLayout(false);
			this.tpFriends.ResumeLayout(false);
			this.tpGames.ResumeLayout(false);
			this.tpAddFriend.ResumeLayout(false);
			this.tpAddFriend.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tcOptions;
        private System.Windows.Forms.TabPage tpFriends;
        private System.Windows.Forms.TabPage tpGames;
        private System.Windows.Forms.TabPage tpBitCoin;
		private System.Windows.Forms.ListView lvContacts;
        private System.Windows.Forms.Panel pnlChat;
		private System.Windows.Forms.TabPage tpAddFriend;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnAddFriend;
		private System.Windows.Forms.TextBox txtFriendName;
		private System.Windows.Forms.TabPage tpSettings;
		private System.Windows.Forms.ListView lvGames;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
    }
}

