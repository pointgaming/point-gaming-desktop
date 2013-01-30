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
	public partial class TestSockets : Form
	{
		public TestSockets()
		{
			InitializeComponent();
		}

		private void TestSockets_Load(object sender, EventArgs e)
		{
			Client socket;

			//Console.WriteLine("Starting TestSocketIOClient Example...");

			socket = new Client("http://dev.pointgaming.net:4000/"); // url to nodejs 

			// register for 'connect' event with io server
			socket.On("connect", (fn) =>
			{
				MessageBox.Show("Connected");
				//Console.WriteLine("\r\nConnected event...\r\n");
				//Console.WriteLine("Emit Part object");

				// emit Json Serializable object, anonymous types, or strings
				//Part newPart = new Part() { PartNumber = "K4P2G324EC", Code = "DDR2", Level = 1 };
				//socket.Emit("partInfo", newPart);
			});

			// register for 'update' events - message is a json 'Part' object
			//socket.On("update", (data) =>
			//{
			//	Console.WriteLine("recv [socket].[update] event");
			//	//Console.WriteLine("  raw message:      {0}", data.RawMessage);
			//	//Console.WriteLine("  string message:   {0}", data.MessageText);
			//	//Console.WriteLine("  json data string: {0}", data.Json.ToJsonString());
			//	//Console.WriteLine("  json raw:         {0}", data.Json.Args[0]);

			//	// cast message as Part - use type cast helper
			//	Part part = data.Json.GetFirstArgAs<Part>();
			//	Console.WriteLine(" Part Level:   {0}\r\n", part.Level);
			//});

			// make the socket.io connection
			socket.Connect();
		}

	}
}
