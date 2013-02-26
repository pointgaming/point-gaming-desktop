using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using PointGaming.Desktop.POCO;
using PointGaming.Desktop.HomeTab;

namespace PointGaming.Desktop.Chat
{
    public partial class ChatWindow : Window
    {
        private ChatManager _manager;

        private readonly Dictionary<string, TabItem> _chatTabs = new Dictionary<string, TabItem>();

        public ChatWindow()
        {
            InitializeComponent();
        }

        public void Init(ChatManager manager)
        {
            Owner = HomeWindow.Home;
            this._manager = manager;
        }

        public void SendMessage(PrivateMessage message)
        {
            _manager.SendMessage(message);
        }

        public void MessageReceived(PrivateMessage message)
        {
            var data = HomeWindow.UserDataManager.GetUserData(message.user_id);
            TabItem tabItem = GetOrCreateTab(data);
            var chatTab = (Chat.ChatTab)tabItem.Content;
            chatTab.MessageReceived(message);
        }

        public void ChatWith(FriendUiData data)
        {
            TabItem tabItem = GetOrCreateTab(data);
            tabControlChats.SelectedItem = tabItem;
        }

        private TabItem GetOrCreateTab(FriendUiData data)
        {
            TabItem tabItem;
            if (!_chatTabs.TryGetValue(data.Id, out tabItem))
            {
                var chatTab = new Chat.ChatTab();
                chatTab.Init(this, data);

                tabItem = new TabItem();
                tabItem.Content = chatTab;
                tabItem.Header = data.Username;

                tabControlChats.Items.Add(tabItem);

                if (tabControlChats.Items.Count == 1)
                    tabControlChats.SelectedIndex = 0;

                _chatTabs[data.Id] = tabItem;
            }
            return tabItem;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _manager.ChatWindowClosed();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            this.StopFlashingWindow();
            var selectedTab = tabControlChats.SelectedItem as TabItem;
            if (selectedTab == null)
                return;
            StopFlashingTab(selectedTab);
        }

        private void tabControlChats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem tabItem = null;
            foreach (var item in e.AddedItems)
            { tabItem = (TabItem)item; break; }
            if (tabItem == null)
                return;
            StopFlashingTab(tabItem);
        }

        public void StartFlashingTab(string userId)
        {
            if (!IsActive && Properties.Settings.Default.ShouldFlashChatWindow)
                this.FlashWindow();

            TabItem tabItem;
            if (!_chatTabs.TryGetValue(userId, out tabItem))
                return;
            if (!IsActive || !tabItem.IsSelected)
                tabItem.SetValue(Control.StyleProperty, (Style)this.Resources["FlashingHeader"]);
        }

        private void StopFlashingTab(TabItem tabItem)
        {
            tabItem.Style = null;
        }
    }
}
