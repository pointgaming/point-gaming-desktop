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
    public class VoiceTester
    {
        private const string PlaybackStreamId = "VoiceTester";
        private static readonly long SampleTicks = TimeSpan.FromMilliseconds(20).Ticks;
        private AudioHardwareSession _nAudioTest;
        private readonly List<byte[]> Data = new List<byte[]>();
        private readonly List<double> PowerValues = new List<double>();
        private bool _shouldPlayback = false;
        private bool _shouldClearOnRecord = true;
        public bool IsAutoPlayback = true;

        public delegate void Event(VoiceTester.EventType type, TimeSpan time, double signalPower);

        public enum EventType
        {
            Recorded,
            RecordEnded,
            Played,
            PlayEnded,
        }
        public event VoiceTester.Event OnVoiceEvent;
        private void OnEvent(EventType type, TimeSpan time, double signalPower)
        {
            var call = OnVoiceEvent;
            if (call != null)
            {
                ((Action)delegate
                {
                    call(type, time, signalPower);
                }).BeginInvokeUI();
            }
        }

        internal void Init(AudioHardwareSession nAudioTest)
        {
            _nAudioTest = nAudioTest;
        }

        internal void Recorded(byte[] data, double signalPower)
        {
            TimeSpan dTime;
            lock (Data)
            {
                if (_shouldClearOnRecord)
                {
                    Data.Clear();
                    PowerValues.Clear();
                    _shouldClearOnRecord = false;
                }

                _shouldPlayback = false;
                Data.Add(data);
                PowerValues.Add(signalPower);
                dTime = TimeSpan.FromTicks(SampleTicks * Data.Count);
            }
            OnEvent(EventType.Recorded, dTime, signalPower);
        }

        internal void RecordEnded()
        {
            _shouldClearOnRecord = true;
            OnEvent(EventType.RecordEnded, TotalTime, 0);
            if (IsAutoPlayback)
                Playback();
        }

        public TimeSpan TotalTime
        {
            get
            {

                TimeSpan dTime;
                lock (Data)
                {
                    dTime = TimeSpan.FromTicks(SampleTicks * Data.Count);
                }
                return dTime;
            }
        }

        public void Playback()
        {
            lock (Data)
            {
                if (Data.Count == 0)
                    return;
                _shouldPlayback = true;
            }

            System.Threading.Thread t = new System.Threading.Thread(_Playback);
            t.IsBackground = true;
            t.Name = "VoiceTester Playback";
            t.Start();
        }

        private void _Playback()
        {
            int offset = 0;
            DateTime start = DateTime.UtcNow;
            while (true)
            {
                byte[] data;
                double signalPower;
                lock (Data)
                {
                    if (offset >= Data.Count || !_shouldPlayback)
                        break;
                    data = Data[offset];
                    signalPower = PowerValues[offset];
                }
                offset++;

                _nAudioTest.AudioReceived(PlaybackStreamId, data);
                var dTime = TimeSpan.FromTicks(SampleTicks * offset);
                OnEvent(EventType.Played, dTime, signalPower);

                var sleepSpan = (start + dTime) - DateTime.UtcNow;
                if (sleepSpan.Ticks > 0)
                    System.Threading.Thread.Sleep(sleepSpan);
            }

            _nAudioTest.AudioReceiveEnded(PlaybackStreamId);
            OnEvent(EventType.PlayEnded, TotalTime, 0);
        }
    }
}
