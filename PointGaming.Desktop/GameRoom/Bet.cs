using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace PointGaming.Desktop.GameRoom
{
    public enum BetOutcome
    {
        undetermined,
        aWins,
        bWins,
        draw,
        canceled,
    }

    public class Bet : INotifyPropertyChanged
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
        private string _matchId;
        public string MatchId
        {
            get { return _matchId; }
            set
            {
                if (value == _matchId)
                    return;
                _matchId = value;
                NotifyChanged("MatchId");
            }
        }
        private BetOutcome _betOutcome;
        public BetOutcome BetOutcome
        {
            get { return _betOutcome; }
            set
            {
                if (value == _betOutcome)
                    return;
                _betOutcome = value;
                NotifyChanged("BetOutcome");
            }
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
        private IBetOperand _loser;
        public IBetOperand Loser
        {
            get { return _loser; }
            set
            {
                if (value == _loser)
                    return;
                _loser = value;
                NotifyChanged("Loser");
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

        private string _odds = "1:1";
        public string Odds
        {
            get { return _odds; }
            set
            {
                if (value == _odds)
                    return;
                _odds = value;
                NotifyChanged("Odds");
                NotifyChanged("LoserOdds");
                NotifyChanged("BookerWager");
                NotifyChanged("BetterWager");
                NotifyChanged("BookerReward");
                NotifyChanged("BetterReward");
            }
        }
        private decimal _wager;
        public decimal Wager
        {
            get { return _wager; }
            set
            {
                if (value == _wager)
                    return;
                _wager = value;
                NotifyChanged("Wager");
                NotifyChanged("BookerWager");
                NotifyChanged("BetterWager");
                NotifyChanged("BookerReward");
                NotifyChanged("BetterReward");
            }
        }

        private PgUser _booker;
        public PgUser Booker
        {
            get { return _booker; }
            set
            {
                if (value == _booker)
                    return;
                _booker = value;
                NotifyChanged("Booker");
            }
        }
        private PgUser _better;
        public PgUser Better
        {
            get { return _better; }
            set
            {
                if (value == _better)
                    return;
                _better = value;
                NotifyChanged("Better");
            }
        }

        public decimal BookerReward { get { return Math.Floor(Wager * BookerMultiplier); } }
        public decimal BookerWager { get { return Wager; } }
        public decimal BetterReward { get { return Wager; } }
        public decimal BetterWager { get { return Math.Floor(Wager * BookerMultiplier); } }

        public string BetterOdds
        {
            get
            {
                var oddsSplit = Odds.Split(':');
                return oddsSplit[1] + ":" + oddsSplit[0];
            }
        }

        private decimal BookerMultiplier
        {
            get
            {
                var oddsSplit = Odds.Split(':');
                var winnerChance = decimal.Parse(oddsSplit[0].Trim());
                var loserChance = decimal.Parse(oddsSplit[1].Trim());
                var multiplier = loserChance / winnerChance;
                return multiplier;
            }
        }
    }
}
