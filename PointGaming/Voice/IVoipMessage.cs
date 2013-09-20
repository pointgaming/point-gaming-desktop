using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PointGaming.Voice
{
    public interface IVoipMessage
    {
        byte MessageType { get; }
        bool Read(byte[] buffer, int offset, int length);
        int Write(byte[] buffer, byte[] key);
    }
}
