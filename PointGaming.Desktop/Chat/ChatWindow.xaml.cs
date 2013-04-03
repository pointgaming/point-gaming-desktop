using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        private UserDataManager _userData = HomeWindow.UserData;
        private ChatManager _manager;

        private readonly Dictionary<string, ClosableTab> _chatTabs2 = new Dictionary<string, ClosableTab>();

        private WindowBoundsPersistor _windowBoundsPersistor;

        public ChatWindow()
        {
            InitializeComponent();
            _windowBoundsPersistor = new ChatWindowBoundsPersistor(this);
            _windowBoundsPersistor.Load();
        }

        public void Init(ChatManager manager)
        {
            Owner = HomeWindow.Home;
            _manager = manager;
        }

        public void SendMessage(PrivateMessageOut message)
        {
            _manager.SendMessage(message);
        }

        public void MessageReceived(PrivateMessageIn message)
        {
            var data = _userData.GetPgUser(message.fromUser);
            ClosableTab tabItem = GetOrCreateTab(data);
            var chatTab = (Chat.ChatTab)tabItem.Content;
            chatTab.MessageReceived(message);
        }
        public void MessageSent(PrivateMessageSent message)
        {
            var data = _userData.GetPgUser(message.toUser);
            ClosableTab tabItem = GetOrCreateTab(data);
            var chatTab = (Chat.ChatTab)tabItem.Content;
            chatTab.MessageSent(message);
        }
        public void MessageSendFailed()
        {
            MessageDialog.Show(this, "Failed to Send Message", "Failed to send message.  User is not online or doesn't exist.");
        }

        public void ChatWith(PgUser data)
        {
            ClosableTab tabItem = GetOrCreateTab(data);
            tabControlChats.SelectedItem = tabItem;
        }

        public void Show(ChatroomSession chatroomSession)
        {
            ClosableTab tabItem = GetOrCreateTab(chatroomSession);
            tabControlChats.SelectedItem = tabItem;
        }

        private ClosableTab GetOrCreateTab(PgUser data)
        {
            var tabId = GetTabId(typeof(ChatTab), data.Id);

            ClosableTab tabItem;
            if (!_chatTabs2.TryGetValue(tabId, out tabItem))
            {
                var chatTab = new ChatTab();
                chatTab.Init(this, data);

                tabItem = AddTab(tabId, chatTab);
            }
            return tabItem;
        }
        private ClosableTab GetOrCreateTab(ChatroomSession chatroomSession)
        {
            ClosableTab tabItem;

            var type = chatroomSession.GetUserControlType();

            var tabId = GetTabId(type, chatroomSession.ChatroomId);
            if (!_chatTabs2.TryGetValue(tabId, out tabItem))
            {
                var chatTab = chatroomSession.GetNewUserControl();
                chatTab.Init(this, chatroomSession);
                tabItem = AddTab(tabId, chatTab);
            }
            
            return tabItem;
        }

        public void AddInvite(ChatroomInviteNew invite)
        {
            ClosableTab tabItem;
            var tabId = GetTabId(typeof(RoomInviteTab), RoomInviteTab.TabId);
            if (!_chatTabs2.TryGetValue(tabId, out tabItem))
            {
                var inviteTab = new RoomInviteTab();
                inviteTab.Init(this);
                tabItem = AddTab(tabId, inviteTab);
            }

            {
                var inviteTab = (RoomInviteTab)tabItem.Content;
                inviteTab.AddInvite(invite);
            }

            StartFlashingTab(typeof(RoomInviteTab), RoomInviteTab.TabId);
        }

        private ClosableTab AddTab(string tabId, ITabWithId content)
        {
            var tabItem = new ClosableTab();
            tabItem.Content = content;
            tabItem.Closing += tabItem_Closing;

            Binding b = new Binding("Header");
            b.Source = content;
            tabItem.SetBinding(TabItem.HeaderProperty, b);

            tabControlChats.Items.Add(tabItem);

            if (tabControlChats.Items.Count == 1)
                tabControlChats.SelectedIndex = 0;

            _chatTabs2[tabId] = tabItem;
            return tabItem;
        }

        void tabItem_Closing(object sender, CancelEventArgs e)
        {
            var tabItem = (ClosableTab)sender;
            var idable = tabItem.Content as ITabWithId;
            if (idable == null)
                return;
            _manager.Leave(idable.Id);
            var tabId = GetTabId(idable.GetType(), idable.Id);
            _chatTabs2.Remove(tabId);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _manager.ChatWindowClosed();
            _windowBoundsPersistor.Save();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            this.StopFlashingWindow();
            var selectedTab = tabControlChats.SelectedItem as ClosableTab;
            if (selectedTab == null)
                return;
            StopFlashingTab(selectedTab);
        }

        private void tabControlChats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource != tabControlChats)
                return;

            ClosableTab tabItem = null;
            foreach (var item in e.AddedItems)
            { tabItem = (ClosableTab)item; break; }
            if (tabItem == null)
                return;
            StopFlashingTab(tabItem);
        }

        public void CloseTab(Type tabType, string id)
        {
            var tabId = GetTabId(tabType, id);
            ClosableTab tabItem;
            if (_chatTabs2.TryGetValue(tabId, out tabItem))
                tabItem.Close();
        }

        public void StartFlashingTab(Type tabType, string id)
        {
            if (!IsActive && Properties.Settings.Default.ShouldFlashChatWindow)
                this.FlashWindow();

            var tabId = GetTabId(tabType, id);

            ClosableTab tabItem;
            if (!_chatTabs2.TryGetValue(tabId, out tabItem))
                return;
            if (!IsActive || !tabItem.IsSelected)
                tabItem.ShouldFlash = true;
        }

        private static string GetTabId(Type tabType, string id)
        {
            string tabId = tabType.Name + "_" + id;
            return tabId;
        }

        private void StopFlashingTab(ClosableTab tabItem)
        {
            tabItem.ShouldFlash = false;
        }

        public void CreateChatroomWith(PgUser a, PgUser b)
        {
            Guid g = Guid.NewGuid();
            var id = "client_" + g.ToString().Replace("-", "");
            _manager.JoinChatroom(id);
            _manager.SendChatroomInvite(new ChatroomInviteOut { _id = id, toUser = a.ToUserBase(), });
            _manager.SendChatroomInvite(new ChatroomInviteOut { _id = id, toUser = b.ToUserBase(), });
        }

        private class ChatWindowBoundsPersistor : WindowBoundsPersistor
        {
            public ChatWindowBoundsPersistor(Window window) : base(window) { }

            protected override Rect GetBounds(out string oldDesktopInfo)
            {
                var r = new Rect(
                    Properties.Settings.Default.ChatWindowBoundsLeft,
                    Properties.Settings.Default.ChatWindowBoundsTop,
                    Properties.Settings.Default.ChatWindowBoundsWidth,
                    Properties.Settings.Default.ChatWindowBoundsHeight
                );
                oldDesktopInfo = Properties.Settings.Default.ChatWindowBoundsDesktopInfo;
                return r;
            }
            protected override void SetBounds(Rect r, string desktopInfo)
            {
                Properties.Settings.Default.ChatWindowBoundsLeft = r.Left;
                Properties.Settings.Default.ChatWindowBoundsTop = r.Top;
                Properties.Settings.Default.ChatWindowBoundsWidth = r.Width;
                Properties.Settings.Default.ChatWindowBoundsHeight = r.Height;
                Properties.Settings.Default.ChatWindowBoundsDesktopInfo = desktopInfo;
            }
        }
    }
}
