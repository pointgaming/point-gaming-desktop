using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Documents;
using System.ComponentModel;
using PointGaming;
using PointGaming.POCO;
using PointGaming.Chat;
using Microsoft.Expression.Interactivity.Core;

namespace PointGaming.GameRoom
{
    class GameRoomAdminDialogModelView : ViewModelBase
    {
        private GameRoom.GameRoomSession _session;
        private ChatManager _manager;

        public GameRoomAdminDialogModelView()
        {
        }

        public void Init(ChatManager manager, GameRoom.GameRoomSession session)
        {
            _manager = manager;
            _session = session;

            _description = _session.GameRoom.Description;
            _password = _session.GameRoom.Password;
            _isBetting = _session.GameRoom.IsBetting;
            _bettingType = _session.GameRoom.BettingType == null ? "1v1" : _session.GameRoom.BettingType;
            _isAdvertising = _session.GameRoom.IsAdvertising;
        }

        public string GameRoomTitle
        {
            get { return _session.GameRoom.DisplayName; }
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
                OnPropertyChanged("Description");
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                if (value == _password)
                    return;
                _password = value;
                OnPropertyChanged("Password");
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
                OnPropertyChanged("IsAdvertising");
            }
        }

        private bool _isBetting;
        public bool IsBetting
        {
            get { return _isBetting; }
            set
            {
                if (value == _isBetting)
                    return;
                _isBetting = value;
                OnPropertyChanged("IsBetting");
            }
        }

        private string _bettingType;
        public bool IsTeamBetting
        {
            get { return _bettingType == "team"; }
            set
            {
                _bettingType = "team";
                OnPropertyChanged("BettingType");
                OnPropertyChanged("IsOneOnOneBetting");
                OnPropertyChanged("IsTeamBetting");
            }
        }

        public bool IsOneOnOneBetting
        {
            get { return _bettingType == "1v1"; }
            set
            {
                _bettingType = "1v1";
                OnPropertyChanged("BettingType");
                OnPropertyChanged("IsOneOnOneBetting");
                OnPropertyChanged("IsTeamBetting");
            }
        }

        public ICommand WindowClosed { get { return new ActionCommand(UpdateGameRoomSettings); } }
        public void UpdateGameRoomSettings()
        {
            var poco = new
            {
                _id = _session.GameRoom.Id,
                description = Description,
                is_advertising = IsAdvertising,
                betting = IsBetting,
                password = Password,
                bettingType = _bettingType
            };
            _session.SetGameRoomSettings(poco);
        }

    }
}
