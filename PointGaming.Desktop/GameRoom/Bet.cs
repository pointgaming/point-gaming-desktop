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
        bookie_won,
        better_won,
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
        private Match _myMatch;
        public Match MyMatch
        {
            get { return _myMatch; }
            set
            {
                if (value == _myMatch)
                    return;
                _myMatch = value;
                NotifyChanged("MyMatch");
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

        private IBetOperand _bookerChoice;
        public IBetOperand BookerChoice
        {
            get { return _bookerChoice; }
            set
            {
                if (value == _bookerChoice)
                    return;
                _bookerChoice = value;
                NotifyChanged("BookerChoice");
            }
        }
        private IBetOperand _betterChoice;
        public IBetOperand BetterChoice
        {
            get { return _betterChoice; }
            set
            {
                if (value == _betterChoice)
                    return;
                _betterChoice = value;
                NotifyChanged("BetterChoice");
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

        public Bet() { }

        public Bet(UserDataManager manager, Match match, POCO.BetPoco poco)
        {
            Id = poco._id;
            MyMatch = match;
            SetOutcome(poco.outcome);

            if (poco.winner_id == match.Player1.Id)
            {
                BookerChoice = match.Player1;
                BetterChoice = match.Player2;
            }
            else
            {
                BookerChoice = match.Player2;
                BetterChoice = match.Player1;
            }

            Booker = manager.GetPgUser(poco.bookie);
            Odds = poco.bookie_odds;
            Wager = poco.bookie_wager;

            if (poco.better != null && !string.IsNullOrWhiteSpace(poco.better._id))
                Better = manager.GetPgUser(poco.better);
        }

        public void SetOutcome(string outcome)
        {
            try
            {
                BetOutcome = (BetOutcome)Enum.Parse(typeof(BetOutcome), outcome);
            }
            catch { BetOutcome = GameRoom.BetOutcome.undetermined; }
        }

        public void AcceptedBy(PgUser user)
        {
            Better = user;
        }
    }
}
