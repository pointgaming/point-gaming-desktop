using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class GameRoomAdminDialog : Window, INotifyPropertyChanged
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

        private readonly ObservableCollection<IBetOperand> _player1Options = new ObservableCollection<IBetOperand>();
        public ObservableCollection<IBetOperand> Player1Options { get { return _player1Options; } }
        private readonly ObservableCollection<IBetOperand> _player2Options = new ObservableCollection<IBetOperand>();
        public ObservableCollection<IBetOperand> Player2Options { get { return _player2Options; } }

        private UserDataManager _userData = HomeWindow.UserData;

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                if (value == _description)
                    return;
                _description = value;
                buttonOK.IsEnabled = !string.IsNullOrWhiteSpace(_description);
                NotifyChanged("Description");
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                if (value == _password)
                    return;
                _password = value;
                NotifyChanged("Password");
            }
        }

        private bool _isAdvertising;
        public bool IsAdvertising
        {
            get { return _isAdvertising; }
            set
            {
                if (value == _isAdvertising)
                    return;
                _isAdvertising = value;
                NotifyChanged("IsAdvertising");
            }
        }

        public GameRoomAdminDialog()
        {
            InitializeComponent();
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

        private void tabControlMatchState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string state = (string)(((TabItem)(tabControlMatchState.SelectedItem)).Header);

            bool canEditDetails = state == "none" || state == "created";
            textBoxBettingMap.IsEnabled = canEditDetails;
            textBoxPlayer1.IsEnabled = canEditDetails;
            textBoxPlayer2.IsEnabled = canEditDetails;

            checkBoxEnableBooking.IsEnabled = state == "created";

            radioButtonPlayerDraw.IsEnabled = state == "started";
            radioButtonPlayer1.IsEnabled = state == "started";
            radioButtonPlayer2.IsEnabled = state == "started";
        }

        private void MatchCreate_Click(object sender, RoutedEventArgs e)
        {
            Match m = new Match {
                Player1 = (IBetOperand)textBoxPlayer1.SelectedValue,
                Player2 = (IBetOperand)textBoxPlayer2.SelectedValue,
            };

            if (m.Player1 == null || m.Player2 == null)
            {
                MessageDialog.Show(this, "Choose players first", "Choose players first.");
                return;
            }
        }

        private void MatchUpdate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MatchStart_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MatchFinish_Click(object sender, RoutedEventArgs e)
        {
            if (radioButtonPlayer1.IsChecked==false && radioButtonPlayer2.IsChecked==false && radioButtonPlayerDraw.IsChecked==false)
            {
                MessageDialog.Show(this, "Choose Winner First", "Choose the winner first.");
                return;
            }


        }
        private void MatchCancel1_Click(object sender, RoutedEventArgs e)
        {

        }
        private void MatchCancel2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void textBoxPlayer1_TextChanged(object sender, TextChangedEventArgs e)
        {
            var t = textBoxPlayer1;
            if (t.Text.Length < 2)
                return;
            _userData.LookupBetOperand(t.Text, Player1AutoComplete);
        }
        private void textBoxPlayer2_TextChanged(object sender, TextChangedEventArgs e)
        {
            var t = textBoxPlayer2;
            if (t.Text.Length < 2)
                return;
            _userData.LookupBetOperand(t.Text, Player2AutoComplete);
        }

        private void Player1AutoComplete(List<POCO.BetOperandPoco> pocos)
        {
            AutoComplete(pocos, Player1Options);
        }
        private void Player2AutoComplete(List<POCO.BetOperandPoco> pocos)
        {
            AutoComplete(pocos, Player2Options);
        }

        private void AutoComplete(List<POCO.BetOperandPoco> pocos, ObservableCollection<IBetOperand> options)
        {
            options.Clear();
            foreach (var item in pocos)
            {
                if (item.type == "User")
                    options.Add(_userData.GetPgUser(new POCO.UserBase { _id = item._id, username = item.name, }));
                else if (item.type == "Team")
                    options.Add(_userData.GetPgTeam(new POCO.TeamBase { _id = item._id, name = item.name, }));
            }
        }

    }
}
