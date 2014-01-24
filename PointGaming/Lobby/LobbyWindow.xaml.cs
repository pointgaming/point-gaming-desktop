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
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PointGaming.HomeTab;
using PointGaming.POCO;
using PointGaming.Chat;

namespace PointGaming.Lobby
{
    public partial class LobbyWindow : Window, IWeakEventListener, INotifyPropertyChanged
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
        private UserDataManager _userData = UserDataManager.UserData;
        private AutoScroller _autoScroller;

        public LobbyWindow()
        {
            InitializeComponent();
            flowDocumentLog.Document = new FlowDocument();
            UpdateChatFont();
            _autoScroller = new AutoScroller(flowDocumentLog);
            PropertyChangedEventManager.AddListener(UserDataManager.UserData.Settings, this, "PropertyChanged");
            WindowTreeManager = new WindowTreeManager(this, HomeWindow.Home.WindowTreeManager);
        }

        private string _MembershipCount = "Total (0)";
        public string MembershipCount
        {
            get { return _MembershipCount; }
            set
            {
                if (value == _MembershipCount)
                    return;
                _MembershipCount = value;
                NotifyChanged("MembershipCount");
            }
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (sender == UserDataManager.UserData.Settings)
            {
                ((Action)UpdateChatFont).BeginInvokeUI();
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
            flowDocumentLog.Document.FontFamily = new FontFamily(UserDataManager.UserData.Settings.ChatFontFamily + ", " + flowDocumentLog.Document.FontFamily);
            flowDocumentLog.Document.FontSize = UserDataManager.UserData.Settings.ChatFontSize;
        }

        public void Init(LobbySession lobbySession)
        {
            _lobbySession = lobbySession;
            Title = _lobbySession.GameInfo.DisplayName;

            InitGroupedMembers( _lobbySession );
            itemsControlGameRoomList.ItemsSource = _lobbySession.GameRoomManager.AllGameRooms;
            itemsControlJoinedGameRoomList.ItemsSource = _lobbySession.JoinedGameRooms;
            listBoxActiveGames.ItemsSource = _lobbySession.ActiveGames;

            PropertyChangedEventManager.AddListener(_lobbySession.GameInfo, this, "PropertyChanged");

            _userData.LookupPendingBets(OnPendingBetsComplete);

            _lobbySession.ChatMessageReceived += ChatMessages_CollectionChanged;
        }

        void ChatMessages_CollectionChanged(ChatMessage item)
        {
            AppendUserMessage(item.Author.Username, item.Message);
        }

        private void InitGroupedMembers(ChatroomSessionBase lobbySession)
        {
            var membershipView = new ActiveGroupingCollectionView(lobbySession.Membership);
            membershipView.CustomSort = PgUser.GetLobbyMemberSorter();
            membershipView.GroupDescriptions.Add(new PropertyGroupDescription("LobbyGroupName"));
            listBoxMembership.DataContext = membershipView;
            lobbySession.Membership.CollectionChanged += Membership_CollectionChanged;
            Membership_CollectionChanged(null, null);
        }

        void Membership_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MembershipCount = "Total (" + _lobbySession.Membership.Count + ")";
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
                    var url = App.Settings.WebServerUrl + "/u/" + user.Username + "/profile";
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
            _lobbySession.ShowUndecidedMatches(_lobbySession.ChatroomId);
        }

        private void OnRequestFriend(string errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                ((Action)(delegate {
                    MessageDialog.Show(HomeWindow.Home, "Request Failed", errorMessage);
                })).BeginInvokeUI();
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
                _lobbySession.TakeOverRoomAt(item, OnMyRoomCreated);
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

        private void buttonTaskbar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.Tag as GameRoomItem;
            _lobbySession.ToggleDisplay(item);
        }

        
        private void lobbyTab_Closing(object sender, CancelEventArgs e)
        {
            _lobbySession.Leave();
        }
    }
}
