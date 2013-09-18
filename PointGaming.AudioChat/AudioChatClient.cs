﻿using System;
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

        public AudioChatClient(IPEndPoint serverEndPoint, string authToken)
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
                            var str = "0x" + BitConverter.ToString(buffer, 0, len).Replace("-", string.Empty).ToLower();
                            Console.WriteLine(str);
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
            if (length == 1 && buffer[0] == 0)
            {
                // server's response from join room.  do nothing
            }
            else if (!HandleEncryptedMessage(buffer, length))
            {
                Console.WriteLine("Unrecognized audio chat message: " + buffer.BytesToHex());
            }
        }

        private bool HandleEncryptedMessage(byte[] buffer, int length)
        {
            var position = 0;

            byte[] iv = new byte[16];
            if (!BufferIO.ReadRawBytes(buffer, length, ref position, iv))
                return false;

            var decryptedData = AesIO.AesDecrypt(_key, iv, buffer, position, length - position);
            buffer = decryptedData;
            position = 0;
            length = buffer.Length;

            position += 4;// nonce

            var antidos = new byte[4];
            if (!BufferIO.ReadRawBytes(buffer, length, ref position, antidos))
                return false;
            if (!AesIO.AntiDosCheck(antidos))
                return false;

            if (position >= length)
                return false;
            var mType = buffer[position++];

            switch (mType)
            {
                case (AudioMessage.MType):
                    {
                        var m = new AudioMessage();
                        if (!m.Read(buffer, position, length))
                            return false;
                        var call = AudioReceived;
                        if (call != null)
                            call(m);
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            return true;
        }

    }
}
