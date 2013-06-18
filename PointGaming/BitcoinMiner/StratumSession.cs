using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using SocketIOClient;
using SocketIOClient.Messages;
using System.Runtime.InteropServices;

namespace PointGaming.BitcoinMiner
{
    public class StratumSession : IDisposable
    {
        private EndPoint Endpoint;
        private string WorkerName;
        private string WorkerPassword;
        private string ExtraNonce1;
        private int ExtraNonce2Size;
        private bool ShouldBeConnected = true;
        private bool IsRunning = true;
        private double Difficulty = 1.0;

        public bool? IsSubscribed { get; private set; }
        public bool? IsAuthorized { get; private set; }

        private Socket MySocket;

        public event Action<bool> ConnectionConcluded;

        private DateTime _connectTimeout;

        public void Connect(EndPoint endpoint, string workerName, string workerPassword, TimeSpan timeout)
        {
            Endpoint = endpoint;
            WorkerName = workerName;
            WorkerPassword = workerPassword;

            _connectTimeout = DateTime.UtcNow.Add(timeout);

            System.Threading.Thread t = new System.Threading.Thread(Connect);
            t.IsBackground = true;
            t.Name = "Stratum Connection";
            t.Start();
        }

        private void Connect()
        {
            MySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            MySocket.Connect(Endpoint);

            Subscribe();
            Authorize();
            MonitorSocket();
        }

        public void Dispose()
        {
            if (!IsRunning)
                return;

            ShouldBeConnected = false;
            while (IsRunning)
                System.Threading.Thread.Sleep(100);
            MySocket.Close();
            MySocket = null;
        }


        private const EXECUTION_STATE DontSleep = EXECUTION_STATE.ES_AWAYMODE_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED;
        private const EXECUTION_STATE AllowSleep = EXECUTION_STATE.ES_CONTINUOUS;

        private static bool EnableSleep { set { SetThreadExecutionState(value ? AllowSleep : DontSleep); } }

        [FlagsAttribute]
        private enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001,
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        private void MonitorSocket()
        {
            try
            {
                EnableSleep = false;

                var myShares = new List<string>();
                var buffer = new byte[32768];// 32kB, a generous buffer size
                int offset = 0;

                bool isConnected = false;

                while (ShouldBeConnected)
                {
                    if (!isConnected)
                    {
                        if (IsSubscribed == false || IsAuthorized == false)
                        {
                            FireConnect(false);
                            break;
                        }

                        if (IsSubscribed == true && IsAuthorized == true)
                        {
                            FireConnect(true);
                            isConnected = true;
                        }
                        else if (DateTime.UtcNow > _connectTimeout)
                        {
                            FireConnect(false);
                            break;
                        }
                    }

                    int byteCount = 0;

                    int readAmount = MySocket.Available;
                    if (readAmount > 0)
                    {
                        int maxReadable = buffer.Length - offset;
                        if (readAmount < maxReadable)
                            maxReadable = readAmount;
                        byteCount = MySocket.Receive(buffer, offset, maxReadable, SocketFlags.None);
                        ScanLine(buffer, ref offset, byteCount);
                    }

                    lock (outgoingShares)
                    {
                        if (outgoingShares.Count > 0)
                        {
                            myShares.AddRange(outgoingShares);
                            outgoingShares.Clear();
                        }
                    }

                    if (myShares.Count > 0)
                    {
                        foreach (var share in myShares)
                        {
                            var submitShareBytes = System.Text.Encoding.ASCII.GetBytes(share);
                            var sent = MySocket.Send(submitShareBytes);
                        }

                        myShares.Clear();
                    }

                    if (byteCount <= 0)
                        System.Threading.Thread.Sleep(100);
                }
            }
            finally
            {
                IsRunning = false;
                EnableSleep = true;
            }
        }

        private void FireConnect(bool isConnected)
        {
            var cc = ConnectionConcluded;
            if (cc != null)
                cc(isConnected);
        }

        private void Authorize()
        {
            var loginMessage = SimpleJson.SimpleJson.SerializeObject(
                new { @params = new string[] { WorkerName, WorkerPassword }, id = 2, method = "mining.authorize" }
            ) + "\n";
            var loginBytes = System.Text.Encoding.ASCII.GetBytes(loginMessage);
            MySocket.Send(loginBytes);
        }

        private void Subscribe()
        {
            var subscribeMessage = SimpleJson.SimpleJson.SerializeObject(
                new { id = 1, method = "mining.subscribe", @params = new string[0] }
            ) + "\n";
            var subscribeBytes = System.Text.Encoding.ASCII.GetBytes(subscribeMessage);
            MySocket.Send(subscribeBytes);
        }

        private void ScanLine(byte[] buffer, ref int offset, int byteCount)
        {
            var endChar = (byte)'\n';

            var totalCount = offset + byteCount;
            for (; offset < totalCount; offset++)
            {
                if (buffer[offset] == endChar)
                {
                    int messageCount = offset + 1;
                    string message = System.Text.Encoding.ASCII.GetString(buffer, 0, messageCount);
                    var remainCount = totalCount - messageCount;
                    Buffer.BlockCopy(buffer, messageCount, buffer, 0, remainCount);
                    offset = -1;
                    totalCount = remainCount;

                    HandleMessage(message);
                }
            }

            if (offset == buffer.Length)
            {
                offset = 0;
                App.LogLine("Stratum error: read buffer overflow");
            }
        }

        private void HandleMessage(string message)
        {
            try
            {
                dynamic o = SimpleJson.SimpleJson.DeserializeObject(message);

                long? id = o.id;
                if (id == null)
                {
                    string method = o.method;
                    if (method == "mining.notify")
                        OnMiningJob(o.@params);
                    else if (method == "mining.set_difficulty")
                        OnMiningDifficulty(o.@params);
                }
                else if (id == 1)
                    OnSubscribeResult(o);
                else if (id == 2)
                    OnLoginResult(o);
                else if (id == 4)
                    OnSubmitShareResult(o);
            }
            catch (Exception e)
            {
                App.LogLine("Stratum error: " + e.Message + "\r\nStack Trace:\r\n" + e.StackTrace);
            }
        }

        private List<string> outgoingShares = new List<string>();

        public void SubmitShare(StratumHeaderBuilder work, string nonce)
        {
            var submitShareMessage = SimpleJson.SimpleJson.SerializeObject(
                new { @params = new string[] {
                    WorkerName,
                    work.Job.JobId,
                    work.ExtraNonce2,
                    ReverseEndian(work.NTime.ToBytes().ToHexString()),
                    nonce
                }, id = 4, method = "mining.submit" }
            ) + "\n";

            lock (outgoingShares)
            {
                outgoingShares.Add(submitShareMessage);
                App.LogLine("Stratum found: " + submitShareMessage.Trim() + " @ " + DateTime.Now);
            }
        }

        private void OnSubmitShareResult(dynamic o)
        {
            if (o.error != null)
            {
                App.LogLine("Statum error on submit share: " + o.error);
                return;
            }

            bool submitShareResult = o.result;
        }

        private void OnMiningDifficulty(SimpleJson.JsonArray array)
        {
            Difficulty = (double)array[0];
        }

        public UInt256 GetHashTarget()
        {
            UInt256 hashTarget = new UInt256("00000000ffff0000000000000000000000000000000000000000000000000000");
            UInt256 maxHashTarget = new UInt256("00000000ffffffff000000000000000000000000000000000000000000000000");

            if (Difficulty == 1.0)
            { }
            else
            {
                // divide by difficulty to figure out the share target
                var asDouble = (double)(hashTarget.ToBigInteger());
                var adjusted = asDouble / Difficulty;
                hashTarget = new UInt256((System.Numerics.BigInteger)adjusted);
                if (hashTarget.CompareTo(maxHashTarget) > 0)
                    hashTarget = maxHashTarget;
            }
            return hashTarget;
        }

        private void OnMiningJob(SimpleJson.JsonArray array)
        {
            var merkleBranchJSon = (SimpleJson.JsonArray)array[4]; // List of hashes, will be used for calculation of merkle root. This is not a list of all transactions, it only contains prepared hashes of steps of merkle tree algorithm. Please read some materials for understanding how merkle trees calculation works. Unfortunately this example don't have any step hashes included, my bad!
            var merkleBranch = new string[merkleBranchJSon.Count];
            for (int i = 0; i < merkleBranch.Length; i++)
                merkleBranch[i] = (string)merkleBranchJSon[i];

            var prevHashBytes = ((string)array[1]).ParseHex();
            prevHashBytes.ByteReverse();
            var prevHash = prevHashBytes.ToHexString();


            var nTimeString = ReverseEndian((string)array[7]);
            var nTimeInt = BitConverter.ToUInt32(nTimeString.ParseHex(), 0);

            //DateTime nTimeDT = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            //nTimeDT = nTimeDT.AddSeconds(nTimeInt);

            StratumJob job = new StratumJob
            {
                JobId = (string)array[0], // ID of the job. Use this ID while submitting share generated from this job.
                PrevHash = prevHash, // Hash of previous block.
                CoinB1 = (string)array[2], // Initial part of coinbase transaction.
                CoinB2 = (string)array[3], // Final part of coinbase transaction.
                MerkleBranch = merkleBranch,
                Version = ReverseEndian((string)array[5]), // Bitcoin block version.
                NBits = ReverseEndian((string)array[6]), // Encoded current network difficulty
                NTime = nTimeInt, // Current ntime/
                ReceiveTime = DateTime.UtcNow,
                Target = GetHashTarget(),
            };

            bool cleanJobs = (bool)array[8]; // When true, server indicates that submitting shares from previous jobs don't have a sense and such shares will be rejected. When this flag is set, miner should also drop all previous jobs, so job_ids can be eventually rotated.

            CurrentJob = job;
        }

        private StratumJob CurrentJob = null;


        private static string ReverseEndian(string s)
        {
            var chars = s.ToCharArray();
            Array.Reverse(chars);
            for (int i = 0; i < chars.Length; i += 2)
            {
                var a = chars[i];
                var b = chars[i + 1];
                chars[i] = b;
                chars[i + 1] = a;
            }
            return new string(chars);
        }

        private void OnLoginResult(dynamic o)
        {
            if (o.error != null)
            {
                App.LogLine("Stratum error on login: " + o.error);
                IsAuthorized = false;
                return;
            }

            bool loginResult = o.result;
            IsAuthorized = loginResult;
        }

        private void OnSubscribeResult(dynamic o)
        {
            if (o.error != null)
            {
                App.LogLine("Stratum error on subscribe: " + o.error);
                IsSubscribed = false;
                return;
            }

            SimpleJson.JsonArray result = o.result;
            SimpleJson.JsonArray subscriptionDetails = (SimpleJson.JsonArray)result[0];
            string miningDifficulty = (string)(((SimpleJson.JsonArray)(subscriptionDetails[0]))[1]);
            string miningNotify = (string)(((SimpleJson.JsonArray)(subscriptionDetails[1]))[1]);
            ExtraNonce1 = (string)result[1];
            ExtraNonce2Size = (int)((long)result[2]);

            IsSubscribed = true;
        }

        internal StratumHeaderBuilder GetLatestBuilder(StratumHeaderBuilder oldBuilder)
        {
            var newJob = CurrentJob;
            if (newJob == null)
                return null;

            if (oldBuilder != null && oldBuilder.Job == newJob)
                return oldBuilder;

            var _extraNonce2 = new byte[ExtraNonce2Size];
            RNG.GetBytes(_extraNonce2);

            var builder = new StratumHeaderBuilder(newJob, ExtraNonce1, _extraNonce2.ToHexString());
            return builder;
        }

        private static System.Security.Cryptography.RandomNumberGenerator RNG = System.Security.Cryptography.RandomNumberGenerator.Create();
    }
}
