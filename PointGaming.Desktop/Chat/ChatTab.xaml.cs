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
        private PgUser _otherUser;
        private SocketSession _session = HomeWindow.Home.SocketSession;
        private AutoScroller _autoScroller;

        public ChatTab()
        {
            InitializeComponent();
            richTextBoxLog.Document = new FlowDocument();
            UpdateChatFont();
            _autoScroller = new AutoScroller(richTextBoxLog);
            Properties.Settings.Default.SettingsSaving += Default_SettingsSaving;
        }

        void Default_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UpdateChatFont();
        }

        private void UpdateChatFont()
        {
            richTextBoxLog.Document.Background = Brushes.White;
            richTextBoxLog.Document.PagePadding = new Thickness(2);
            richTextBoxLog.Document.FontFamily = new FontFamily(Properties.Settings.Default.ChatFontFamily + ", " + richTextBoxLog.Document.FontFamily);
            richTextBoxLog.Document.FontSize = Properties.Settings.Default.ChatFontSize;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxInput.Focus();
        }
        
        public void Init(ChatWindow window, PgUser otherUser)
        {
            _chatWindow = window;
            _otherUser = otherUser;
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
            if (!ChatTabCommon.FilterMessage(textBoxInput.Text, out send, out remain))
                return;
            textBoxInput.Text = remain;

            AppendUserMessage(_session.Data.User.Username, send);
            var privateMessage = new PrivateMessage{ user_id = _otherUser.Id, message = send };
            _chatWindow.SendMessage(privateMessage);
        }

        public void MessageReceived(PrivateMessage message)
        {
            _chatWindow.StartFlashingTab(this.GetType(), _otherUser.Id);
            AppendUserMessage(_otherUser.Username, message.message);
        }

        private void AppendUserMessage(string username, string message)
        {
            var time = DateTime.Now;

            string timeString = time.ToString("HH:mm");

            _autoScroller.PreAppend();

            var p = new Paragraph();
            p.Inlines.Add(new Run(timeString + " "));
            p.Inlines.Add(new Bold(new Run(username + ": ")));
            ChatTabCommon.Format(message, p.Inlines);
            richTextBoxLog.Document.Blocks.Add(p);

            _autoScroller.PostAppend();
        }

        #region drag & drop sink
        private bool CanHandleDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(PgUser).FullName))
            {
                PgUser anotherUser = e.Data.GetData(typeof(PgUser).FullName) as PgUser;
                if (anotherUser.Id != _otherUser.Id)
                    return true;
            }
            return false;
        }

        private void TabPreviewDragQuery(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (CanHandleDrop(e))
            {
                e.Effects = DragDropEffects.Link;
                e.Handled = true;
            }
        }
        private void TabPreviewDrop(object sender, DragEventArgs e)
        {
            if (!CanHandleDrop(e))
                return;

            if (e.Data.GetDataPresent(typeof(PgUser).FullName))
            {
                PgUser anotherUser = e.Data.GetData(typeof(PgUser).FullName) as PgUser;
                _chatWindow.CreateChatroomWith(_otherUser, anotherUser);
                e.Handled = true;
            }
        }
        #endregion
    }
}
