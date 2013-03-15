using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using PointGaming.Desktop.Chat;
using PointGaming.Desktop.GameRoom;

namespace PointGaming.Desktop.Lobby
{
    public class LobbySession : ChatroomSession
    {
        public class JoinedGameRoom : INotifyPropertyChanged
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

            private string _displayName;
            public string DisplayName
            {
                get { return _displayName; }
                set
                {
                    if (value == _displayName)
                        return;
                    _displayName = value;
                    NotifyChanged("DisplayName");
                }
            }
        }

        private ObservableCollection<JoinedGameRoom> _gameRooms = new ObservableCollection<JoinedGameRoom>();
        public ObservableCollection<JoinedGameRoom> JoinedGameRooms { get { return _gameRooms; } }

        public LobbySession(ChatManager manager)
            : base(manager)
        {
        }

        public override Type GetUserControlType()
        {
            return typeof(LobbyTab);
        }

        public override IChatroomTab GetNewUserControl()
        {
            return new LobbyTab();
        }
    }
}
