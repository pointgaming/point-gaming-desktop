using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PointGaming.Desktop.POCO;
using RestSharp;
using SocketIOClient;
using SocketIOClient.Messages;
using PointGaming.Desktop.HomeTab;
using System.Security;
using System.Security.Cryptography;

namespace PointGaming.Desktop
{
    public class UserDataManager
    {
        public readonly PgUser User;
        public readonly SocketSession PgSession;
        private Chat.ChatManager _chatManager;
        
        private readonly ObservableCollection<PgUser> _friends = new ObservableCollection<PgUser>();
        public ObservableCollection<PgUser> Friends { get { return _friends; } }
        private Dictionary<string, PgUser> _friendLookup = new Dictionary<string, PgUser>();

        private Dictionary<string, PgUser> _userLookup = new Dictionary<string, PgUser>();

        public UserDataManager(SocketSession session)
        {
            PgSession = session;
            User = session.User;
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
            PgUser user;
            if (_userLookup.TryGetValue(userBase._id, out user))
                return user;
            user = new PgUser { Id = userBase._id, Username = userBase.username, Status = "unknown" };
            _userLookup[userBase._id] = user;
            // todo dean gores 2013-02-26: should probably look this user up so that real info can be shown
            return user;
        }
    }
}
