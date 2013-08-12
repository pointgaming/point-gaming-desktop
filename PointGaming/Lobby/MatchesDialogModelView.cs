using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using PointGaming.GameRoom;
using PointGaming.Chat;

namespace PointGaming.Lobby
{
    class MatchesDialogModelView : ViewModelBase
    {
        private UserDataManager _userData = HomeWindow.UserData;
        private ChatroomSession _session;
        private SessionManager _manager;

        public ObservableCollection<Match> Matches = new ObservableCollection<Match>();

        public MatchesDialogModelView()
        {
        }

        public void Init(SessionManager manager, ChatroomSession session)
        {
            _manager = manager; 
            _session = session;
            
            LoadMatches();
        }

        private void LoadMatches()
        {
        }
    }
}
