using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PointGaming.HomeTab;
using PointGaming.POCO;
using PointGaming.NAudio;
using NA = NAudio;

namespace PointGaming.Chat
{
    public partial class PrivateChatWindow : Window, IWeakEventListener
    {
        public WindowTreeManager WindowTreeManager;
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged(string propertyName)
        {
            var changedCallback = PropertyChanged;
            if (changedCallback == null)
                return;
            var args = new PropertyChangedEventArgs(propertyName);
            changedCallback(this, args);
        }
        private PgUser _otherUser;
        private UserDataManager _userData = HomeWindow.UserData;
        private AutoScroller _autoScroller;

        private PrivateChatSession _session;
        private NAudioTest _nAudioTest;

        public PrivateChatWindow()
        {
            InitializeComponent();
            flowDocumentLog.Document = new FlowDocument();
            UpdateChatFont();
            _autoScroller = new AutoScroller(flowDocumentLog);
            PropertyChangedEventManager.AddListener(Properties.Settings.Default, this, "PropertyChanged");
            WindowTreeManager = new WindowTreeManager(this, HomeWindow.Home.WindowTreeManager);

            _nAudioTest = new NAudioTest(new WideBandSpeexCodec());
            _nAudioTest.TriggerKey = (System.Windows.Forms.Keys)Properties.Settings.Default.MicTriggerKey;
            _nAudioTest.AudioRecorded += _nAudioTest_AudioRecorded;
        }


        void _nAudioTest_AudioRecorded(NAudioTest source, byte[] data, bool isLast)
        {
            if (data.Length > 0)
            {
                var b64 = Convert.ToBase64String(data);
                _session.SendMessage("speex_" + b64);
            }
            if (isLast)
                _session.SendMessage("speex_=");
        }

        public void Init(PrivateChatSession session, PgUser otherUser)
        {
            _session = session;
            _otherUser = otherUser;
            PropertyChangedEventManager.AddListener(_otherUser, this, "PropertyChanged");
            Title = otherUser.Username;

            _session.ChatMessageReceived += ChatMessages_CollectionChanged;
            _session.SendMessageFailed += MessageSendFailed;

            for (int n = 0; n < NA.Wave.WaveIn.DeviceCount; n++)
            {
                var capabilities = NA.Wave.WaveIn.GetCapabilities(n);
                comboBoxRecordingDevices.Items.Add(capabilities.ProductName);
            }
            comboBoxRecordingDevices.SelectedIndex = 0;
        }

        void ChatMessages_CollectionChanged(ChatMessage item)
        {
            bool hasText = false;
            bool allFromSelf = true;
            
            var user = item.Author;
            var message = item.Message;

            if (message.StartsWith("speex_"))
            {
                HandleAudio(user, message);
            }
            else
            {
                AppendUserMessage(user, message);
                if (item.Author != _userData.User)
                    allFromSelf = false;
                hasText = true;
            }
            
            if (hasText && !allFromSelf)
                this.FlashWindowSmartly();
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (sender == Properties.Settings.Default)
            {
                this.BeginInvokeUI(UpdateChatFont);
                return true;
            }
            else if (sender == _otherUser)
            {
                if (((PropertyChangedEventArgs)e).PropertyName == "Username")
                    Title = _otherUser.Username;
                return true;
            }
            return false;
        }
        
        private void UpdateChatFont()
        {
            flowDocumentLog.Document.Background = Brushes.White;
            flowDocumentLog.Document.PagePadding = new Thickness(2);
            flowDocumentLog.Document.FontFamily = new FontFamily(Properties.Settings.Default.ChatFontFamily + ", " + flowDocumentLog.Document.FontFamily);
            flowDocumentLog.Document.FontSize = Properties.Settings.Default.ChatFontSize;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxInput.Focus();
        }
        
        private void buttonSendInput_Click(object sender, RoutedEventArgs e)
        {
            SendInput();
        }

        private void textBoxInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isShiftDown = e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift);

            if (e.Key == Key.Enter && !isShiftDown)
            {
                SendInput();
                e.Handled = true;
                return;
            }
        }

        private void SendInput()
        {
            string send, remain;
            if (!ChatCommon.FilterMessage(textBoxInput.Text, out send, out remain))
                return;
            textBoxInput.Text = remain;

            _session.SendMessage(send);
        }
        
        private void AppendUserMessage(PgUser user, string message)
        {
            var time = DateTime.Now;

            string timeString = time.ToString("HH:mm");

            _autoScroller.PreAppend();

            var p = new Paragraph();
            p.Inlines.Add(new Run(timeString + " "));
            p.Inlines.Add(new Bold(new Run(user.Username + ": ")));
            ChatCommon.Format(message, p.Inlines);
            flowDocumentLog.Document.Blocks.Add(p);

            _autoScroller.PostAppend();
        }

        private void HandleAudio(PgUser user, string message)
        {
            message = message.Substring("speex_".Length);
            bool isEnd = message == "=";

            if (user == _userData.User)
            {
                if (isEnd)
                    radioButtonTransmit.IsChecked = false;
                else
                    radioButtonTransmit.IsChecked = true;
            }
            else
            {
                if (isEnd)
                    radioButtonReceive.IsChecked = false;
                else
                {
                    radioButtonReceive.IsChecked = true;
                    var data = Convert.FromBase64String(message);
                    _nAudioTest.AudioReceived(data);
                }
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            this.StopFlashingWindow();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _session.SendMessageFailed -= MessageSendFailed;
            _session.Leave();
            _nAudioTest.Dispose();
        }

        public void MessageSendFailed(string message)
        {
            MessageDialog.Show(this, "Failed to Send Message", "Failed to send message.  User is not online or doesn't exist.");
        }

        private void checkboxEnableMic_Checked(object sender, RoutedEventArgs e)
        {
            _nAudioTest.InputDeviceNumber = comboBoxRecordingDevices.SelectedIndex;
            _nAudioTest.Enable();
        }

        private void checkboxEnableMic_Unchecked(object sender, RoutedEventArgs e)
        {
            _nAudioTest.Disable();
        }

        private void comboBoxRecordingDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(checkboxEnableMic.IsChecked == true))
                return;

            _nAudioTest.Disable();
            _nAudioTest.InputDeviceNumber = comboBoxRecordingDevices.SelectedIndex;
            _nAudioTest.Enable();
        }

        private void buttonSetMicKey_Click(object sender, RoutedEventArgs e)
        {
            App.KeyDown += App_KeyDown;
            MessageDialog.Show(this, "Press Key", "Press the key you want to trigger the microphone. Then click OK.");
            Properties.Settings.Default.MicTriggerKey = (int)_lastKeyDown;
            Properties.Settings.Default.Save();
            App.KeyDown -= App_KeyDown;
            _nAudioTest.TriggerKey = (System.Windows.Forms.Keys)Properties.Settings.Default.MicTriggerKey;
        }

        System.Windows.Forms.Keys _lastKeyDown;

        void App_KeyDown(System.Windows.Forms.Keys obj)
        {
            _lastKeyDown = obj;
        }
    }
}
