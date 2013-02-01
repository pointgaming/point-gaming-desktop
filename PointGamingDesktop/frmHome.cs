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

		
		Client friendsSocket;

		AuthEmit ae;
		ApiResponse ar;
		
		string firstSelectedItem;

		public frmHome()
		{
			InitializeComponent();
		}

		private void frmHome_Load(object sender, EventArgs e)
		{
			FriendResponseRootObject fro;

			friendsSocket = new Client("http://dev.pointgaming.net:4000/");

			friendsSocket.On("connect", (fn) =>
			{
				try
				{
					this.Invoke((MethodInvoker)delegate()
					{
						ae = new AuthEmit() { auth_token = Persistence.AuthToken };
						friendsSocket.Emit("auth", ae);
					});
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message.ToString());
				}

			});

			friendsSocket.On("auth_resp", (data) =>
			{
				try
				{
					ar = new ApiResponse();
					ar = data.Json.GetFirstArgAs<ApiResponse>();
					tmrUserStatus.Start();
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message.ToString());
				}

			});

			friendsSocket.On("friends", (data) =>
			{
				ListViewItem dataItem;

				this.Invoke((MethodInvoker)delegate()
				{
					fro = new FriendResponseRootObject();
					fro = data.Json.GetFirstArgAs<FriendResponseRootObject>();
					var status = fro.success;
					var count = fro.friends.Count;
					lvContacts.Items.Clear();
					for (int i = 0; i < count; i++)
					{
						dataItem = new ListViewItem(fro.friends[i].username.ToString());
						dataItem.SubItems.Add(fro.friends[i].status);
						lvContacts.Items.AddRange(new ListViewItem[] { dataItem });
					}

				});
			});

			friendsSocket.Connect();
		}

		private void btnAddFriend_Click(object sender, EventArgs e)
		{
			User u = new User();
			u.username = txtFriendName.Text;
			UserRootObject uRoot = new UserRootObject();
			uRoot.user = u;

			var friendsApiCall = ConfigurationManager.AppSettings["friends"].ToString() + Persistence.AuthToken;
			var client = new RestClient(friendsApiCall);

			var request = new RestRequest(Method.POST);
			request.RequestFormat = DataFormat.Json;
			request.AddBody(uRoot);

			RestResponse<ApiResponse> apiResponse = (RestSharp.RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
			var status = apiResponse.Data.success;

			if (status)
			{
				MessageBox.Show("Friend Successfully Added!");
			}
			else
			{
				MessageBox.Show("Error: " + apiResponse.Data.message);
			}

		}

		private void tmrUserStatus_Tick(object sender, EventArgs e)
		{
			friendsSocket.Emit("friends", null);
		}

		private void btnLogOut_Click(object sender, EventArgs e)
		{
			var baseUrl = ConfigurationManager.AppSettings["BaseUrl"].ToString() + "sessions/destroy?auth_token=" + Persistence.AuthToken;
			var client = new RestClient(baseUrl);
			var request = new RestRequest(Method.DELETE);
			RestResponse<ApiResponse> apiResponse = (RestSharp.RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
			var status = apiResponse.Data.success;

			if (status == true)
			{
				MessageBox.Show("Logged out Successfully!");
				this.Hide();
				Persistence.AuthToken = String.Empty;
				Persistence.loggedInUsername = String.Empty;
				//chatSocket.Close();
				friendsSocket.Close();
				tmrUserStatus.Stop();
				frmLogin loginForm = new frmLogin();
				loginForm.Show();
			}

		}

		private void lvContacts_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		
		private void lvContacts_Click(object sender, EventArgs e)
		{
			firstSelectedItem = lvContacts.SelectedItems[0].Text.ToString();
			frmChatWindow chatWindow = new frmChatWindow();
			chatWindow.Text = firstSelectedItem;
			chatWindow.Show();
		}
	}
}
