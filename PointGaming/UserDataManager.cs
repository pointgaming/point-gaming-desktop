﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using PointGaming.POCO;
using RestSharp;
using SocketIOClient;
using SocketIOClient.Messages;
using PointGaming.HomeTab;
using System.Security;
using System.Security.Cryptography;
using PointGaming.Lobby;
using PointGaming.GameRoom;
using PointGaming.Chat;
using PointGaming.Voice;

namespace PointGaming
{
    public class UserDataManager
    {
        public static UserDataManager UserData;

        public readonly PgUser User;
        public readonly SocketSession PgSession;
        public readonly FriendshipManager Friendship;
        private SessionManager _sessionManager;
        private DispatcherTimer timer;


        private readonly ObservableCollection<PgUser> _friends = new ObservableCollection<PgUser>();
        public ObservableCollection<PgUser> Friends { get { return _friends; } }
        private Dictionary<string, PgUser> _friendLookup = new Dictionary<string, PgUser>();

        private Dictionary<string, PgUser> _userLookup = new Dictionary<string, PgUser>();
        private Dictionary<string, PgTeam> _teamLookup = new Dictionary<string, PgTeam>();

        public readonly ObservableCollection<LauncherInfo> Launchers = new ObservableCollection<LauncherInfo>();

        public VoipSession Voip;

        public readonly PointGaming.Settings.SettingsUser Settings;

        public UserDataManager(SocketSession session)
        {
            UserData = this;
            Settings = PointGaming.Settings.SettingsUser.Load(session.User.username);
            PgSession = session;
            User = GetPgUser(session.User);
            User.Status = "online";
            _sessionManager = new SessionManager();
            Friendship = new FriendshipManager(PgSession);
            Voip = new VoipSession(this);
            Voip.Enable();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += new EventHandler(CheckIdle);
            timer.Start();
        }

        public void StartChat()
        {
            _sessionManager.Init(this);
        }
        public void ChatWith(PgUser friend)
        {
            if (friend == User)
                return;
            _sessionManager.ChatWith(friend);
        }
        public void CreateChatroomWith(PgUser a, PgUser b)
        {
            Guid g = Guid.NewGuid();
            var id = "client_" + g.ToString().Replace("-", "");
            _sessionManager.JoinChatroom(id);
            _sessionManager.SendChatroomInvite(new ChatroomInviteOut { _id = id, toUser = a.ToUserBase(), });
            _sessionManager.SendChatroomInvite(new ChatroomInviteOut { _id = id, toUser = b.ToUserBase(), });
        }
        public void JoinChat(string id)
        {
            _sessionManager.JoinChatroom(id);
        }

        private void CheckIdle(object sender, EventArgs e)
        {
            var isIdle = App.UserIdleTimespan.TotalMinutes > UserDataManager.UserData.Settings.UserIdleMinutes;
            if ((User.Status == "online" && isIdle) || (User.Status == "idle" && !isIdle))
            {
                User.Status = isIdle ? "idle" : "online";
                PgSession.EmitLater("idle", new { idle = (isIdle ? "1" : "0") });
            }
        }
        
        public void LoggedOut()
        {
            lock (_userLookup)
            {
                _userLookup.Clear();
            }
            Friends.Clear();
            _friendLookup.Clear();

            timer.Stop();
            Voip.Dispose();

            PgSession.Begin(PgSession.Logout);
            _sessionManager = null;
        }

        public void AddFriend(PgUser friend)
        {
            friend.IsFriend = true;
            if (!_friendLookup.ContainsKey(friend.Id))
            {
                Friends.Add(friend);
                _friendLookup[friend.Id] = friend;
            }
        }
        public void RemoveFriend(PgUser friend)
        {
            friend.IsFriend = false;
            if (_friendLookup.Remove(friend.Id))
                Friends.Remove(friend);
        }

        public bool IsFriend(string id)
        {
            return _friendLookup.ContainsKey(id);
        }
        public bool TryGetFriend(string id, out PgUser friend)
        {
            return _friendLookup.TryGetValue(id, out friend);
        }

        public PgUser GetPgUser(string id)
        {
            var userBase = new UserBase { _id = id };
            return GetPgUser(userBase);
        }

        public PgUser GetPgUser(UserBase userBase)
        {
            PgUser user;
            lock (_userLookup)
            {
                if (_userLookup.TryGetValue(userBase._id, out user))
                    return user;

                user = new PgUser
                {
                    Id = userBase._id,
                    Username = userBase.username ?? "unknown",
                    Status = "unknown",
                };

                _userLookup[userBase._id] = user;
            }
            LookupUserData(userBase._id);
            return user;
        }

        public PgTeam GetPgTeam(TeamBase teamBase)
        {
            if (teamBase == null || teamBase._id == null)
                return null;

            PgTeam team;
            if (!_teamLookup.TryGetValue(teamBase._id, out team))
            {
                team = new PgTeam { Id = teamBase._id, Name = teamBase.name };
                _teamLookup[teamBase._id] = team;
            }
            var teamFull = teamBase as TeamFull;
            if (teamFull != null)
            {
                team.Name = teamFull.name;
                team.Temporarily = teamFull.temporarily;
                // todo copy the rest of the details
            }

            return team;
        }

        public bool TryGetGame(string gameId, out LauncherInfo info)
        {
            foreach (var item in Launchers)
            {
                if (item.Id == gameId)
                {
                    info = item;
                    return true;
                }
            }
            info = null;
            return false;
        }

        public void LookupUserData(string userId)
        {
            RestResponse<UserFullResponse> response = null;
            PgSession.BeginAndCallback(delegate
            {
                var url = PgSession.GetWebAppFunction("/api", "/users/" + userId + ".json");
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
                response = (RestResponse<UserFullResponse>)client.Execute<UserFullResponse>(request);
            }, delegate
            {
                if (response.IsOk())
                {
                    PgUser user;
                    lock (_userLookup)
                    {
                        _userLookup.TryGetValue(userId, out user);
                    }
                    if (user != null)
                    {
                        ((Action)(delegate
                        {
                            var rUser = response.Data.user;
                            user.Username = rUser.username;
                            user.Slug = rUser.slug;
                            user.Points = rUser.points;
                            user.Avatar = rUser.avatar;
                            user.Team = GetPgTeam(rUser.team);
                        })).BeginInvokeUI();
                    }
                }
            });
        }

        public void LookupBetOperand(string query, Action<List<BetOperandPoco>> callback)
        {
            RestResponse<List<BetOperandPoco>> response = null;
            PgSession.BeginAndCallback(delegate
            {
                var url = PgSession.GetWebAppFunction("", "/search/playable.json", "query=" + query);
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
                response = (RestResponse<List<BetOperandPoco>>)client.Execute<List<BetOperandPoco>>(request);
            }, delegate
            {
                if (response.IsOk())
                {
                    callback(response.Data);
                }
            });
        }

        public void LookupPendingBets(Action<List<BetPoco>> callback)
        {
            // TODO: get list of pending bets from rails API
            List<BetPoco> bets = new List<BetPoco>();
            callback(bets);
        }

        public bool CanTakeOverRoom(GameRoomItem gameRoom)
        {
            var url = PgSession.GetWebAppFunction("/api", "/game_rooms/" + gameRoom.Id + "/can_take_over");
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            RestResponse response = (RestResponse)client.Execute(request);
            var result = Newtonsoft.Json.Linq.JObject.Parse(response.Content);
            return Convert.ToBoolean(((Newtonsoft.Json.Linq.JProperty)result.First).Value.ToString());
        }

        public bool CanHoldRoom(GameRoomItem gameRoom)
        {
            var url = PgSession.GetWebAppFunction("/api", "/game_rooms/" + gameRoom.Id + "/can_hold");
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            RestResponse response = (RestResponse)client.Execute(request);
            var result = Newtonsoft.Json.Linq.JObject.Parse(response.Content);
            return Convert.ToBoolean(((Newtonsoft.Json.Linq.JProperty)result.First).Value.ToString());
        }
    }

    public class ContextMenuRights
    {
        private PgUser User;
        public ContextMenuRights(PgUser user)
        {
            User = user;
        }

        public bool CanSendMessage { get { return true; } }

        public bool CanSendFriendRequest { get { return true; } }

        public bool CanInviteToTeam { get { return User.HasTeam; } }

        public bool CanViewProfile { get { return true; } }

        public bool CanBlock { get { return true; } }

        public bool CanUnblock { get { return true; } }

        public bool CanAddAsRinger { get { return true; } }

        public bool CanKickFromGameRoom { get { return true; } }

        public bool CanSendWarning { get { return true; } }

        public bool CanKickFromLobby { get { return true; } }

        public bool CanBanForTime(double hoursCount)
        {
            return true;
        }

        public bool CanCreditPoints { get { return true; } }

        public bool CanRemovePoints { get { return true; } }
    }
}
