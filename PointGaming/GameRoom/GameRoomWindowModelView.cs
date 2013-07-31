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
    class GameRoomWindowModelView : ViewModelBase
    {
        private UserDataManager _userData = HomeWindow.UserData;
        private GameRoom.GameRoomSession _session;
        private SessionManager _manager;

        private ObservableCollection<PgUser> _groupedMembershipList = new ObservableCollection<PgUser>();
        private ActiveGroupingCollectionView _groupedMembership;
        public ActiveGroupingCollectionView Membership
        {
            get {
                if (_groupedMembership == null)
                    _groupedMembership = new ActiveGroupingCollectionView(_groupedMembershipList);
                return _groupedMembership; 
            }
        }

        private string _MembershipCount = "Total (0)";
        public string MembershipCount
        {
            get { return _MembershipCount; }
            set
            {
                if (value == _MembershipCount)
                    return;
                _MembershipCount = value;
                OnPropertyChanged("MembershipCount");
            }
        }

        public GameRoomWindowModelView()
        {
        }

        public void Init(SessionManager manager, GameRoom.GameRoomSession session)
        {
            _manager = manager; // the mediator and messaging service (sort of)
            _session = session; // the model (sort of)

            // socket membership messages trigger on the session, so update room's membership when session members change
            _session.Membership.CollectionChanged += Membership_CollectionChanged;
            _session.GameRoom.PropertyChanged += GameRoom_PropertyChanged;
            _session.MyMatch.PropertyChanged += MyMatch_PropertyChanged;
            _session.ChatMessages.CollectionChanged += ChatMessages_CollectionChanged;
            
            InitMembership();
        }

        private void InitMembership()
        {
            Membership.GroupDescriptions.Add(new PropertyGroupDescription("GameRoomGroupName"));
            Membership.CustomSort = PgUser.GetGameRoomMemberSorter();

            foreach (var item in _session.Membership)
                _groupedMembershipList.Add(item);

            CheckBots();

            SetMembershipCount();
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

        private void ChatMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (ChatMessage item in e.NewItems)
            {
                AddChatMessage(item.Author.Username, item.Message);
            }
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
            ChatCommon.Format(message, p.Inlines);
            _chatMessages.Add(p);
            OnPropertyChanged("ChatMessages");
        }

        public ObservableCollection<Bet> RoomBets
        {
            get { return _session.RoomBets; }
        }

        private readonly List<PgUser> _teamBots = new List<PgUser>();

        private void Membership_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    _groupedMembershipList.Remove(item as PgUser);
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    _groupedMembershipList.Add(item as PgUser);
            
            CheckBots();

            SetMembershipCount();
        }

        private void CheckBots()
        {
            List<PgUser> removes = new List<PgUser>();

            foreach (var teamBot in _teamBots)
            {
                bool shouldPlace = false;
                foreach (var user in _session.Membership)
                {
                    if (user.HasTeam && user.Team == teamBot.Team)
                    {
                        shouldPlace = true;
                        break;
                    }
                }
                if (!shouldPlace)
                    removes.Add(teamBot);
            }

            foreach (var bot in removes)
            {
                _teamBots.Remove(bot);
                _groupedMembershipList.Remove(bot);
            }

            foreach (var user in _session.Membership)
            {
                if (!user.HasTeam)
                    continue;

                bool shouldPlace = true;
                foreach (var teamBot in _teamBots)
                {
                    if (user.Team == teamBot.Team)
                    {
                        shouldPlace = false;
                        break;
                    }
                }
                if (shouldPlace)
                {
                    var newBot = CreateBotForTeam(user);
                    _teamBots.Add(newBot);
                    _groupedMembershipList.Add(newBot);
                }
            }
        }

        private PgUser CreateBotForTeam(PgUser user)
        {
            // placeholder bot group; TODO: get bot(s) from socket API
            PgUser botUser = new PgUser();
            botUser.Id = "bot_" + user.Team.Id;
            botUser.Username = "bot_" + user.Team.Name;
            botUser.Team = user.Team;
            return botUser;
        }

        private void SetMembershipCount()
        {
            MembershipCount = "Total (" + _session.Membership.Count + ")";
        }

        private void GameRoom_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // route all game room model changes to any MV listeners (i.e. the UI)
            OnPropertyChanged(e);
        }

        public ICommand WindowClosed { get { return new ActionCommand(ExitGameRoom); } }
        public void ExitGameRoom()
        {
            _session.Leave();
        }

        public ICommand WindowStateChanged { get { return new ActionCommand(HandleWindowState); } }
        public void HandleWindowState()
        {
            var window = _session.Window;
            if (window.WindowState == System.Windows.WindowState.Minimized)
            {
                // DisableProcessing prevents a flicker activate of another window
                using (var d = window.Dispatcher.DisableProcessing())
                {
                    _session.ShowLobby(true);
                    window.Hide();
                }
            }
            else
                window.Show();
        }

        public ICommand ShowAdmin { get { return new ActionCommand(ShowAdminDialog); } }
        public void ShowAdminDialog()
        {
            if (IsGameRoomOwner)
            {
                _manager.AdminGameRoom(_session.ChatroomId);
            }
            else
            {
                _manager.ShowMessage(_session.ChatroomId, "Admin Game Room", "Only the game room's owner can admin this room. Take over the room for admin rights.");
            }
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
                return _session.MyMatch.State == MatchState.created;
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
