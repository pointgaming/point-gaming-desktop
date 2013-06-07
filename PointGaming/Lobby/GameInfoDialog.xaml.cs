using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PointGaming.Lobby
{
    public partial class GameInfoDialog : Window
    {
        public GameInfoDialog()
        {
            InitializeComponent();
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void Show(GameRoomItem item)
        {
            GameInfoDialog info = new GameInfoDialog
            {
                Title = item.DisplayName
            };
            info.ShowDialog();
        }
        
        public String TakeOverAmount
        {
            get { return "Take Over Amount: 19,000 points"; }
        }

        public String Avatar
        {
            get { return "http://forums.pointgaming.com/assets/logo-3b643498dc7635d6ce4598843b5fcf0e.png"; }
        }
    }
}
