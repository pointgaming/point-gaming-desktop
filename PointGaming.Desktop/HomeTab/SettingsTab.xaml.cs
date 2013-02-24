using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using PointGaming.Desktop.POCO;
using RestSharp;

namespace PointGaming.Desktop.HomeTab
{
    /// <summary>
    /// Interaction logic for SettingTab.xaml
    /// </summary>
    public partial class SettingsTab : UserControl
    {
        public SettingsTab()
        {
            InitializeComponent();
        }

        private void buttonScanForGames_Click(object sender, RoutedEventArgs e)
        {
            App.LogLine("Not Implemented");
        }
        
        private void buttonLogOut_Click(object sender, RoutedEventArgs e)
        {
            HomeWindow.Home.LogOut();
        }

        private void checkBoxSynchGameConfigs_Checked(object sender, RoutedEventArgs e)
        {
            App.LogLine("Not Implemented");
        }
        private void checkBoxSynchGameConfigs_Unchecked(object sender, RoutedEventArgs e)
        {
            App.LogLine("Not Implemented");
        }

        private void checkBoxHideGameConfigs_Checked(object sender, RoutedEventArgs e)
        {
            App.LogLine("Not Implemented");
        }
        private void checkBoxHideGameConfigs_Unchecked(object sender, RoutedEventArgs e)
        {
            App.LogLine("Not Implemented");
        }
    }
}
