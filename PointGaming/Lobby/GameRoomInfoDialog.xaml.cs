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
using SocketIOClient;
using SocketIOClient.Messages;
using PointGaming.POCO;

namespace PointGaming.Lobby
{
    public partial class GameRoomInfoDialog : Window
    {
        public SocketSession UserSession;
        public PgUser[] Members;

        public GameRoomInfoDialog()
        {
            InitializeComponent();
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void Show(GameRoomItem item, SocketSession session)
        {
            GameRoomInfoDialog info = new GameRoomInfoDialog
            {
                UserSession = session,
                Title = item.DisplayName,
                Members = item.Members
            };

            System.ComponentModel.ICollectionView mv = CollectionViewSource.GetDefaultView(info.Members);
            mv.GroupDescriptions.Add(new PropertyGroupDescription("TeamName"));
            info.listBoxMembership.DataContext = mv;


            info.ShowDialog();
        }
        
        public String TakeOverAmount
        {
            get { return "19,000 points"; }
        }

        public String Avatar
        {
            get { return "http://forums.pointgaming.com/assets/logo-3b643498dc7635d6ce4598843b5fcf0e.png"; }
        }
    }
}
