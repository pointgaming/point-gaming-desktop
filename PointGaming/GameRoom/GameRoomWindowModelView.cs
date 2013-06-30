using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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

            // players group
            foreach (PgUser user in _session.Membership)
            {
                PgUser groupedUser = user.ShallowCopy();
                groupedUser.GroupName = "Players";
                _groupedMembership.AddNewItem(groupedUser);
            }

            // admin group
            foreach (PgUser user in _session.Membership)
            {
                if (!user.IsAdmin) continue;

                PgUser groupedUser = user.ShallowCopy();
                groupedUser.GroupName = "Admin";
                _groupedMembership.AddNewItem(groupedUser);
            }

            // friends group
            foreach (PgUser user in _session.Membership)
            {
                if (!_userData.IsFriend(user.Id)) continue;

                PgUser groupedUser = user.ShallowCopy();
                groupedUser.GroupName = "Friends";
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
            if (!CanBet) return;

            _manager.OpenBetting(_session.ChatroomId);
        }
        private bool CanBet
        {
            get
            {
                return _session.MyMatch.IsBetting && 
                       _session.MyMatch.State == MatchState.created;
            }
        }
    }
}
