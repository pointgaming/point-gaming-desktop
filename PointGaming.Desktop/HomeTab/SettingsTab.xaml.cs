using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using PointGaming.Desktop.POCO;
using RestSharp;
using System.ComponentModel;

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

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            checkBoxMinimizeToTray.IsChecked = Properties.Settings.Default.MinimizeToTray;
        }

        private void buttonScanForGames_Click(object sender, RoutedEventArgs e)
        {
            App.LogLine("Not Implemented");
        }
        
        private void buttonLogOut_Click(object sender, RoutedEventArgs e)
        {
            HomeWindow.Home.LogOut(true);
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

        private void checkBoxMinimizeToTray_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.MinimizeToTray = true;
            Properties.Settings.Default.Save();
            HomeWindow.Home.UpdateMinimizeToTray();
        }
        private void checkBoxMinimizeToTray_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.MinimizeToTray = false;
            Properties.Settings.Default.Save();
            HomeWindow.Home.UpdateMinimizeToTray();
        }
    }
}
