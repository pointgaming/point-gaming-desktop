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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.tcOptions = new System.Windows.Forms.TabControl();
			this.tpFriends = new System.Windows.Forms.TabPage();
			this.lvContacts = new System.Windows.Forms.ListView();
			this.colMember = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tpGames = new System.Windows.Forms.TabPage();
			this.tpBitCoin = new System.Windows.Forms.TabPage();
			this.pnlChat = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.tcOptions.SuspendLayout();
			this.tpFriends.SuspendLayout();
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
			this.splitContainer1.Size = new System.Drawing.Size(940, 660);
			this.splitContainer1.SplitterDistance = 234;
			this.splitContainer1.TabIndex = 1;
			// 
			// tcOptions
			// 
			this.tcOptions.Controls.Add(this.tpFriends);
			this.tcOptions.Controls.Add(this.tpGames);
			this.tcOptions.Controls.Add(this.tpBitCoin);
			this.tcOptions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tcOptions.Location = new System.Drawing.Point(0, 0);
			this.tcOptions.Name = "tcOptions";
			this.tcOptions.SelectedIndex = 0;
			this.tcOptions.Size = new System.Drawing.Size(232, 658);
			this.tcOptions.TabIndex = 0;
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
            this.colMember,
            this.colStatus});
			this.lvContacts.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvContacts.FullRowSelect = true;
			this.lvContacts.GridLines = true;
			this.lvContacts.Location = new System.Drawing.Point(3, 3);
			this.lvContacts.MultiSelect = false;
			this.lvContacts.Name = "lvContacts";
			this.lvContacts.Size = new System.Drawing.Size(218, 626);
			this.lvContacts.TabIndex = 6;
			this.lvContacts.UseCompatibleStateImageBehavior = false;
			this.lvContacts.View = System.Windows.Forms.View.Details;
			// 
			// colMember
			// 
			this.colMember.Text = "Member";
			this.colMember.Width = 150;
			// 
			// colStatus
			// 
			this.colStatus.Text = "Status";
			// 
			// tpGames
			// 
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
			// pnlChat
			// 
			this.pnlChat.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlChat.Location = new System.Drawing.Point(0, 0);
			this.pnlChat.Name = "pnlChat";
			this.pnlChat.Size = new System.Drawing.Size(700, 658);
			this.pnlChat.TabIndex = 0;
			this.pnlChat.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlChat_Paint);
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
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tcOptions;
        private System.Windows.Forms.TabPage tpFriends;
        private System.Windows.Forms.TabPage tpGames;
        private System.Windows.Forms.TabPage tpBitCoin;
        private System.Windows.Forms.ListView lvContacts;
        private System.Windows.Forms.ColumnHeader colMember;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.Panel pnlChat;
    }
}

