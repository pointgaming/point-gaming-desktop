using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PointGaming.POCO;

namespace PointGaming.Chat
{
    public class ChatroomSession : ChatroomSessionBase
    {
        private ChatroomTab _window;
        public ChatroomSession(SessionManager manager) : base(manager) { }


        public override void ShowControl(bool shouldActivate)
        {
            if (_window == null)
            {
                _window = new ChatroomTab();
                _window.Init(this);
            }
            _window.ShowNormal(shouldActivate);
        }
    }
}
