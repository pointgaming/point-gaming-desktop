using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;

namespace PointGaming.Lobby
{
    public class GameRoomItem : INotifyPropertyChanged
    {
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

        private string _gameId;
        public string GameId
        {
            get { return _gameId; }
            set
            {
                if (value == _gameId)
                    return;
                _gameId = value;
                NotifyChanged("GameId");
            }
        }

        private string _matchId;
        public string MatchId
        {
            get { return _matchId; }
            set
            {
                if (value == _matchId)
                    return;
                _matchId = value;
                NotifyChanged("MatchId");
            }
        }

        private int _position;
        public int Position
        {
            get { return _position; }
            set
            {
                if (value == _position)
                    return;
                _position = value;
                NotifyChanged("Position");
                NotifyChanged("DisplayName");
            }
        }
        private bool _isLocked;
        public bool IsLocked
        {
            get { return _isLocked; }
            set
            {
                if (value == _isLocked)
                    return;
                _isLocked = value;
                NotifyChanged("IsLocked");
                NotifyChanged("DisplayName");
            }
        }
        public string DisplayName { get { return "Game " + _position + (_isLocked ? " (Locked)" : ""); } }

        private int _memberCount;
        public int MemberCount
        {
            get { return _memberCount; }
            set
            {
                if (value == _memberCount)
                    return;
                _memberCount = value;
                NotifyChanged("MemberCount");
                NotifyChanged("MemberStatus");
            }
        }
        private int _maxMemberCount;
        public int MaxMemberCount
        {
            get { return _maxMemberCount; }
            set
            {
                if (value == _maxMemberCount)
                    return;
                _maxMemberCount = value;
                NotifyChanged("MaxMemberCount");
                NotifyChanged("MemberStatus");
                NotifyChanged("IsNotNew");
                NotifyChanged("IsNew");
            }
        }
        public string MemberStatus { get { return _memberCount + "/" + MaxMemberCount; } }
        public bool IsNew {
            get { return Id == null; }
            set
            {
                NotifyChanged("IsNotNew");
                NotifyChanged("IsNew");
            }  
        }
        public bool IsNotNew {
            get { return Id != null; }  
            set 
            {
                NotifyChanged("IsNotNew");
                NotifyChanged("IsNew");
            } 
        }
        public bool IsJoinable
        {
            get { return MemberCount < MaxMemberCount; }
            set
            {
                NotifyChanged("IsJoinable");
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                if (value == _description)
                    return;
                _description = value;
                NotifyChanged("Description");
                NotifyChanged("DescriptionDocument");
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                var newValue = value == null ? "" : value.Trim();
                if (newValue == _password)
                    return;
                _password = newValue;
                NotifyChanged("Password");
                IsLocked = !string.IsNullOrWhiteSpace(newValue);
                NotifyChanged("IsLocked");
                NotifyChanged("DisplayName");
            }
        }

        private PgUser _owner;
        public PgUser Owner
        {
            get { return _owner; }
            set
            {
                if (value == _owner)
                    return;
                _owner = value;
                NotifyChanged("Owner");
                NotifyChanged("Members");
            }
        }
        private PgUser[] _members = new PgUser[0];
        public PgUser[] Members
        {
            get 
            {
                PgUser[] allMembers = new PgUser[_members.Length + 1];
                _members.CopyTo(allMembers, 0);
                allMembers[_members.Length] = Owner;
                return allMembers; 
            }
            set
            {
                if (value == _members)
                    return;
                _members = value;
                NotifyChanged("Members");
            }
        }

        private bool _isAdvertising;
        public bool IsAdvertising
        {
            get { return _isAdvertising; }
            set
            {
                if (value == _isAdvertising)
                    return;
                _isAdvertising = value;
                NotifyChanged("IsAdvertising");
            }
        }

        public FlowDocument DescriptionDocument
        {
            get
            {
                var p = new Paragraph();
                Chat.ChatTabCommon.Format(_description, p.Inlines);

                var doc = new FlowDocument();
                doc.SetPointGamingDefaults();
                doc.Blocks.Add(p);
                return doc;
            }
        }

        public GameRoomItem() { }

        public GameRoomItem(POCO.GameRoomPoco poco)
        {
            Id = poco._id;
            GameId = poco.game_id;
            Description = poco.description;
            MaxMemberCount = poco.max_member_count;
            MemberCount = poco.member_count;
            Position = poco.position;
            IsLocked = poco.is_locked;
            Password = poco.password;
            IsAdvertising = poco.is_advertising;
            Owner = HomeWindow.UserData.GetPgUser(poco.owner);
        }

        public POCO.GameRoomPoco ToPoco()
        {
            POCO.GameRoomPoco poco = new POCO.GameRoomPoco
            {
                _id = Id,
                game_id = GameId,
                position = Position,
                owner = new POCO.UserBase { _id = Owner.Id, username = Owner.Username },
                description = Description,
                is_advertising = IsAdvertising,
                is_locked = IsLocked,
                password = Password,
                max_member_count = MaxMemberCount,
                member_count = MemberCount,
            };
            return poco;
        }

        public void Update(POCO.GameRoomPoco poco)
        {
            Description = poco.description;
            MaxMemberCount = poco.max_member_count;
            MemberCount = poco.member_count;
            Password = poco.password;
            IsLocked = poco.is_locked;
            IsAdvertising = poco.is_advertising;
            Owner = HomeWindow.UserData.GetPgUser(poco.owner);
            MatchId = poco.match_id;
        }
    }
}
