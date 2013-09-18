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
using PointGaming.Audio;
using PointGaming.AudioChat;
using NA = NAudio;

namespace PointGaming.Chat
{
    public partial class PrivateChatWindow : Window, IWeakEventListener, INotifyPropertyChanged
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

        public PrivateChatWindow()
        {
            InitializeComponent();
            flowDocumentLog.Document = new FlowDocument();
            UpdateChatFont();
            _autoScroller = new AutoScroller(flowDocumentLog);
            PropertyChangedEventManager.AddListener(Properties.Settings.Default, this, "PropertyChanged");
            WindowTreeManager = new WindowTreeManager(this, HomeWindow.Home.WindowTreeManager);
            _userData.AudioSystem.AudioStarted += AudioSystem_AudioStarted;
            _userData.AudioSystem.AudioStopped += AudioSystem_AudioStopped;
            _userData.AudioSystem.SpeakingRoomChanged += AudioSystem_SpeakingRoomChanged;
        }

        private bool _IsSelfSpeaking;
        public bool IsSelfSpeaking
        {
            get { return _IsSelfSpeaking; }
            set
            {
                if (value == _IsSelfSpeaking)
                    return;
                _IsSelfSpeaking = value;
                NotifyChanged("IsSelfSpeaking");
            }
        }
        private bool _IsOtherSpeaking;
        public bool IsOtherSpeaking
        {
            get { return _IsOtherSpeaking; }
            set
            {
                if (value == _IsOtherSpeaking)
                    return;
                _IsOtherSpeaking = value;
                NotifyChanged("IsOtherSpeaking");
            }
        }

        private void AudioSystem_AudioStopped(PgUser obj, string roomId)
        {
            if (roomId != AudioRoomName)
                return;
            if (obj == _otherUser)
                IsOtherSpeaking = false;
            else
                IsSelfSpeaking = false;
        }

        private void AudioSystem_AudioStarted(PgUser obj, string roomId)
        {
            if (roomId != AudioRoomName)
                return;
            if (obj == _otherUser)
                IsOtherSpeaking = true;
            else
                IsSelfSpeaking = true;
        }

        private void AudioSystem_SpeakingRoomChanged(string obj)
        {
            if (obj != AudioRoomName)
                checkboxSpeak.IsChecked = false;
        }
        
        public void Init(PrivateChatSession session, PgUser otherUser)
        {
            _session = session;
            _otherUser = otherUser;
            PropertyChangedEventManager.AddListener(_otherUser, this, "PropertyChanged");
            Title = otherUser.Username;

            _session.ChatMessageReceived += ChatMessages_CollectionChanged;
            _session.SendMessageFailed += MessageSendFailed;
        }

        void ChatMessages_CollectionChanged(ChatMessage item)
        {
            bool allFromSelf = true;
            
            var user = item.Author;
            var message = item.Message;

            AppendUserMessage(user, message);
            if (item.Author != _userData.User)
                allFromSelf = false;
            
            if (!allFromSelf)
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

        private void Window_Activated(object sender, EventArgs e)
        {
            this.StopFlashingWindow();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _session.SendMessageFailed -= MessageSendFailed;
            _session.Leave();
            _userData.AudioSystem.UnsetSpeakingRoomId(AudioRoomName);
            _userData.AudioSystem.LeaveRoom(AudioRoomName);
        }

        public void MessageSendFailed(string message)
        {
            MessageDialog.Show(this, "Failed to Send Message", "Failed to send message.  User is not online or doesn't exist.");
        }

        private string AudioRoomName
        {
            get
            {
                var ids = new string[] { _userData.User.Id, _otherUser.Id };
                Array.Sort(ids);
                return ids[0] + " " + ids[1];
            }
        }

        private void checkboxSpeak_Checked(object sender, RoutedEventArgs e)
        {
            if (checkboxListen.IsChecked != true)
                checkboxListen.IsChecked = true;// todo verify that this causes checkboxListen_Checked to be called
            _userData.AudioSystem.SpeakingIntoRoomId = AudioRoomName;
        }

        private void checkboxSpeak_Unchecked(object sender, RoutedEventArgs e)
        {
            _userData.AudioSystem.UnsetSpeakingRoomId(AudioRoomName);
        }

        private void checkboxListen_Checked(object sender, RoutedEventArgs e)
        {
            _userData.AudioSystem.JoinRoom(AudioRoomName);
        }

        private void checkboxListen_Unchecked(object sender, RoutedEventArgs e)
        {
            if (checkboxSpeak.IsChecked == true)
                checkboxSpeak.IsChecked = false;// todo verify that this causes checkboxSpeak_Unchecked to be called
            _userData.AudioSystem.LeaveRoom(AudioRoomName);
        }
    }
}
