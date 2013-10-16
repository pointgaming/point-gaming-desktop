using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.Voice
{
    class PacketOrderer
    {
        private static readonly byte[] _static = new byte[]
            {
                72, 169, 123, 76, 4, 105, 125, 8, 56, 83, 146, 204,
                177, 123, 128, 246, 196, 42, 200, 215, 148, 61, 179,
                115, 102, 107, 152, 217, 9, 202, 147, 226, 240, 173,
                131, 191, 150, 94, 168, 177, 43, 36, 224, 167, 235,
                73, 121, 80, 126, 18, 41, 216, 60, 250, 185, 170,
                164, 118, 31, 23, 69, 8, 27, 96, 29, 204, 120, 22,
                32, 119, 150, 50, 64
            };

        private readonly VoipSession _voipSession;
        private readonly VoipMessageVoice[] _voices = new VoipMessageVoice[200];// 20ms per: 4 seconds total

        private int _nextPlayNumber = 0;
        private int _maxPlayNumber = -1;
        private double _jitterNumber = 0;
        private int _jitterNumberActive = 10;// put 200ms delay on the first transmittion until jitter is figured out better
        private int _jitterWait;
        private int[] _jitters = new int[8];
        private int _jittersIx = 0;


        private DateTime _streamStartTime;

        public PacketOrderer(VoipSession vs)
        {
            _voipSession = vs;
        }

        private VoipMessageVoice _firstItem;
        private int _lastStreamNumber = -1;
        private readonly List<int> _prevStreamNumbers = new List<int>();

        private SerialPacketStream _ps;

        public void AddMessage(VoipMessageVoice item)
        {
            if (VoipSession.DebugSaveReceivedPackets)
                SavePacket(item);

            lock (_voices)
            {
                if (!ShouldAddToStream(item))
                    return;

                CalcJitter(item.MessageNumber);

                if (item.MessageNumber > _maxPlayNumber)
                    _maxPlayNumber = item.MessageNumber;

                var index = item.MessageNumber % _voices.Length;
                _voices[index] = item;
            }
        }
        
        private bool ShouldAddToStream(VoipMessageVoice item)
        {
            if (item.StreamNumber == _lastStreamNumber)
                return true;

            if (_prevStreamNumbers.Contains(item.StreamNumber))
                return false;// don't play packets from recently played streams

            if (_maxPlayNumber > 50)
                SetActiveJitter();

            InitStream(item);

            return true;
        }

        private void CalcJitter(int messageNumber)
        {
            var now = DateTime.UtcNow;
            var actualTime = (now - _streamStartTime).TotalMilliseconds;
            var expectedTime = messageNumber * 20.0;

            var diff = Math.Abs(actualTime - expectedTime);
            if (diff > _jitterNumber)
                _jitterNumber = diff;
        }

        private void InitStream(VoipMessageVoice item)
        {
            if (VoipSession.DebugSaveReceivedPackets)
                SavePacketStream(); 

            _nextPlayNumber = 0;
            _maxPlayNumber = -1;
            _jitterNumber = 0;
            _jitterWait = 0;
            _firstItem = item;
            _streamStartTime = DateTime.UtcNow;

            _lastStreamNumber = item.StreamNumber;
            while (_prevStreamNumbers.Count >= 4)
                _prevStreamNumbers.RemoveAt(3);
            _prevStreamNumbers.Insert(0, _lastStreamNumber);
        }

        private void SavePacket(VoipMessageVoice item)
        {
            if (_ps == null)
                _ps = new SerialPacketStream(item, true);
            var poco = new SerialPacket(item);
            _ps.Parts.Add(poco);
        }
        private void SavePacketStream()
        {
            if (_ps != null)
            {
                _ps.Write(SerialPacketStream.AppDataPath(_ps));
                _ps = null;
            }
        }

        private void SetActiveJitter()
        {
            var active = (int)Math.Ceiling(_jitterNumber / 20);
            if (active > _voices.Length >> 1)
                active = _voices.Length >> 1;
            _jitters[(_jittersIx++) & 0x7] = active;

            var max = 0;
            foreach (var v in _jitters)
                if (v > max)
                    max = v;

            _jitterNumberActive = max;
            VoipSession.VoipDebug(VoipSession.DebugCountTick, "New _jitterNumber: " + _jitterNumberActive);
        }

        public void Process()
        {
            VoipMessageVoice cur;

            lock (_voices)
            {
                if (_nextPlayNumber > _maxPlayNumber)
                    return;
                if (_nextPlayNumber == 0)
                {
                    _jitterWait++;
                    if (_jitterWait <= _jitterNumberActive)
                        return;
                }

                var ix = _nextPlayNumber % _voices.Length;
                cur = _voices[ix];
                _voices[ix] = null;

                if (cur == null)
                {
                    cur = new VoipMessageVoice
                    {
                        Audio = _static,
                        FromUserId = _firstItem.FromUserId,
                        IsTeamOnly = _firstItem.IsTeamOnly,
                        MessageNumber = _nextPlayNumber,
                        RoomName = _firstItem.RoomName,
                    };
                }

                _nextPlayNumber++;
            }

            _voipSession.CountTick(cur.FromUserId);

            _voipSession.PlayVoice(cur);
        }
    }

}
