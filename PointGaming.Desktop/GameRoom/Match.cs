using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;


namespace PointGaming.Desktop.GameRoom
{
    public enum MatchState
    {
        @new,
        start,
        cancel,
        finalize,
    }

    public class Match : INotifyPropertyChanged
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

        private string _matchHash;
        public string MatchHash
        {
            get { return _matchHash; }
            set
            {
                if (value == _matchHash)
                    return;
                _matchHash = value;
                NotifyChanged("MatchHash");
            }
        }

        private string _roomId;
        public string RoomId
        {
            get { return _roomId; }
            set
            {
                if (value == _roomId)
                    return;
                _roomId = value;
                NotifyChanged("RoomId");
            }
        }

        private string _roomType;
        public string RoomType
        {
            get { return _roomType; }
            set
            {
                if (value == _roomType)
                    return;
                _roomType = value;
                NotifyChanged("RoomType");
            }
        }
        
        public bool _isBetting;
        public bool IsBetting
        {
            get { return _isBetting; }
            set
            {
                if (value == _isBetting)
                    return;
                _isBetting = value;
                NotifyChanged("IsBetting");
            }
        }

        public MatchState _state;
        public MatchState State
        {
            get { return _state; }
            set
            {
                if (value == _state)
                    return;
                _state = value;
                NotifyChanged("State");
            }
        }

        private string _map;
        public string Map
        {
            get { return _map; }
            set
            {
                if (value == _map)
                    return;
                _map = value;
                NotifyChanged("Map");
            }
        }

        private IBetOperand _player1;
        public IBetOperand Player1
        {
            get { return _player1; }
            set
            {
                if (value == _player1)
                    return;
                _player1 = value;
                NotifyChanged("Player1");
            }
        }

        private IBetOperand _player2;
        public IBetOperand Player2
        {
            get { return _player2; }
            set
            {
                if (value == _player2)
                    return;
                _player2 = value;
                NotifyChanged("Player2");
            }
        }

        private IBetOperand _winner;
        public IBetOperand Winner
        {
            get { return _winner; }
            set
            {
                if (value == _winner)
                    return;
                _winner = value;
                NotifyChanged("Winner");
            }
        }

        public void Update(UserDataManager manager, POCO.MatchPoco poco)
        {
            Id = poco._id;
            RoomId = poco.room_id;
            RoomType = poco.room_type;
            IsBetting = poco.betting;
            State = (MatchState)Enum.Parse(typeof(MatchState), poco.state);
            Map = poco.map;

            Player1 = GetBetOperand(manager, poco.player_1_type, poco.player_1_id, poco.player_1_name);
            Player2 = GetBetOperand(manager, poco.player_2_type, poco.player_2_id, poco.player_2_name);
            
            if (string.IsNullOrWhiteSpace(poco.winner_id))
                Winner = null;
            else
            {
                if (poco.winner_id == Player1.Id)
                    Winner = Player1;
                else if (poco.winner_id == Player2.Id)
                    Winner = Player2;
            }
        }

        private static IBetOperand GetBetOperand(UserDataManager manager, string type, string id, string name)
        {
            IBetOperand player = null;
            if (type == "Team")
            {
                player = manager.GetPgTeam(new POCO.TeamBase { _id = id, name = name });
            }
            else if (type == "User")
            {
                player = manager.GetPgUser(new POCO.UserBase { _id = id, username = name });
            }
            else
                throw new Exception("Player type " + type + " not recognized.");
            return player;
        }
    }
}
