using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PointGaming.POCO;

namespace PointGaming.Chat
{
    public class PrivateChatSession : ChatSessionBase
    {
        private PgUser _otherUser;
        private ChatTab _window;

        public PrivateChatSession(SessionManager manager, PgUser otherUser) : base(manager)
        {
            _otherUser = otherUser;
        }

        public override void ShowControl(bool shouldActivate)
        {
            if (_window == null)
            {
                _window = new ChatTab();
                _window.Init(this, _otherUser);
            }

            _window.ShowNormal(shouldActivate);
        }

        public void SendMessage(string send)
        {
            var privateMessage = new PrivateMessageOut { _id = _otherUser.Id, message = send };
            _manager.SendMessage(privateMessage);
        }

        public void Leave()
        {
            _manager.LeavePrivateChat(_otherUser);
        }
    }
}
