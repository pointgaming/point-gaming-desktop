using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using PointGaming;
using PointGaming.GameRoom;
using PointGaming.Chat;
using PointGaming.POCO;

namespace PointGaming.Lobby
{
    class MatchesDialogModelView : ViewModelBase
    {
        private UserDataManager _userData = HomeWindow.UserData;
        private LobbySession _session;
        private SessionManager _manager;

        public ObservableCollection<Match> Matches = new ObservableCollection<Match>();

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
                Matches.Add(item);
            }
        }
    }
}
