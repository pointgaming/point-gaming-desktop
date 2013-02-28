﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class ChatroomTab : UserControl
    {
        private ChatWindow _chatWindow;
        private ChatManager.ChatroomUsage _roomManager;
        private SocketSession _session = HomeWindow.Home.SocketSession;
        private AutoScroller _autoScroller;

        public ChatroomTab()
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

        public void Init(ChatWindow window, ChatManager.ChatroomUsage roomManager)
        {
            _chatWindow = window;
            _roomManager = roomManager;
            _roomManager.ReceivedMessage += ReceivedMessage;
            listBoxMembership.ItemsSource = _roomManager.Membership;
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
            if (!ChatTabCommon.FilterMessage(textBoxInput.Text, out send, out remain))
                return;
            textBoxInput.Text = remain;

            AppendUserMessage(_session.Data.User.Username, send);
            _roomManager.SendMessage(send);
        }

        private void ReceivedMessage(UserBase fromUser, string message)
        {
            _chatWindow.StartFlashingTab(this.GetType(), _roomManager.ChatroomId);
            AppendUserMessage(fromUser.username, message);
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

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item;
            if (((DependencyObject)sender).TryGetParent(out item))
            {
                var pgUser = (PgUser)item.DataContext;
                HomeWindow.Home.ChatWith(pgUser);
            }
        }

        #region drag & drop source
        private Point _previewMouseLeftButtonDownPosition;
        private ListBoxItem _previewMouseLeftButtonDownItem;
        private PgUser _previewMouseLeftButtonDownUser;
        private bool _previewMouseLeftButtonDownCaptured;
        private DataObject _previewMouseLeftButtonDownDragData;
        private void listBoxMembership_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ResetDrag();
            ListBoxItem item;
            PgUser pgUser;
            if (listBoxMembership.TryGetItemAndItem(e, out item, out pgUser))
            {
                _previewMouseLeftButtonDownItem = item;
                _previewMouseLeftButtonDownUser = pgUser;
                _previewMouseLeftButtonDownPosition = e.GetPosition(null);
                _previewMouseLeftButtonDownCaptured = listBoxMembership.CaptureMouse();
            }
        }

        private void listBoxMembership_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResetDrag();
        }

        private void ResetDrag()
        {
            _previewMouseLeftButtonDownCaptured = false;
            listBoxMembership.ReleaseMouseCapture();
            _previewMouseLeftButtonDownDragData = null;
        }

        private void listBoxMembership_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_previewMouseLeftButtonDownCaptured || _previewMouseLeftButtonDownDragData != null)
                return;

            Vector diff = _previewMouseLeftButtonDownPosition - e.GetPosition(null);

            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            _previewMouseLeftButtonDownDragData = new DataObject(typeof(PgUser).FullName, _previewMouseLeftButtonDownUser);
            DragDrop.DoDragDrop(_previewMouseLeftButtonDownItem, _previewMouseLeftButtonDownDragData, DragDropEffects.Link);
        }
        #endregion

        #region drag & drop sink
        private void TabPreviewDragQuery(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (CanHandleDrop(e))
            {
                e.Effects = DragDropEffects.Link;
                e.Handled = true;
            }
        }
        private bool CanHandleDrop(DragEventArgs e)
        {
            return e.Data.GetDataPresent(typeof(PgUser).FullName) && _previewMouseLeftButtonDownDragData == null;
        }
        private void TabPreviewDrop(object sender, DragEventArgs e)
        {
            if (!CanHandleDrop(e))
                return;

            if (e.Data.GetDataPresent(typeof(PgUser).FullName))
            {
                PgUser anotherUser = e.Data.GetData(typeof(PgUser).FullName) as PgUser;
                _roomManager.Invite(anotherUser);
                e.Handled = true;
            }
        }
        #endregion
    }
}