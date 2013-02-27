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
using PointGaming.Desktop.HomeTab;
using PointGaming.Desktop.POCO;

namespace PointGaming.Desktop.Chat
{
    public partial class ChatTab : UserControl
    {
        private ChatWindow _chatWindow;
        public PgUser OtherUser;

        public ChatTab()
        {
            InitializeComponent();
            richTextBoxLog.Document.Blocks.Clear();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer s = richTextBoxLog.FindDescendant<ScrollViewer>();
            if (s != null)
            {
                s.ScrollChanged += ScrollChanged;
            }
            textBoxInput.Focus();
        }

        private bool _isAtEnd = true;
        void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _isAtEnd = e.ExtentHeight - (e.VerticalOffset + e.ViewportHeight) <= 1.0;
        }

        public void Init(ChatWindow window, PgUser otherUser)
        {
            _chatWindow = window;
            OtherUser = otherUser;
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
            var message = textBoxInput.Text;
            message = message.Trim();
            textBoxInput.Text = "";
            if (message == "")
                return;

            if (message.Length > 1024)
            {
                message = message.Substring(0, 1024);
                message = message.Trim();
            }

            AppendUserMessage(HomeWindow.UserDataManager.User.Username, message);
            var privateMessage = new PrivateMessage{ user_id = OtherUser.Id, message = message };
            _chatWindow.SendMessage(privateMessage);
        }

        public void MessageReceived(PrivateMessage message)
        {
            _chatWindow.StartFlashingTab(OtherUser.Id);
            AppendUserMessage(OtherUser.Username, message.message);
        }

        private void AppendUserMessage(string username, string message)
        {
            var time = DateTime.Now;

            string timeString = time.ToString("HH:mm");

            bool isAtEnd = _isAtEnd;

            var p = new Paragraph();
            p.Inlines.Add(new Run(timeString + " "));
            p.Inlines.Add(new Bold(new Run(username + ": ")));
            p.Inlines.Add(new Run(message));
            richTextBoxLog.Document.Blocks.Add(p);

            if (isAtEnd)
                richTextBoxLog.ScrollToEnd();
        }

        private void AppendLine(string line)
        {
            bool isAtEnd = _isAtEnd;

            var p = new Paragraph();
            p.Inlines.Add(new Run(line));
            richTextBoxLog.Document.Blocks.Add(p);

            if (isAtEnd)
                richTextBoxLog.ScrollToEnd();
        }
    }
}
