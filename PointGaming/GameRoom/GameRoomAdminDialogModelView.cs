﻿using System;
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
            _members = _session.GameRoom.Members;
            //temporary admins view does not include owner cause he cannot be disabled now, later admins must be initialized with complete list
            //_admins = session.GameRoom.Admins;
            _admins = session.GameRoom.AdminsWithoutOwner;
            _notAdmins = session.GameRoom.MembersNotAdmins;

            PrepareControls();
        }

        public string GameRoomTitle
        {
            get { return _session.GameRoom.DisplayName; }
        }

        private bool _shouldSendPasswordDescriptionRequest;
        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                if (value == _description)
                    return;
                _description = value;
                _shouldSendPasswordDescriptionRequest = !_shouldSendPasswordDescriptionRequest;
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
                _shouldSendPasswordDescriptionRequest = !_shouldSendPasswordDescriptionRequest;
                OnPropertyChanged("Password");
            }
        }

        private bool _shouldSendAdvertisingRequest;
        private bool _isAdvertising;
        public bool IsAdvertising
        {
            get { return _isAdvertising; }
            set
            {
                if (value == _isAdvertising)
                    return;
                _isAdvertising = value;
                _shouldSendAdvertisingRequest = !_shouldSendAdvertisingRequest;
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

        private PgUser[] _members;
        public PgUser[] Members
        {
            get { return _members; }
            set
            {
                if (value == _members)
                    return;
                _members = value;
            }
        }

        private PgUser[] _admins;
        public PgUser[] Admins
        {
            get { return _admins; }
            set
            {
                if (value == _admins)
                    return;
                _admins = value;
            }
        }

        private PgUser[] _notAdmins;
        public PgUser[] NotAdmins
        {
            get { return _notAdmins; }
        }

        public void UpdateGameRoomSettings()
        {
            string[] adminsIds = new string[Admins.Length];
            for (int i = 0; i < adminsIds.Length; i++)
                adminsIds[i] = Admins[i].Id;
            _session.GameRoom.Admins = Admins;
            var poco = new
            {
                _id = _session.GameRoom.Id,
                description = Description,
                is_advertising = IsAdvertising,
                password = Password,
                betting_type = _bettingType,
                is_team_bot_placed = IsTeamBotPlaced,
                admins = adminsIds
            };
            // TO DO: remove true when the smart logic for drag&drop is done
            if (_shouldSendAdvertisingRequest || _shouldSendHoldRequest || _shouldSendPasswordDescriptionRequest || true)
                _session.SetGameRoomSettings(poco);
        }

        public void PrepareControls()
        {
            var settingsUrl = UserDataManager.UserData.PgSession.GetWebAppFunction("/api", "/game_rooms/" + _session.GameRoomId + "/settings");
            var client = new RestSharp.RestClient(settingsUrl);
            var request = new RestSharp.RestRequest(RestSharp.Method.GET);
            RestSharp.RestResponse response = (RestSharp.RestResponse)client.Execute(request);

            var result = Newtonsoft.Json.Linq.JObject.Parse(response.Content);
            this.CanHold = Convert.ToBoolean(((Newtonsoft.Json.Linq.JProperty)result.First).Value.ToString());
            this.IsTeamBotPlaced = Convert.ToBoolean(((Newtonsoft.Json.Linq.JProperty)result.First.Next).Value.ToString());
            this.IsAdvertising = Convert.ToBoolean(((Newtonsoft.Json.Linq.JProperty)result.First.Next.Next).Value.ToString());
            _shouldSendHoldRequest = _shouldSendAdvertisingRequest = _shouldSendPasswordDescriptionRequest = false;
        }
    }
}
