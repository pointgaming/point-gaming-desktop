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

namespace PointGaming.Desktop
{
    public class UserDataManager
    {
        public string AuthToken { get; set; }
        public FriendUiData User = new FriendUiData { Id= "", Status = "", Username = ""};

        private readonly ObservableCollection<FriendUiData> _friends = new ObservableCollection<FriendUiData>();
        public ObservableCollection<FriendUiData> Friends { get { return _friends; } }
        private Dictionary<string, FriendUiData> _friendLookup = new Dictionary<string, FriendUiData>();

        private Dictionary<string, FriendUiData> _userLookup = new Dictionary<string, FriendUiData>();
        
        public void LoggedOut()
        {
            _userLookup.Clear();
            _friends.Clear();
            _friendLookup.Clear();
        }

        public void AddFriend(FriendUiData friend)
        {
            _friends.Add(friend);
            _friendLookup[friend.Id] = friend;
            _userLookup[friend.Id] = friend;
        }
        public void RemoveFriend(FriendUiData friend)
        {
            _friends.Remove(friend);
            _friendLookup.Remove(friend.Id);
            _userLookup.Remove(friend.Id);
        }
        public bool TryGetFriend(string id, out FriendUiData friend)
        {
            return _friendLookup.TryGetValue(id, out friend);
        }

        public FriendUiData GetUserData(string userId)
        {
            FriendUiData user;
            if (_userLookup.TryGetValue(userId, out user))
                return user;
            user = new FriendUiData{ Id = userId, Username = "unknown", Status = "unknown" };
            // todo dean gores 2013-02-26: should probably look this user up so that real info can be shown
            return user;
        }
    }
}
