using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PointGaming.Lobby
{
    public sealed class VsGameRoomItemCollection : ObservableCollection<GameRoomItem>
    {
        public VsGameRoomItemCollection()
        {
            var item = new GameRoomItem {
                Id = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10),
                Description = "5 vs 5 Dust 2 No Scrubs Will ban for being bad No 8 digs",
                MaxMemberCount = 10,
                MemberCount = 10,
                Position = 1,
                IsLocked = false,
            };
            Add(item);

            item = new GameRoomItem
            {
                Id = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10),
                Description = "Team dP",
                MaxMemberCount = 99,
                MemberCount = 1,
                Position = 2,
                IsLocked = true,
            };
            Add(item);

            item = new GameRoomItem
            {
                Id = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10),
                Description = "",
                MaxMemberCount = 99,
                MemberCount = 1,
                Position = 3,
                IsLocked = false,
            };
            Add(item);
        }
    }
}
