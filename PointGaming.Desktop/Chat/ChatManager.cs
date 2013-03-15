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
        private UserDataManager _userData;
        private ChatWindow _chatWindow;

        private readonly Dictionary<string, ChatroomSession> _chatroomUsage = new Dictionary<string, ChatroomSession>();

        public void Init(UserDataManager userData)
        {
            _userData = userData;
            var session = _userData.PgSession;

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
            ChatroomSession chatroomUsage;
            if (_chatroomUsage.TryGetValue(id, out chatroomUsage))
            {
                Disconnect(chatroomUsage);
            }
        }
        private void Disconnect(ChatroomSession item)
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
            _userData.PgSession.EmitLater("Message.send", message);
        }
        #endregion

        #region chatroom
        private void OnChatroomUserList(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomUserList>();
            foreach (var id in received.chatrooms)
            {
                ChatroomSession usage;
                if (!_chatroomUsage.TryGetValue(id, out usage))
                    JoinChatroom(id);
            }
        }
        private void OnChatroomMemberList(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomMemberList>();
            var id = received._id;
            ChatroomSession usage;
            if (!_chatroomUsage.TryGetValue(id, out usage))
                return;

            usage.State = ChatroomState.Connected;
            foreach (var item in received.members)
            {
                var pgUser = _userData.GetPgUser(item);
                if (!usage.Membership.Contains(pgUser))
                    usage.Membership.Add(pgUser);
            }

            ChatWindow.ShowChatroom(usage);
        }
        private void OnChatroomMemberChange(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomMemberChange>();
            var id = received._id;
            ChatroomSession usage;
            if (!_chatroomUsage.TryGetValue(id, out usage))
                return;

            var pgUser = _userData.GetPgUser(received.user);

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
            ChatroomSession usage;
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
            if (_userData.IsFriend(received.fromUser._id))
            {
                ChatroomSession usage;
                if (!_chatroomUsage.TryGetValue(id, out usage)
                    || usage.State == ChatroomState.Disconnected)
                {
                    if (id.StartsWith("lobby_") || id.StartsWith("gameroom_"))
                    {
                        ChatWindow.AddInvite(received);
                    }
                    else
                        JoinChatroom(id);
                }
            }
            else
            {
                ChatWindow.AddInvite(received);
            }
        }

        public void JoinChatroom(string id)
        {
            ChatroomSession usage;
            if (_chatroomUsage.TryGetValue(id, out usage))
            {
                if (usage.State == ChatroomState.Connected
                    || usage.State == ChatroomState.New)
                {
                    ChatWindow.ShowChatroom(usage);
                    return;
                }
            }

            usage = new ChatroomSession(this) { ChatroomId = id, State = ChatroomState.New, };
            _chatroomUsage[id] = usage;

            var chatroom = new Chatroom { _id = id };
            ChatroomUserJoin(chatroom);
            ChatroomMemberGetList(chatroom);
        }

        private void ChatroomUserJoin(Chatroom chatroom)
        {
            _userData.PgSession.EmitLater("Chatroom.join", chatroom);
        }
        private void ChatroomUserLeave(Chatroom chatroom)
        {
            _userData.PgSession.EmitLater("Chatroom.leave", chatroom);
        }
        private void ChatroomUserGetList()
        {
            _userData.PgSession.EmitLater("Chatroom.User.getList", null);
        }
        private void ChatroomMemberGetList(Chatroom chatroom)
        {
            _userData.PgSession.EmitLater("Chatroom.Member.getList", chatroom);
        }
        public void ChatroomMessageSend(ChatroomMessageOut message)
        {
            _userData.PgSession.EmitLater("Chatroom.Message.send", message);
        }
        public void SendChatroomInvite(ChatroomInviteOut invite)
        {
            ChatroomInviteSend(invite);
        }
        public void ChatroomInviteSend(ChatroomInviteOut invite)
        {
            _userData.PgSession.EmitLater("Chatroom.Invite.send", invite);
        }
        #endregion
    }
}
