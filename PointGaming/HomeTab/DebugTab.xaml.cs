using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PointGaming.HomeTab
{
    public partial class DebugTab : UserControl
    {
        public DebugTab()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            App.DebugBox = textBoxConsole;
        }

        private void buttonChooseChatFont_Click(object sender, RoutedEventArgs e)
        {
            PgFonts.FontChooser fontChooser = new PgFonts.FontChooser();
            fontChooser.Owner = HomeWindow.Home;

            if (UserDataManager.UserData.Settings.ChatFontFamily != "")
                textBoxFontChoice.FontFamily = new System.Windows.Media.FontFamily(UserDataManager.UserData.Settings.ChatFontFamily + ", " + textBoxFontChoice.FontFamily);
            if (UserDataManager.UserData.Settings.ChatFontSize != 0)
                textBoxFontChoice.FontSize = UserDataManager.UserData.Settings.ChatFontSize;
            fontChooser.SetPropertiesFromObject(textBoxFontChoice);
            fontChooser.PreviewSampleText = textBoxFontChoice.Text;

            if (fontChooser.ShowDialog().Value)
            {
                fontChooser.ApplyPropertiesToObject(textBoxFontChoice);
                var family = textBoxFontChoice.FontFamily.ToString();
                var size = textBoxFontChoice.FontSize;
                UserDataManager.UserData.Settings.ChatFontFamily = family;
                UserDataManager.UserData.Settings.ChatFontSize = size;
                UserDataManager.UserData.Settings.Save();
            }
        }
    }
}
