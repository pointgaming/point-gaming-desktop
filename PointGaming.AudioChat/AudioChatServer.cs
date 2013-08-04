using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PointGaming.AudioChat
{
    public class AudioChatServer
    {
        public const int DefaultPort = 13275;
        public event Action<AudioChatServer> Stopped;
        public event Action<int> MessageReceived;

        private readonly int _port;
        private volatile bool _isRunning = false;
        private volatile bool _shouldRun = false;
        public bool IsRunning { get { return _isRunning; } }

        private QuickMessage _quickMessage = new QuickMessage();
        private Socket _serverOut;

        private Dictionary<Utf8String, List<ClientConnection>> _rooms = new Dictionary<Utf8String, List<ClientConnection>>();

        private class ClientConnection
        {
            public readonly Utf8String Id;
            public readonly EndPoint Endp;

            public ClientConnection(Utf8String id, EndPoint ep) { Id = id; Endp = ep; }

            public override bool Equals(object obj)
            {
                var other = obj as ClientConnection;
                if (other == null)
                    return false;
                return Id.Equals(other.Id) && Endp.Equals(other.Endp);
            }
            public override int GetHashCode()
            {
                return Id.GetHashCode() | Endp.GetHashCode();
            }
            public override string ToString()
            {
                return Id.ToString() + " @ " + Endp.ToString();
            }
        }

        private readonly List<UdpMessage> _freeUdpMessage = new List<UdpMessage>();
        private readonly List<UdpMessage> _messageQueue = new List<UdpMessage>();

        private class UdpMessage
        {
            public readonly List<EndPoint> To = new List<EndPoint>();
            public readonly byte[] Data = new byte[ushort.MaxValue];
            public int Length;
        }

        private UdpMessage NewMessage()
        {
            var count = _freeUdpMessage.Count;
            if (count == 0)
                return new UdpMessage();
            count--;
            var res = _freeUdpMessage[count];
            _freeUdpMessage.RemoveAt(count);
            res.To.Clear();
            return res;
        }
        
        public AudioChatServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            _shouldRun = true;
            _isRunning = true;
            var t = new System.Threading.Thread(Run);
            t.IsBackground = true;
            t.Name = "Udp Audio Message Server";
            t.Start();
        }

        public void Stop()
        {
            _shouldRun = false;
        }

        private void Run()
        {
            try
            {
                _serverOut = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _port);
                _serverOut.Bind(endPoint);

                EndPoint ep = new IPEndPoint(IPAddress.Any, _port);
                var buffer = new byte[ushort.MaxValue];

                while (_shouldRun)
                {
                    try
                    {
                        int read = _serverOut.ReceiveFrom(buffer, ref ep);
                        if (read > 0)
                        {
                            HandleMessage(buffer, read, (IPEndPoint)ep);
                        }
                    }
                    catch (SocketException e)
                    {
                        if (e.SocketErrorCode != SocketError.TimedOut)
                            throw e;
                    }

                    if (_messageQueue.Count > 0)
                    {
                        foreach (var message in _messageQueue)
                        {
                            foreach (var sendEp in message.To)
                            {
                                _serverOut.SendTo(message.Data, message.Length, SocketFlags.None, sendEp);
                            }
                        }
                        _freeUdpMessage.AddRange(_messageQueue);
                        _messageQueue.Clear();
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
                _isRunning = false;
                var call = Stopped;
                if (call != null)
                    call(this);
            }
        }

        private void HandleMessage(byte[] buffer, int read, IPEndPoint ep)
        {
            var qm = _quickMessage;
            if (!qm.Read(buffer, read))
                return;

            var type = qm.MessageType;

            var call = MessageReceived;
            if (call != null)
                call(type);

            if (type == JoinRoomMessage.MType)
            {
                List<ClientConnection> eps;
                if (!_rooms.TryGetValue(qm.RoomName, out eps))
                {
                    eps = new List<ClientConnection>();
                    _rooms[qm.RoomName.DeepCopy()] = eps;
                }
                eps.Add(new ClientConnection(qm.FromUserId.DeepCopy(), Copy(ep)));
            }
            else if (type == LeaveRoomMessage.MType)
            {
                List<ClientConnection> eps;
                if (_rooms.TryGetValue(qm.RoomName, out eps))
                {
                    for (int i =0; i<eps.Count; i++)
                    {
                        var item = eps[i];
                        if (item.Id.Equals(qm.FromUserId) && item.Endp.Equals(ep))
                        {
                            eps.RemoveAt(i);
                            break;
                        }
                    }
                    if (eps.Count == 0)
                        _rooms.Remove(qm.RoomName);
                }
            }
            else if (type == AudioMessage.MType)
            {
                List<ClientConnection> eps;
                if (_rooms.TryGetValue(qm.RoomName, out eps))
                {
                    var m = CreateMessage(buffer, read, qm, eps);
                    _messageQueue.Add(m);
                }
            }
        }

        private IPEndPoint Copy(IPEndPoint ep)
        {
            var ser = ep.Serialize();
            return (IPEndPoint)ep.Create(ser);
        }

        private UdpMessage CreateMessage(byte[] buffer, int read, QuickMessage qm, List<ClientConnection> eps)
        {
            var m = NewMessage();
            Buffer.BlockCopy(buffer, 0, m.Data, 0, read);
            m.Length = read;
            foreach (var item in eps)
            {
                if (item.Id.Equals(qm.FromUserId))
                    continue;
                m.To.Add(item.Endp);
            }
            return m;
        }
    }
}
