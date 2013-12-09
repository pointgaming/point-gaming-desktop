using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PointGaming.Lobby
{
    public partial class GameRoomPanel : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event RoutedEventHandler JoinClick;
        public event RoutedEventHandler InfoClick;
        public event RoutedEventHandler StartClick;
        public event RoutedEventHandler TakeoverClick;

        private void NotifyChanged(string propertyName)
        {
            var changedCallback = PropertyChanged;
            if (changedCallback == null)
                return;
            var args = new PropertyChangedEventArgs(propertyName);
            changedCallback(this, args);
        }
        private UserDataManager _userData = UserDataManager.UserData;

        public GameRoomPanel()
        {
            InitializeComponent();
        }

        private void hyperLinkInfoClick(object sender, RoutedEventArgs e)
        {
            var del = InfoClick;
            if (del != null)
                del(this, new RoutedEventArgs());
        }

        private void buttonButtonJoin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                // TODO: show context menu only if user has takeover rights
                GameRoomItem item;
                var a = ((DependencyObject)sender).TryGetPresentedParent(out item);
                if (_userData.CanTakeOverRoom(item) == false)
                    ((Control)buttonButtonJoin.ContextMenu.Items[1]).IsEnabled = false;
            }
        }
        
        private void buttonButtonJoin_Click(object sender, RoutedEventArgs e)
        {
            var del = JoinClick;
            if (del != null)
                del(this, new RoutedEventArgs());
        }

        private void buttonStartGameRoom_Click(object sender, RoutedEventArgs e)
        {
            var del = StartClick;
            if (del != null)
                del(this, new RoutedEventArgs());
        }

        private void buttonTakeoverGameRoom_Click(object sender, RoutedEventArgs e)
        {
            var del = TakeoverClick;
            if (del != null)
                del(this, new RoutedEventArgs());
        }

        public static readonly DependencyProperty GameRoomTitleProperty = DependencyProperty.Register(
            "GameRoomTitle", typeof(string), typeof(GameRoomPanel)
            //,
            //new FrameworkPropertyMetadata("Game #", //FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            //    new PropertyChangedCallback(OnFirstPropertyChanged)
            //    //, null, false, UpdateSourceTrigger.PropertyChanged
            //    )
            );
        public string GameRoomTitle
        {
            get {return this.GetValue(GameRoomTitleProperty) as string; }
            set { this.SetValue(GameRoomTitleProperty, value); }
        }

        public static readonly DependencyProperty GameRoomPopulationProperty = DependencyProperty.Register(
            "GameRoomPopulation", typeof(string), typeof(GameRoomPanel));
        public string GameRoomPopulation
        {
            get { return this.GetValue(GameRoomPopulationProperty) as string; }
            set { this.SetValue(GameRoomPopulationProperty, value); }
        }
        public static readonly DependencyProperty GameRoomDescriptionProperty = DependencyProperty.Register(
            "GameRoomDescription", typeof(FlowDocument), typeof(GameRoomPanel));
        public FlowDocument GameRoomDescription
        {
            get { return this.GetValue(GameRoomDescriptionProperty) as FlowDocument; }
            set { this.SetValue(GameRoomDescriptionProperty, value); }
        }

        public static readonly DependencyProperty IsNewProperty = DependencyProperty.Register(
            "IsNew", typeof(bool), typeof(GameRoomPanel));
        public bool IsNew
        {
            get { return (bool)this.GetValue(IsNewProperty); }
            set { this.SetValue(IsNewProperty, value); }
        }

        public static readonly DependencyProperty IsNotNewProperty = DependencyProperty.Register(
            "IsNotNew", typeof(bool), typeof(GameRoomPanel));
        public bool IsNotNew
        {
            get { return (bool)this.GetValue(IsNotNewProperty); }
            set { this.SetValue(IsNotNewProperty, value); }
        }

        public static readonly DependencyProperty IsJoinableProperty = DependencyProperty.Register(
            "IsJoinable", typeof(bool), typeof(GameRoomPanel));
        public bool IsJoinable
        {
            get { return (bool)this.GetValue(IsJoinableProperty); }
            set { this.SetValue(IsJoinableProperty, value); }
        }
        
        public static readonly DependencyProperty MembersProperty = DependencyProperty.Register(
            "Members", typeof(PgUser[]), typeof(GameRoomPanel));
        public PgUser[] Members
        {
            get { return (PgUser[])this.GetValue(MembersProperty); }
            set { this.SetValue(MembersProperty, value); }
        }
    }
}
