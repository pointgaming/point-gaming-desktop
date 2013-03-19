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
using PointGaming.Desktop.HomeTab;
using PointGaming.Desktop.POCO;
using PointGaming.Desktop.Chat;

namespace PointGaming.Desktop.Lobby
{
    public partial class LobbyTab : UserControl, IWeakEventListener, IChatroomTab
    {
        private ChatWindow _chatWindow;
        private LobbySession _lobbySession;
        private UserDataManager _userData = HomeWindow.UserData;
        private AutoScroller _autoScroller;

        public string Id { get { return _lobbySession.ChatroomId; } }

        public LobbyTab()
        {
            InitializeComponent();
            flowDocumentLog.Document = new FlowDocument();
            UpdateChatFont();
            _autoScroller = new AutoScroller(flowDocumentLog);
            PropertyChangedEventManager.AddListener(Properties.Settings.Default, this, "PropertyChanged");
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

        public void Init(ChatWindow window, ChatroomSession lobbySession)
        {
            _chatWindow = window;
            _lobbySession = (LobbySession)lobbySession;
            _lobbySession.ReceivedMessage += ReceivedMessage;
            listBoxMembership.ItemsSource = _lobbySession.Membership;
            itemsControlGameRoomList.ItemsSource = _lobbySession.AllGameRooms;
            itemsControlJoinedGameRoomList.ItemsSource = _lobbySession.JoinedGameRooms;
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

            _lobbySession.SendMessage(send);
        }

        private void ReceivedMessage(UserBase fromUser, string message)
        {
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
                _lobbySession.Invite(anotherUser);
                e.Handled = true;
            }
        }
        #endregion

        private void GameRoomPanel_JoinClick(object sender, RoutedEventArgs e)
        {
            GameRoomItem item;
            if (((DependencyObject)sender).TryGetPresentedParent(out item))
            {
                if (item.Id == null)
                {
                    _lobbySession.CreateRoomAt(item.Position, "Hello world!", OnMyRoomCreated);
                }
                else
                    _userData.JoinChat(Chat.ChatManager.PrefixGameRoom + item.Id);
            }
        }

        private void OnMyRoomCreated(GameRoomItem room)
        {
            _userData.JoinChat(Chat.ChatManager.PrefixGameRoom + room.Id);
        }

        private void GameRoomPanel_InfoClick(object sender, RoutedEventArgs e)
        {
            GameRoomItem item;
            if (((DependencyObject)sender).TryGetPresentedParent(out item))
                MessageDialog.Show(_userData.GetChatWindow(), "Info", "TODO: Information goes here.  GameRoom Id = " + item.Id);
        }

        private GameRoomItem _joinedItemMouseOver;
        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            GameRoomItem item;
            if (((DependencyObject)sender).TryGetPresentedParent(out item))
                _joinedItemMouseOver = item;
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            _joinedItemMouseOver = null;
        }

        private void itemsControlJoinedGameRoomList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _userData.JoinChat(Chat.ChatManager.PrefixGameRoom + _joinedItemMouseOver.Id);
            _joinedItemMouseOver = null;
        }
    }
}
