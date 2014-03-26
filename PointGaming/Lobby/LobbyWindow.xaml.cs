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
            //BuildContextMenu();
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

            //BuildContextMenu();
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
        /*
        private void userContextMenuTaunt_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog.Show(this, "Taunt User", "TODO: show taunt selection UI");
        }
        */
        private void userContextMenuBlock_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog.Show(this, "Block User", "TODO: ???");
        }

        private void userContextMenuUnBlock_Click(object sender, RoutedEventArgs e)
        {

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

        private void userContextMenuBan30Minutes_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                PgUser user = menuItem.CommandParameter as PgUser;
                if (user != null)
                {
                    _lobbySession.Ban(user, 0.5);
                }
            }
        }

        private void userContextMenuBan24Hours_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                PgUser user = menuItem.CommandParameter as PgUser;
                if (user != null)
                {
                    _lobbySession.Ban(user, 24.0);
                }
            }
        }

        private void userContextMenuBan48Hours_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                PgUser user = menuItem.CommandParameter as PgUser;
                if (user != null)
                {
                    _lobbySession.Ban(user, 48.0);
                }
            }
        }

        private void userContextMenuBan1Week_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                PgUser user = menuItem.CommandParameter as PgUser;
                if (user != null)
                {
                    _lobbySession.Ban(user, 24.0 * 7.0);
                }
            }
        }

        private void userContextMenuBanLifeTime_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                PgUser user = menuItem.CommandParameter as PgUser;
                if (user != null)
                {
                    _lobbySession.Ban(user, -1.0);
                }
            }
        }

        private void userContextMenuInviteToTeam_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

        }

        private void userContextMenuAddAsRinger_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

        }

        private void userContextMenuSendWarning_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

        }

        private void userContextMenuKickFromLobby_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
        }

        private void userContextMenuCreditPoints_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                PgUser user = menuItem.CommandParameter as PgUser;
                if (user != null)
                {
                    HomeTab.InputValueDialog inputDialog = new InputValueDialog();
                    var result = inputDialog.ShowDialog();
                    if (result == false)
                        return;
                    var pointsCount = 0;
                    try
                    {
                        pointsCount = Convert.ToInt32(inputDialog.Value);
                    }
                    catch
                    {
                        MessageBox.Show(this, "The operation ss not completed. Bad data format.");
                        return;
                    }
                    _lobbySession.CreditPoints(user, pointsCount);
                }
                listBoxMembership.Items.Refresh();
            }
        }

        private void userContextMenuRemovePoints_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                PgUser user = menuItem.CommandParameter as PgUser;
                if (user != null)
                {
                    HomeTab.InputValueDialog inputDialog = new InputValueDialog();
                    var result = inputDialog.ShowDialog();
                    if (result == false)
                        return;
                    var pointsCount = Convert.ToInt32(inputDialog.Value);
                    _lobbySession.RemovePoints(user, pointsCount);
                }
                listBoxMembership.Items.Refresh();
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
                    _lobbySession.RequestRights();
                    if (_lobbySession.IsBanned != true)
                        _userData.JoinChat(SessionManager.PrefixGameRoom + item.Id);
                    else
                    {
                        Notifier.NotifyBannedUser(_userData.User, item.GameId);
                        this.Close();
                    }
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

        //relates to the context menu, rights logic, etc.
        /*
        private void BuildContextMenu()
        {
            ContextMenu newContextMenu = new ContextMenu()/*listBoxMembership.ContextMenu;
            MenuItemInfo[] menuItemsInfo = GetMenuItemsInfo();
            foreach (var item in menuItemsInfo)
                if (item.canDo == true)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = item.header;
                    //handler defining is turned off, turn on it when the methods are implemented
                    menuItem.Click += item.handler;
                    menuItem.CommandParameter = new Binding();
                    newContextMenu.Items.Add(menuItem);
                }
            listBoxMembership.ContextMenu = newContextMenu;
        }

        private MenuItemInfo[] GetMenuItemsInfo()
        {
            PointGaming.ContextMenuRights rights = new ContextMenuRights(_userData.User);
            int menuItemsCount = 12;
            MenuItemInfo[] menuItemsInfo = new MenuItemInfo[menuItemsCount];
            menuItemsInfo[0] = new MenuItemInfo("Message", rights.CanSendMessage, userContextMenuMessage_Click);
            menuItemsInfo[1] = new MenuItemInfo("Send Friend Request", rights.CanSendFriendRequest, userContextMenuFriendRequest_Click);
            menuItemsInfo[2] = new MenuItemInfo("Invite to team", rights.CanInviteToTeam, userContextMenuInviteToTeam_Click);
            menuItemsInfo[3] = new MenuItemInfo("View Profile", rights.CanViewProfile, userContextMenuViewProfile_Click);
            menuItemsInfo[4] = new MenuItemInfo("Block (Mute)", rights.CanBlock, userContextMenuBlock_Click);
            menuItemsInfo[5] = new MenuItemInfo("Unblock", rights.CanUnblock, userContextMenuUnBlock_Click);
            menuItemsInfo[6] = new MenuItemInfo("Add as Ringer", rights.CanAddAsRinger, userContextMenuAddAsRinger_Click);
            //menuItemsInfo[7] = new MenuItemInfo("Kick", rights.CanKickFromGameRoom, userContextMenuKick_Click);
            menuItemsInfo[7] = new MenuItemInfo("Send Warning", rights.CanSendWarning, userContextMenuSendWarning_Click);
            menuItemsInfo[8] = new MenuItemInfo("Kick from Lobby", rights.CanKickFromLobby, userContextMenuKickFromLobby_Click);
            menuItemsInfo[9] = new MenuItemInfo("Ban (30 Minutes)", rights.CanBanForTime(0.5), userContextMenuBan30_Click);
            menuItemsInfo[10] = new MenuItemInfo("Credit Points", rights.CanCreditPoints, userContextMenuCreditPoints_Click);
            menuItemsInfo[11] = new MenuItemInfo("Remove Points", rights.CanRemovePoints, userContextMenuRemovePoints_Click);
            return menuItemsInfo;
        }

        class MenuItemInfo
        {
            public MenuItemInfo(string header, bool canDo, RoutedEventHandler handler)
            {
                this.header = header;
                this.canDo = canDo;
                this.handler = handler;
            }

            public string header;
            public bool canDo;
            public RoutedEventHandler handler;
        }
         */

        private void userContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            _lobbySession.RequestRights();
            var contextMenu = sender as ContextMenu;
            if (_lobbySession.IsBanned)
            {
                foreach (var item in contextMenu.Items)
                    ((Control)item).IsEnabled = !_lobbySession.IsBanned;
                return;
            }

            string[] myItemsNames = new string[3] {"menuItemViewProfile", "menuItemCreditPoints", "menuItemRemovePoints"};
            ListViewItem element = contextMenu.PlacementTarget as ListViewItem;
            var user = (PgUser)element.Content;
            if (user.Id == _userData.User.Id)
                foreach (var item in contextMenu.Items)
                {
                    string menuItemName = ((Control)item).Name;
                    if (menuItemName != null && myItemsNames.Contains<string>(menuItemName) == true)
                        ((Control)item).Visibility = System.Windows.Visibility.Visible;
                    else
                        ((Control)item).Visibility = System.Windows.Visibility.Collapsed;
                }
            else
                foreach (var item in contextMenu.Items)
                    ((Control)item).Visibility = System.Windows.Visibility.Visible;

        }
    }
}
