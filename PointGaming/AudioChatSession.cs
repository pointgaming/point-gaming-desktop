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
using PointGaming.AudioChat;
using NA = NAudio;

namespace PointGaming
{
    public delegate void AudioStreamExistEvent(PgUser user, string roomId);

    public class AudioChatSession : IDisposable
    {
        private UserDataManager _userData;
        private NAudioTest _nAudioTest;
        private AudioChatClient _audioChatClient;

        public event Action<string> SpeakingRoomChanged;
        public event AudioStreamExistEvent AudioStarted;
        public event AudioStreamExistEvent AudioStopped;
        public event Action<int> RecordingDeviceChanged;

        public DispatcherTimer _udpKeepAliveTimer;

        private readonly List<PgUser> _audioSenders = new List<PgUser>();
        private string _speakingIntoRoomId;
        public string SpeakingIntoRoomId
        {
            get { return _speakingIntoRoomId; }
            set
            {
                if (value == _speakingIntoRoomId)
                    return;
                if (_messageNumber != 1)
                    _nAudioTest_AudioRecordEnded();
                _speakingIntoRoomId = value;
                SpeakingRoomChanged(value);
            }
        }

        public void UnsetSpeakingRoomId(string id)
        {
            if (_speakingIntoRoomId == id)
                SpeakingIntoRoomId = null;
        }

        private readonly HashSet<string> _joinedRooms = new HashSet<string>();

        public void JoinRoom(string id)
        {
            _joinedRooms.Add(id);
            SendJoinRoom(id);
        }
        public void LeaveRoom(string id)
        {
            _joinedRooms.Remove(id);
            SendLeaveRoom(id);
        }
        
        public AudioChatSession(UserDataManager userData)
        {
            _userData = userData;
            _nAudioTest = new NAudioTest(new Opus16kCodec());
            _nAudioTest.InputDeviceNumber = Properties.Settings.Default.AudioInputDeviceIndex;
            _nAudioTest.TriggerKey = (System.Windows.Input.Key)Properties.Settings.Default.MicTriggerKey;
            _nAudioTest.AudioRecorded += _nAudioTest_AudioRecorded;
            _nAudioTest.AudioRecordEnded += _nAudioTest_AudioRecordEnded;
            _nAudioTest.InputDeviceNumberChanged += _nAudioTest_InputDeviceNumberChanged;

            var audioChatIp = System.Net.IPAddress.Parse(Properties.Settings.Default.AudioChatIp);
            var audioChatPort = Properties.Settings.Default.AudioChatPort;
            var endpoint = new System.Net.IPEndPoint(audioChatIp, audioChatPort);
            _audioChatClient = new AudioChatClient(endpoint, _userData.PgSession.AuthTokenBytes);
            _audioChatClient.AudioReceived += _audioChatClient_MessageReceived;
            _audioChatClient.Start();

            _udpKeepAliveTimer = new DispatcherTimer();
            _udpKeepAliveTimer.Interval = TimeSpan.FromSeconds(55);
            _udpKeepAliveTimer.Tick += _udpKeepAliveTimer_Tick;
            _udpKeepAliveTimer.Start();
        }

        private void _udpKeepAliveTimer_Tick(object sender, EventArgs e)
        {
            foreach (var id in _joinedRooms)
            {
                SendJoinRoom(id);
            }
        }

        private void SendJoinRoom(string id)
        {
            var message = new JoinRoomMessage
            {
                RoomName = id,
                FromUserId = _userData.User.Id
            };
            _audioChatClient.Send(message);
        }

        private void SendLeaveRoom(string id)
        {
            var message = new LeaveRoomMessage
            {
                RoomName = id,
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
            if (_nAudioTest == null)
                return;

            _nAudioTest.Dispose();
            _nAudioTest = null;

            _audioChatClient.Stop();
            _audioChatClient = null;

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

            if (isEnd)
            {
                _nAudioTest.AudioReceiveEnded(obj.FromUserId);
                OnAudioStopped(_userData.GetPgUser(obj.FromUserId), obj.RoomName);
            }
            else
            {
                _nAudioTest.AudioReceived(obj.FromUserId, obj.Audio);
                OnAudioSending(_userData.GetPgUser(obj.FromUserId), obj.RoomName);
            }
        }

        private int _messageNumber = 1;

        private void _nAudioTest_AudioRecorded(NAudioTest source, byte[] data)
        {
            var roomId = _speakingIntoRoomId;
            if (roomId == null)
                return;

            var message = new AudioMessage
            {
                RoomName = roomId,
                FromUserId = _userData.User.Id,
                MessageNumber = _messageNumber++,
                Audio = data,
            };
            
            _audioChatClient.Send(message);
            OnAudioSending(_userData.User, roomId);
        }

        private void _nAudioTest_AudioRecordEnded()
        {
            var roomId = _speakingIntoRoomId;
            if (roomId == null)
            {
                _messageNumber = 1;
                return;
            }

            var message = new AudioMessage
            {
                RoomName = roomId,
                FromUserId = _userData.User.Id,
                MessageNumber = _messageNumber++,
                Audio = new byte[0],
            };

            _audioChatClient.Send(message);
            OnAudioStopped(_userData.User, roomId);
            _messageNumber = 1;
        }

        private void OnAudioSending(PgUser user, string roomId)
        {
            HomeWindow.Home.BeginInvokeUI((Action)delegate
            {
                if (!_audioSenders.Contains(user))
                {
                    _audioSenders.Add(user);
                    var call = AudioStarted;
                    if (call != null)
                        call(user, roomId);
                }
            });
        }

        private void OnAudioStopped(PgUser user, string roomId)
        {
            HomeWindow.Home.BeginInvokeUI((Action)delegate
            {
                _audioSenders.Remove(user);
                var call = AudioStopped;
                if (call != null)
                    call(user, roomId);
            });
        }
    }

}
