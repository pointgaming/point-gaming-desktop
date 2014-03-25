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

namespace PointGaming.HomeTab
{
    /// <summary>
    /// Логика взаимодействия для InputValueDialog.xaml
    /// </summary>
    public partial class InputValueDialog : Window
    {

        private string value;

        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        public InputValueDialog()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Value = ValueField.Text;
            DialogResult = true;
            this.Close();
        }
    }
}
