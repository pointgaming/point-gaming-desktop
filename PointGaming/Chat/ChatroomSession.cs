using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PointGaming.POCO;

namespace PointGaming.Chat
{
    public enum ChatroomState
    {
        New,
        Invited,
        Connected,
        Disconnected,
    }
    public class ChatroomSession
    {
        public event ReceivedMessage ReceivedMessage;
        public string ChatroomId;
        public ChatroomState State;
        public readonly ObservableCollection<PgUser> Membership = new ObservableCollection<PgUser>();
        protected ChatManager _manager;

        public ChatroomSession(ChatManager manager)
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

        public virtual Type GetUserControlType()
        {
            return typeof(ChatroomTab);
        }

        public virtual IChatroomTab GetNewUserControl()
        {
            return new ChatroomTab();
        }
    }
}
