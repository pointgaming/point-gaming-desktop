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
        private SessionManager _manager;

        public GameRoomAdminDialogModelView()
        {
        }

        public void Init(SessionManager manager, GameRoom.GameRoomSession session)
        {
            _manager = manager;
            _session = session;

            _description = _session.GameRoom.Description;
            _password = _session.GameRoom.Password;
            _isBetting = _session.GameRoom.IsBetting;
            _isTeamBotPlaced = _session.GameRoom.IsTeamBotPlaced;
            _bettingType = _session.GameRoom.BettingType == null ? "1v1" : _session.GameRoom.BettingType;
            _isAdvertising = _session.GameRoom.IsAdvertising;

            PrepareHoldControl();
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
            set { }
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
        private bool _shouldSendHoldRequest;
        private bool _isTeamBotPlaced;
        public bool IsTeamBotPlaced
        {
            get { return _isTeamBotPlaced; }
            set
            {
                if (value == _isTeamBotPlaced)
                    return;
                _isTeamBotPlaced = value;
                _shouldSendHoldRequest = !_shouldSendHoldRequest;
                OnPropertyChanged("IsTeamBotPlaced");
            }
        }

        private bool _canHold;
        public bool CanHold
        {
            get { return _canHold; }
            set
            {
                if (value == _canHold)
                    return;
                _canHold = value;
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

        public void UpdateGameRoomSettings()
        {
            var poco = new
            {
                _id = _session.GameRoom.Id,
                description = Description,
                is_advertising = IsAdvertising,
                password = Password,
                betting_type = _bettingType
            };
            _session.SetGameRoomSettings(poco);

            // TODO: set team bot if bot assignment has changed.
            if (_shouldSendHoldRequest)
            {
                var server_action = IsTeamBotPlaced == true ? "/hold" : "/unhold";
                var url = UserDataManager.UserData.PgSession.GetWebAppFunction("/api", "/game_rooms/" + poco._id + server_action);
                var client = new RestSharp.RestClient(url);
                var request = new RestSharp.RestRequest(RestSharp.Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
                RestSharp.RestResponse response = (RestSharp.RestResponse)client.Execute(request);
            }
        }

        public void PrepareHoldControl()
        {
            var canHoldUrl = UserDataManager.UserData.PgSession.GetWebAppFunction("/api", "/game_rooms/" + _session.GameRoomId + "/can_hold");
            var client = new RestSharp.RestClient(canHoldUrl);
            var request = new RestSharp.RestRequest(RestSharp.Method.GET);
            RestSharp.RestResponse response = (RestSharp.RestResponse)client.Execute(request);
            var result = Newtonsoft.Json.Linq.JObject.Parse(response.Content);

            this.CanHold = Convert.ToBoolean(((Newtonsoft.Json.Linq.JProperty)result.First).Value.ToString());
            this.IsTeamBotPlaced = Convert.ToBoolean(((Newtonsoft.Json.Linq.JProperty)result.First.Next).Value.ToString());

            _shouldSendHoldRequest = false;
        }

    }
}
