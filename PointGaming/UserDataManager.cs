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
using PointGaming.NAudio;
using PointGaming.AudioChat;
using NA = NAudio;


namespace PointGaming
{
    public class UserDataManager
    {
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

        public AudioChatSession AudioSystem;

        public UserDataManager(SocketSession session)
        {
            PgSession = session;
            User = GetPgUser(session.User);
            User.Status = "online";
            _sessionManager = new SessionManager();
            Friendship = new FriendshipManager(PgSession);
            AudioSystem = new AudioChatSession(this);
            AudioSystem.Enable();

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
            var isIdle = App.UserIdleTimespan.TotalMinutes > PointGaming.Properties.Settings.Default.UserIdleMinutes;
            if ((User.Status == "online" && isIdle) || (User.Status == "idle" && !isIdle))
            {
                User.Status = isIdle ? "idle" : "online";
                PgSession.EmitLater("idle", new { idle = (isIdle ? "1" : "0") });
            }
        }
        
        public void LoggedOut()
        {
            _userLookup.Clear();
            Friends.Clear();
            _friendLookup.Clear();

            timer.Stop();
            AudioSystem.Disable();
            AudioSystem.Dispose();

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
            if (_userLookup.TryGetValue(userBase._id, out user))
                return user;

            user = new PgUser
            {
                Id = userBase._id,
                Username = userBase.username ?? "unknown",
                Status = "unknown",
                Rank = userBase.rank,
                Team = GetPgTeam(userBase.team),
                Avatar = userBase.avatar,
                Slug = userBase.slug,
            };
            _userLookup[userBase._id] = user;
            LookupUserData(userBase._id);
            return user;
        }

        public PgTeam GetPgTeam(TeamBase teamBase)
        {
            if (teamBase == null)
                return null;

            PgTeam team;
            if (_teamLookup.TryGetValue(teamBase._id, out team))
                return team;
            team = new PgTeam { Id = teamBase._id, Name = teamBase.name };
            _teamLookup[teamBase._id] = team;
            // todo dean gores 2013-02-26: should probably look this user up so that real info can be shown
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
            RestResponse<UserBase> response = null;
            PgSession.BeginAndCallback(delegate
            {
                var url = PgSession.GetWebAppFunction("/api/v1", "/users/" + userId + ".json");
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
                response = (RestResponse<UserBase>)client.Execute<UserBase>(request);
            }, delegate
            {
                App.LogLine(response.Content);
                if (response.IsOk())
                {
                    PgUser user;
                    if (_userLookup.TryGetValue(userId, out user))
                    {
                        user.Rank = response.Data.rank;
                        user.Username = response.Data.username;
                        user.Slug = response.Data.slug;
                        user.Points = response.Data.points;
                        user.Team = GetPgTeam(response.Data.team);
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
    }
}
