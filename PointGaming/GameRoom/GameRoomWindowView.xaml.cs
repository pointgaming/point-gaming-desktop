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
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace PointGaming.GameRoom
{
    public partial class GameRoomWindow : Window
    {
        public GameRoomWindow()
        {
            InitializeComponent();
        }

        private void ChatTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isShiftDown = e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift);

            // Treat the chatbox enter input as a send button click
            if (e.Key == Key.Enter && !isShiftDown)
            {
                SendChatButton.Command.Execute(ChatTextBox.Text);

                ChatTextBox.Text = null;
                e.Handled = true;
            }
        }

        private void SendChatButton_Click(object sender, RoutedEventArgs e)
        {
            SendChatButton.Command.Execute(ChatTextBox.Text);

            // Clear input after text sent
            ChatTextBox.Text = null;
            e.Handled = true;
        }
    }
}
