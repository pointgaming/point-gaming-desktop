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
    public partial class KeySelectDialog : Window
    {
        public Key? SelectedKey;

        public KeySelectDialog()
        {
            InitializeComponent();
        }

        public string Message { get { return textBoxMessage.Text; } set { textBoxMessage.Text = value; } }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public static Key? Show(Window owner, string title, string message)
        {
            var md = new KeySelectDialog
            {
                Owner = owner,
                Title = title,
                Message = message,
            };
            md.PreviewKeyUp += md.MyPreviewKeyUp;
            md.ShowDialog();
            return md.SelectedKey;
        }

        private void MyPreviewKeyUp(object sender, KeyEventArgs e)
        {
            SelectedKey = e.Key;
            DialogResult = true;
            Close();
            e.Handled = true;
        }
    }
}
