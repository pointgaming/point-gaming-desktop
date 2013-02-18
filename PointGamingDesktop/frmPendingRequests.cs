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
	public partial class frmPendingRequests : Form
	{
		public frmPendingRequests()
		{
			InitializeComponent();
		}

		private void frmPendingRequests_Load(object sender, EventArgs e)
		{

			//for (int i = 0; i < 3; i++)
			//{
			//	Button folderButton = new Button();
			//	folderButton.Location.X = 10;

			//	folderButton.Width = 50;
			//	folderButton.Height = 30;
			//	folderButton.ForeColor = Color.Black;
			//	folderButton.Text = "test" + i.ToString();
			//	//folderButton.Click += ButtonClickHandler;
			//	this.Controls.Add(folderButton);
			//}


			for (int i = 0; i < Persistence.friendRequests.friend_requests.Count; i++)
			{
				Label label = new Label();
				label.Location = new Point(100, 41 * i);
				label.Text = Persistence.friendRequests.friend_requests[i].ToString
				label.Width = 50;
				label.Height = 20;

				Button button = new Button();
				button.Location = new Point(150, 40 * i);
				button.Width = 50;
				button.Height = 20;
				button.Text = "test" + i.ToString();

				//button.Click += new EventHandler(ButtonClickOneEvent);
				//button.Tag = i;
				this.Controls.Add(label);
				this.Controls.Add(button);
			}
		}
	}
}
