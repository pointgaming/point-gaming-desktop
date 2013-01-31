using RestSharp;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PointGaming
{
	public partial class frmHome : Form
	{


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

		private void showChatView(bool val)
		{
			txtChatBox.Visible = val;
			txtChatText.Visible = val;
			btnSend.Visible = val;
		}

		private void frmHome_Load(object sender, EventArgs e)
		{
			showChatView(false);

			var friendsApiCall = ConfigurationSettings.AppSettings["friends"].ToString() + AuthTokenStatic.GlobalVar;
			var client = new RestClient(friendsApiCall);
			var request = new RestRequest(Method.GET);
			RestResponse<FriendResponseRootObject> friendsApiResponse = (RestSharp.RestResponse<FriendResponseRootObject>)client.Execute<FriendResponseRootObject>(request);

			var status = friendsApiResponse.Data.success;
			var count = friendsApiResponse.Data.friends.Count;
			lvContacts.Items.Clear();
			ListViewItem dataItem;

			for (int i = 0; i < count; i++)
			{
				dataItem = new ListViewItem(friendsApiResponse.Data.friends[i].username.ToString());
				dataItem.SubItems.Add("Online");
				lvContacts.Items.AddRange(new ListViewItem[] { dataItem });
			}


			var gamesApiCall = ConfigurationSettings.AppSettings["games"].ToString() + AuthTokenStatic.GlobalVar;
			client = new RestClient(gamesApiCall);
			request = new RestRequest(Method.GET);
			RestResponse<GamesRootObject> gamesApiResponse = (RestSharp.RestResponse<GamesRootObject>)client.Execute<GamesRootObject>(request);
			status = gamesApiResponse.Data.success;
			count = gamesApiResponse.Data.games.Count;
			lvGames.Items.Clear();

			for (int i = 0; i < count; i++)
			{
				dataItem = new ListViewItem(gamesApiResponse.Data.games[i].name.ToString());
				dataItem.SubItems.Add(gamesApiResponse.Data.games[i].player_count.ToString());
				lvGames.Items.AddRange(new ListViewItem[] { dataItem });
			}


		}

		private void btnAddFriend_Click(object sender, EventArgs e)
		{

			User u = new User();
			u.username = txtFriendName.Text;
			UserRootObject uRoot = new UserRootObject();
			uRoot.user = u;

			var friendsApiCall = ConfigurationSettings.AppSettings["friends"].ToString() + AuthTokenStatic.GlobalVar;
			var client = new RestClient(friendsApiCall);

			var request = new RestRequest(Method.POST);
			request.RequestFormat = DataFormat.Json;
			request.AddBody(uRoot);

			RestResponse<ApiResponse> apiResponse = (RestSharp.RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
			var status = apiResponse.Data.success;
			if (status)
			{
				MessageBox.Show("Friend SuccessFully Added!");
				tpFriends.Show();
			}
			else
			{
				MessageBox.Show("Error: " + apiResponse.Data.message);
			}

		}

		private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
		{

		}

		private void tcOptions_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void tcOptions_TabIndexChanged(object sender, EventArgs e)
		{
			if (tcOptions.TabIndex == 0)
			{
				MessageBox.Show("1");
			}
		}

		private void tcOptions_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (e.TabPageIndex == 0)
			{
				var friendsApiCall = ConfigurationSettings.AppSettings["friends"].ToString() + AuthTokenStatic.GlobalVar;
				var client = new RestClient(friendsApiCall);
				var request = new RestRequest(Method.GET);
				RestResponse<FriendResponseRootObject> apiResponse = (RestSharp.RestResponse<FriendResponseRootObject>)client.Execute<FriendResponseRootObject>(request);

				var status = apiResponse.Data.success;
				var count = apiResponse.Data.friends.Count;
				lvContacts.Items.Clear();
				ListViewItem dataItem;

				for (int i = 0; i < count; i++)
				{
					dataItem = new ListViewItem(apiResponse.Data.friends[i].username.ToString());
					dataItem.SubItems.Add("Online");
					lvContacts.Items.AddRange(new ListViewItem[] { dataItem });
					//lvContacts.Items.Add(apiResponse.Data.friends[i].username.ToString());
				}

			}

			if (e.TabPageIndex == 1)
			{
				var gamesApiCall = ConfigurationSettings.AppSettings["games"].ToString() + AuthTokenStatic.GlobalVar;
				var client = new RestClient(gamesApiCall);
				var request = new RestRequest(Method.GET);
				RestResponse<GamesRootObject> gamesApiResponse = (RestSharp.RestResponse<GamesRootObject>)client.Execute<GamesRootObject>(request);
				var status = gamesApiResponse.Data.success;
				var count = gamesApiResponse.Data.games.Count;
				lvGames.Items.Clear();
				ListViewItem dataItem;
				for (int i = 0; i < count; i++)
				{
					dataItem = new ListViewItem(gamesApiResponse.Data.games[i].name.ToString());
					dataItem.SubItems.Add(gamesApiResponse.Data.games[i].player_count.ToString());
					lvGames.Items.AddRange(new ListViewItem[] { dataItem });
				}

			}

		}

		private void lvContacts_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void lvContacts_Click(object sender, EventArgs e)
		{
			var firstSelectedItem = lvContacts.SelectedItems[0].Text.ToString();

			Client socket;
			showChatView(true);

			socket = new Client("http://dev.pointgaming.net:4000/");
			//MessageBox.Show(firstSelectedItem);

			socket.On("connect", (fn) =>
			{
				try
				{
					this.Invoke((MethodInvoker)delegate()
					{
						txtChatBox.Text = "Connected" + Environment.NewLine;
						txtChatBox.Text= "Chat with " + firstSelectedItem + "started!" + Environment.NewLine;
					});

				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message.ToString());

				}
			});

			socket.Connect();
		}
	}
}
