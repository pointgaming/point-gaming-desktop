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
        public PgUser User = new PgUser { Id= "", Status = "", Username = ""};

        private readonly ObservableCollection<PgUser> _friends = new ObservableCollection<PgUser>();
        public ObservableCollection<PgUser> Friends { get { return _friends; } }
        private Dictionary<string, PgUser> _friendLookup = new Dictionary<string, PgUser>();

        private Dictionary<string, PgUser> _userLookup = new Dictionary<string, PgUser>();

        static byte[] entropy = System.Text.Encoding.UTF8.GetBytes("ln^lQ+0pX0_%>3b[,5Ulfbl,b\\.myWENxJJeF&*u3p");

        public static string EncryptString(string input)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.UTF8.GetBytes(input),
                entropy,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        public static bool TryDecryptString(string encryptedData, out string plain)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                plain = System.Text.Encoding.UTF8.GetString(decryptedData);
                return true;
            }
            catch
            {
            }
            plain = null;
            return false;
        }
                
        public void LoggedOut()
        {
            _userLookup.Clear();
            _friends.Clear();
            _friendLookup.Clear();
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
