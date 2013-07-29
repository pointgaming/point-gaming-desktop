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
    class GameRoomWindowModelView : ViewModelBase
    {
        private UserDataManager _userData = HomeWindow.UserData;
        private GameRoom.GameRoomSession _session;
        private ChatManager _manager;

        public GameRoomWindowModelView()
        {
        }

        public void Init(ChatManager manager, GameRoom.GameRoomSession session)
        {
            _manager = manager; // the mediator and messaging service (sort of)
            _session = session; // the model (sort of)

            // socket membership messages trigger on the session, so update room's membership when session members change
            _session.Membership.CollectionChanged += Membership_CollectionChanged;

            _session.GameRoom.PropertyChanged += GameRoom_PropertyChanged;
            _session.MyMatch.PropertyChanged += MyMatch_PropertyChanged;
            _session.ReceivedMessage += ChatMessages_ReceivedMessage;
        }

        public string TeamAvatar
        {
            get { return "http://forums.pointgaming.com/assets/logo-3b643498dc7635d6ce4598843b5fcf0e.png"; }
            set { }
        }

        public string TeamName
        {
            get { return "[team name]"; }
            set { }
        }

        public FlowDocument DescriptionDocument
        {
            get { return _session.GameRoom.DescriptionDocument; }
        }

        private void MyMatch_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("MatchDescription");
        }
        public string MatchDescription
        {
            get
            {
                var match = _session.MyMatch;
                string bettingState = match.IsBetting ? "enabled" : "disabled";
                string description;

                switch (match.State)
                {
                    case MatchState.created:
                        description = string.Format("{0} vs {1} on map {2}. Booking is {3}.", match.Player1Description, match.Player2Description, match.Map, bettingState);
                        break;
                    case MatchState.canceled:
                        description = "Match canceled.";
                        break;
                    case MatchState.started:
                        description = string.Format("{0} vs {1} on map {2}. Match in progress.", match.Player1Description, match.Player2Description, match.Map);
                        break;
                    case MatchState.finalized:
                        description = string.Format("{0} vs {1} on map {2}. {3} won!", match.Player1Description, match.Player2Description, match.Map, match.WinnerDescription);
                        break;
                    default:
                        description = "No match yet.";
                        break;
                }
                return description;
            }
        }

        private FlowDocument _chatDocument;
        private ObservableCollection<Paragraph> _chatMessages = new ObservableCollection<Paragraph>();
        public FlowDocument ChatMessages {
            get
            {
                _chatDocument = new FlowDocument();
                foreach (Paragraph p in _chatMessages) 
                {
                    _chatDocument.Blocks.Add(p);
                }
                return _chatDocument;
            }
        }

        private void ChatMessages_ReceivedMessage(UserBase fromUser, string message)
        {
            AddChatMessage(fromUser.username, message);
        }

        public ICommand SendChat { get { return new ActionCommand<string>(SendChatMessage); } }
        private void SendChatMessage(string messageText)
        {
            if (messageText.Length > 0)
            {
                _session.SendMessage(messageText);
            }
        }

        private void AddChatMessage(string username, string message)
        {
            var time = DateTime.Now;
            string timeString = time.ToString("HH:mm");
            var p = new Paragraph();

            p.Inlines.Add(new Run(timeString + " "));
            p.Inlines.Add(new Bold(new Run(username + ": ")));
            ChatTabCommon.Format(message, p.Inlines);
            _chatMessages.Add(p);
            OnPropertyChanged("ChatMessages");
        }

        public ObservableCollection<Bet> RoomBets
        {
            get { return _session.RoomBets; }
        }

        private ListCollectionView _groupedMembership = new ListCollectionView(new ObservableCollection<PgUser>());
        public ListCollectionView Membership
        {
            get { return _groupedMembership; }
        }

        private void Membership_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _groupedMembership.Refresh();
            _groupedMembership.GroupDescriptions.Clear();

            // placeholder bot group; TODO: get bot(s) from socket API
            if (_session.Membership.Count > 0)
            {
                PgUser botUser = new PgUser();
                botUser.Id = "mock";
                botUser.Username = "dProductions";
                botUser.GroupName = "Game Room Team Bot";
                _groupedMembership.AddNewItem(botUser);
            }

            // team groups
            foreach (PgUser user in _session.Membership)
            {
                PgUser groupedUser = user.ShallowCopy();
                groupedUser.GroupName = groupedUser.HasTeam ? groupedUser.TeamName : "Other (No Team)";
                _groupedMembership.AddNewItem(groupedUser);
            }

            _groupedMembership.CommitNew();
            _groupedMembership.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
            OnPropertyChanged("Membership");
        }

        private void GameRoom_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // route all game room model changes to any MV listeners (i.e. the UI)
            OnPropertyChanged(e);
        }

        public ICommand WindowClosed { get { return new ActionCommand(ExitGameRoom); } }
        public void ExitGameRoom()
        {
            _manager.GameRoomWindowClosed(_session.ChatroomId);
        }

        public ICommand ShowAdmin { get { return new ActionCommand(ShowAdminDialog); } }
        public void ShowAdminDialog()
        {
            _manager.AdminGameRoom(_session.ChatroomId);
        }

        public ICommand ProposeBet { get { return new ActionCommand(ShowBetDialog); } }
        public void ShowBetDialog()
        {
            if (CanBet)
            {
                _manager.OpenBetting(_session.ChatroomId);
            }
            else
            {
                _manager.ShowMessage(_session.ChatroomId, "Cannot Bet", "Betting is not allowed. Current Match State: " + _session.MyMatch.State);
            }
        }

        private bool CanBet
        {
            get
            {
                return _session.GameRoom.IsBetting &&
                       _session.MyMatch.State == MatchState.created;
            }
        }

        public bool IsGameRoomOwner
        {
            get { return _session.GameRoom.Owner.Equals(_userData.User); }
        }

        private bool _canAdmin = false;
        public bool CanAdmin
        {
            get { return _canAdmin; }
        }

        public bool IsAdvertising
        {
            get { return _session.GameRoom.IsAdvertising; }
            set
            {
                var poco = new
                {
                    _id = _session.GameRoom.Id,
                    is_advertising = value,
                };
                _session.GameRoom.IsAdvertising = value;
                _session.SetGameRoomSettings(poco);
                OnPropertyChanged("IsAdvertising");
            }
        }

        private bool _isChatMuted = false;
        public bool IsChatMuted
        {
            get { return _isChatMuted; } // TODO: load mute status from API
            set { }
        }
        
        public ICommand ToggleChatMuted { get { return new ActionCommand(MuteChat); } }
        private void MuteChat(object sender)
        {
            _manager.ShowMessage(_session.ChatroomId, "Mute/Unmute Chat", "TODO: mute chat in socket API");
        }

        private bool _isMicMuted = false;
        public bool IsMicMuted
        {
            get { return _isMicMuted; } // TODO: load mic mute status from API
            set { }
        }

        public ICommand ToggleChatMicMuted { get { return new ActionCommand(MuteMicChat); } }
        private void MuteMicChat(object sender)
        {
            _manager.ShowMessage(_session.ChatroomId, "Mute/Unmute Chat Microphone", "TODO: mute chat microphone");
        }

        private bool _isTeamOnlyChat = false;
        public bool IsTeamOnlyChat
        {
            get { return _isTeamOnlyChat; } // TODO: load team chat status from API
            set { }
        }

        public ICommand ToggleTeamOnlyChat { get { return new ActionCommand(ChatTeamOnly); } }
        private void ChatTeamOnly(object sender)
        {
            _manager.ShowMessage(_session.ChatroomId, "Change Team Only Requirement", "TODO: change chat's team only status in socket API");
        }

        public ICommand CloseChat { get { return new ActionCommand(CloseChatRoom); } }
        private void CloseChatRoom(object sender)
        {
            _manager.ShowMessage(_session.ChatroomId, "Close", "TODO: close chat somehow?");
        }

        public ICommand CheckUserCanAdmin { get { return new ActionCommand(CheckCanAdmin); } }
        private void CheckCanAdmin(object sender)
        {
            _canAdmin = IsGameRoomOwner && !sender.Equals(_userData.User);
            OnPropertyChanged("CanAdmin");
        }

        public ICommand KickUser { get { return new ActionCommand(KickUserFromRoom); } }
        private void KickUserFromRoom(object sender)
        {
        }

        public ICommand BanUser { get { return new ActionCommand(BanUserFromRoom); } }
        private void BanUserFromRoom(object sender)
        {
        }

        public ICommand PromoteUser { get { return new ActionCommand(PromoteUserToRoomOwner); } }
        private void PromoteUserToRoomOwner(object sender)
        {
            PgUser user = sender as PgUser;
            var poco = new
            {
                _id = _session.GameRoom.Id,
                owner_id = user.Id,
            };
            _session.SetGameRoomSettings(poco);
        }
    }
}
