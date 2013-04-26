using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace PointGaming.Chat
{
    public interface ITabWithId : INotifyPropertyChanged
    {
        string Id { get; }
        string Header { get; }
    }
    public interface IChatroomTab : ITabWithId
    {
        void Init(ChatWindow window, ChatroomSession roomManager);
    }
}
