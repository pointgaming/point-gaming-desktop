using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RestSharp;
namespace Demo
{
	public partial class frmLogin : Form
	{
		public frmLogin()
		{
			InitializeComponent();
		}

		private void btnLogin_Click(object sender, EventArgs e)
		{
			var client = new RestClient("http://dev.pointgaming.net:3000/api/v1/");

			var request = new RestRequest("sessions", Method.POST);

			request.RequestFormat = DataFormat.Json;
			request.AddBody(new UserLogin { username = "sultansaadat", password = "sultan123" });
			RestResponse response = (RestSharp.RestResponse)client.Execute(request);
			var content = response.Content; // raw content as string

			MessageBox.Show(content);
		}
	}
}
