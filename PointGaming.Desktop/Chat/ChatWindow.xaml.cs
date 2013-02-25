using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Collections.Generic;

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

        public void SendMessage(string usernameTo, string message)
        {
            _manager.SendMessage(usernameTo, message);
        }

        public void MessageReceived(string usernameFrom, string message)
        {
            TabItem tabItem = GetOrCreateTab(usernameFrom);
            var chatTab = (Chat.ChatTab)tabItem.Content;
            chatTab.MessageReceived(usernameFrom, message);
        }

        public void ChatWith(string username)
        {
            TabItem tabItem = GetOrCreateTab(username);
            tabControlChats.SelectedItem = tabItem;
        }

        private TabItem GetOrCreateTab(string username)
        {
            TabItem tabItem;
            if (!_chatTabs.TryGetValue(username, out tabItem))
            {
                var chatTab = new Chat.ChatTab();
                chatTab.Init(this, username);

                tabItem = new TabItem();
                tabItem.Content = chatTab;
                tabItem.Header = username;

                tabControlChats.Items.Add(tabItem);

                if (tabControlChats.Items.Count == 1)
                    tabControlChats.SelectedIndex = 0;

                _chatTabs[username] = tabItem;
            }
            return tabItem;
        }
    }
}
