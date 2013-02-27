using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PointGaming.Desktop.POCO;
using SocketIOClient;
using SocketIOClient.Messages;
using RestSharp;

namespace PointGaming.Desktop.Chat
{
    public delegate void ReceivedMessage(UserBase fromUser, string message);

    public class ChatManager
    {
        private SocketSession _session;
        private ChatWindow _chatWindow;

        private enum ChatroomState
        {
            New,
            Invited,
            Connected,
            Disconnected,
        }

        private class ChatroomUsage
        {
            public event ReceivedMessage ReceivedMessage;
            public string ChatroomId;
            public ChatroomState State;
            public readonly ObservableCollection<UserBase> Membership = new ObservableCollection<UserBase>();

            public void OnMessageNew(UserBase fromUser, string message)
            {
                var method = ReceivedMessage;
                if (method != null)
                    method(fromUser, message);
            }
        }

        private readonly Dictionary<string, ChatroomUsage> _chatroomUsage = new Dictionary<string, ChatroomUsage>();

        public void Init(SocketSession session)
        {
            _session = session;

            session.OnThread("message", OnPrivateMessage);
            session.OnThread("Chatroom.User.list", OnChatroomUserList);
            session.OnThread("Chatroom.Member.list", OnChatroomMemberList);
            session.OnThread("Chatroom.Member.change", OnChatroomMemberChange);
            session.OnThread("Chatroom.Message.new", OnChatroomMessageNew);
            session.OnThread("Chatroom.Invite.new", OnChatroomInviteNew);

            ChatroomUserGetList();
        }

        public ChatWindow ChatWindow
        {
            get
            {
                if (_chatWindow != null)
                    return _chatWindow;

                _chatWindow = new ChatWindow();
                _chatWindow.Init(this);
                _chatWindow.Show();
                HomeWindow.Home.AddChildWindow(_chatWindow);
                return _chatWindow;
            }
        }

        public void ChatWindowClosed()
        {
            foreach (var item in _chatroomUsage.Values)
            {
                if (item.State == ChatroomState.Connected)
                {
                    item.State = ChatroomState.Disconnected;
                    ChatroomUserLeave(new Chatroom { _id = item.ChatroomId, });
                }
            }

            _chatWindow = null;
            HomeWindow.Home.RemoveChildWindow(_chatWindow);
        }

        #region private messages
        public void ChatWith(PgUser friend)
        {
            var chatWindow = ChatWindow;
            chatWindow.ChatWith(friend);
        }

        private void OnPrivateMessage(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<PrivateMessage>();

            HomeWindow.Home.InvokeUI(delegate
            {
                var chatWindow = ChatWindow;
                chatWindow.MessageReceived(received);
            });
        }
        public void SendMessage(PrivateMessage message)
        {
            _session.EmitLater("message", message);
        }
        #endregion

        #region chatroom
        private void OnChatroomUserList(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomUserList>();
            foreach (var item in received.chatrooms)
            {
                var id = item._id;
                ChatroomUsage usage;
                if (!_chatroomUsage.TryGetValue(id, out usage))
                    JoinChatroom(id);
            }
        }
        private void OnChatroomMemberList(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomMemberList>();
            var id = received._id;
            ChatroomUsage usage;
            if (!_chatroomUsage.TryGetValue(id, out usage))
                return;

            usage.State = ChatroomState.Connected;
            foreach (var item in received.membership)
                usage.Membership.Add(item);

            // todo dean 2013-02-17: show this chat in the UI
        }
        private void OnChatroomMemberChange(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomMemberChange>();
            var id = received._id;
            ChatroomUsage usage;
            if (!_chatroomUsage.TryGetValue(id, out usage))
                return;

            if (received.status == "joined")
                usage.Membership.Add(received.user);
            else if (received.status == "left")
                usage.Membership.Remove(received.user);
        }
        private void OnChatroomMessageNew(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomMessageNew>();
            var id = received._id;
            ChatroomUsage usage;
            if (!_chatroomUsage.TryGetValue(id, out usage))
                return;
            if (usage.State != ChatroomState.Connected)
                return;

            usage.OnMessageNew(received.fromUser, received.message);
        }
        private void OnChatroomInviteNew(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomInviteNew>();
            var id = received._id;
            if (_session.Data.IsFriend(received.fromUser._id))
            {
                ChatroomUsage usage;
                if (!_chatroomUsage.TryGetValue(id, out usage)
                    || usage.State == ChatroomState.Disconnected)
                {
                    JoinChatroom(id);
                }
            }
            else
            {
                // todo dean 2013-02-17: handle chat invites from non-friends
            }
        }

        public void JoinChatroom(string id)
        {
            ChatroomUsage usage;
            if (_chatroomUsage.TryGetValue(id, out usage))
            {
                if (usage.State == ChatroomState.Connected
                    || usage.State == ChatroomState.New)
                    return;
            }

            usage = new ChatroomUsage { ChatroomId = id, State = ChatroomState.New, };
            _chatroomUsage[id] = usage;

            var chatroom = new Chatroom { _id = id };
            ChatroomUserJoin(chatroom);
            ChatroomMemberGetList(chatroom);
        }

        public void ChatroomUserJoin(Chatroom chatroom)
        {
            _session.EmitLater("Chatroom.User.join", chatroom);
        }
        public void ChatroomUserLeave(Chatroom chatroom)
        {
            _session.EmitLater("Chatroom.User.leave", chatroom);
        }
        public void ChatroomUserGetList()
        {
            _session.EmitLater("Chatroom.User.getList", null);
        }
        public void ChatroomMemberGetList(Chatroom chatroom)
        {
            _session.EmitLater("Chatroom.Member.getList", chatroom);
        }
        public void ChatroomMessageSend(ChatroomMessageOut message)
        {
            _session.EmitLater("Chatroom.Message.send", message);
        }
        public void ChatroomInviteSend(ChatroomInviteOut invite)
        {
            _session.EmitLater("Chatroom.Invite.send", invite);
        }
        #endregion
    }
}
