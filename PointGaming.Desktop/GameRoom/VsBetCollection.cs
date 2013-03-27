using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PointGaming.Desktop.GameRoom
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
            
            item = new Bet { Winner = winner, Loser = loser, Booker = booker, Better = better, Map = "Jungle", Odds = "5:1", Wager = 10m, };
            Add(item);
            item = new Bet { Winner = winner, Loser = loser, Booker = booker, Better = better, Map = "Desert", Odds = "1:5", Wager = 10m, };
            Add(item);
            item = new Bet { Winner = winner, Loser = loser, Booker = booker, Better = better, Map = "Skyscraper", Odds = "1:1", Wager = 10m, };
            Add(item);

            item = new Bet { Winner = winner, Loser = loser, Booker = booker, Better = better, Map = "Jungle1", Odds = "5:1", Wager = 10m, };
            Add(item);
            item = new Bet { Winner = winner, Loser = loser, Booker = booker, Better = better, Map = "Desert1", Odds = "1:5", Wager = 10m, };
            Add(item);
            item = new Bet { Winner = winner, Loser = loser, Booker = booker, Better = better, Map = "Skyscraper1", Odds = "1:1", Wager = 10m, };
            Add(item);

            item = new Bet { Winner = winner, Loser = loser, Booker = booker, Better = better, Map = "Jungle2", Odds = "5:1", Wager = 10m, };
            Add(item);
            item = new Bet { Winner = winner, Loser = loser, Booker = booker, Better = better, Map = "Desert2", Odds = "1:5", Wager = 10m, };
            Add(item);
            item = new Bet { Winner = winner, Loser = loser, Booker = booker, Better = better, Map = "Skyscraper2", Odds = "1:1", Wager = 10m, };
            Add(item);

            item = new Bet { Winner = winner, Loser = loser, Booker = booker, Better = better, Map = "Jungle3", Odds = "5:1", Wager = 10m, };
            Add(item);
            item = new Bet { Winner = winner, Loser = loser, Booker = booker, Better = better, Map = "Desert3", Odds = "1:5", Wager = 10m, };
            Add(item);
            item = new Bet { Winner = winner, Loser = loser, Booker = booker, Better = better, Map = "Skyscraper3", Odds = "1:1", Wager = 10m, };
            Add(item);
        }
    }
}
