using SocketIOClient;
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
	public partial class frmChatWindow : Form
	{

		Client chatSocket;
		OutgoingMessages oMsg;
		ReceivedMessages rMsg;
		AuthEmit ae;
		ApiResponse ar;
		public frmChatWindow()
		{
			InitializeComponent();
		}

		private void frmChatWindow_Load(object sender, EventArgs e)
		{
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			chatSocket = new Client("http://dev.pointgaming.net:4000/");

			chatSocket.On("connect", (fn) =>
			{
				try
				{
					this.Invoke((MethodInvoker)delegate()
					{
						ae = new AuthEmit() { auth_token = Persistence.AuthToken };
						txtChatBox.Text = "Connected" + Environment.NewLine;
						txtChatBox.Text = "Chat with " + this.Text + " started!" + Environment.NewLine;
						chatSocket.Emit("auth", ae);
					});

				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message.ToString());
				}

			});

			chatSocket.On("message", (data) =>
			{
				this.Invoke((MethodInvoker)delegate()
				{
					rMsg = new ReceivedMessages();
					rMsg = data.Json.GetFirstArgAs<ReceivedMessages>();
					txtChatBox.Text += rMsg.username + ": " + rMsg.message + Environment.NewLine;
				});
			});

			chatSocket.On("auth_resp", (data) =>
			{
				ar = new ApiResponse();
				ar = data.Json.GetFirstArgAs<ApiResponse>();
			});

			chatSocket.Connect();
		}

		private void btnSend_Click(object sender, EventArgs e)
		{
			oMsg = new OutgoingMessages() { user = this.Text, message = txtChatText.Text };
			txtChatBox.Text += Persistence.loggedInUsername + ": " + oMsg.message + Environment.NewLine;
			chatSocket.Emit("message", oMsg);
			txtChatText.Clear();
		}

		private void txtChatText_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				btnSend.PerformClick();
				e.Handled = true;
			}
		}
	}
}
