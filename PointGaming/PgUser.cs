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

    public class PgTeam : ViewModelBase, IBetOperand
    {
        public string PocoType { get { return "Team"; } }

        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                if (value == _id)
                    return;
                _id = value;
                OnPropertyChanged("Id");
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
                OnPropertyChanged("Name");
                OnPropertyChanged("ShortDescription");
            }
        }

        public string ShortDescription { get { return _name; } }

        private int _points;

        public int Points
        {
            get { return _points; }
            set
            {
                if (value == _points)
                    return;
                _points = value;
                OnPropertyChanged("Points");
            }
        }
    }

    public class PgUser : ViewModelBase, IBetOperand
    {
        public string PocoType { get { return "User"; } }
        
        /// <summary>
        /// Warning: you should probably be using UserDataManager.GetPgUser() so that only one PgUser is initalized per user._id
        /// </summary>
        public PgUser()
        {
            Lobbies.CollectionChanged += Lobbies_CollectionChanged;
        }

        void Lobbies_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Lobby");
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
                OnPropertyChanged("Id");
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
                OnPropertyChanged("Username");
                OnPropertyChanged("ShortDescription");
                OnPropertyChanged("DisplayName");
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
                OnPropertyChanged("Rank");
                OnPropertyChanged("IsAdmin");
                OnPropertyChanged("LobbyGroupName");
                OnPropertyChanged("DisplayName");
            }
        }

        private bool _isAdmin;
        public bool IsAdmin { 
            get { return _isAdmin; }
            set
            {
                if (value == _isAdmin)
                    return;
                _isAdmin = value;

            }
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
                OnPropertyChanged("IsFriend");
                OnPropertyChanged("LobbyGroupName");
            }
        }
        
        public string DisplayName
        {
            get { return Rank != null ? Rank + Username : Username; }
        }

        public string DisplayPoints
        {
            get { return Convert.ToString(Points); }
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
                var TeamBotGroupName = GameRoom.GameRoomWindowModelView.TeamBotGroupName;
                var specialPrefix = "!";
                var aGameRoomGroupName = a.GameRoomGroupName == TeamBotGroupName ? specialPrefix + TeamBotGroupName : a.GameRoomGroupName;
                var bGameRoomGroupName = b.GameRoomGroupName == TeamBotGroupName ? specialPrefix + TeamBotGroupName : b.GameRoomGroupName;
                var groupCmp = aGameRoomGroupName.CompareTo(bGameRoomGroupName);
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
                OnPropertyChanged("Slug");
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
                OnPropertyChanged("Status");
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
                OnPropertyChanged("Points");
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
                OnPropertyChanged("Avatar");
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
                OnPropertyChanged("Team");
                OnPropertyChanged("TeamName");
                OnPropertyChanged("HasTeam");
                OnPropertyChanged("GameRoomGroupName");
                if (_team != null)
                    _team.PropertyChanged += _team_PropertyChanged;
            }
        }

        void _team_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                OnPropertyChanged("TeamName");
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

        private bool _IsMuted;
        public bool IsMuted
        {
            get { return _IsMuted; }
            set
            {
                SetProperty(ref _IsMuted, value, () => IsMuted);
                    
            }
        }
        private string _SpeakingRoomId;
        public string SpeakingRoomId
        {
            get { return _SpeakingRoomId; }
            set
            {
                SetProperty(ref _SpeakingRoomId, value, () => SpeakingRoomId);
            }
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

        private bool _isOwner;
        public bool isOwner
        {
            get { return _isOwner; }
            set
            {
                if (value == _isOwner)
                    return;
                _isOwner = value;
            }
        }
    }

}
