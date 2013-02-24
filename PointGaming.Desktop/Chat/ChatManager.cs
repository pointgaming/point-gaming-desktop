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
        private Client _client;
        private ChatWindow _chatWindow;

        public void OnAuthorized(Client client)
        {
            _client = client;
            client.On("message", OnMessage);
        }

        private void OnMessage(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ReceivedMessages>();

            HomeWindow.Home.InvokeUI(delegate
            {
                var chatWindow = GetOrCreateChatWindow();
                chatWindow.MessageReceived(received.username, received.message);
            });
        }

        public void SendMessage(string usernameTo, string message)
        {
            var outgoing = new OutgoingMessages { user = usernameTo, message = message };
            _client.Emit("message", outgoing);
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

        public void ChatWith(string username)
        {
            var chatWindow = GetOrCreateChatWindow();
            chatWindow.ChatWith(username);
        }
    }
}
