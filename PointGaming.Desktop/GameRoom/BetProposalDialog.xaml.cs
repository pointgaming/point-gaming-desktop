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

namespace PointGaming.Desktop.GameRoom
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

        private int _betAmount = 0;
        public int BetAmount
        {
            get { return _betAmount; }
            set
            {
                if (value == _betAmount)
                    return;
                if (value < 0)
                    throw new Exception("Amount must be > 0");
                _betAmount = value;
                NotifyChanged("BetAmount");
                buttonOK.IsEnabled = _betAmount > 0;
                UpdateSummary();
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
            }
        }

        public BetProposalDialog()
        {
            InitializeComponent();
        }

        private IBetOperand _betOperandA;
        private IBetOperand _betOperandB;

        public void SetBetOperands(IBetOperand operandA, IBetOperand operandB)
        {
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

            IBetOperand winner, loser;
            if (comboBoxOutcome.SelectedIndex == 0)
            {
                winner = _betOperandA;
                loser = _betOperandB;
            }
            else
            {
                winner = _betOperandB;
                loser = _betOperandA;
            }
            int loseAmount = BetAmount;
            decimal multiplier = 1m;
            ComboBoxItem selectedOdds = (ComboBoxItem)comboBoxOdds.SelectedItem;
            if (selectedOdds == oneTo1) multiplier = 1m;
            else if (selectedOdds == oneTo2) multiplier = 2m;
            else if (selectedOdds == oneTo3) multiplier = 3m;
            else if (selectedOdds == oneTo4) multiplier = 4m;
            else if (selectedOdds == oneTo5) multiplier = 5m;
            else if (selectedOdds == oneTo6) multiplier = 6m;
            else if (selectedOdds == oneTo7) multiplier = 7m;
            else if (selectedOdds == oneTo8) multiplier = 8m;
            else if (selectedOdds == oneTo9) multiplier = 9m;
            else if (selectedOdds == oneTo10) multiplier = 10m;
            else
                throw new NotImplementedException("Woops! Didn't implement odds '" + selectedOdds.Content + "'");

            int winAmount = (int)Math.Floor(multiplier * loseAmount);

            string summaryFormat = "If {0} wins, you win {1}.\r\nIf {2} wins, you lose {3}.";
            textBoxSummary.Text = string.Format(summaryFormat, winner.ShortDescription, winAmount, loser.ShortDescription, loseAmount);
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
