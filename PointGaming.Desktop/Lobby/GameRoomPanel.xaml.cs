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

namespace PointGaming.Desktop.Lobby
{
    public partial class GameRoomPanel : UserControl, INotifyPropertyChanged
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

        public GameRoomPanel()
        {
            InitializeComponent();
        }

        private void hyperLinkInfoClick(object sender, RoutedEventArgs e)
        {
            MessageDialog.Show(HomeWindow.Home.GetChatWindow(), "Info", "TODO: Information goes here.");
        }
        
        private void buttonButtonJoin_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog.Show(HomeWindow.Home.GetChatWindow(), "Game Room", "TODO: Show game room.");
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
    }
}
