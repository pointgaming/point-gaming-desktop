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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PointGaming.AudioChat
{
    public partial class MainWindow : Window
    {
        private AudioChatServer _server;

        public MainWindow()
        {
            InitializeComponent();
            textBoxPort.Text = "" + AudioChatServer.DefaultPort;
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (_server != null)
                return;

            int port;
            if (!int.TryParse(textBoxPort.Text, out port))
                return;
            _server = new AudioChatServer(port);
            _server.Stopped += _server_Stopped;
            _server.MessageReceived += _server_MessageReceived;
            checkBoxRunning.IsChecked = true;
            _server.Start();
        }

        void _server_MessageReceived(int obj)
        {
            this.Dispatcher.BeginInvoke((Action)delegate {
                textBoxHandledMessage.Text = DateTime.Now + " " + obj;
            }, null);
        }

        void _server_Stopped(AudioChatServer obj)
        {
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                checkBoxRunning.IsChecked = false;
                _server = null;
            }, null);
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null)
                return;

            _server.Stop();
            _server = null;
        }
    }
}
