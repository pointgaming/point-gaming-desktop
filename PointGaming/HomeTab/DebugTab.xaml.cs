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

        private void buttonPlayChecked_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxA.IsChecked == true)
                AskAndPlayFile();
            if (checkBoxB.IsChecked == true)
                AskAndPlayFile();
        }

        private static void AskAndPlayFile()
        {
            try
            {
                var fileName = AskForFile();
                if (fileName != null)
                {
                    var stream = Voice.SerialPacketStream.Read(fileName);
                    UserDataManager.UserData.Voip.Play(stream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to play file: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private static string AskForFile()
        {
            // Create OpenFileDialog
            var dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".pga";
            dlg.Filter = "Point Gaming Audio (.pga)|*.pga";
            // Display OpenFileDialog by calling ShowDialog method
            var result = dlg.ShowDialog();
            // Get the selected file name and display in a TextBox
            if (result != true)
                return null;
            return dlg.FileName;
        }
    }
}
