using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PointGaming.POCO;
using SocketIOClient;
using SocketIOClient.Messages;
using RestSharp;

namespace PointGaming.Chat
{
    public delegate void ReceivedMessage(UserBase fromUser, string message);



    public class ChatManager
    {
        public const string PrefixGameLobby = "Game_";
        public const string PrefixGameRoom = "GameRoom_";

        public const string ReasonInvalidPassword = "invalid password";
        public const string ReasonPasswordRequired = "password required";

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
            
            session.OnThread("Chatroom.Join.fail", OnChatroomJoinFailed);
            session.OnThread("Chatroom.User.list", OnChatroomUserList);
            session.OnThread("Chatroom.Member.list", OnChatroomMemberList);
            session.OnThread("Chatroom.Member.change", OnChatroomMemberChange);
            session.OnThread("Chatroom.Message.new", OnChatroomMessageNew);
            session.OnThread("Chatroom.Invite.new", OnChatroomInviteNew);

            session.OnThread("GameRoom.new", OnGameRoomNew);
            session.OnThread("GameRoom.update", OnGameRoomUpdate);
            session.OnThread("GameRoom.destroy", OnGameRoomDestroy);

            session.OnThread("Match.new", OnMatchNew);
            session.OnThread("Match.update", OnMatchUpdate);
            session.OnThread("Bet.new", OnBetNew);
            session.OnThread("Bet.Taker.new", OnBetTakerNew);
            session.OnThread("Bet.update", OnBetUpdate);
            session.OnThread("Bet.destroy", OnBetDestroy);

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
            var id = item.ChatroomId;
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

        private void OnChatroomJoinFailed(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ApiResponse>();
            var reason = received.message;
            string password = null;
            if (reason == ReasonInvalidPassword)
            {
                GameRoom.PasswordDialog.Show(_chatWindow, "Join Failed", "Incorrect password.", out password);
            }
            else if (reason == ReasonPasswordRequired)
            {
                GameRoom.PasswordDialog.Show(_chatWindow, "Join Failed", "Password required.", out password);
            }
            else
            {
                MessageDialog.Show(_chatWindow, "Join Failed", reason);
            }

            if (password != null)
            {
                JoinChatroom(received._id, password);
            }
        }
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
            ChatroomSession chatroomSession;
            if (!_chatroomUsage.TryGetValue(id, out chatroomSession))
                return;

            chatroomSession.Membership.Clear();

            chatroomSession.State = ChatroomState.Connected;
            foreach (var item in received.members)
            {
                var pgUser = _userData.GetPgUser(item);
                if (!chatroomSession.Membership.Contains(pgUser))
                    chatroomSession.Membership.Add(pgUser);
            }

            ChatWindow.Show(chatroomSession);
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
                    if (id.StartsWith(PrefixGameLobby) || id.StartsWith(PrefixGameRoom))
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

        public void JoinChatroom(string id, string password = null)
        {
            ChatroomSession chatroomSession;
            if (_chatroomUsage.TryGetValue(id, out chatroomSession))
            {
                if (chatroomSession.State == ChatroomState.Connected
                    || chatroomSession.State == ChatroomState.New)
                {
                    ChatWindow.Show(chatroomSession);
                    return;
                }
            }
            if (id.StartsWith(PrefixGameLobby))
            {
                var gameId = id.Substring(PrefixGameLobby.Length);
                HomeTab.LauncherInfo gameInfo;
                if (_userData.TryGetGame(gameId, out gameInfo))
                {
                    var session = new Lobby.LobbySession(this, gameInfo);
                    session.LoadGameRoomsComplete += session_LoadGameRoomsComplete;
                    session.LoadGameRooms();
                    chatroomSession = session;
                }
            }
            else if (id.StartsWith(PrefixGameRoom))
            {
                bool isFound = false;
                var gameRoomId = id.Substring(PrefixGameRoom.Length);
                foreach (var session in _chatroomUsage.Values)
                {
                    if (session is Lobby.LobbySession)
                    {
                        var lobbySession = (Lobby.LobbySession)session;
                        Lobby.GameRoomItem gameRoomItem;
                        if (lobbySession.GameRoomLookup.TryGetValue(gameRoomId, out gameRoomItem))
                        {
                            var grSession = new GameRoom.GameRoomSession(this, lobbySession, gameRoomItem);
                            grSession.LoadMatch();
                            chatroomSession = grSession;
                            isFound = true;
                            break;
                        }
                    }
                }

                if (!isFound)
                {
                    Lobby.LobbySession.LookupGameRoom(_userData, gameRoomId, JoinLobbyAndGameRoom);
                    return;
                }
            }
            else
                chatroomSession = new ChatroomSession(this);
            
            chatroomSession.ChatroomId = id;
            chatroomSession.State = ChatroomState.New;

            _chatroomUsage[id] = chatroomSession;

            var chatroom = new Chatroom { _id = id };
            var joinChatroom = (password == null)
                ? chatroom
                : new ChatroomWithPassword { _id = id, password = password, };
            ChatroomUserJoin(joinChatroom);
            ChatroomMemberGetList(chatroom);
        }

        void session_LoadGameRoomsComplete(Lobby.LobbySession obj)
        {
            List<string> removes = new List<string>();
            foreach (var item in _gameRoomsToJoinAfterLobbyJoin)
            {
                if (obj.GameRoomLookup.ContainsKey(item))
                {
                    JoinChatroom(PrefixGameRoom + item);
                    removes.Add(item);
                }
            }
            foreach (var item in removes)
                _gameRoomsToJoinAfterLobbyJoin.Remove(item);
        }

        private List<string> _gameRoomsToJoinAfterLobbyJoin = new List<string>();

        private void JoinLobbyAndGameRoom(Lobby.GameRoomItem room)
        {
            ChatroomSession session;
            if (_chatroomUsage.TryGetValue(PrefixGameLobby + room.GameId, out session))
            {
                var lobbySession = (Lobby.LobbySession)session;
                Lobby.GameRoomItem gameRoomItem;
                if (lobbySession.GameRoomLookup.TryGetValue(room.Id, out gameRoomItem))
                    JoinChatroom(PrefixGameRoom + room.Id);
            }
            else
            {
                _gameRoomsToJoinAfterLobbyJoin.Add(room.Id);
                JoinChatroom(PrefixGameLobby + room.GameId);
            }
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

        #region game room
        private void OnGameRoomNew(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<GameRoomSinglePoco>();
            var gameRoom = received.game_room;

            Lobby.LobbySession lobby;
            if (TryGetGameLobby(gameRoom.game_id, out lobby))
                lobby.OnGameRoomNew(gameRoom);
        }

        private void OnGameRoomUpdate(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<GameRoomSinglePoco>();
            var gameRoom = received.game_room;
            
            GameRoom.GameRoomSession gameRoomSession;
            if (TryGetGameRoom(gameRoom._id, out gameRoomSession))
                gameRoomSession.OnUpdate(gameRoom);

            Lobby.LobbySession lobby;
            if (TryGetGameLobby(gameRoom.game_id, out lobby))
                lobby.OnGameRoomUpdate(gameRoom);
        }

        private void OnGameRoomDestroy(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<GameRoomSinglePoco>();
            var gameRoom = received.game_room;

            GameRoom.GameRoomSession gameRoomSession;
            if (TryGetGameRoom(gameRoom._id, out gameRoomSession))
                gameRoomSession.OnDestroy(gameRoom);

            Lobby.LobbySession lobby;
            if (TryGetGameLobby(gameRoom.game_id, out lobby))
                lobby.OnGameRoomDestroy(gameRoom);
        }

        private bool TryGetGameLobby(string gameId, out Lobby.LobbySession lobby)
        {
            lobby = null;

            var lobbyId = PrefixGameLobby + gameId;
            ChatroomSession usage;
            if (!_chatroomUsage.TryGetValue(lobbyId, out usage))
                return false;

            lobby = (Lobby.LobbySession)usage;
            return true;
        }

        private bool TryGetGameRoom(string gameRoomId, out GameRoom.GameRoomSession gameRoomSession)
        {
            gameRoomSession = null;

            var usageId = PrefixGameRoom + gameRoomId;
            ChatroomSession usage;
            if (!_chatroomUsage.TryGetValue(usageId, out usage))
                return false;

            gameRoomSession = (GameRoom.GameRoomSession)usage;
            return true;
        }
        #endregion


        private void OnMatchNew(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<MatchSinglePoco>();
            var match = received.match;
            _matchToGameRoom[match._id] = match.room_id;

            GameRoom.GameRoomSession gameRoom;
            if (TryGetGameRoom(match.room_id, out gameRoom))
                gameRoom.OnMatchNew(match);
        }

        private void OnMatchUpdate(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<MatchSinglePoco>();
            var match = received.match;
            _matchToGameRoom[match._id] = match.room_id;

            GameRoom.GameRoomSession gameRoom;
            if (TryGetGameRoom(match.room_id, out gameRoom))
                gameRoom.OnMatchUpdate(match);
        }
        private Dictionary<string, string> _matchToGameRoom = new Dictionary<string, string>();

        private void OnBetNew(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<BetSinglePoco>();
            var bet = received.bet;

            string gameId;
            if (!_matchToGameRoom.TryGetValue(bet.match_id, out gameId))
                return;

            GameRoom.GameRoomSession gameRoom;
            if (TryGetGameRoom(gameId, out gameRoom))
                gameRoom.OnBetNew(bet);
        }
        private void OnBetTakerNew(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<BetSinglePoco>();
            var bet = received.bet;

            string gameId;
            if (!_matchToGameRoom.TryGetValue(bet.match_id, out gameId))
                return;

            GameRoom.GameRoomSession gameRoom;
            if (TryGetGameRoom(gameId, out gameRoom))
                gameRoom.OnBetTakerNew(bet);
        }
        private void OnBetUpdate(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<BetSinglePoco>();
            var bet = received.bet;

            string gameId;
            if (!_matchToGameRoom.TryGetValue(bet.match_id, out gameId))
                return;

            GameRoom.GameRoomSession gameRoom;
            if (TryGetGameRoom(gameId, out gameRoom))
                gameRoom.OnBetUpdate(bet);
        }
        private void OnBetDestroy(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<BetSinglePoco>();
            var bet = received.bet;

            string gameId;
            if (!_matchToGameRoom.TryGetValue(bet.match_id, out gameId))
                return;

            GameRoom.GameRoomSession gameRoom;
            if (TryGetGameRoom(gameId, out gameRoom))
                gameRoom.OnBetDestroy(bet);
        }
    }
}
