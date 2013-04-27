﻿using System;
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

        private void NotifyChanged(string propertyName)
        {
            var changedCallback = PropertyChanged;
            if (changedCallback == null)
                return;
            var args = new PropertyChangedEventArgs(propertyName);
            changedCallback(this, args);
        }
        private UserDataManager _userData = HomeWindow.UserData;

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
        
        private void buttonButtonJoin_Click(object sender, RoutedEventArgs e)
        {
            var del = JoinClick;
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
    }
}