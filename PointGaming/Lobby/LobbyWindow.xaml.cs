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
using PointGaming.Chat;

namespace PointGaming.Lobby
{
    public partial class LobbyWindow : Window, IWeakEventListener
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

        private LobbySession _lobbySession;
        private UserDataManager _userData = HomeWindow.UserData;
        private AutoScroller _autoScroller;
        private ObservableCollection<PgUser> _groupedUsers;

        public LobbyWindow()
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
            if (sender == Properties.Settings.Default)
            {
                this.BeginInvokeUI(UpdateChatFont);
                return true;
            }
            else if (sender == _lobbySession.GameInfo)
            {
                if (((PropertyChangedEventArgs)e).PropertyName == "DisplayName")
                    NotifyChanged("Header");
                return true;
            }
            return false;
        }

        public bool CanAdminUser
        {
            get { return _userData.User.IsAdmin; }
        }

        private void UpdateChatFont()
        {
            flowDocumentLog.Document.Background = Brushes.White;
            flowDocumentLog.Document.PagePadding = new Thickness(2);
            flowDocumentLog.Document.FontFamily = new FontFamily(Properties.Settings.Default.ChatFontFamily + ", " + flowDocumentLog.Document.FontFamily);
            flowDocumentLog.Document.FontSize = Properties.Settings.Default.ChatFontSize;
        }

        public void Init(LobbySession lobbySession)
        {
            _lobbySession = lobbySession;
            Title = _lobbySession.GameInfo.DisplayName;

            InitGroupedMembers( _lobbySession );
            itemsControlGameRoomList.ItemsSource = _lobbySession.AllGameRooms;
            itemsControlJoinedGameRoomList.ItemsSource = _lobbySession.JoinedGameRooms;

            PropertyChangedEventManager.AddListener(_lobbySession.GameInfo, this, "PropertyChanged");

            _userData.LookupPendingBets(OnPendingBetsComplete);

            _lobbySession.ChatMessages.CollectionChanged += ChatMessages_CollectionChanged;
        }

        void ChatMessages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (ChatMessage item in e.NewItems)
                AppendUserMessage(item.Author.Username, item.Message);
        }

        private void InitGroupedMembers(ChatroomSessionBase lobbySession)
        {
            _groupedUsers = new ObservableCollection<PgUser>();
            
            // players group
            foreach (PgUser user in lobbySession.Membership)
            {
                PgUser groupedUser = user.ShallowCopy();
                groupedUser.GroupName = "Players";
                _groupedUsers.Add(groupedUser);
            }

            // admin group
            foreach (PgUser user in lobbySession.Membership)
            {
                if (!user.IsAdmin) continue;

                PgUser groupedUser = user.ShallowCopy();
                groupedUser.GroupName = "Admin";
                _groupedUsers.Add(groupedUser);
            }

            // friends group
            foreach (PgUser user in lobbySession.Membership)
            {
                if (!_userData.IsFriend(user.Id)) continue;

                PgUser groupedUser = user.ShallowCopy();
                groupedUser.GroupName = "Friends";
                _groupedUsers.Add(groupedUser);
            }

            System.ComponentModel.ICollectionView mv = CollectionViewSource.GetDefaultView(_groupedUsers);
            mv.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
            listBoxMembership.DataContext = mv;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxInput.Focus();
        }

        private void buttonSendInput_Click(object sender, RoutedEventArgs e)
        {
            SendInput();
        }

        private void userContextMenuTaunt_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog.Show(this, "Taunt User", "TODO: show taunt selection UI");
        }

        private void userContextMenuBlock_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog.Show(this, "Block User", "TODO: ???");
        }

        private void userContextMenuMessage_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                PgUser user = menuItem.CommandParameter as PgUser;
                if (user != null)
                {
                    _userData.ChatWith(user);
                }
            } 
        }

        private void userContextMenuViewProfile_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                PgUser user = menuItem.CommandParameter as PgUser;
                if (user != null)
                {
                    var url = Properties.Settings.Default.WebServerUrl + "/u/" + user.Username + "/profile";
                    System.Diagnostics.Process.Start(url);
                }
            } 
        }

        private void userContextMenuFriendRequest_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                PgUser user = menuItem.CommandParameter as PgUser;
                if (user != null)
                {
                    _userData.Friendship.RequestFriend(user.Username, OnRequestFriend);
                }
            } 
        }

        private void reportMatchWinnerButton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog.Show(this, "Report Match Winner", "TODO: get list of pending user bets from REST API");
        }

        private void OnRequestFriend(string errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                this.BeginInvokeUI(delegate
                {
                    MessageDialog.Show(HomeWindow.Home, "Request Failed", errorMessage);
                });
            }
        }

        private void OnPendingBetsComplete(List<POCO.BetPoco> pocos)
        {
            // TODO: open dialog showing all pending bets for user
            reportMatchWinnerButton.Visibility = System.Windows.Visibility.Visible;
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

            _lobbySession.SendMessage(send);
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
                    _lobbySession.CreateRoomAt(item.Position, "New game room", OnMyRoomCreated);
                }
                else
                {
                    _userData.JoinChat(SessionManager.PrefixGameRoom + item.Id);
                }
            }
        }

        private void GameRoomPanel_TakeoverClick(object sender, RoutedEventArgs e)
        {
            GameRoomItem item;
            if (((DependencyObject)sender).TryGetPresentedParent(out item))
            {
                _lobbySession.CreateRoomAt(item.Position, "New game room", OnMyRoomCreated, true);
            }
        }

        private void OnMyRoomCreated(string id)
        {
            _userData.JoinChat(SessionManager.PrefixGameRoom + id);
        }

        private void GameRoomPanel_InfoClick(object sender, RoutedEventArgs e)
        {
            GameRoomItem item;
            if (((DependencyObject)sender).TryGetPresentedParent(out item))
            {
                GameRoomInfoDialog.Show(item, _userData.PgSession);
            }
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
            if (_joinedItemMouseOver == null)
                return;
            _userData.JoinChat(SessionManager.PrefixGameRoom + _joinedItemMouseOver.Id);
            _joinedItemMouseOver = null;
        }

        private void lobbyTab_Closing(object sender, CancelEventArgs e)
        {
            _lobbySession.Leave();
        }
    }
}
