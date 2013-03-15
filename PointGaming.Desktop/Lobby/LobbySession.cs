using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PointGaming.Desktop.Chat;
using PointGaming.Desktop.GameRoom;

namespace PointGaming.Desktop.Lobby
{
    public class LobbySession
    {
        private ChatroomSession _chatroom;

        private ObservableCollection<GameRoomSession> _gameRooms = new ObservableCollection<GameRoomSession>();
        public ObservableCollection<GameRoomSession> GameRooms { get { return _gameRooms; } }


    }
}
