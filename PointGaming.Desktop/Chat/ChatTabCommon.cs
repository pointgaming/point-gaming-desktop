using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.Desktop.Chat
{
    public class ChatTabCommon
    {
        public static bool FilterMessage(string messageIn, out string send, out string remain)
        {
            send = messageIn.Trim();

            remain = "";
            if (send.Length > 1024)
            {
                send = send.Substring(0, 1024);
                send = send.Trim();
                remain = send.Substring(1024);
            }

            return send != "";
        }
    }
}
