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
        private readonly ObservableCollection<GameRoomItem> _allGameRooms = new ObservableCollection<GameRoomItem>();
        public ObservableCollection<GameRoomItem> AllGameRooms { get { return _allGameRooms; } }

        private readonly ObservableCollection<GameRoomItem> _joinedGameRooms = new ObservableCollection<GameRoomItem>();
        public ObservableCollection<GameRoomItem> JoinedGameRooms { get { return _joinedGameRooms; } }

        public LobbySession(ChatManager manager)
            : base(manager)
        {
            foreach (var item in new FileCollection())
                _allGameRooms.Add(item);

            _joinedGameRooms.Add(_allGameRooms.First());
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
