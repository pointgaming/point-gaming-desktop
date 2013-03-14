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
        private void buttonChooseChatFont_Click(object sender, RoutedEventArgs e)
        {
            PgFonts.FontChooser fontChooser = new PgFonts.FontChooser();
            fontChooser.Owner = HomeWindow.Home;

            if (Properties.Settings.Default.ChatFontFamily != "")
                textBoxFontChoice.FontFamily = new System.Windows.Media.FontFamily(Properties.Settings.Default.ChatFontFamily + ", " + textBoxFontChoice.FontFamily);
            if (Properties.Settings.Default.ChatFontSize != 0)
                textBoxFontChoice.FontSize = Properties.Settings.Default.ChatFontSize;
            fontChooser.SetPropertiesFromObject(textBoxFontChoice);
            fontChooser.PreviewSampleText = textBoxFontChoice.Text;

            if (fontChooser.ShowDialog().Value)
            {
                fontChooser.ApplyPropertiesToObject(textBoxFontChoice);
                var family = textBoxFontChoice.FontFamily.ToString();
                var size = textBoxFontChoice.FontSize;
                Properties.Settings.Default.ChatFontFamily = family;
                Properties.Settings.Default.ChatFontSize = size;
                Properties.Settings.Default.Save();
            }
        }
        
        private void buttonLogOut_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Password = "";
            Properties.Settings.Default.Save();
            HomeWindow.Home.LogOut(true);
        }

        private void checkBoxSyncGameConfigs_Checked(object sender, RoutedEventArgs e)
        {
            App.LogLine("Not Implemented");
        }
        private void checkBoxSyncGameConfigs_Unchecked(object sender, RoutedEventArgs e)
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
