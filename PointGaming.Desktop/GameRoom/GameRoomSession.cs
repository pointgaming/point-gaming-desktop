using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PointGaming.Desktop.Chat;

namespace PointGaming.Desktop.GameRoom
{
    public class GameRoomSession : ChatroomSession
    {
        public GameRoomSession(ChatManager manager)
            : base(manager)
        {
        }

        public override Type GetUserControlType()
        {
            return typeof(GameRoomTab);
        }

        public override IChatroomTab GetNewUserControl()
        {
            return new GameRoomTab();
        }
    }
}
