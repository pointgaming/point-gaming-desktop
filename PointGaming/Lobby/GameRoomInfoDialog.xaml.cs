using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
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
    public partial class GameRoomInfoDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged(string propertyName)
        {
            var changedCallback = PropertyChanged;
            if (changedCallback == null)
                return;
            var args = new PropertyChangedEventArgs(propertyName);
            changedCallback(this, args);
        }

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

        private void gameRoomUrl_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(URL);
        }

        public static void Show(GameRoomItem item, SocketSession session)
        {
            GameRoomInfoDialog info = new GameRoomInfoDialog
            {
                UserSession = session,
                Title = item.DisplayName,
                Members = item.Members,
                Logo = "http://forums.pointgaming.com/assets/logo-3b643498dc7635d6ce4598843b5fcf0e.png",
                URL = item.URL
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

        private string _url;
        public string URL
        {
            get { return _url; }
            set
            {
                if (value == _url)
                    return;
                _url = value;
                NotifyChanged("URL");
            }
        }

        private string _logo;
        public string Logo
        {
            get { return _logo; }
            set
            {
                if (value == _logo)
                    return;
                _logo = value;
                NotifyChanged("Logo");
            }
        }
    }
}
