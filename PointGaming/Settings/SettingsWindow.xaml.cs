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
        }

        public void SetInitialTab(Type tabType)
        {
            if (tabType == typeof(VoiceTab))
                tabControlMain.SelectedItem = tabVoice;
            if (tabType == typeof(AboutTab))
                tabControlMain.SelectedItem = tabAbout;
        }

        public static void ShowDialog(WindowTreeManager parent, Type tabType)
        {
            Settings.SettingsWindow sw = new Settings.SettingsWindow();
            parent.AddChild(sw);
            sw.Owner = parent.Self;
            if (tabType != null)
                sw.SetInitialTab(tabType);
            sw.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var item in tabControlMain.Items)
            {
                TabItem ti = item as TabItem;
                var settingsTab = ti.Content as ISettingsTab;
                if (settingsTab != null)
                    settingsTab.SettingsClosing();
            }
            UserDataManager.UserData.Settings.Save();
        }
    }
}
