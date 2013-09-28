using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.Voice
{
    public interface IVoiceTester
    {
        string VoiceTesterId { get; }

        void OnRecordingStarted();
        void OnRecordingStopped();
        void OnPlaybackStarted(TimeSpan duration);
        void OnPlaybackStopped();
    }
}
