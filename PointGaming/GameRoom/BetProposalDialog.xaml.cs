using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PointGaming.GameRoom
{
    public partial class BetProposalDialog : Window, INotifyPropertyChanged
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

        private decimal _wager = 0;
        public decimal Wager
        {
            get { return _wager; }
            set
            {
                if (value == _wager)
                    return;
                if (value < 0)
                    throw new Exception("Wager must be > 0");
                if (Math.Floor(value) != value)
                    throw new Exception("Wager must be a whole number");
                _wager = value;
                NotifyChanged("Wager");
                NotifyChanged("CanPlaceBet");
                UpdateSummary();
            }
        }

        public bool CanPlaceBet
        {
            get 
            {
                return _wager > 0 && _mapName.Length > 0; 
            }
            set { }
        }

        public string OffererChoice
        {
            get
            {
                if (IsTeamBetting)
                {
                    if (HomeWindow.UserData.User.HasTeam)
                    {
                        return "Team 1 (" + HomeWindow.UserData.User.TeamName + ")";
                    }
                    else
                    {
                        return "NO AVAILABLE TEAM!";
                    }
                }
                else
                {
                    return "Player 1 (" + HomeWindow.UserData.User.DisplayName+")";
                }
            }
        }

        private string _mapName = "";
        public string MapName
        {
            get { return _mapName; }
            set
            {
                if (value == _mapName)
                    return;
                _mapName = value;
                NotifyChanged("MapName");
                NotifyChanged("CanPlaceBet");
            }
        }

        private string BettingType = "1v1";
        private bool IsOneOnOneBetting
        {
            get { return BettingType == "1v1"; }
        }
        private bool IsTeamBetting
        {
            get { return BettingType == "team"; }
        }

        public BetProposalDialog(string bettingType)
        {
            InitializeComponent();

            if (bettingType.Equals("team")) BettingType = "team";
            NotifyChanged("OffererChoice");
        }

        private IBetOperand _betOperandA;
        private IBetOperand _betOperandB;

        private Match _match;

        public void SetMatch(Match match)
        {
            MapName = match.Map;

            _match = match;
            var operandA = match.Player1;
            var operandB = match.Player2;

            _betOperandA = operandA;
            _betOperandB = operandB;
            aBeatsB.Content = operandA.ShortDescription + " beats " + operandB.ShortDescription;
            bBeatsA.Content = operandB.ShortDescription + " beats " + operandA.ShortDescription;
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            if (!IsLoaded)
                return;

            Bet bet = ToBet();
            string offererChoice = bet.OffererChoice != null ? bet.OffererChoice.ShortDescription : "Unkown";
            string takerChoice = bet.TakerChoice != null ? bet.TakerChoice.ShortDescription : "Unkown";
            string summaryFormat = "If {0} wins, you win {1:#,0}.\r\nIf {2} wins, you lose {3:#,0}.";
            textBoxSummary.Text = string.Format(summaryFormat,
                offererChoice, bet.OffererReward,
                takerChoice, bet.OffererWager);
        }

        public Bet ToBet()
        {
            var selectedOdds = (ComboBoxItem)comboBoxOdds.SelectedItem;
            Bet bet = new Bet
            {
                Offerer = HomeWindow.UserData.User,
                OffererWager = Wager,
                OffererOdds = selectedOdds.Content.ToString(),
            };

            if (IsOneOnOneBetting)
            {
                bet.OffererChoice = HomeWindow.UserData.User;
            }
            else if (IsTeamBetting && HomeWindow.UserData.User.HasTeam)
            {
                bet.OffererChoice = HomeWindow.UserData.User.Team;
            }

            if (_match == null)
            {
                bet.MyMatch = new Match();
                bet.MyMatch.Map = _mapName;
            } 
            else 
            {
                bet.MyMatch = _match;
                bet.MatchHash = _match.MatchHash;
            }

            return bet;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void comboBoxOdds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSummary();
        }

        private void comboBoxOutcome_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSummary();
        }

        private void myWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSummary();
        }
    }
}
