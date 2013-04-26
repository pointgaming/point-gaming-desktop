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
    public partial class LauncherEditorDialog : Window
    {
        private LauncherInfo _launcher = new LauncherInfo("Notepad", "C:\\Windows\\notepad.exe", "C:\\HelloWorld.txt");
        public LauncherInfo Launcher
        {
            get { return _launcher; }
            set
            {
                _launcher.CopyFrom(value);
            }
        }

        public LauncherEditorDialog()
        {
            InitializeComponent();
        }

        private void buttonSelectExecutable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selector = new Microsoft.Win32.OpenFileDialog();
                selector.Filter = "Executables (.exe)|*.exe";
                selector.Multiselect = false;
                selector.DereferenceLinks = true;
                selector.Title = "Chose Executable to Launch";
                selector.CheckFileExists = true;
                selector.CheckPathExists = true;
                var result = true == selector.ShowDialog();
                if (result)
                {
                    if (!System.IO.File.Exists(selector.FileName))
                        MessageDialog.Show(HomeWindow.Home, "Failed to Select Executable", "File doesn't exist!");
                    else
                        _launcher.FilePath = selector.FileName;
                }
            }
            catch (Exception ee)
            {
                MessageDialog.Show(HomeWindow.Home, "Failed to Select Executable", ee.Message);
            }
        }

        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            _launcher.Launch();
        }

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
    }
}
