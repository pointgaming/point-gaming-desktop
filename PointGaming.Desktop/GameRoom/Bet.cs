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

        private string _matchHash;
        public string MatchHash
        {
            get { return _matchHash; }
            set
            {
                if (value == _matchHash)
                    return;
                _matchHash = value;
                NotifyChanged("MatchHash");
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

        private IBetOperand _offererChoice;
        public IBetOperand OffererChoice
        {
            get { return _offererChoice; }
            set
            {
                if (value == _offererChoice)
                    return;
                _offererChoice = value;
                NotifyChanged("OffererChoice");
            }
        }
        private IBetOperand _takerChoice;
        public IBetOperand TakerChoice
        {
            get { return _takerChoice; }
            set
            {
                if (value == _takerChoice)
                    return;
                _takerChoice = value;
                NotifyChanged("TakerChoice");
            }
        }

        private string _offererOdds = "1:1";
        public string OffererOdds
        {
            get { return _offererOdds; }
            set
            {
                if (value == _offererOdds)
                    return;
                _offererOdds = value;
                NotifyChanged("OffererOdds");
                NotifyChanged("TakerOdds");
                NotifyChanged("OffererReward");
                NotifyChanged("TakerWager");
                NotifyChanged("TakerReward");
            }
        }
        private decimal _offererWager;
        public decimal OffererWager
        {
            get { return _offererWager; }
            set
            {
                if (value == _offererWager)
                    return;
                _offererWager = value;
                NotifyChanged("OffererWager");
                NotifyChanged("OffererReward");
                NotifyChanged("TakerWager");
                NotifyChanged("TakerReward");
            }
        }

        private PgUser _offerer;
        public PgUser Offerer
        {
            get { return _offerer; }
            set
            {
                if (value == _offerer)
                    return;
                _offerer = value;
                NotifyChanged("Offerer");
            }
        }
        private PgUser _taker;
        public PgUser Taker
        {
            get { return _taker; }
            set
            {
                if (value == _taker)
                    return;
                _taker = value;
                NotifyChanged("Taker");
            }
        }

        public decimal OffererReward { get { return Math.Floor(OffererWager * OffererMultiplier); } }
        public decimal TakerReward { get { return OffererWager; } }
        public decimal TakerWager { get { return OffererReward; } }

        public string TakerOdds
        {
            get
            {
                var oddsSplit = OffererOdds.Split(':');
                return oddsSplit[1] + ":" + oddsSplit[0];
            }
        }

        private decimal OffererMultiplier
        {
            get
            {
                var oddsSplit = OffererOdds.Split(':');
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
            MatchHash = poco.match_hash;

            SetOutcome(poco.outcome);

            if (poco.offerer_choice_id == match.Player1.Id)
            {
                OffererChoice = match.Player1;
                TakerChoice = match.Player2;
            }
            else
            {
                OffererChoice = match.Player2;
                TakerChoice = match.Player1;
            }

            Offerer = manager.GetPgUser(poco.offerer);
            OffererOdds = poco.offerer_odds;
            OffererWager = poco.offerer_wager;

            if (poco.taker != null && !string.IsNullOrWhiteSpace(poco.taker._id))
                Taker = manager.GetPgUser(poco.taker);
        }

        public POCO.BetPoco ToPoco()
        {
            var poco = new POCO.BetPoco
            {
                match_hash = MyMatch.MatchHash,
                offerer_choice_id = OffererChoice.Id,
                offerer_choice_name = OffererChoice.ShortDescription,
                offerer_choice_type = OffererChoice.PocoType,
                taker_choice_id = TakerChoice.Id,
                taker_choice_name = TakerChoice.ShortDescription,
                taker_choice_type = TakerChoice.PocoType,
                offerer_wager = OffererWager,
                offerer_odds = OffererOdds,
            };
            return poco;
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
            Taker = user;
        }
    }
}
