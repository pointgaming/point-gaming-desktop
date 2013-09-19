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

namespace PointGaming
{
    public interface IBetOperand : INotifyPropertyChanged
    {
        string Id { get; }
        string ShortDescription { get; }
        string PocoType { get; }
    }

    public class PgTeam : IBetOperand
    {
        public string PocoType { get { return "Team"; } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged(string propertyName)
        {
            var changedCallback = PropertyChanged;
            if (changedCallback == null)
                return;
            var args = new PropertyChangedEventArgs(propertyName);
            changedCallback(this, args);
        }


        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                if (value == _id)
                    return;
                _id = value;
                NotifyChanged("Id");
            }
        }
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name)
                    return;
                _name = value;
                NotifyChanged("Name");
                NotifyChanged("ShortDescription");
            }
        }

        public string ShortDescription { get { return _name; } }
    }

    public class PgUser : IBetOperand
    {
        public string PocoType { get { return "User"; } }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged(string propertyName)
        {
            var changedCallback = PropertyChanged;
            if (changedCallback == null)
                return;
            var args = new PropertyChangedEventArgs(propertyName);
            changedCallback(this, args);
        }

        /// <summary>
        /// Warning: you should probably be using UserDataManager.GetPgUser() so that only one PgUser is initalized per user._id
        /// </summary>
        public PgUser()
        {
            Lobbies.CollectionChanged += Lobbies_CollectionChanged;
        }

        void Lobbies_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyChanged("Lobby");
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                if (value == _id)
                    return;
                _id = value;
                NotifyChanged("Id");
            }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                if (value == _username)
                    return;
                _username = value;
                NotifyChanged("Username");
                NotifyChanged("ShortDescription");
                NotifyChanged("DisplayName");
            }
        }

        private string _rank;
        public string Rank
        {
            get { return _rank; }
            set
            {
                if (value == _rank)
                    return;
                _rank = value;
                NotifyChanged("Rank");
                NotifyChanged("IsAdmin");
                NotifyChanged("LobbyGroupName");
                NotifyChanged("DisplayName");
            }
        }
        public bool IsAdmin { 
            get { return !string.IsNullOrEmpty(Rank); } 
        }

        private bool _isFriend;
        public bool IsFriend
        {
            get { return _isFriend; }
            set
            {
                if (value == _isFriend)
                    return;
                _isFriend = value;
                NotifyChanged("IsFriend");
                NotifyChanged("LobbyGroupName");
            }
        }
        
        public string DisplayName
        {
            get { return Rank != null ? Rank + Username : Username; }
        }
        public string ShortDescription { get { return _username; } }

        public string GameRoomGroupName
        {
            get
            {
                if (HasTeam)
                    return TeamName;
                return "Other (No Team)";
            }
        }
        public string LobbyGroupName
        {
            get
            {
                if (IsAdmin)
                    return "Admins";
                if (IsFriend || UserDataManager.UserData.User == this)
                    return "Friends";
                return "Players";
            }
        }

        private static readonly string[] LobbyGroupNames = new string[] { "Total", "Friends", "Admins", "Players", };
        public static System.Collections.IComparer GetLobbyMemberSorter()
        {
            return new LobbyMemberSorter();
        }
        private class LobbyMemberSorter : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                PgUser a = x as PgUser;
                PgUser b = y as PgUser;
                if (a.LobbyGroupName == b.LobbyGroupName)
                {
                    return -1 * a.Points.CompareTo(b.Points);
                }
                var aix = Array.IndexOf(LobbyGroupNames, a.LobbyGroupName);
                var bix = Array.IndexOf(LobbyGroupNames, b.LobbyGroupName);
                return aix.CompareTo(bix);
            }
        }

        public static System.Collections.IComparer GetGameRoomMemberSorter()
        {
            return new GameRoomMemberSorter();
        }
        private class GameRoomMemberSorter : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                PgUser a = x as PgUser;
                PgUser b = y as PgUser;
                var groupCmp = a.GameRoomGroupName.CompareTo(b.GameRoomGroupName);
                if (groupCmp != 0)
                    return groupCmp;
                return -1 * a.Points.CompareTo(b.Points);
            }
        }

        private string _slug;
        public string Slug
        {
            get { return _slug; }
            set
            {
                if (value == _slug)
                    return;
                _slug = value;
                NotifyChanged("Slug");
            }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                if (value == _status)
                    return;
                _status = value;
                NotifyChanged("Status");
            }
        }

        private int _points;
        public int Points
        {
            get { return _points; }
            set
            {
                if (value == _points)
                    return;
                _points = value;
                NotifyChanged("Points");
            }
        }

        private string _avatar;
        public string Avatar
        {
            get
            {
                if (_avatar == string.Empty || _avatar == null)
                    return "";

                return App.Settings.WebServerUrl + _avatar;
            }
            set
            {
                if (value == _avatar)
                    return;
                _avatar = value;
                NotifyChanged("Avatar");
            }
        }

        private PgTeam _team;
        public PgTeam Team
        {
            get { return _team; }
            set
            {
                if (value == _team)
                    return;
                if (_team != null)
                    _team.PropertyChanged -= _team_PropertyChanged;
                _team = value;
                NotifyChanged("Team");
                NotifyChanged("TeamName");
                NotifyChanged("HasTeam");
                NotifyChanged("GameRoomGroupName");
                if (_team != null)
                    _team.PropertyChanged += _team_PropertyChanged;
            }
        }

        void _team_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                NotifyChanged("TeamName");
            }
        }
        public string TeamName
        {
            get { return Team != null ? Team.Name : "Other"; }
        }
        public bool HasTeam
        {
            get { return Team != null; }
        }

        public void ViewProfile()
        {
            System.Diagnostics.Process.Start(App.Settings.WebServerUrl + "/u/" + Slug);
        }

        public readonly ObservableCollection<HomeTab.LauncherInfo> _lobbies = new ObservableCollection<HomeTab.LauncherInfo>();
        public ObservableCollection<HomeTab.LauncherInfo> Lobbies { get { return _lobbies; } }
        public HomeTab.LauncherInfo Lobby
        {
            get
            {
                return Lobbies.Count == 0 ? null : Lobbies[0];
            }
        }


        public UserBase ToUserBase()
        {
            return new UserBase { _id = Id, username = Username, };
        }

        public override string ToString()
        {
            return Username;
        }
    }

}
