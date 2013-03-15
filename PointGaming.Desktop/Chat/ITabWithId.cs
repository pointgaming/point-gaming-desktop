using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.Desktop.Chat
{
    public interface ITabWithId
    {
        string Id { get; }
    }
    public interface IChatroomTab : ITabWithId
    {
        void Init(ChatWindow window, ChatroomSession roomManager);
    }
}
