using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace PointGaming.Desktop.HomeTab
{
    /// <summary>
    /// Interaction logic for PaymentTab.xaml
    /// </summary>
    public partial class PaymentTab : UserControl
    {
        public PaymentTab()
        {
            InitializeComponent();
        }

        private void buttonRunTest_Click(object sender, RoutedEventArgs e)
        {
            App.LogLine("Not Implemented");
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            RunMiner();
        }

        private void RunMiner()
        {
            var dllLocation = typeof(PaymentTab).Assembly.Location;
            var dllFileInfo = new System.IO.FileInfo(dllLocation);
            var executableInfo = dllFileInfo.Directory.GetDirectories("poclbm")[0].GetFiles("poclbm.exe")[0];

            Process poclbm = new Process();
            poclbm.StartInfo.FileName = executableInfo.FullName;
            poclbm.StartInfo.Arguments = @"http://pointgaming:po!ntgam!ng@96.126.125.144:8332 --device=0 --platform=0 --verbose";
            poclbm.StartInfo.UseShellExecute = false;
            poclbm.StartInfo.RedirectStandardOutput = true;
            poclbm.Start();
            poclbm.OutputDataReceived += new DataReceivedEventHandler(OnDataReceived);
            poclbm.ErrorDataReceived += new DataReceivedEventHandler(OnDataReceived);

            App.LogLine(poclbm.StandardOutput.ReadToEnd());
            poclbm.WaitForExit();
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                string temp = (e.Data) + Environment.NewLine;
            }
        }

        private void checkBoxMineBitcoins_Checked(object sender, RoutedEventArgs e)
        {
            App.LogLine("Not Implemented");
        }
        private void checkBoxMineBitcoins_Unchecked(object sender, RoutedEventArgs e)
        {
            App.LogLine("Not Implemented");
        }

        private void checkBoxOnlyMineWhenIdle_Checked(object sender, RoutedEventArgs e)
        {
            App.LogLine("Not Implemented");
        }
        private void checkBoxOnlyMineWhenIdle_Unchecked(object sender, RoutedEventArgs e)
        {
            App.LogLine("Not Implemented");
        }

        private void checkBoxFreeProAccount_Checked(object sender, RoutedEventArgs e)
        {
            App.LogLine("Not Implemented");
        }
        private void checkBoxFreeProAccount_Unchecked(object sender, RoutedEventArgs e)
        {
            App.LogLine("Not Implemented");
        }
    }
}
