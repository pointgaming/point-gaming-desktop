using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PointGaming.HomeTab
{
    public partial class DebugTab : UserControl
    {
        public DebugTab()
        {
            InitializeComponent();

            foreach (var item in HomeWindow.UserData.AudioSystem.GetAudioInputDevices())
                comboBoxRecordingDevices.Items.Add(item);
            comboBoxRecordingDevices.SelectedIndex = Properties.Settings.Default.AudioInputDeviceIndex;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            App.DebugBox = textBoxConsole;
        }

        public string ProgramVersion { get { return "Version " + App.Version; } }

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

        private void buttonSetMicKey_Click(object sender, RoutedEventArgs e)
        {
            SetAudioInputDeviceTriggerKey();
        }


        private void SetAudioInputDeviceTriggerKey()
        {
            App.KeyDown += App_KeyDown;
            MessageDialog.Show(HomeWindow.Home, "Press Key", "Press the key you want to trigger the microphone. Then click OK.");
            Properties.Settings.Default.MicTriggerKey = (int)_lastKeyDown;
            Properties.Settings.Default.Save();
            App.KeyDown -= App_KeyDown;
            HomeWindow.UserData.AudioSystem.TriggerKey = (System.Windows.Forms.Keys)Properties.Settings.Default.MicTriggerKey;
        }

        System.Windows.Forms.Keys _lastKeyDown;

        void App_KeyDown(System.Windows.Forms.Keys obj)
        {
            _lastKeyDown = obj;
        }

        private void comboBoxRecordingDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var deviceIndex = comboBoxRecordingDevices.SelectedIndex;
            Properties.Settings.Default.AudioInputDeviceIndex = deviceIndex;
            Properties.Settings.Default.Save();
            HomeWindow.UserData.AudioSystem.SetAudioInputDevice(deviceIndex);
        }
    }
}
