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

namespace PointGaming.Settings
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            
            var wtm = new WindowTreeManager(this, HomeWindow.Home.WindowTreeManager);
        }

        public void SetInitialTab(Type tabType)
        {
            if (tabType == typeof(VoiceTab))
                tabControlMain.SelectedItem = tabVoice;
            if (tabType == typeof(AboutTab))
                tabControlMain.SelectedItem = tabAbout;
        }

        public static void ShowDialog(Type tabType)
        {
            Settings.SettingsWindow sw = new Settings.SettingsWindow();
            sw.Owner = HomeWindow.Home;
            if (tabType != null)
                sw.SetInitialTab(tabType);
            sw.ShowDialog();
        }
    }
}
