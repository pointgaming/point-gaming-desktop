using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PointGaming.AudioChat
{
    public class AudioChatClient
    {
        public event Action<AudioChatClient> Stopped;
        public event Action<AudioMessage> AudioReceived;
        public event Action<JoinRoomMessage> JoinReceived;
        public event Action<LeaveRoomMessage> LeaveReceived;

        private readonly byte[] _key;
        private readonly IPEndPoint _serverEndPoint;
        private volatile bool _isRunning = false;
        private volatile bool _shouldRun = false;
        public bool IsRunning { get { return _isRunning; } }

        private Socket _clientOut;

        private System.Threading.AutoResetEvent _are = new System.Threading.AutoResetEvent(false);
        private List<IChatMessage> _messageQueue = new List<IChatMessage>();

        public AudioChatClient(IPEndPoint serverEndPoint, byte[] key)
        {
            _serverEndPoint = serverEndPoint;
            _key = key;
        }

        public void Start()
        {
            _shouldRun = true;
            _isRunning = true;
            var t = new System.Threading.Thread(Run);
            t.IsBackground = true;
            t.Name = "Udp Audio Message Client Receiver";
            t.Start();
        }

        public void Stop()
        {
            _shouldRun = false;
        }

        public void Send(IChatMessage message)
        {
            lock (_messageQueue)
            {
                _messageQueue.Add(message);
            }
            _are.Set();
        }

        private void Run()
        {
            try
            {
                _clientOut = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _serverEndPoint.Port);
                _clientOut.Bind(endPoint);

                var buffer = new byte[ushort.MaxValue];

                var t = new System.Threading.Thread(SendRun);
                t.IsBackground = true;
                t.Name = "Udp Audio Message Client Sender";
                t.Start();

                while (_shouldRun)
                {
                    try
                    {
                        int read = _clientOut.Receive(buffer);
                        if (read > 0)
                        {
                            HandleMessage(buffer, read);
                        }
                    }
                    catch (SocketException e)
                    {
                        if (e.SocketErrorCode != SocketError.TimedOut)
                            throw e;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                _shouldRun = false;
                _isRunning = false;
                var call = Stopped;
                if (call != null)
                    call(this);
            }
        }

        private void SendRun()
        {
            try
            {
                var queue = new List<IChatMessage>();
                var buffer = new byte[ushort.MaxValue];

                while (_shouldRun)
                {
                    _are.WaitOne();

                    lock (_messageQueue)
                    {
                        queue.AddRange(_messageQueue);
                        _messageQueue.Clear();
                    }

                    if (queue.Count > 0)
                    {
                        foreach (var message in queue)
                        {
                            int len = message.Write(buffer, _key);
                            //var str = "0x" + BitConverter.ToString(buffer, 0, len).Replace("-", string.Empty);
                            //Console.WriteLine(str);
                            _clientOut.SendTo(buffer, len, SocketFlags.None, _serverEndPoint);
                        }
                        queue.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                _shouldRun = false;
            }
        }

        private void HandleMessage(byte[] buffer, int read)
        {
            var type = buffer[0];
            switch (type)
            {
                case (AudioMessage.MType):
                    {
                        var m = new AudioMessage();
                        if (!m.Read(buffer, read, _key))
                            return;
                        var call = AudioReceived;
                        if (call != null)
                            call(m);
                        break;
                    }
                case (JoinRoomMessage.MType):
                    {
                        var m = new JoinRoomMessage();
                        if (!m.Read(buffer, read, _key))
                            return;
                        var call = JoinReceived;
                        if (call != null)
                            call(m);
                        break;
                    }
                case (LeaveRoomMessage.MType):
                    {
                        var m = new LeaveRoomMessage();
                        if (!m.Read(buffer, read, _key))
                            return;
                        var call = LeaveReceived;
                        if (call != null)
                            call(m);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
