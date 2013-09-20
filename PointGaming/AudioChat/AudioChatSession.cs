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
using PointGaming.Audio;
using NA = NAudio;

namespace PointGaming.AudioChat
{
    public delegate void AudioStreamExistEvent(PgUser user, string roomId);

    public interface IAudioRoom
    {
        string AudioRoomId { get; }
        bool IsVoiceTeamOnly { get; }
        bool IsVoiceEnabled { get; }
        
        void OnVoiceStarted(PgUser user);
        void OnVoiceStopped(PgUser user);
    }

    public class AudioChatSession : IDisposable
    {
        private UserDataManager _userData;
        private NAudioSession _nAudioTest;
        private AudioChatClient _audioChatClient;
        private object _audioChatSynch = new object();
        private bool _hasBeenDisposed = false;
        
        public event Action<int> RecordingDeviceChanged;

        public DispatcherTimer _udpKeepAliveTimer;

        private long _microphonePriority = 1;

        public void TakeMicrophoneFocus(IAudioRoom room)
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
            public readonly IAudioRoom R;
            public long MicrophonePriority;
            public readonly List<PgUser> Senders = new List<PgUser>();

            public AudioRoomEx(IAudioRoom room)
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
                HomeWindow.Home.BeginInvokeUI((Action)delegate
                {
                    R.OnVoiceStarted(user);
                });
            }
            public void OnVoiceStopped(PgUser user)
            {
                lock (Senders)
                {
                    if (!Senders.Remove(user))
                        return;
                }
                HomeWindow.Home.BeginInvokeUI((Action)delegate
                {
                    R.OnVoiceStopped(user);
                });
            }
        }

        private readonly Dictionary<string, AudioRoomEx> _joinedRooms = new Dictionary<string, AudioRoomEx>();

        public void JoinRoom(IAudioRoom audioRoom)
        {
            AudioRoomEx roomEx;
            if (!_joinedRooms.TryGetValue(audioRoom.AudioRoomId, out roomEx))
            {
                roomEx = new AudioRoomEx(audioRoom);
                _joinedRooms[audioRoom.AudioRoomId] = roomEx;
            }
            SendJoinRoom(roomEx);
        }

        public void LeaveRoom(IAudioRoom audioRoom)
        {
            AudioRoomEx roomEx;
            if (!_joinedRooms.TryGetValue(audioRoom.AudioRoomId, out roomEx))
                return;

            foreach (var sender in roomEx.Senders)
                _nAudioTest.AudioReceiveEnded(sender.Id);

            _joinedRooms.Remove(audioRoom.AudioRoomId);
            SendLeaveRoom(roomEx);
        }
        
        public AudioChatSession(UserDataManager userData)
        {
            _userData = userData;
            _nAudioTest = new NAudioSession(new Opus16kCodec());
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
            _audioChatClient = new AudioChatClient(endpoint, _userData.PgSession.AuthToken);
            _audioChatClient.AudioReceived += _audioChatClient_MessageReceived;
            _audioChatClient.Stopped += _audioChatClient_Stopped;
            _audioChatClient.Start();

            _udpKeepAliveTimer.Start();
        }

        void _audioChatClient_Stopped(AudioChatClient obj)
        {
            lock (_audioChatSynch)
            {
                _udpKeepAliveTimer.Stop();

                if (_hasBeenDisposed)
                    return;

                _audioChatClient.AudioReceived -= _audioChatClient_MessageReceived;
                _audioChatClient.Stopped -= _audioChatClient_Stopped;

                ((Action)Connect).DelayInvoke(TimeSpan.FromSeconds(5));
            }
        }

        private void _udpKeepAliveTimer_Tick(object sender, EventArgs e)
        {
            foreach (var room in _joinedRooms.Values)
                SendJoinRoom(room);
        }

        private void SendJoinRoom(AudioRoomEx roomEx)
        {
            var message = new JoinRoomMessage
            {
                RoomName = roomEx.R.AudioRoomId,
                FromUserId = _userData.User.Id
            };
            _audioChatClient.Send(message);
        }

        private void SendLeaveRoom(AudioRoomEx roomEx)
        {
            var message = new LeaveRoomMessage
            {
                RoomName = roomEx.R.AudioRoomId,
                FromUserId = _userData.User.Id
            };
            _audioChatClient.Send(message);
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
                _audioChatClient.AudioReceived -= _audioChatClient_MessageReceived;
                _audioChatClient.Stopped -= _audioChatClient_Stopped;
                _audioChatClient.Stop();
                _audioChatClient = null;
            }

            _udpKeepAliveTimer.Stop();
        }

        public List<string> GetAudioInputDevices()
        {
            var devices = new List<string>();
            for (int n = 0; n < NA.Wave.WaveIn.DeviceCount; n++)
            {
                var capabilities = NA.Wave.WaveIn.GetCapabilities(n);
                devices.Add(capabilities.ProductName);
            }
            return devices;
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

        private void _audioChatClient_MessageReceived(AudioMessage obj)
        {
            if (obj.FromUserId == _userData.User.Id)
                return;

            var isEnd = obj.Audio.Length == 0;

            //App.LogLine(obj.FromUserId + " " + obj.MessageNumber + " " + obj.Audio.Length);

            var user = _userData.GetPgUser(obj.FromUserId);

            var isListening = false;
            AudioRoomEx roomEx;
            if (_joinedRooms.TryGetValue(obj.RoomName, out roomEx))
            {
                isListening = roomEx.R.IsVoiceEnabled && !user.IsMuted;
            }

            if (isEnd)
            {
                _nAudioTest.AudioReceiveEnded(obj.FromUserId);
                roomEx.OnVoiceStopped(user);
            }
            else
            {
                if (isListening)
                    _nAudioTest.AudioReceived(obj.FromUserId, obj.Audio);
                roomEx.OnVoiceSent(user);
            }
        }

        private AudioRoomEx _speakingRoom = null;
        private bool _speakingRoomTeamOnly;

        private void _nAudioTest_AudioRecorded(NAudioSession source, byte[] data)
        {
            var isFirst = _speakingRoom == null;
            if (isFirst)
                _speakingRoom = GetSpeakingRoom();

            if (_speakingRoom == null)
                return;

            if (isFirst)
                _speakingRoomTeamOnly = _speakingRoom.R.IsVoiceTeamOnly;

            var message = new AudioMessage
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
            if (_speakingRoom == null)
                return;

            var message = new AudioMessage
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
