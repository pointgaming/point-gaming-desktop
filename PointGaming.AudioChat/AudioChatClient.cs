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
        public const int DefaultPort = 13275;
        public event Action<AudioChatClient> Stopped;
        public event Action<AudioMessage> MessageReceived;

        private readonly IPEndPoint _serverEndPoint;
        private volatile bool _isRunning = false;
        private volatile bool _shouldRun = false;
        public bool IsRunning { get { return _isRunning; } }

        private Socket _clientOut;

        private System.Threading.AutoResetEvent _are = new System.Threading.AutoResetEvent(false);
        private List<IChatMessage> _messageQueue = new List<IChatMessage>();

        public AudioChatClient(IPEndPoint serverEndPoint)
        {
            _serverEndPoint = serverEndPoint;
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
                            int len = message.Write(buffer);
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
            if (type == AudioMessage.MType)
            {
                var audio = new AudioMessage();
                if (!audio.Read(buffer, read))
                    return;
                var call = MessageReceived;
                if (call != null)
                    call(audio);
            }
        }
    }
}
