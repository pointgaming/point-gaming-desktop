using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PointGaming.Desktop.POCO;

namespace PointGaming.Desktop.Chat
{
    public enum ChatroomState
    {
        New,
        Invited,
        Connected,
        Disconnected,
    }
    public class ChatroomInfo
    {
        public event ReceivedMessage ReceivedMessage;
        public string ChatroomId;
        public ChatroomState State;
        public readonly ObservableCollection<PgUser> Membership = new ObservableCollection<PgUser>();
        private ChatManager _manager;

        public ChatroomInfo(ChatManager manager)
        {
            _manager = manager;
        }

        public void OnMessageNew(UserBase fromUser, string message)
        {
            var method = ReceivedMessage;
            if (method != null)
                method(fromUser, message);
        }

        public void SendMessage(string message)
        {
            var messageOut = new ChatroomMessageOut { _id = ChatroomId, message = message, };
            _manager.ChatroomMessageSend(messageOut);
        }

        public void Invite(PgUser other)
        {
            if (Membership.Contains(other))
                return;
            _manager.ChatroomInviteSend(new ChatroomInviteOut { _id = ChatroomId, toUser = other.ToUserBase(), });
        }
    }
}
