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
		Client socket;
		AuthEmit ae;
		ApiResponse ar;
		OutgoingMessages oMsg;
		ReceivedMessages rMsg;
		string firstSelectedItem;

		public frmHome()
		{
			InitializeComponent();
		}

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

			var friendsApiCall = ConfigurationManager.AppSettings["friends"].ToString() + AuthTokenStatic.GlobalVar;
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

			var gamesApiCall = ConfigurationManager.AppSettings["games"].ToString() + AuthTokenStatic.GlobalVar;
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

			var friendsApiCall = ConfigurationManager.AppSettings["friends"].ToString() + AuthTokenStatic.GlobalVar;
			var client = new RestClient(friendsApiCall);

			var request = new RestRequest(Method.POST);
			request.RequestFormat = DataFormat.Json;
			request.AddBody(uRoot);

			RestResponse<ApiResponse> apiResponse = (RestSharp.RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
			var status = apiResponse.Data.success;

			if (status)
			{
				MessageBox.Show("Friend SuccessFully Added!");
				//tpFriends.Show();
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

		}

		private void tcOptions_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (e.TabPageIndex == 0)
			{
				var friendsApiCall = ConfigurationManager.AppSettings["friends"].ToString() + AuthTokenStatic.GlobalVar;
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
				}

			}

			if (e.TabPageIndex == 1)
			{
				var gamesApiCall = ConfigurationManager.AppSettings["games"].ToString() + AuthTokenStatic.GlobalVar;
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
			firstSelectedItem = lvContacts.SelectedItems[0].Text.ToString();
			showChatView(true);

			socket = new Client("http://dev.pointgaming.net:4000/");

			socket.On("connect", (fn) =>
			{
				try
				{
					this.Invoke((MethodInvoker)delegate()
					{
						ae = new AuthEmit() { auth_token = AuthTokenStatic.GlobalVar };
						txtChatBox.Text = "Connected" + Environment.NewLine;
						txtChatBox.Text = "Chat with " + firstSelectedItem + " started!" + Environment.NewLine;
						socket.Emit("auth", ae);
					});

				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message.ToString());
				}

			});


			socket.On("message", (data) =>
			{
				this.Invoke((MethodInvoker)delegate()
					{
						rMsg = new ReceivedMessages();
						rMsg = data.Json.GetFirstArgAs<ReceivedMessages>();
						txtChatBox.Text += rMsg.username + ": " + rMsg.message + Environment.NewLine;
					});
			});

			socket.On("auth_resp", (data) =>
			{
				ar = new ApiResponse();
				ar = data.Json.GetFirstArgAs<ApiResponse>();
			});

			socket.Connect();
		}

		private void btnSend_Click(object sender, EventArgs e)
		{

			oMsg = new OutgoingMessages() { user = firstSelectedItem, message = txtChatText.Text + Environment.NewLine };
			txtChatBox.Text += AuthTokenStatic.loggedInUsername + ": " + oMsg.message + Environment.NewLine;
			socket.Emit("message", oMsg);
			txtChatText.Clear();

		}
	}
}
