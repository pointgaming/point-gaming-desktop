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

        private readonly Dictionary<string, ChatroomInfo> _chatroomUsage = new Dictionary<string, ChatroomInfo>();

        public void Init(SocketSession session)
        {
            _session = session;

            session.OnThread("Message.Send.new", OnPrivateMessageSent);
            session.OnThread("Message.Receive.new", OnPrivateMessageReceived);
            session.OnThread("Message.Send.fail", OnPrivateMessageSendFailed);
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
                    Disconnect(item);
            }

            _chatWindow = null;
            HomeWindow.Home.RemoveChildWindow(_chatWindow);
        }
        public void Leave(string id)
        {
            ChatroomInfo chatroomUsage;
            if (_chatroomUsage.TryGetValue(id, out chatroomUsage))
            {
                Disconnect(chatroomUsage);
            }
        }
        private void Disconnect(ChatroomInfo item)
        {
            item.State = ChatroomState.Disconnected;
            ChatroomUserLeave(new Chatroom { _id = item.ChatroomId, });
        }

        #region private messages
        public void ChatWith(PgUser friend)
        {
            var chatWindow = ChatWindow;
            chatWindow.ChatWith(friend);
        }

        private void OnPrivateMessageSent(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<PrivateMessageSent>();
            var chatWindow = ChatWindow;
            chatWindow.MessageSent(received);
        }
        private void OnPrivateMessageSendFailed(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<PrivateMessageOut>();
            var chatWindow = ChatWindow;
            chatWindow.MessageSendFailed();
        }
        private void OnPrivateMessageReceived(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<PrivateMessageIn>();
            var chatWindow = ChatWindow;
            chatWindow.MessageReceived(received);
        }
        public void SendMessage(PrivateMessageOut message)
        {
            _session.EmitLater("Message.send", message);
        }
        #endregion

        #region chatroom
        private void OnChatroomUserList(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomUserList>();
            foreach (var id in received.chatrooms)
            {
                ChatroomInfo usage;
                if (!_chatroomUsage.TryGetValue(id, out usage))
                    JoinChatroom(id);
            }
        }
        private void OnChatroomMemberList(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomMemberList>();
            var id = received._id;
            ChatroomInfo usage;
            if (!_chatroomUsage.TryGetValue(id, out usage))
                return;

            usage.State = ChatroomState.Connected;
            foreach (var item in received.members)
            {
                var pgUser = _session.Data.GetPgUser(item);
                if (!usage.Membership.Contains(pgUser))
                    usage.Membership.Add(pgUser);
            }

            ChatWindow.ShowChatroom(usage);
        }
        private void OnChatroomMemberChange(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomMemberChange>();
            var id = received._id;
            ChatroomInfo usage;
            if (!_chatroomUsage.TryGetValue(id, out usage))
                return;

            var pgUser = _session.Data.GetPgUser(received.user);

            if (received.status == "joined")
            {
                if (!usage.Membership.Contains(pgUser))
                    usage.Membership.Add(pgUser);
            }
            else if (received.status == "left")
                usage.Membership.Remove(pgUser);
        }
        private void OnChatroomMessageNew(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomMessageNew>();
            var id = received._id;
            ChatroomInfo usage;
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
                ChatroomInfo usage;
                if (!_chatroomUsage.TryGetValue(id, out usage)
                    || usage.State == ChatroomState.Disconnected)
                {
                    JoinChatroom(id);
                }
            }
            else
            {
                // todo dean 2013-02-17: handle chat invites from non-friends, and invites to lobbies/gamerooms
            }
        }

        public void JoinChatroom(string id)
        {
            ChatroomInfo usage;
            if (_chatroomUsage.TryGetValue(id, out usage))
            {
                if (usage.State == ChatroomState.Connected
                    || usage.State == ChatroomState.New)
                {
                    ChatWindow.ShowChatroom(usage);
                    return;
                }
            }

            usage = new ChatroomInfo(this) { ChatroomId = id, State = ChatroomState.New, };
            _chatroomUsage[id] = usage;

            var chatroom = new Chatroom { _id = id };
            ChatroomUserJoin(chatroom);
            ChatroomMemberGetList(chatroom);
        }

        private void ChatroomUserJoin(Chatroom chatroom)
        {
            _session.EmitLater("Chatroom.join", chatroom);
        }
        private void ChatroomUserLeave(Chatroom chatroom)
        {
            _session.EmitLater("Chatroom.leave", chatroom);
        }
        private void ChatroomUserGetList()
        {
            _session.EmitLater("Chatroom.User.getList", null);
        }
        private void ChatroomMemberGetList(Chatroom chatroom)
        {
            _session.EmitLater("Chatroom.Member.getList", chatroom);
        }
        public void ChatroomMessageSend(ChatroomMessageOut message)
        {
            _session.EmitLater("Chatroom.Message.send", message);
        }

        public void SendChatroomInvite(ChatroomInviteOut invite)
        {
            ChatroomInviteSend(invite);
        }
        public void ChatroomInviteSend(ChatroomInviteOut invite)
        {
            _session.EmitLater("Chatroom.Invite.send", invite);
        }
        #endregion
    }
}
