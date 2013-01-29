using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PointGaming
{
    public partial class frmHome : Form
    {
        Random rndTop, rndLeft;

        public frmHome()
        {
            InitializeComponent();
        }

		//private void Form1_Load(object sender, EventArgs e)
		//{
		//	// Create three items and three sets of subitems for each item.
		//	ListViewItem item1 = new ListViewItem("David Booth", 0);
		//	ListViewItem item2 = new ListViewItem("Kavin", 1);
		//	ListViewItem item3 = new ListViewItem("Nathon Astle", 2);
		//	ListViewItem item4 = new ListViewItem("Jeff Brown", 3);

		//	item1.SubItems.Add("Online");
		//	item2.SubItems.Add("Offline");
		//	item3.SubItems.Add("Online");
		//	item4.SubItems.Add("Online");

		//	////Add the items to the ListView.
		//	lvContacts.Items.AddRange(new ListViewItem[] { item1, item2, item3, item4 });

		//	// Create two ImageList objects.

		//	//ImageList imageListLarge = new ImageList();

		//	// Initialize the ImageList objects with bitmaps.

		//	//imageListLarge.Images.Add(Bitmap.FromFile("C:\\1.jpg"));
		//	//imageListLarge.Images.Add(Bitmap.FromFile("C:\\1.jpg"));

		//	ImageList imageListSmall = new ImageList();
		//	imageListSmall.Images.Add(Bitmap.FromFile(Application.StartupPath + "\\Images\\1.jpg"));
		//	imageListSmall.Images.Add(Bitmap.FromFile(Application.StartupPath + "\\Images\\2.jpg"));
		//	imageListSmall.Images.Add(Bitmap.FromFile(Application.StartupPath + "\\Images\\3.jpg"));
		//	imageListSmall.Images.Add(Bitmap.FromFile(Application.StartupPath + "\\Images\\4.jpg"));

		//	lvContacts.SmallImageList = imageListSmall;
		//	rndLeft = new Random(10);
		//	rndTop = new Random(10);            
		//}

		//private void lvContacts_DoubleClick(object sender, EventArgs e)
		//{
		//	if (lvContacts.SelectedItems != null)
		//	{
		//		ListViewItem lvContact = lvContacts.SelectedItems[0];

		//		Control[] ctlChat = pnlChat.Controls.Find(lvContact.Text.Replace(" ", ""), true);

		//		if (ctlChat.Length == 0)
		//		{
		//			ucChat frmChat = new ucChat(lvContact.Text);
		//			frmChat.Name = lvContact.Text.Replace(" ", "");
		//			//frmChat.Left = rndLeft.Next(100);
		//			//frmChat.Top = rndTop.Next(100);
		//			pnlChat.Controls.Add(frmChat);
		//			frmChat.Show();
		//		}
		//		else
		//		{
		//			ucChat frmChat = (ucChat)ctlChat[0];
		//			frmChat.BringToFront();
		//		}
		//	}
		//}

		private void pnlChat_Paint(object sender, PaintEventArgs e)
		{

		}

		private void frmHome_Load(object sender, EventArgs e)
		{
			//get a list of all friends
			MessageBox.Show(AuthTokenStatic.GlobalVar);
		}
    }
}
