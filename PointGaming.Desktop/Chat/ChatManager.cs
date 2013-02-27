using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PointGaming.Desktop.POCO;
using SocketIOClient;
using SocketIOClient.Messages;
using RestSharp;

namespace PointGaming.Desktop.Chat
{
    public class ChatManager
    {
        private SocketSession _session;
        private ChatWindow _chatWindow;

        public void Init(SocketSession session)
        {
            _session = session;
            session.OnThread("message", OnMessage);
        }

        private void OnMessage(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<PrivateMessage>();

            HomeWindow.Home.InvokeUI(delegate
            {
                var chatWindow = GetOrCreateChatWindow();
                chatWindow.MessageReceived(received);
            });
        }

        public void SendMessage(PrivateMessage message)
        {
            _session.EmitLater("message", message);
        }

        public ChatWindow GetOrCreateChatWindow()
        {
            if (_chatWindow != null)
                return _chatWindow;

            _chatWindow = new ChatWindow();
            _chatWindow.Init(this);
            _chatWindow.Show();
            HomeWindow.Home.AddChildWindow(_chatWindow);
            return _chatWindow;
        }

        public void ChatWindowClosed()
        {
            _chatWindow = null;
            HomeWindow.Home.RemoveChildWindow(_chatWindow);
        }

        public void ChatWith(PgUser friend)
        {
            var chatWindow = GetOrCreateChatWindow();
            chatWindow.ChatWith(friend);
        }
    }
}
