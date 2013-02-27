using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PointGaming.Desktop.HomeTab
{
    /// <summary>
    /// Interaction logic for DebugTab.xaml
    /// </summary>
    public partial class DebugTab : UserControl
    {
        public DebugTab()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            App.DebugBox = textBoxConsole;
        }

    }
}
