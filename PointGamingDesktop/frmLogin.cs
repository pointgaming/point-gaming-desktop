using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RestSharp;
using System.Configuration;

namespace PointGaming
{
	public partial class frmLogin : Form
	{
		public frmLogin()
		{
			InitializeComponent();
		}

		private void btnLogin_Click(object sender, EventArgs e)
		{
			var baseUrl=ConfigurationManager.AppSettings["BaseUrl"].ToString();
			var client = new RestClient(baseUrl);
			var request = new RestRequest("sessions", Method.POST);
			request.RequestFormat = DataFormat.Json;
			request.AddBody(new UserLogin { username = txtUserName.Text, password = txtPassword.Text });

			RestResponse<ApiResponse> apiResponse = (RestSharp.RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
			var status = apiResponse.Data.success;
			
			if (status == true)
			{

				Persistence.AuthToken = apiResponse.Data.auth_token;
				Persistence.loggedInUsername = txtUserName.Text;

				frmHome home = new frmHome();
				home.StartPosition = FormStartPosition.CenterScreen;
				home.Show();
				this.Hide();
				
			}
			else
			{
				MessageBox.Show("Invalid Username or Password");
			}
		}

		private void frmLogin_Load(object sender, EventArgs e)
		{
			this.StartPosition = FormStartPosition.CenterParent;

		}
	}
}
