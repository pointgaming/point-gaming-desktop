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

        private int _streamNumberNoise;

        private void buttonPlayChecked_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxA.IsChecked == true)
                AskAndPlayFile();
            if (checkBoxB.IsChecked == true)
                AskAndPlayFile();

            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var format = UserDataManager.UserData.Voip.Codec.RecordFormat;
            var sampleRate = format.SampleRate;
            var samplesPer20ms = sampleRate / 50;



            var stream = new Voice.SerialPacketStream
            {
                Id = "x",
                Index = 0,
                IsEncoded = false,
                IsTeamOnly = false,
                Parts = new System.Collections.Generic.List<Voice.SerialPacket>(),
                RoomName = "x",
                StreamNumber = _streamNumberNoise++,
            };

            for (int j = 0; j < 3; j++)
            {
                byte[] data = new byte[samplesPer20ms * 2];
                rng.GetBytes(data);
                var packet = new Voice.SerialPacket
                {
                    MessageNumber = j,
                    RxTime = Voice.DateTimePrecise.UtcNow,
                    Audio = data,
                };
                stream.Parts.Add(packet);
            }

            stream.Write(Voice.SerialPacketStream.AppDataPath(stream));

            UserDataManager.UserData.Voip.Play(stream);
        }

        private static void AskAndPlayFile()
        {
            try
            {
                var fileName = AskForFile();
                if (fileName != null)
                {
                    var stream = Voice.SerialPacketStream.Read(new System.IO.FileInfo(fileName));
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
