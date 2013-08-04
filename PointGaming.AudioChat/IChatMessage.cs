using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PointGaming.AudioChat
{
    public interface IChatMessage
    {
        byte MessageType { get; }
        bool Read(byte[] buffer, int length);
        int Write(byte[] buffer);
    }
}
