using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.AudioChat
{
    public interface IAudioRoom
    {
        string AudioRoomId { get; }
        bool IsVoiceTeamOnly { get; }
        bool IsVoiceEnabled { get; }

        void OnVoiceStarted(PgUser user);
        void OnVoiceStopped(PgUser user);
        void OnVoiceConnectionChanged(bool isConnected);
    }
}
