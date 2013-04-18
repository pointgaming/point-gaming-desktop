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
            
            item = new Bet { BookerChoice = winner, BetterChoice = loser, Booker = booker, Better = better, Odds = "5:1", Wager = 10m, };
            Add(item);
            item = new Bet { BookerChoice = winner, BetterChoice = loser, Booker = booker, Better = better, Odds = "1:5", Wager = 10m, };
            Add(item);
            item = new Bet { BookerChoice = winner, BetterChoice = loser, Booker = booker, Better = better, Odds = "1:1", Wager = 10m, };
            Add(item);

            item = new Bet { BookerChoice = winner, BetterChoice = loser, Booker = booker, Better = better, Odds = "5:1", Wager = 10m, };
            Add(item);
            item = new Bet { BookerChoice = winner, BetterChoice = loser, Booker = booker, Better = better, Odds = "1:5", Wager = 10m, };
            Add(item);
            item = new Bet { BookerChoice = winner, BetterChoice = loser, Booker = booker, Better = better, Odds = "1:1", Wager = 10m, };
            Add(item);

            item = new Bet { BookerChoice = winner, BetterChoice = loser, Booker = booker, Better = better, Odds = "5:1", Wager = 10m, };
            Add(item);
            item = new Bet { BookerChoice = winner, BetterChoice = loser, Booker = booker, Better = better, Odds = "1:5", Wager = 10m, };
            Add(item);
            item = new Bet { BookerChoice = winner, BetterChoice = loser, Booker = booker, Better = better, Odds = "1:1", Wager = 10m, };
            Add(item);

            item = new Bet { BookerChoice = winner, BetterChoice = loser, Booker = booker, Better = better, Odds = "5:1", Wager = 10m, };
            Add(item);
            item = new Bet { BookerChoice = winner, BetterChoice = loser, Booker = booker, Better = better, Odds = "1:5", Wager = 10m, };
            Add(item);
            item = new Bet { BookerChoice = winner, BetterChoice = loser, Booker = booker, Better = better, Odds = "1:1", Wager = 10m, };
            Add(item);
        }
    }
}
