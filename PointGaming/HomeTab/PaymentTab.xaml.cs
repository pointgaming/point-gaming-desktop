using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using RestSharp;

namespace PointGaming.HomeTab
{
    public partial class PaymentTab : UserControl
    {
        BitcoinMiner.StratumSession _stratumSession;
        List<BitcoinMiner.Miner> _miners;
        private DispatcherTimer _reconnectTimer;
        private DispatcherTimer _progressTimer;

        public PaymentTab()
        {
            InitializeComponent();

            _reconnectTimer = new DispatcherTimer();
            _reconnectTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            _reconnectTimer.Interval = new TimeSpan(0, 5, new Random().Next(60) - 30);


            _progressTimer = new DispatcherTimer();
            _progressTimer.Interval = TimeSpan.FromMinutes(15);
            _progressTimer.Tick += _progressTimer_Tick;
            _progressTimer.Start();
            _progressTimer_Tick(null, null);
        }

        private void _progressTimer_Tick(object sender, EventArgs e)
        {
            var session = HomeWindow.UserData.PgSession;
            var url = session.GetWebApiV1Function("/better");
            RestResponse<PointGaming.POCO.BetterAccountPoco> response = null;
            session.BeginAndCallback(delegate
            {
                response = SocketSession.Rest<PointGaming.POCO.BetterAccountPoco>(url, Method.GET, null);
            }, delegate
            {
                if (!response.IsOk())
                    return;
                var status = response.Data;
                var timeSpan = TimeSpan.FromSeconds(status.seconds);
                double rolls = Math.Floor(timeSpan.TotalDays / 15.0);
                double progressDays = timeSpan.TotalDays - (15.0 * rolls);
                double progressPercent = (progressDays / 15.0) * 100.0;
                progressBar1.Value = progressPercent;
                textBlockPercentComplete.Text = string.Format("{0:0}%", progressPercent);
                textBlockProgressDays.Text = string.Format("{0:0.0} of 15 days", progressDays);
            });
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            bool isEnabled = PointGaming.Properties.Settings.Default.BitcoinMinerEnabled;
            checkBoxFreeBetterAccount.IsChecked = isEnabled;
        }

        private void checkBoxFreeBetterAccount_Checked(object sender, RoutedEventArgs e)
        {
            SaveMinerEnabled(true);
            StartMining();
        }

        private void checkBoxFreeProAccount_Unchecked(object sender, RoutedEventArgs e)
        {
            SaveMinerEnabled(false);
            StopMining();
        }

        private void StartMining()
        {
            if (_stratumSession != null)
                return;

            _stratumSession = new BitcoinMiner.StratumSession();
            _stratumSession.ConnectionConcluded += _stratumSession_ConnectionConcluded;
            _stratumSession.ConnectionFailure += _stratumSession_ConnectionFailure;

            var serverAddress = Properties.Settings.Default.StratumIp;
            var serverPort = Properties.Settings.Default.StratumPort;
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(serverAddress), serverPort);
            var workerName = HomeWindow.UserData.User.Username;
            var workerPassword = "";

            TimeSpan timeoutTimespan = new TimeSpan(0, 0, 30);
            _stratumSession.Connect(endpoint, workerName, workerPassword, timeoutTimespan);
        }

        private void StopMining()
        {
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

        private void _stratumSession_ConnectionFailure()
        {
            this.BeginInvokeUI(delegate
            {
                StopMining();
                _reconnectTimer.Start();
            });
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            _reconnectTimer.Stop();
            StartMining();
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
                    miner.BeginMining(_stratumSession);
                }
            }
            else
            {
                this.BeginInvokeUI(delegate
                {
                    StopMining();
                    _reconnectTimer.Start();
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

        public void LoggingOut()
        {
            StopMining();
            _progressTimer.Stop();
        }

        private void buttonLearnMore_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Properties.Settings.Default.WebServerUrl);
        }

        private void hyperLinkDisclaimer_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog.Show(HomeWindow.Home,
                "Disclaimer Notice", "The \"Free Better Account\" feature utilizes your computer's GPU to compute proof-of-work solutions to the Bitcoin block chain.  "
                + "A computer system that is not designed to handle continuous GPU usage may experience temporary and/or permanent failures.  "
                + "The GPU computations cause your GPU to consume more electricity, which may increase your electricity bill.  Power usage depends on your GPU's efficiency.  "
                + "By using this feature to work for a better account, you agree that Poing Gaming LLC has no responsibility for any damage, losses, or expenses caused by your use of this feature.");
        }
    }
}
