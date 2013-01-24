﻿using System;
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
			request.AddParameter("username", "sultansaadat"); 
				request.AddParameter("password", "sultan123"); 


			RestResponse response = (RestSharp.RestResponse) client.Execute(request);
			var content = response.Content; // raw content as string

			MessageBox.Show(content);

			//// or automatically deserialize result
			//// return content type is sniffed but can be explicitly set via RestClient.AddHandler();
			//RestResponse<> response2 = client.Execute<Person>(request);
			//var name = response2.Data.Name;

			//// easy async support
			//client.ExecuteAsync(request, response =>
			//{
			//	Console.WriteLine(response.Content);
			//});

			//// async with deserialization
			//var asyncHandle = client.ExecuteAsync<Person>(request, response =>
			//{
			//	Console.WriteLine(response.Data.Name);
			//});

			//// abort the request on demand
			//asyncHandle.Abort();


		}
	}
}
