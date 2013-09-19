using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PointGaming;
using PointGaming.GameRoom;
using PointGaming.Chat;
using PointGaming.POCO;

namespace PointGaming.Lobby
{
    class MatchesDialogModelView : ViewModelBase
    {
        private UserDataManager _userData = UserDataManager.UserData;
        private LobbySession _session;
        private SessionManager _manager;

        private ObservableCollection<Match> _matches = new ObservableCollection<Match>();
        public ObservableCollection<Match> Matches
        {
            get
            {
                return _matches;
            }
        }

        public MatchesDialogModelView()
        {
        }

        public void Init(SessionManager manager, LobbySession session)
        {
            _manager = manager; 
            _session = session;
            
            LoadMatches();
        }

        private void LoadMatches()
        {
            _session.RequestUndecidedMatches(OnMatchesLoaded);
        }

        public void OnMatchesLoaded(List<MatchPoco> matches)
        {
            foreach (var match in matches)
            {
                Match item = new Match(match);
                _matches.Add(item);
            }
            OnPropertyChanged("Matches");
        }

        public ICommand ReportWinner { get { return new ActionCommand<Match>(ReportMatchWinner); } }
        public void ReportMatchWinner(Match match)
        {
            _session.ReportMatchWinner(match, OnWinnerReported);
        }

        public void OnWinnerReported(Match match)
        {
            if (match == null)
            {
                LoadMatches();
            }
            else
            {
                _matches.Remove(match);
                OnPropertyChanged("Matches");
            }
        }
    }
}
