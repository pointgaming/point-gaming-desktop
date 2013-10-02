using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using PointGaming.POCO;
using RestSharp;
using SocketIOClient;
using SocketIOClient.Messages;
using PointGaming.HomeTab;
using System.Security;
using System.Security.Cryptography;
using PointGaming.Lobby;
using PointGaming.GameRoom;
using PointGaming.Chat;
using PointGaming.Voice;

namespace PointGaming.Voice
{
    public class VoipSession : IDisposable
    {
        private UserDataManager _userData;
        private AudioHardwareSession _nAudioTest;
        private VoipClient _audioChatClient;
        private object _audioChatSynch = new object();
        private bool _hasBeenDisposed = false;
        
        public event Action<int> RecordingDeviceChanged;

        public DispatcherTimer _udpKeepAliveTimer;

        private long _microphonePriority = 1;

        public void TestVoiceStart(VoiceTester tester)
        {
            tester.Init(_nAudioTest);
            _voiceTester = tester;
        }
        public void TestVoiceStop()
        {
            _voiceTester = null;
        }

        public void TakeMicrophoneFocus(IVoiceRoom room)
        {
            AudioRoomEx roomEx;
            if (!_joinedRooms.TryGetValue(room.AudioRoomId, out roomEx))
                return;

            if (roomEx.MicrophonePriority == _microphonePriority)
                return;

            _microphonePriority++;
            roomEx.MicrophonePriority = _microphonePriority;
        }

        private AudioRoomEx GetSpeakingRoom()
        {
            long maxP = 0;
            AudioRoomEx max = null;
            foreach (var roomEx in _joinedRooms.Values)
            {
                if (roomEx.MicrophonePriority > maxP)
                {
                    maxP = roomEx.MicrophonePriority;
                    max = roomEx;
                }
            }

            if (max != null && !max.R.IsVoiceEnabled)
                max = null;

            return max;
        }


        private class AudioRoomEx
        {
            public readonly IVoiceRoom R;
            public long MicrophonePriority;
            public readonly List<PgUser> Senders = new List<PgUser>();

            public AudioRoomEx(IVoiceRoom room)
            {
                this.R = room;
            }

            public void OnVoiceSent(PgUser user)
            {
                lock (Senders)
                {
                    if (Senders.Contains(user))
                        return;
                }
                // todo: dean: what happens when sending/recieving right when the home window is being closed?
                ((Action)(delegate
                {
                    R.OnVoiceStarted(user);
                })).BeginInvokeUI();
            }
            public void OnVoiceStopped(PgUser user)
            {
                lock (Senders)
                {
                    if (!Senders.Remove(user))
                        return;
                }
                ((Action)(delegate
                {
                    R.OnVoiceStopped(user);
                })).BeginInvokeUI();
            }
        }

        private readonly Dictionary<string, AudioRoomEx> _joinedRooms = new Dictionary<string, AudioRoomEx>();

        public void JoinRoom(IVoiceRoom audioRoom)
        {
            AudioRoomEx roomEx;
            if (!_joinedRooms.TryGetValue(audioRoom.AudioRoomId, out roomEx))
            {
                roomEx = new AudioRoomEx(audioRoom);
                _joinedRooms[audioRoom.AudioRoomId] = roomEx;
            }
            SendJoinRoom(roomEx);
        }

        public void LeaveRoom(IVoiceRoom audioRoom)
        {
            AudioRoomEx roomEx;
            if (!_joinedRooms.TryGetValue(audioRoom.AudioRoomId, out roomEx))
                return;

            foreach (var sender in roomEx.Senders)
                _nAudioTest.AudioReceiveEnded(sender.Id);

            _joinedRooms.Remove(audioRoom.AudioRoomId);
            SendLeaveRoom(roomEx);
        }
        
        public VoipSession(UserDataManager userData)
        {
            _userData = userData;
            _nAudioTest = new AudioHardwareSession(new Opus16kCodec());
            _nAudioTest.InputDeviceNumber = App.Settings.AudioInputDeviceIndex;
            _nAudioTest.TriggerKey = (System.Windows.Input.Key)_userData.Settings.MicTriggerKey;
            _nAudioTest.AudioRecorded += _nAudioTest_AudioRecorded;
            _nAudioTest.AudioRecordEnded += _nAudioTest_AudioRecordEnded;
            _nAudioTest.InputDeviceNumberChanged += _nAudioTest_InputDeviceNumberChanged;

            _udpKeepAliveTimer = new DispatcherTimer();
            _udpKeepAliveTimer.Interval = TimeSpan.FromSeconds(55);
            _udpKeepAliveTimer.Tick += _udpKeepAliveTimer_Tick;

            Connect();
        }

        private void Connect()
        {
            if (_hasBeenDisposed)
                return;

            var audioChatIp = System.Net.IPAddress.Parse(App.Settings.AudioChatIp);
            var audioChatPort = App.Settings.AudioChatPort;
            var endpoint = new System.Net.IPEndPoint(audioChatIp, audioChatPort);
            _audioChatClient = new VoipClient(endpoint, _userData.PgSession.AuthToken);
            _audioChatClient.MessageReceived += _audioChatClient_MessageReceived;
            _audioChatClient.Stopped += _audioChatClient_Stopped;
            _audioChatClient.Start();

            _udpKeepAliveTimer.Start();
        }

        void _audioChatClient_Stopped(VoipClient obj)
        {
            lock (_audioChatSynch)
            {
                _udpKeepAliveTimer.Stop();

                if (_hasBeenDisposed)
                    return;

                _audioChatClient.MessageReceived -= _audioChatClient_MessageReceived;
                _audioChatClient.Stopped -= _audioChatClient_Stopped;

                ((Action)Connect).DelayInvokeUI(TimeSpan.FromSeconds(5));
            }
        }

        private void _udpKeepAliveTimer_Tick(object sender, EventArgs e)
        {
            CheckJoinTimeouts();
            
            foreach (var room in _joinedRooms.Values)
                SendJoinRoom(room);
        }

        private void SendJoinRoom(AudioRoomEx roomEx)
        {
            var message = new VoipMessageJoinRoom
            {
                RoomName = roomEx.R.AudioRoomId,
                FromUserId = _userData.User.Id
            };
            _audioChatClient.Send(message);
            AddJoinRoomTimeout(roomEx);
        }
        
        private void SendLeaveRoom(AudioRoomEx roomEx)
        {
            var message = new VoipMessageLeaveRoom
            {
                RoomName = roomEx.R.AudioRoomId,
                FromUserId = _userData.User.Id
            };
            _audioChatClient.Send(message);
            OnVoiceConnectionChanged(roomEx, false);
        }

        void _nAudioTest_InputDeviceNumberChanged(int index)
        {
            var call = RecordingDeviceChanged;
            if (call != null)
                call(index);
        }
        
        public void Dispose()
        {
            _hasBeenDisposed = true;
            if (_nAudioTest == null)
                return;

            _nAudioTest.Dispose();
            _nAudioTest = null;

            lock (_audioChatSynch)
            {
                _audioChatClient.MessageReceived -= _audioChatClient_MessageReceived;
                _audioChatClient.Stopped -= _audioChatClient_Stopped;
                _audioChatClient.Stop();
                _audioChatClient = null;
            }

            _udpKeepAliveTimer.Stop();
        }
        
        public void SetAudioInputDevice(int deviceNumber)
        {
            if (!_nAudioTest.IsEnabled)
                return;

            _nAudioTest.InputDeviceNumber = deviceNumber;
            Enable();
        }

        public void Enable()
        {
            _nAudioTest.Disable();
            _nAudioTest.Enable();
        }

        public void Disable()
        {
            _nAudioTest.Disable();
        }

        public System.Windows.Input.Key TriggerKey
        {
            set
            {
                _nAudioTest.TriggerKey = value;
            }
        }

        private void _audioChatClient_MessageReceived(IVoipMessage message)
        {
            switch (message.MessageType)
            {
                case (VoipMessageVoice.MType):
                    ReceivedVoice((VoipMessageVoice)message);
                    break;
                case (VoipMessageJoinRoom.MType):
                    ReceivedJoin((VoipMessageJoinRoom)message);
                    break;
                case (VoipMessageLeaveRoom.MType):
                    ReceivedLeave((VoipMessageLeaveRoom)message);
                    break;
                default:
                    Console.WriteLine("Unhandled message type: " + message.MessageType);
                    break;
            }
        }
        
        private void ReceivedLeave(VoipMessageLeaveRoom message)
        {
            AudioRoomEx roomEx;
            if (!_joinedRooms.TryGetValue(message.RoomName, out roomEx))
                return;

            OnVoiceConnectionChanged(roomEx, false);
        }

        private void ReceivedJoin(VoipMessageJoinRoom message)
        {
            AudioRoomEx roomEx;
            if (!_joinedRooms.TryGetValue(message.RoomName, out roomEx))
                return;

            OnVoiceConnectionChanged(roomEx, message.IsSuccess);
            RemoveJoinRoomTimeout(message.RoomName);
        }

        #region JoinTimeout

        private class JoinTimeout
        {
            public AudioRoomEx RoomEx;
            public DateTime Timeout;
        }

        private List<JoinTimeout> _joinTimeouts = new List<JoinTimeout>();

        private void AddJoinRoomTimeout(AudioRoomEx roomEx)
        {
            var jr = new JoinTimeout
            {
                RoomEx = roomEx,
                Timeout = DateTime.UtcNow + TimeSpan.FromSeconds(3),
            };
            lock (_joinTimeouts)
            {
                _joinTimeouts.Add(jr);
            }
        }

        private void RemoveJoinRoomTimeout(string roomName)
        {
            lock (_joinTimeouts)
            {
                foreach (var item in _joinTimeouts)
                {
                    if (item.RoomEx.R.AudioRoomId == roomName)
                    {
                        _joinTimeouts.Remove(item);
                        break;
                    }
                }
            }
        }

        private void CheckJoinTimeouts()
        {
            var now = DateTime.UtcNow;
            List<JoinTimeout> timeouts = new List<JoinTimeout>();
            lock (_joinTimeouts)
            {
                foreach (var item in _joinTimeouts)
                {
                    if (now >= item.Timeout)
                        timeouts.Add(item);
                    else
                        break;// added in chronological order
                }
                foreach (var item in timeouts)
                    _joinTimeouts.Remove(item);
            }
            foreach (var item in timeouts)
            {
                OnVoiceConnectionChanged(item.RoomEx, false);
            }
        }

        #endregion JoinTimeout

        private void OnVoiceConnectionChanged(AudioRoomEx roomEx, bool isConnected)
        {
            ((Action)(delegate
            {
                roomEx.R.OnVoiceConnectionChanged(isConnected);
            })).BeginInvokeUI();
        }
            
        private void ReceivedVoice(VoipMessageVoice voiceMessage)
        {
            if (voiceMessage.FromUserId == _userData.User.Id)
                return;

            var isEnd = voiceMessage.Audio.Length == 0;

            //App.LogLine(obj.FromUserId + " " + obj.MessageNumber + " " + obj.Audio.Length);

            var user = _userData.GetPgUser(voiceMessage.FromUserId);

            var isListening = false;
            AudioRoomEx roomEx;
            if (_joinedRooms.TryGetValue(voiceMessage.RoomName, out roomEx))
            {
                isListening = roomEx.R.IsVoiceEnabled && !user.IsMuted;

                if (isListening && roomEx.R.IsVoiceTeamOnly)
                    isListening = _userData.User.Team == user.Team;
            }

            if (isEnd)
            {
                _nAudioTest.AudioReceiveEnded(voiceMessage.FromUserId);
                roomEx.OnVoiceStopped(user);
            }
            else
            {
                if (isListening)
                    _nAudioTest.AudioReceived(voiceMessage.FromUserId, voiceMessage.Audio);
                roomEx.OnVoiceSent(user);
            }
        }

        private AudioRoomEx _speakingRoom = null;
        private bool _speakingRoomTeamOnly;

        private VoiceTester _voiceTester = null;

        private void _nAudioTest_AudioRecorded(AudioHardwareSession source, byte[] data, double signalPower)
        {
            var vt = _voiceTester;
            if (vt != null)
            {
                vt.Recorded(data, signalPower);
                return;
            }

            var isFirst = _speakingRoom == null;
            if (isFirst)
                _speakingRoom = GetSpeakingRoom();

            if (_speakingRoom == null)
                return;

            if (isFirst)
                _speakingRoomTeamOnly = _speakingRoom.R.IsVoiceTeamOnly;

            var message = new VoipMessageVoice
            {
                RoomName = _speakingRoom.R.AudioRoomId,
                FromUserId = _userData.User.Id,
                Audio = data,
                IsTeamOnly = _speakingRoomTeamOnly,
            };
            
            _audioChatClient.Send(message);
            if (isFirst)
                _speakingRoom.OnVoiceSent(_userData.User);
        }

        private void _nAudioTest_AudioRecordEnded()
        {
            var vt = _voiceTester;
            if (vt != null)
            {
                vt.RecordEnded();
                return;
            }

            if (_speakingRoom == null)
                return;

            var message = new VoipMessageVoice
            {
                RoomName = _speakingRoom.R.AudioRoomId,
                FromUserId = _userData.User.Id,
                Audio = new byte[0],
                IsTeamOnly = _speakingRoomTeamOnly,
            };

            _audioChatClient.Send(message);
            _speakingRoom.OnVoiceStopped(_userData.User);
        }
    }

}
