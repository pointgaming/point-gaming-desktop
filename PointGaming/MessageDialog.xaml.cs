using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PointGaming
{
    public partial class MessageDialog : Window
    {
        public MessageDialog()
        {
            InitializeComponent();
        }

        public string Message { get { return textBoxMessage.Text; } set { textBoxMessage.Text = value; } }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void Show(Window owner, string title, string message)
        {
            var md = new MessageDialog
            {
                Owner = owner,
                Title = title,
                Message = message,
            };

            md.ShowDialog();
        }
    }
}
