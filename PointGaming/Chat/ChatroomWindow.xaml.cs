using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using PointGaming.HomeTab;
using PointGaming.POCO;

namespace PointGaming.Chat
{
    public partial class ChatroomWindow : Window, IWeakEventListener
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

        private ChatroomSession _chatroomSession;
        private UserDataManager _userData = HomeWindow.UserData;
        private AutoScroller _autoScroller;

        public ChatroomWindow()
        {
            InitializeComponent();
            flowDocumentLog.Document = new FlowDocument();
            UpdateChatFont();
            _autoScroller = new AutoScroller(flowDocumentLog);
            PropertyChangedEventManager.AddListener(Properties.Settings.Default, this, "PropertyChanged");
            WindowTreeManager = new WindowTreeManager(this, HomeWindow.Home.WindowTreeManager);
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            this.BeginInvokeUI(UpdateChatFont);
            return true;
        }

        private void UpdateChatFont()
        {
            flowDocumentLog.Document.Background = Brushes.White;
            flowDocumentLog.Document.PagePadding = new Thickness(2);
            flowDocumentLog.Document.FontFamily = new FontFamily(Properties.Settings.Default.ChatFontFamily + ", " + flowDocumentLog.Document.FontFamily);
            flowDocumentLog.Document.FontSize = Properties.Settings.Default.ChatFontSize;
        }

        public void Init(ChatroomSession roomManager)
        {
            _chatroomSession = roomManager;
            listBoxMembership.ItemsSource = _chatroomSession.Membership;
            Title = roomManager.ChatroomId;
            _chatroomSession.ChatMessages.CollectionChanged += ChatMessages_CollectionChanged;
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

            _chatroomSession.SendMessage(send);
        }


        void ChatMessages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            bool allFromSelf = true;
            foreach (ChatMessage item in e.NewItems)
            {
                AppendUserMessage(item.Author.Username, item.Message);
                if (item.Author != _userData.User)
                    allFromSelf = false;
            }
            if (!allFromSelf)
                this.FlashWindowSmartly();
        }
        
        private void AppendUserMessage(string username, string message)
        {
            var time = DateTime.Now;

            string timeString = time.ToString("HH:mm");

            _autoScroller.PreAppend();

            var p = new Paragraph();
            p.Inlines.Add(new Run(timeString + " "));
            p.Inlines.Add(new Bold(new Run(username + ": ")));
            ChatCommon.Format(message, p.Inlines);
            flowDocumentLog.Document.Blocks.Add(p);

            _autoScroller.PostAppend();
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item;
            if (((DependencyObject)sender).TryGetParent(out item))
            {
                var pgUser = (PgUser)item.DataContext;
                _userData.ChatWith(pgUser);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _chatroomSession.Leave();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            this.StopFlashingWindow();
        }
    }
}
