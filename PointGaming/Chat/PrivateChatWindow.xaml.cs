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

namespace PointGaming.Chat
{
    public partial class PrivateChatWindow : Window, IWeakEventListener, INotifyPropertyChanged, Voice.IVoiceRoom
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
        private UserDataManager _userData = UserDataManager.UserData;
        private AutoScroller _autoScroller;

        private PrivateChatSession _session;

        public PrivateChatWindow()
        {
            InitializeComponent();
            flowDocumentLog.Document = new FlowDocument();
            UpdateChatFont();
            _autoScroller = new AutoScroller(flowDocumentLog);
            PropertyChangedEventManager.AddListener(UserDataManager.UserData.Settings, this, "PropertyChanged");
            WindowTreeManager = new WindowTreeManager(this, HomeWindow.Home.WindowTreeManager);
            _userData.AudioSystem.JoinRoom(this);
        }

        public bool IsVoiceTeamOnly { get { return false; } }
        public bool IsVoiceEnabled { get { return checkboxMute.IsChecked == false; } }

        public void OnVoiceStarted(PgUser user)
        {
            if (user == _otherUser)
                IsOtherSpeaking = true;
            else
                IsSelfSpeaking = true;
        }
        public void OnVoiceStopped(PgUser user)
        {
            if (user == _otherUser)
                IsOtherSpeaking = false;
            else
                IsSelfSpeaking = false;
        }

        public void OnVoiceConnectionChanged(bool isConnected)
        {
            IsVoiceConnected = isConnected;
        }

        private bool _IsVoiceConnected = false;
        public bool IsVoiceConnected
        {
            get { return _IsVoiceConnected; }
            set
            {
                if (_IsVoiceConnected == value)
                    return;
                _IsVoiceConnected = value;
                NotifyChanged("IsVoiceConnected");
            }
        }

        public string AudioRoomId
        {
            get
            {
                var ids = new string[] { _userData.User.Id, _otherUser.Id };
                Array.Sort(ids);
                return ids[0] + " " + ids[1];
            }
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
            if (sender == UserDataManager.UserData.Settings)
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
            flowDocumentLog.Document.FontFamily = new FontFamily(UserDataManager.UserData.Settings.ChatFontFamily + ", " + flowDocumentLog.Document.FontFamily);
            flowDocumentLog.Document.FontSize = UserDataManager.UserData.Settings.ChatFontSize;
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
            _userData.AudioSystem.TakeMicrophoneFocus(this);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _session.SendMessageFailed -= MessageSendFailed;
            _session.Leave();
            _userData.AudioSystem.LeaveRoom(this);
        }

        public void MessageSendFailed(string message)
        {
            MessageDialog.Show(this, "Failed to Send Message", "Failed to send message.  User is not online or doesn't exist.");
        }
    }
}
