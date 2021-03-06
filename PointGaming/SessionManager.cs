﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PointGaming.POCO;
using PointGaming.GameRoom;
using SocketIOClient;
using SocketIOClient.Messages;
using RestSharp;
using PointGaming.Chat;
using PointGaming.Lobby;

namespace PointGaming
{
    public delegate void ReceivedMessage(UserBase fromUser, string message);



    public class SessionManager
    {
        public const string PrefixGameLobby = "Game_";
        public const string PrefixGameRoom = "GameRoom_";

        public const string ReasonInvalidPassword = "invalid password";
        public const string ReasonPasswordRequired = "password required";

        private UserDataManager _userData;
        private readonly Dictionary<PgUser, ChatSessionBase> _privateChats = new Dictionary<PgUser, ChatSessionBase>();
        private readonly Dictionary<string, ChatroomSessionBase> _chatroomUsage = new Dictionary<string, ChatroomSessionBase>();
        
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

        public void LeavePrivateChat(PgUser other)
        {
            _privateChats.Remove(other);
        }

        public void Leave(string id)
        {
            ChatroomSessionBase session;
            if (_chatroomUsage.TryGetValue(id, out session) && session.State == ChatroomState.Connected)
            {
                Disconnect(session);
            }
        }
        private void Disconnect(ChatroomSessionBase item)
        {
            var id = item.ChatroomId;
            item.State = ChatroomState.Disconnected;
            ChatroomUserLeave(new Chatroom { _id = item.ChatroomId, });
            _chatroomUsage.Remove(id);// 2013-07-29 Dean Gores: at one point I didn't remove them from here, I can't remember why now
        }

        #region private messages
        public void ChatWith(PgUser friend)
        {
            ChatSessionBase chatSession = GetOrCreatePrivateChatSession(friend);
            chatSession.ShowControl(true);
        }

        private void OnPrivateMessageSent(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<PrivateMessageSent>();
            var toUser = _userData.GetPgUser(received.toUser);

            ChatSessionBase chatSession = GetOrCreatePrivateChatSession(toUser);
            var cm = new ChatMessage(_userData.User, received.message);
            chatSession.OnChatMessageReceived(cm);
        }
        private void OnPrivateMessageSendFailed(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<PrivateMessageOut>();
            var toUser = _userData.GetPgUser(new UserBase { _id = received._id });
            ChatSessionBase chatSession = GetOrCreatePrivateChatSession(toUser);
            chatSession.OnSendMessageFailed(received.message);
        }
        private void OnPrivateMessageReceived(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<PrivateMessageIn>();
            var fromUser = _userData.GetPgUser(received.fromUser);

            ChatSessionBase chatSession = GetOrCreatePrivateChatSession(fromUser);
            var cm = new ChatMessage(fromUser, received.message);
            chatSession.OnChatMessageReceived(cm);
        }
        public void SendMessage(PrivateMessageOut message)
        {
            _userData.PgSession.EmitLater("Message.send", message);
        }
        private ChatSessionBase GetOrCreatePrivateChatSession(PgUser user)
        {
            ChatSessionBase chat;
            if (_privateChats.TryGetValue(user, out chat))
                return chat;

            var session = new PrivateChatSession(this, user);
            _privateChats[user] = session;
            session.ShowControl(false);
            return session;
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
                GameRoom.PasswordDialog.Show(HomeWindow.Home, "Join Failed", "Incorrect password.", out password);
            }
            else if (reason == ReasonPasswordRequired)
            {
                GameRoom.PasswordDialog.Show(HomeWindow.Home, "Join Failed", "Password required.", out password);
            }
            else
            {
                MessageDialog.Show(HomeWindow.Home, "Join Failed", reason);
            }

            if (password != null)
            {
                JoinChatroom(received._id, password);
            }
        }

        private void OnChatroomUserList(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomUserList>();

            var chatrooms = new List<string>();
            var lobbies = new List<string>();
            var gameRooms = new List<string>();

            foreach (var id in received.chatrooms)
            {
                if (id.StartsWith(PrefixGameLobby))
                    lobbies.Add(id);
                else if (id.StartsWith(PrefixGameRoom))
                    gameRooms.Add(id);
                else
                    chatrooms.Add(id);
            }

            // client needs to join lobbies before game rooms
            var all = new[] { chatrooms, lobbies, gameRooms };
            foreach (var type in all) 
            {
                foreach (var id in type)
                {
                    ChatroomSessionBase usage;
                    if (!_chatroomUsage.TryGetValue(id, out usage))
                        JoinChatroom(id);
                }
            }
        }

        private void OnChatroomMemberList(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomMemberList>();
            var id = received._id;
            ChatroomSessionBase chatroomSession;
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

            chatroomSession.ShowControl(true);
        }
        private void OnChatroomMemberChange(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomMemberChange>();
            var id = received._id;
            ChatroomSessionBase usage;
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
            ChatroomSessionBase session;
            if (!_chatroomUsage.TryGetValue(id, out session))
                return;
            if (session.State != ChatroomState.Connected)
                return;

            var chatMessage = new ChatMessage(_userData.GetPgUser(received.fromUser), received.message);
            session.OnChatMessageReceived(chatMessage);
        }
        private void OnChatroomInviteNew(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomInviteNew>();
            var id = received._id;
            if (_userData.IsFriend(received.fromUser._id))
            {
                ChatroomSessionBase usage;
                if (!_chatroomUsage.TryGetValue(id, out usage))
                {
                    if (id.StartsWith(PrefixGameLobby) || id.StartsWith(PrefixGameRoom))
                    {
                        AddInvite(received);
                    }
                    else
                        JoinChatroom(id);
                }
            }
            else
            {
                AddInvite(received);
            }
        }

        private RoomInviteWindow _inviteTab = null;

        public void AddInvite(ChatroomInviteNew invite)
        {
            if (_inviteTab == null)
            {
                _inviteTab = new RoomInviteWindow();
                _inviteTab.Closed += _inviteTab_Closed;
                _inviteTab.ShowNormal(false);
            }
            _inviteTab.AddInvite(invite);
            _inviteTab.FlashWindowSmartly();
        }

        void _inviteTab_Closed(object sender, EventArgs e)
        {
            _inviteTab = null;
        }

        public void ShowUndecidedMatches(string id)
        {
            ChatroomSessionBase session;
            if (_chatroomUsage.TryGetValue(id, out session))
            {
                MatchesDialogModelView modelView = new MatchesDialogModelView();
                modelView.Init(this, session as LobbySession);

                var dialog = new MatchesDialog();
                dialog.DataContext = modelView;
                dialog.ShowDialog();

                if (dialog.DialogResult == true)
                {
                }
            }
        }

        public void JoinChatroom(string id, string password = null)
        {
            ChatroomSessionBase chatroomSession;
            if (_chatroomUsage.TryGetValue(id, out chatroomSession))
            {
                if (chatroomSession.State == ChatroomState.New)
                    return;// we are already joining
                else if (chatroomSession.State == ChatroomState.Connected)
                {
                    chatroomSession.ShowControl(true);// show it
                    return;
                }
                // else state == invited, so join it
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
                var gameRoomId = id.Substring(PrefixGameRoom.Length);
                Lobby.LobbySession lobbySession;
                Lobby.GameRoomItem gameRoomItem;
                if (!TryGetGameRoomAndSession(gameRoomId, out lobbySession, out gameRoomItem))
                {
                    Lobby.LobbySession.LookupGameRoom(_userData, gameRoomId, JoinLobbyAndGameRoom);
                    return;
                }

                var gameRoomSession = new GameRoom.GameRoomSession(this, lobbySession, gameRoomItem);
                chatroomSession = gameRoomSession;
            }
            else
                chatroomSession = new ChatroomSession(this);
            
            chatroomSession.ChatroomId = id;

            _chatroomUsage[id] = chatroomSession;

            var chatroom = new Chatroom { _id = id };
            var joinChatroom = (password == null)
                ? chatroom
                : new ChatroomWithPassword { _id = id, password = password, };
            ChatroomUserJoin(joinChatroom);
            ChatroomMemberGetList(chatroom);
        }
        
        private bool TryGetGameRoomAndSession(string id, out Lobby.LobbySession session, out Lobby.GameRoomItem room)
        {
            foreach (var chatSession in _chatroomUsage.Values)
            {
                if (chatSession is Lobby.LobbySession)
                {
                    session = chatSession as Lobby.LobbySession;
                    if (session.GameRoomManager.TryGetItemById(id, out room))
                    {
                        return true;
                    }
                }
            }

            session = null;
            room = null;
            return false;
        }

        void session_LoadGameRoomsComplete(Lobby.LobbySession obj)
        {
            List<string> removes = new List<string>();
            foreach (var id in _gameRoomsToJoinAfterLobbyJoin)
            {
                Lobby.GameRoomItem item;
                if (obj.GameRoomManager.TryGetItemById(id, out item))
                {
                    JoinChatroom(PrefixGameRoom + id);
                    removes.Add(id);
                }
            }
            foreach (var item in removes)
                _gameRoomsToJoinAfterLobbyJoin.Remove(item);
        }

        private List<string> _gameRoomsToJoinAfterLobbyJoin = new List<string>();

        private void JoinLobbyAndGameRoom(Lobby.GameRoomItem room)
        {
            ChatroomSessionBase session;
            if (_chatroomUsage.TryGetValue(PrefixGameLobby + room.GameId, out session))
            {
                var lobbySession = (Lobby.LobbySession)session;
                Lobby.GameRoomItem gameRoomItem;
                if (lobbySession.GameRoomManager.TryGetItemById(room.Id, out gameRoomItem))
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

        public void AdminGameRoom(string id)
        {
            ChatroomSessionBase session;
            if (_chatroomUsage.TryGetValue(id, out session))
            {
                GameRoomSession gameRoomSession = session as GameRoomSession;

                GameRoomAdminDialogModelView modelView = new GameRoomAdminDialogModelView();
                modelView.Init(this, gameRoomSession);

                GameRoomAdminDialog dialog = new GameRoomAdminDialog();
                dialog.DataContext = modelView;
                dialog.ShowDialog();
                RefreshAdminsList(dialog, modelView);

                if (dialog.DialogResult == true)
                {
                    modelView.UpdateGameRoomSettings();
                }
            }
        }

        public void OpenBetting(string id)
        {
            ChatroomSessionBase session;
            if (_chatroomUsage.TryGetValue(id, out session))
            {
                GameRoomSession gameRoomSession = session as GameRoomSession;
                var dialog = new BetProposalDialog(gameRoomSession.GameRoom.BettingType);
                dialog.Owner = gameRoomSession.Window;
                dialog.ShowDialog();
                if (dialog.DialogResult == true)
                {
                    gameRoomSession.CreateBet(dialog.ToBet());
                }
            }
        }

        public void ViewBetting(string id, Bet bet)
        {
            ChatroomSessionBase session;
            if (_chatroomUsage.TryGetValue(id, out session))
            {
                GameRoomSession gameRoomSession = session as GameRoomSession;

                BetDetailsDialogModelView modelView = new BetDetailsDialogModelView();
                modelView.Init(this, gameRoomSession, bet);

                var dialog = new BetDetailsDialogView();
                dialog.Owner = gameRoomSession.Window;
                dialog.DataContext = modelView;

                dialog.ShowDialog();
                if (dialog.DialogResult == true)
                {
                    gameRoomSession.AcceptBet(bet);
                }
            }
        }

        public void ShowMessage(string id, string title, string message)
        {
            ChatroomSessionBase session;
            if (_chatroomUsage.TryGetValue(id, out session))
            {
                GameRoomSession gameRoomSession = session as GameRoomSession;
                MessageDialog.Show(gameRoomSession.Window, title, message);
            }
        }

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
            ChatroomSessionBase usage;
            if (!_chatroomUsage.TryGetValue(lobbyId, out usage))
                return false;

            lobby = (Lobby.LobbySession)usage;
            return true;
        }

        private bool TryGetGameRoom(string gameRoomId, out GameRoom.GameRoomSession gameRoomSession)
        {
            gameRoomSession = null;

            var usageId = PrefixGameRoom + gameRoomId;
            ChatroomSessionBase usage;
            if (!_chatroomUsage.TryGetValue(usageId, out usage))
                return false;

            gameRoomSession = (GameRoom.GameRoomSession)usage;
            return true;
        }
        #endregion

        #region betting

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
        #endregion
        private void RefreshAdminsList(GameRoomAdminDialog dialog, GameRoomAdminDialogModelView modelView)
        {
            var adminsCollection = dialog.adminsListBox.Items;
            PgUser[] admins = new PgUser[adminsCollection.Count];
            for (int i = 0; i < admins.Length; i++)
                admins[i] = (PgUser)adminsCollection[i];
            modelView.Admins = admins;
        }
    }
}
