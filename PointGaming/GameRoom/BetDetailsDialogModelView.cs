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
    class BetDetailsDialogModelView : ViewModelBase
    {
        private GameRoom.GameRoomSession _session;
        private SessionManager _manager;
        private Bet _bet;

        public BetDetailsDialogModelView()
        {
        }
        
        public void Init(SessionManager manager, GameRoom.GameRoomSession session, Bet bet)
        {
            _manager = manager;
            _session = session;
            _bet = bet;
        }

        public string ProposedBy
        {
            get { return "Proposed by: " + _bet.MyMatch.Player1Description; }
        }

        public string Map
        {
            get { return "Map: " + _bet.MyMatch.Map; }
        }

        public string Amount
        {
            get { return "Amount: " + _bet.TakerRewardAmount; }
        }

        public string Odds
        {
            get { return "Odds: " + _bet.OffererOdds; }
        }

        public string RiskAmount
        {
            get { return "Risk Amount: " + _bet.OffererOdds; }
        }

        public string WinAmount
        {
            get { return "Win Amount: " + _bet.OffererOdds; }
        }

        public bool IsTeamBet
        {
            get { return _bet.MyMatch.RoomType == "Team"; }
        }

        public bool CanAdmin
        {
            get
            {
                return _bet.IsAdministratable;
            }
        }

        public ICommand AcceptBet { get { return new ActionCommand(AcceptGameRoomBet); } }
        private void AcceptGameRoomBet(object sender)
        {
        }

        public ICommand RemoveUser { get { return new ActionCommand(RemoveTeamMemberFromBet); } }
        private void RemoveTeamMemberFromBet(object sender)
        {
        }
    }
}
