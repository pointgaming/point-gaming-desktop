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

        public MatchState _matchState;
        public MatchState MatchState
        {
            get { return _matchState; }
            set
            {
                if (value == _matchState)
                    return;
                _matchState = value;
                NotifyChanged("MatchState");
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
    }
}
