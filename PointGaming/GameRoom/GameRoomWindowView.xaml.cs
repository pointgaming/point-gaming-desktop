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
using System.Windows.Shapes;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace PointGaming.GameRoom
{
    public partial class GameRoomWindow : Window
    {
        public WindowTreeManager WindowTreeManager;

        public GameRoomWindow()
        {
            InitializeComponent();
            WindowTreeManager = new WindowTreeManager(this, null);
            Activated += GameRoomWindow_Activated;
            //BuildContextMenu();
        }

        void GameRoomWindow_Activated(object sender, EventArgs e)
        {
            var viewModel = (GameRoomWindowModelView)DataContext;
            if (viewModel.Activated.CanExecute(null))
                viewModel.Activated.Execute(null);
        }

        private void ChatTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isShiftDown = e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift);

            // Treat the chatbox enter input as a send button click
            if (e.Key == Key.Enter && !isShiftDown)
            {
                SendChatButton.Command.Execute(ChatTextBox.Text);

                ChatTextBox.Text = null;
                e.Handled = true;
            }
        }

        private void SendChatButton_Click(object sender, RoutedEventArgs e)
        {
            SendChatButton.Command.Execute(ChatTextBox.Text);

            // Clear input after text sent
            ChatTextBox.Text = null;
            e.Handled = true;
        }

        private void ShowVoiceSettingsClick(object sender, RoutedEventArgs e)
        {
            Settings.SettingsWindow.ShowDialog(WindowTreeManager, typeof(Settings.VoiceTab));
        }

        private void userContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var contextMenu = sender as ContextMenu;
            ListViewItem element = contextMenu.PlacementTarget as ListViewItem;
            var user = (PgUser)element.Content;
            var addAsRinger = contextMenu.Items[4]; var removeFromTeam = contextMenu.Items[5];
            if ((user.Team != null) && (user.Team.Temporarily == true))
            {
                ((Control)(addAsRinger)).Visibility = Visibility.Collapsed;
                ((Control)(removeFromTeam)).Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                ((Control)(addAsRinger)).Visibility = Visibility.Visible;
                ((Control)(removeFromTeam)).Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        /*
        private void BuildContextMenu()
        {
            listBoxMembership.ContextMenu = null;
            MenuItemInfo[] menuItemsInfo = GetMenuItemsInfo();
            ContextMenu newContextMenu = new System.Windows.Controls.ContextMenu();
            foreach (var item in menuItemsInfo)
                if (item.canDo == true)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = item.header;
                    //handler defining is turned off, turn on it when the methods are implemented
                    menuItem.Click += item.handler;
                    newContextMenu.Items.Add(menuItem);
                }
            listBoxMembership.ContextMenu = newContextMenu;
        }

        private MenuItemInfo[] GetMenuItemsInfo()
        {
            PointGaming.ContextMenuRights rights = new ContextMenuRights(UserDataManager.UserData.User);
            int menuItemsCount = 1;
            MenuItemInfo[] menuItemsInfo = new MenuItemInfo[menuItemsCount];
            //menuItemsInfo[0] = new MenuItemInfo("Message", rights.CanSendMessage, userContextMenuMessage_Click);
            //menuItemsInfo[1] = new MenuItemInfo("Send Friend Request", rights.CanSendFriendRequest, userContextMenuFriendRequest_Click);
            //menuItemsInfo[2] = new MenuItemInfo("Invite to team", rights.CanInviteToTeam, userContextMenuInviteToTeam_Click);
            //menuItemsInfo[3] = new MenuItemInfo("View Profile", rights.CanViewProfile, userContextMenuViewProfile_Click);
            //menuItemsInfo[4] = new MenuItemInfo("Block (Mute)", rights.CanBlock, userContextMenuBlock_Click);
            //menuItemsInfo[5] = new MenuItemInfo("Unblock", rights.CanUnblock, userContextMenuUnBlock_Click);
            //menuItemsInfo[6] = new MenuItemInfo("Add as Ringer", rights.CanAddAsRinger, userContextMenuAddAsRinger_Click);
            //menuItemsInfo[7] = new MenuItemInfo("Kick", rights.CanKickFromGameRoom, userContextMenuKick_Click);
            //menuItemsInfo[8] = new MenuItemInfo("Send Warning", rights.CanSendWarning, userContextMenuSendWarning_Click);
            menuItemsInfo[0] = new MenuItemInfo("Ban (30 Minutes)", rights.CanBanForTime(0.5), userContextMenuBan30_Click);
            //menuItemsInfo[10] = new MenuItemInfo("Credit Points", rights.CanCreditPoints, userContextMenuCreditPoints_Click);
            //menuItemsInfo[11] = new MenuItemInfo("Remove Points", rights.CanRemovePoints, userContextMenuRemovePoints_Click);
            return menuItemsInfo;
        }

        private void userContextMenuBan30_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
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
    }
}
