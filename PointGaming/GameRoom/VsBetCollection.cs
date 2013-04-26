using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PointGaming.GameRoom
{
    public sealed class VsBetCollection : ObservableCollection<Bet>
    {
        public VsBetCollection()
        {
            var winner = new PgUser { Id = Guid.NewGuid().ToString(), Username = "Mr.Apple" };
            var loser = new PgUser { Id = Guid.NewGuid().ToString(), Username = "Mr.Banana" };
            var booker = new PgUser { Id = Guid.NewGuid().ToString(), Username = "dean" };
            var better = new PgUser { Id = Guid.NewGuid().ToString(), Username = "dean2" };

            Bet item;
            
            item = new Bet { OffererChoice = winner, TakerChoice = loser, Offerer = booker, Taker = better, OffererOdds = "5:1", OffererWager = 10m, };
            Add(item);
            item = new Bet { OffererChoice = winner, TakerChoice = loser, Offerer = booker, Taker = better, OffererOdds = "1:5", OffererWager = 10m, };
            Add(item);
            item = new Bet { OffererChoice = winner, TakerChoice = loser, Offerer = booker, Taker = better, OffererOdds = "1:1", OffererWager = 10m, };
            Add(item);

            item = new Bet { OffererChoice = winner, TakerChoice = loser, Offerer = booker, Taker = better, OffererOdds = "5:1", OffererWager = 10m, };
            Add(item);
            item = new Bet { OffererChoice = winner, TakerChoice = loser, Offerer = booker, Taker = better, OffererOdds = "1:5", OffererWager = 10m, };
            Add(item);
            item = new Bet { OffererChoice = winner, TakerChoice = loser, Offerer = booker, Taker = better, OffererOdds = "1:1", OffererWager = 10m, };
            Add(item);

            item = new Bet { OffererChoice = winner, TakerChoice = loser, Offerer = booker, Taker = better, OffererOdds = "5:1", OffererWager = 10m, };
            Add(item);
            item = new Bet { OffererChoice = winner, TakerChoice = loser, Offerer = booker, Taker = better, OffererOdds = "1:5", OffererWager = 10m, };
            Add(item);
            item = new Bet { OffererChoice = winner, TakerChoice = loser, Offerer = booker, Taker = better, OffererOdds = "1:1", OffererWager = 10m, };
            Add(item);

            item = new Bet { OffererChoice = winner, TakerChoice = loser, Offerer = booker, Taker = better, OffererOdds = "5:1", OffererWager = 10m, };
            Add(item);
            item = new Bet { OffererChoice = winner, TakerChoice = loser, Offerer = booker, Taker = better, OffererOdds = "1:5", OffererWager = 10m, };
            Add(item);
            item = new Bet { OffererChoice = winner, TakerChoice = loser, Offerer = booker, Taker = better, OffererOdds = "1:1", OffererWager = 10m, };
            Add(item);
        }
    }
}
