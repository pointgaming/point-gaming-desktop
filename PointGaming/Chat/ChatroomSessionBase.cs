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

    public abstract class ChatroomSessionBase : ChatSessionBase
    {
        public event EventHandler SessionStateChanged;
        public string ChatroomId;
        private ChatroomState _state = ChatroomState.New;
        public ChatroomState State
        {
            get { return _state; }
            set
            {
                if (value == _state)
                    return;
                _state = value;
                OnSessionStateChanged();
            }
        }
        internal void OnSessionStateChanged()
        {
            var func = SessionStateChanged;
            if (func != null)
                func(this, new EventArgs());
        }

        public readonly ObservableCollection<PgUser> Membership = new ObservableCollection<PgUser>();
        
        public ChatroomSessionBase(SessionManager manager) : base(manager)
        {
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

        public void Leave()
        {
            _manager.Leave(ChatroomId);
        }
    }
}
