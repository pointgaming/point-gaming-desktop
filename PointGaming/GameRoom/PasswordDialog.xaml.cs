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

namespace PointGaming.GameRoom
{
    public partial class PasswordDialog : Window
    {
        public PasswordDialog()
        {
            InitializeComponent();
        }

        public string Message { get { return textBoxMessage.Text; } set { textBoxMessage.Text = value; } }
        public string Password { get { return textBoxPassword.Text; } set { textBoxPassword.Text = value; } }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public static bool? Show(Window owner, string title, string message, out string password)
        {
            var md = new PasswordDialog
            {
                Owner = owner,
                Title = title,
                Message = message,
            };

            var result = md.ShowDialog();
            password = null;
            if (result == true)
                password = md.Password;

            return result;
        }
    }
}
