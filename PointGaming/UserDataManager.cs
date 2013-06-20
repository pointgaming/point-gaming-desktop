using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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


namespace PointGaming
{
    public class UserDataManager
    {
        public readonly PgUser User;
        public readonly SocketSession PgSession;
        private ChatManager _chatManager;

        private readonly ObservableCollection<PgUser> _friends = new ObservableCollection<PgUser>();
        public ObservableCollection<PgUser> Friends { get { return _friends; } }
        private Dictionary<string, PgUser> _friendLookup = new Dictionary<string, PgUser>();

        private Dictionary<string, PgUser> _userLookup = new Dictionary<string, PgUser>();
        private Dictionary<string, PgTeam> _teamLookup = new Dictionary<string, PgTeam>();

        public readonly ObservableCollection<LauncherInfo> Launchers = new ObservableCollection<LauncherInfo>();

        public UserDataManager(SocketSession session)
        {
            PgSession = session;
            User = session.User;
            _userLookup[User.Id] = User;
            _chatManager = new Chat.ChatManager();
        }

        public void StartChat()
        {
            _chatManager.Init(this);
        }
        public void ChatWith(PgUser friend)
        {
            _chatManager.ChatWith(friend);
        }
        public void JoinChat(string id)
        {
            _chatManager.JoinChatroom(id);
        }

        public Chat.ChatWindow GetChatWindow()
        {
            return _chatManager.ChatWindow;
        }
        
        public void LoggedOut()
        {
            _userLookup.Clear();
            _friends.Clear();
            _friendLookup.Clear();

            PgSession.Begin(PgSession.Logout);
            _chatManager = null;
        }

        public void AddFriend(PgUser friend)
        {
            _friends.Add(friend);
            _friendLookup[friend.Id] = friend;
            _userLookup[friend.Id] = friend;
        }
        public void RemoveFriend(PgUser friend)
        {
            _friends.Remove(friend);
            _friendLookup.Remove(friend.Id);
            _userLookup.Remove(friend.Id);
        }

        public bool IsFriend(string id)
        {
            return _friendLookup.ContainsKey(id);
        }
        public bool TryGetFriend(string id, out PgUser friend)
        {
            return _friendLookup.TryGetValue(id, out friend);
        }

        public PgUser GetPgUser(UserBase userBase)
        {
            PgUser user = new PgUser { Id = userBase._id, Username = userBase.username, Status = "unknown", Team = GetPgTeam(userBase.team) };
            return user;
        }
        public PgTeam GetPgTeam(TeamBase teamBase)
        {
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

        public void LookupBetOperand(string query, Action<List<BetOperandPoco>> callback)
        {
            RestResponse<List<BetOperandPoco>> response = null;
            PgSession.BeginAndCallback(delegate
            {
                var url = Properties.Settings.Default.BetOperandQuery + query + "&auth_token=" + PgSession.AuthToken;
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
