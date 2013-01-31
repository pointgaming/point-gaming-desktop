using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Demo
{
    public partial class ucChat : UserControl
    {
        public ucChat(string sContactName)
        {
            InitializeComponent();

            lblName.Text = sContactName;
        }

		private void button1_Click(object sender, EventArgs e)
		{

		}
    }
}
