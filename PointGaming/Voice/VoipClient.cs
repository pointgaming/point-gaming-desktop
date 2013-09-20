using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PointGaming.Voice
{
    class VoipClient
    {
        public event Action<VoipClient> Stopped;
        public event Action<IVoipMessage> MessageReceived;

        private readonly byte[] _key;
        private readonly IPEndPoint _serverEndPoint;
        private volatile bool _isRunning = false;
        private volatile bool _shouldRun = false;
        public bool IsRunning { get { return _isRunning; } }

        private Socket _clientOut;

        private System.Threading.AutoResetEvent _are = new System.Threading.AutoResetEvent(false);
        private List<IVoipMessage> _messageQueue = new List<IVoipMessage>();

        public VoipClient(IPEndPoint serverEndPoint, string authToken)
        {
            _serverEndPoint = serverEndPoint;
            _key = authToken.GuidToHex().HexToBytes();
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

        public void Send(IVoipMessage message)
        {
            if (!_isRunning)
                return;
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
                Console.WriteLine("Voip client starting...");
                _clientOut = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _serverEndPoint.Port);
                _clientOut.Bind(endPoint);

                var buffer = new byte[ushort.MaxValue];

                var t = new System.Threading.Thread(SendRun);
                t.IsBackground = true;
                t.Name = "Udp Audio Message Client Sender";
                t.Start();

                Console.WriteLine("Voip client started.");

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
                try
                {
                    if (_clientOut != null)
                        _clientOut.Dispose();
                }
                catch { }
                Console.WriteLine("Voip client stopped.");

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
                var queue = new List<IVoipMessage>();
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
                            //var str = "tx: 0x" + BitConverter.ToString(buffer, 0, len).Replace("-", string.Empty).ToLower();
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


        private void HandleMessage(byte[] buffer, int length)
        {
            Console.WriteLine("rx: " + buffer.BytesToHex(0, length));

            if (length == 1 && buffer[0] == 0)
            {
                // todo remove this case.  Nicks's original response from join room... lame
            }
            else if (!HandleEncryptedMessage(buffer, length))
            {
                Console.WriteLine("Unrecognized audio chat message: " + buffer.BytesToHex(0, length));
            }
        }

        private bool HandleEncryptedMessage(byte[] buffer, int length)
        {
            var position = 0;

            byte[] iv = new byte[16];
            if (!VoipSerialization.ReadRawBytes(buffer, length, ref position, iv))
                return false;

            var decryptedData = VoipCrypt.Decrypt(_key, iv, buffer, position, length - position);
            buffer = decryptedData;
            position = 0;
            length = buffer.Length;

            position += 4;// nonce

            var antidos = new byte[4];
            if (!VoipSerialization.ReadRawBytes(buffer, length, ref position, antidos))
                return false;
            if (!VoipCrypt.AntiDosCheck(antidos))
                return false;

            if (position >= length)
                return false;
            var mType = buffer[position++];

            var voiceMessage = Instantiate(mType);
            if (voiceMessage == null)
                return false;
            if (!voiceMessage.Read(buffer, position, length))
                return false;

            var call = MessageReceived;
            if (call != null)
                call(voiceMessage);

            return true;
        }

        private static IVoipMessage Instantiate(byte mType)
        {
            IVoipMessage m;
            switch (mType)
            {
                case (VoipMessageVoice.MType):
                    m = new VoipMessageVoice();
                    break;
                case (VoipMessageJoinRoom.MType):
                    m = new VoipMessageJoinRoom();
                    break;
                case (VoipMessageLeaveRoom.MType):
                    m = new VoipMessageLeaveRoom();
                    break;
                default:
                    m = null;
                    break;
            }
            return m;
        }

    }
}
