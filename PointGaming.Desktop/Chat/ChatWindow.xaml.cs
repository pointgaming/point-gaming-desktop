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
        private SocketSession _session = HomeWindow.Home.SocketSession;
        private ChatManager _manager;

        private readonly Dictionary<string, TabItem> _chatTabs2 = new Dictionary<string, TabItem>();

        public ChatWindow()
        {
            InitializeComponent();
        }

        public void Init(ChatManager manager)
        {
            Owner = HomeWindow.Home;
            _manager = manager;
        }

        public void SendMessage(PrivateMessage message)
        {
            _manager.SendMessage(message);
        }

        public void MessageReceived(PrivateMessage message)
        {
            var data = _session.Data.GetPgUser(message.user_id);
            TabItem tabItem = GetOrCreateTab(data);
            var chatTab = (Chat.ChatTab)tabItem.Content;
            chatTab.MessageReceived(message);
        }

        public void ChatWith(PgUser data)
        {
            TabItem tabItem = GetOrCreateTab(data);
            tabControlChats.SelectedItem = tabItem;
        }

        public void ShowChatroom(ChatManager.ChatroomUsage roomManager)
        {
            TabItem tabItem = GetOrCreateTab(roomManager);
            tabControlChats.SelectedItem = tabItem;
        }

        private TabItem GetOrCreateTab(PgUser data)
        {
            var tabId = GetTabId(typeof(ChatTab), data.Id);

            TabItem tabItem;
            if (!_chatTabs2.TryGetValue(tabId, out tabItem))
            {
                var chatTab = new ChatTab();
                chatTab.Init(this, data);

                tabItem = AddTab(data.Username, tabId, chatTab);
            }
            return tabItem;
        }
        private TabItem GetOrCreateTab(ChatManager.ChatroomUsage roomManager)
        {
            var tabId = GetTabId(typeof(ChatroomTab), roomManager.ChatroomId);

            TabItem tabItem;
            if (!_chatTabs2.TryGetValue(tabId, out tabItem))
            {
                var chatTab = new ChatroomTab();
                chatTab.Init(this, roomManager);

                tabItem = AddTab(roomManager.ChatroomId, tabId, chatTab);
            }
            return tabItem;
        }

        private TabItem AddTab(string title, string tabId, object content)
        {
            var tabItem = new TabItem();
            tabItem.Content = content;
            tabItem.Header = title;

            tabControlChats.Items.Add(tabItem);

            if (tabControlChats.Items.Count == 1)
                tabControlChats.SelectedIndex = 0;

            _chatTabs2[tabId] = tabItem;
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

        public void StartFlashingTab(Type tabType, string id)
        {
            if (!IsActive && Properties.Settings.Default.ShouldFlashChatWindow)
                this.FlashWindow();

            var tabId = GetTabId(tabType, id);

            TabItem tabItem;
            if (!_chatTabs2.TryGetValue(tabId, out tabItem))
                return;
            if (!IsActive || !tabItem.IsSelected)
                tabItem.SetValue(Control.StyleProperty, (Style)this.Resources["FlashingHeader"]);
        }

        private static string GetTabId(Type tabType, string id)
        {
            string tabId = tabType.Name + "_" + id;
            return tabId;
        }

        private void StopFlashingTab(TabItem tabItem)
        {
            tabItem.Style = null;
        }

        public void CreateChatroomWith(PgUser a, PgUser b)
        {
            Guid g = Guid.NewGuid();
            var id = "client_" + g;
            _manager.JoinChatroom(id);
            _manager.ChatroomInviteSend(new ChatroomInviteOut { _id = id, toUser = a.ToUserBase(), });
            _manager.ChatroomInviteSend(new ChatroomInviteOut { _id = id, toUser = b.ToUserBase(), });
        }
    }
}
