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

namespace PointGaming.GameRoom
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

        private GameRoomSession _gameRoomSession;
        private Match _prevMatch;

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
        public void Init(Window owner, GameRoomSession session)
        {
            Owner = owner;
            _gameRoomSession = session;
            Description = session.GameRoom.Description;
            Password = session.GameRoom.Password;
            IsAdvertising = session.GameRoom.IsAdvertising;
            _prevMatch = session.MyMatch;
            _prevMatch.PropertyChanged += match_PropertyChanged;

            InitMatch();
        }

        void match_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            gridMatchSettings.IsEnabled = _prevMatch.IsEditable;
            InitMatch();
        }

        private void InitMatch()
        {
            var match = _prevMatch;
            if (match.State == MatchState.invalid || match.State == MatchState.finalized || match.State == MatchState.canceled)
            {
                checkBoxEnableBooking.IsChecked = true;
                tabControlMatchState.SelectedIndex = 0;
                textBoxPlayer1.SelectedValue = null;
                textBoxPlayer2.SelectedValue = null;
                textBoxBettingMap.Text = "";
                return;
            }

            checkBoxEnableBooking.IsChecked = match.IsBetting;
            textBoxPlayer1.SelectedValue = match.Player1;
            textBoxPlayer2.SelectedValue = match.Player2;
            textBoxBettingMap.Text = match.Map;

            if (match.State == MatchState.created)
                tabControlMatchState.SelectedIndex = 1;
            else if (match.State == MatchState.started)
                tabControlMatchState.SelectedIndex = 2;
            else
                throw new Exception("Unexpected match state: " + match.State);
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
            checkBoxEnableBooking.IsEnabled = canEditDetails;

            radioButtonPlayer1.IsEnabled = state == "started";
            radioButtonPlayer2.IsEnabled = state == "started";
        }

        private void MatchCreate_Click(object sender, RoutedEventArgs e)
        {
            Match m;
            if (!TryGetMatch(out m))
                return;

            m.State = MatchState.create;
            m.Winner = null;

            _gameRoomSession.CreateMatch(m);
        }

        private void MatchUpdate_Click(object sender, RoutedEventArgs e)
        {
            Match m;
            if (!TryGetMatch(out m))
                return;

            m.State = MatchState.created;
            m.Winner = null;

            _gameRoomSession.UpdateMatch(m);
        }

        private bool TryGetMatch(out Match m)
        {
            var p1 = (IBetOperand)textBoxPlayer1.SelectedValue;
            var p2 = (IBetOperand)textBoxPlayer2.SelectedValue;

            if (p1 == null || p2 == null)
            {
                MessageDialog.Show(this, "Choose players first", "Choose players first.");
                m = null;
                return false;
            }

            m = new Match
            {
                Player1 = p1,
                Player2 = p2,
                Id = _prevMatch.State == MatchState.invalid ? null : _prevMatch.Id,
                GameId = _gameRoomSession.GameId,
                IsBetting = checkBoxEnableBooking.IsChecked == true,
                Map = textBoxBettingMap.Text,
                MatchHash = _prevMatch.State == MatchState.invalid ? null : _prevMatch.MatchHash,
                RoomId = _gameRoomSession.GameRoomId,
                RoomType = "GameRoom",
            };
            return true;
        }

        private void MatchStart_Click(object sender, RoutedEventArgs e)
        {
            Match m;
            if (!TryGetMatch(out m))
                return;

            if (m.Player1 != _prevMatch.Player1 || m.Player2 != _prevMatch.Player2 || m.Map != _prevMatch.Map)
            {
                MessageDialog.Show(this, "Cannot Start", "Match details changed, update first.");
                return;
            }

            _gameRoomSession.StartMatch();
        }

        private void MatchFinish_Click(object sender, RoutedEventArgs e)
        {
            IBetOperand winner = null;
            if (radioButtonPlayer1.IsChecked == true)
                winner = _prevMatch.Player1;
            else if (radioButtonPlayer2.IsChecked == true)
                winner = _prevMatch.Player2;
            else
            {
                MessageDialog.Show(this, "Choose Winner First", "Choose the winner first.");
                return;
            }

            _gameRoomSession.FinishMatch(winner);
        }
        private void MatchCancel1_Click(object sender, RoutedEventArgs e)
        {
            _gameRoomSession.CancelMatch();
        }

        private void textBoxPlayer1_TextChanged(object sender, TextChangedEventArgs e)
        {
            var t = textBoxPlayer1;
            if (t.Text.Length < 1)
                return;
            _userData.LookupBetOperand(t.Text, Player1AutoComplete);
        }
        private void textBoxPlayer2_TextChanged(object sender, TextChangedEventArgs e)
        {
            var t = textBoxPlayer2;
            if (t.Text.Length < 1)
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
