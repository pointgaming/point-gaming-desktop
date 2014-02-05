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
    class GameRoomWindowModelView : ViewModelBase, Voice.IVoiceRoom
    {
        private UserDataManager _userData = UserDataManager.UserData;
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
            _session.RoomBets.CollectionChanged += RoomBets_CollectionChanged;
            _session.MyMatch.PropertyChanged += MyMatch_PropertyChanged;
            _session.ChatMessageReceived += ChatMessages_CollectionChanged;
            
            InitMembership();
            _userData.Voip.JoinRoom(this);
        }

        private void InitMembership()
        {
            Membership.GroupDescriptions.Add(new PropertyGroupDescription("GameRoomGroupName"));
            Membership.CustomSort = PgUser.GetGameRoomMemberSorter();

            foreach (var item in _session.Membership)
                _groupedMembershipList.Add(item);

            CheckBots();
            SynchronizeMembersList();
            SetMembershipCount();
        }
                
        public string DisplayName { get { return _session.GameRoom.DisplayName; } }

        public string TeamAvatar
        {
            get { return "http://forums.pointgaming.com/assets/logo-3b643498dc7635d6ce4598843b5fcf0e.png"; }
            set { }
        }

        public string OwnerName
        {
            get { return _session.GameRoom.Owner.DisplayName; }
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

        private void ChatMessages_CollectionChanged(ChatMessage item)
        {
            AddChatMessage(item.Author.Username, item.Message);
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

        private void RoomBets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("RoomBets");
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
            SynchronizeMembersList();
            Membership.Refresh();
            SetMembershipCount();
        }

        private void SynchronizeMembersList()
        {
            PgUser[] newList = new PgUser[Membership.Count];
            int i = 0;
            foreach (var item in Membership)
                if (((PgUser)item).GameRoomGroupName != "Team Bot")
                    newList[i++] = (PgUser)item;
            this._session.GameRoom.Members = newList;
        }

        private void CheckBots()
        {
            LoadTeamBot();
            if ( _session.GameRoom.IsTeamBotPlaced == true)
            {
                var teamBot = _session.GameRoom.TeamBot;
                if (_teamBots.Count == 0)
                {
                    _teamBots.Add(teamBot);
                    _groupedMembershipList.Add(teamBot);
                }
            }
            else if (_teamBots.Count > 0)
            {
                var teamBot = _teamBots[0];
                _teamBots.RemoveAt(0);
                _groupedMembershipList.Remove(teamBot);
            }

        }

        public static string TeamBotGroupName
        {
            get { return "Team Bot"; }
        }

        private void LoadTeamBot()
        {
            var url = UserDataManager.UserData.PgSession.GetWebAppFunction("/api", "/game_rooms/" + this._session.GameRoom.Id + "/team_bot");
            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            RestSharp.RestResponse response = (RestSharp.RestResponse)client.Execute(request);
            var result = Newtonsoft.Json.Linq.JObject.Parse(response.Content);
            if (((Newtonsoft.Json.Linq.JProperty)result.First).Name != "errors")
            {
                var teamBot = new PgUser();
                var name = ((Newtonsoft.Json.Linq.JProperty)result.First).Value.ToString();
                var team = new PgTeam();
                team.Name = GameRoomWindowModelView.TeamBotGroupName;
                teamBot.Id = "bot_" + name;
                teamBot.Username = name;
                teamBot.Points = Convert.ToInt32(((Newtonsoft.Json.Linq.JProperty)result.First.Next).Value.ToString());
                teamBot.Team = team;
                _session.GameRoom.TeamBot = teamBot;
                _session.GameRoom.IsTeamBotPlaced = true;
            }
            else
                _session.GameRoom.IsTeamBotPlaced = false;
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
            _userData.Voip.LeaveRoom(this);
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
                CheckBots();
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

        public ICommand ViewBet { get { return new ActionCommand<Bet>(ViewBetDetails); } }
        public void ViewBetDetails(Bet bet)
        {
            _manager.ViewBetting(_session.ChatroomId, bet);
        }

        public ICommand CancelBet { get { return new ActionCommand<Bet>(CancelGameRoomBet); } }
        public void CancelGameRoomBet(Bet bet)
        {
            _session.CancelBet(bet);
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

        public ICommand Activated { get { return new ActionCommand(ActivatedF); } }
        private void ActivatedF(object sender)
        {
            _userData.Voip.TakeMicrophoneFocus(this);
        }

        public string AudioRoomId { get { return _session.GameRoomId; } }
        public bool IsVoiceEnabled { get { return !IsVoiceMuted; } }

        public void OnVoiceStarted(PgUser user)
        {
            user.SpeakingRoomId = AudioRoomId;
        }
        public void OnVoiceStopped(PgUser user)
        {
            if (user.SpeakingRoomId == AudioRoomId)
                user.SpeakingRoomId = null;
        }
        public void OnVoiceConnectionChanged(bool isConnected)
        {
            IsVoiceConnected = isConnected;
        }

        private bool _IsVoiceConnected = false;
        public bool IsVoiceConnected
        {
            get { return _IsVoiceConnected; }
            set
            {
                SetProperty(ref _IsVoiceConnected, value, () => IsVoiceConnected);
            }
        }

        private bool _isVoiceMuted = false;
        public bool IsVoiceMuted
        {
            get { return _isVoiceMuted; }
            set
            {
                SetProperty(ref _isVoiceMuted, value, () => IsVoiceMuted);
            }
        }

        private bool _isVoiceTeamOnly = false;
        public bool IsVoiceTeamOnly
        {
            get { return _isVoiceTeamOnly; }
            set
            {
                SetProperty(ref _isVoiceTeamOnly, value, () => IsVoiceTeamOnly);
            }
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

        public ICommand MuteUser { get { return new ActionCommand(MuteUserF); } }
        private void MuteUserF(object sender)
        {
            PgUser user = sender as PgUser;
            user.IsMuted = !user.IsMuted;
        }
    }


    public class GameRoomSpeakingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string speakingRoomId = (string)values[0];
            bool isMuted = (bool)values[1];
            string audioRoomId = (string)values[2];
            bool isConnected = (bool)values[3];

            var isSpeaking = speakingRoomId == audioRoomId;
            var assembly = typeof(GameRoomSpeakingConverter).Assembly;
            var defaultUri = "pack://application:,,,/" + assembly.GetName().Name + ";component/Resources/";

            if (!isConnected)
            {
                defaultUri += "VoiceDisabled.png";
            }
            else if (isSpeaking)
            {
                if (isMuted)
                    defaultUri += "VoiceMutedTalking.png";
                else
                    defaultUri += "VoiceEnabledTalking.png";
            }
            else
            {
                if (isMuted)
                    defaultUri += "VoiceMuted.png";
                else
                    defaultUri += "VoiceEnabled.png";
            }

            var source = new System.Windows.Media.ImageSourceConverter().ConvertFromString(defaultUri) as System.Windows.Media.ImageSource;
            return source;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
