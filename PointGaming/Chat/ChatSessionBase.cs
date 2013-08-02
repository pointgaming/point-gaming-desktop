using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PointGaming.POCO;
using System.Windows;
using System.Windows.Controls;

namespace PointGaming.Chat
{

    public class ChatMessage
    {
        public PgUser Author;
        public string Message;

        public ChatMessage(PgUser author, string message)
        {
            this.Author = author;
            this.Message = message;
        }
    }

    public abstract class ChatSessionBase
    {
        public event Action<string> SendMessageFailed;
        public event Action<ChatMessage> ChatMessageReceived;

        protected readonly SessionManager _manager;

        public ChatSessionBase(SessionManager manager) { _manager = manager; }

        public abstract void ShowControl(bool shouldActivate);

        internal void OnSendMessageFailed(string message)
        {
            var func = SendMessageFailed;
            if (func != null)
                func(message);
        }

        internal void OnChatMessageReceived(ChatMessage message)
        {
            var call = ChatMessageReceived;
            if (call != null)
                call(message);
        }
    }
}
