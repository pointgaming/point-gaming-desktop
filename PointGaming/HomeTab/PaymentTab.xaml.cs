using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Net;
using System.Runtime.InteropServices;

namespace PointGaming.HomeTab
{
    public partial class PaymentTab : UserControl
    {
        BitcoinMiner.StratumSession _stratumSession;
        List<BitcoinMiner.Miner> _miners;

        public PaymentTab()
        {
            InitializeComponent();
        }

        private void buttonLearnMore_Click(object sender, RoutedEventArgs e)
        {

        }

        private void hyperLinkDisclaimer_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog.Show(HomeWindow.Home, 
                "Disclaimer Notice", "The \"Free Better Account\" feature utilizes your computer's GPU to compute proof-of-work solutions to the Bitcoin block chain.  "
                + "A computer system that is not designed to handle continuous GPU usage may experience temporary and/or permanent failures.  "
                + "The GPU computations cause your GPU to consume more electricity, which may increase your electricity bill.  Power usage depends on your GPU's efficiency.  "
                + "By using this feature to work for a better account, you agree that Poing Gaming LLC has no responsibility for any damage, losses, or expenses caused by your use of this feature.");
        }

        private void checkBoxFreeBetterAccount_Checked(object sender, RoutedEventArgs e)
        {
            _stratumSession = new BitcoinMiner.StratumSession();
            _stratumSession.ConnectionConcluded += _stratumSession_ConnectionConcluded;

            var serverAddress = Properties.Settings.Default.StratumIp;
            var serverPort = Properties.Settings.Default.StratumPort;
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(serverAddress), serverPort);
            var workerName = HomeWindow.UserData.User.Username;
            var workerPassword = "";

            TimeSpan timeoutTimespan = new TimeSpan(0, 0, 1);
            _stratumSession.Connect(endpoint, workerName, workerPassword, timeoutTimespan);
        }

        private void _stratumSession_ConnectionConcluded(bool isConnected)
        {
            if (isConnected)
            {
                _miners = BitcoinMiner.OpenCLMiner.GetAvailableMiners();

                foreach (var miner in _miners)
                {
                    miner.FPSLimit = 60;
                    miner.UsageLimit = 100;
                    miner.UMLimit = 1000;
                    miner.BeginWorkFor(_stratumSession);
                }

                this.BeginInvokeUI(delegate
                {
                    SaveMinerEnabled(true);
                });
            }
            else
            {
                this.BeginInvokeUI(delegate
                {
                    checkBoxFreeBetterAccount.IsChecked = false;
                });
            }
        }

        private static void SaveMinerEnabled(bool isEnabled)
        {
            if (isEnabled != PointGaming.Properties.Settings.Default.BitcoinMinerEnabled)
            {
                PointGaming.Properties.Settings.Default.BitcoinMinerEnabled = isEnabled;
                Properties.Settings.Default.Save();
            }
        }

        private void checkBoxFreeProAccount_Unchecked(object sender, RoutedEventArgs e)
        {
            SaveMinerEnabled(false);

            if (_stratumSession == null)
                return;

            if (_miners != null)
            {
                foreach (var miner in _miners)
                    miner.IsStopRequested = true;
                _miners = null;
            }

            _stratumSession.Dispose();
            _stratumSession = null;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            bool isEnabled = PointGaming.Properties.Settings.Default.BitcoinMinerEnabled;
            checkBoxFreeBetterAccount.IsChecked = isEnabled;
        }
    }
}
