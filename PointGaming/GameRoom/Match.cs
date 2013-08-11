using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;


namespace PointGaming.GameRoom
{
    public enum MatchState
    {
        invalid,
        create,
        created,
        start,
        started,
        cancel,
        canceled,
        finalize,
        finalized,
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


        private bool _isEditable = true;
        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                if (value == _isEditable)
                    return;
                _isEditable = value;
                NotifyChanged("IsEditable");
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

        public MatchState _state = MatchState.created;
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
        public string Player1Description
        {
            get { return Player1 == null ? "" : Player1.ShortDescription; }
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
        public string Player2Description
        {
            get { return Player2 == null ? "" : Player2.ShortDescription; }
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
        public string WinnerDescription
        {
            get { return Winner == null ? "" : Winner.ShortDescription; }
        }

        public void Update(UserDataManager manager, POCO.MatchPoco poco)
        {
            Id = poco._id;
            MatchHash = poco.match_hash;
            GameId = poco.game_id;
            RoomId = poco.room_id;
            RoomType = poco.room_type;
            IsBetting = poco.betting;
            Map = poco.map;
            
            if (poco.state == "new")
                State = MatchState.created;
            else if (poco.state == "started")
                State = MatchState.started;
            else if (poco.state == "cancelled")
                State = MatchState.canceled;
            else if (poco.state == "finalized")
                State = MatchState.finalized;
            else
            {
                State = MatchState.invalid;
                throw new Exception("Unrecognized state: " + poco.state);
            }

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
            if (id == null) return null;

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

        public Match()
        {
        }
        
        public Match(POCO.MatchPoco poco){
            Map = poco.map;
        }

        public POCO.MatchPoco ToPoco()
        {
            var poco = new POCO.MatchPoco
            {
                map = Map
            };
            return poco;
        }
    }
}
