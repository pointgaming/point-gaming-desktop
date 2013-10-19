using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PointGaming.Voice
{
    [JsonObject(MemberSerialization.OptIn)]
    class SerialPacketStream
    {
        [JsonProperty]
        public bool IsEncoded { get; set; }
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public int StreamNumber { get; set; }
        [JsonProperty]
        public string RoomName { get; set; }
        [JsonProperty]
        public bool IsTeamOnly { get; set; }

        public int Index { get; set; }
        [JsonProperty]
        public List<SerialPacket> Parts { get; set; }

        public SerialPacketStream() { }

        public SerialPacketStream(VoipMessageVoice item, bool isEncoded)
        {
            Id = item.FromUserId;
            StreamNumber = item.StreamNumber;
            RoomName = item.RoomName;
            IsTeamOnly = item.IsTeamOnly;
            IsEncoded = isEncoded;
            Parts = new List<SerialPacket>();
        }

        public void Write(string filePath)
        {
            Parts.Sort((one, two) => (one.MessageNumber.CompareTo(two.MessageNumber)));

            using (var tw = new System.IO.StreamWriter(System.IO.File.Open(filePath, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.Read), Encoding.UTF8))
            {
                var ser = new Newtonsoft.Json.JsonSerializer();

                var index = Index;
                Index = 0;
                ser.Serialize(tw, this);
                Index = index;
            }
        }

        public static SerialPacketStream Read(System.IO.FileInfo filePath)
        {
            using (var stream = System.IO.File.Open(filePath.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            {
                return Read(stream);
            }
        }

        public static SerialPacketStream Read(System.IO.Stream stream)
        {
            using (var tr = new System.IO.StreamReader(stream, Encoding.UTF8))
            {
                Newtonsoft.Json.JsonSerializer ser = new Newtonsoft.Json.JsonSerializer();

                var ps = ser.Deserialize(tr, typeof(SerialPacketStream)) as SerialPacketStream;
                return ps;
            }
        }

        public static string AppDataPath(SerialPacketStream ps)
        {
            var directory = App.ApplicationSettingsPath;
            var path = System.IO.Path.Combine(directory, ps.Id.FilterFilename() + "__" + ps.StreamNumber + ".pga");
            return path;
        }
    }

    class SerialPacket
    {
        public DateTime RxTime { get; set; }
        public int MessageNumber { get; set; }
        public byte[] Audio { get; set; }

        public SerialPacket() { }

        public SerialPacket(VoipMessageVoice item)
        {
            RxTime = DateTimePrecise.UtcNow;
            MessageNumber = item.MessageNumber;
            Audio = item.Audio;
        }
        public SerialPacket(int messageNumber, byte[] data)
        {
            RxTime = DateTimePrecise.UtcNow;
            MessageNumber = messageNumber;
            Audio = data;
        }
    }

}
