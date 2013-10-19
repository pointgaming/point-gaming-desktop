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
using PointGaming.Settings;

namespace PointGaming
{
    public partial class KeySelectDialog : Window
    {
        public ControlBinding Binding;

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

        public static ControlBinding Show(Window owner, string title, string message, PermittedControlBinding permits)
        {
            var md = new KeySelectDialog
            {
                Owner = owner,
                Title = title,
                Message = message,
            };
            if (permits.HasFlag(PermittedControlBinding.KeyboardKeys))
                md.PreviewKeyUp += md.MyPreviewKeyUp;
            if (permits.HasFlag(PermittedControlBinding.MouseButtons))
                md.textBoxMessage.PreviewMouseUp += md.textBoxMessage_PreviewMouseUp;
            md.ShowDialog();
            return md.Binding;
        }

        void textBoxMessage_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Binding = new ControlBinding { MButton = e.ChangedButton, };
            DialogResult = true;
            Close();
            e.Handled = true;
        }


        private void MyPreviewKeyUp(object sender, KeyEventArgs e)
        {
            Binding = new ControlBinding { KeyboardKey = e.Key, };
            DialogResult = true;
            Close();
            e.Handled = true;
        }
    }
}
