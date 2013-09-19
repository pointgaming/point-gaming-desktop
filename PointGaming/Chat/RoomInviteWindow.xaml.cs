using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PointGaming.POCO;

namespace PointGaming.Chat
{
    public partial class RoomInviteWindow : Window
    {
        public WindowTreeManager WindowTreeManager;
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged(string propertyName)
        {
            var changedCallback = PropertyChanged;
            if (changedCallback == null)
                return;
            var args = new PropertyChangedEventArgs(propertyName);
            changedCallback(this, args);
        }
        private UserDataManager _userData = UserDataManager.UserData;
        
        private readonly ObservableCollection<ChatroomInviteNew> _invites = new ObservableCollection<ChatroomInviteNew>();
        public ObservableCollection<ChatroomInviteNew> Invites { get { return _invites; } }

        public RoomInviteWindow()
        {
            InitializeComponent();
            WindowTreeManager = new WindowTreeManager(this, HomeWindow.Home.WindowTreeManager);
        }

        public void AddInvite(ChatroomInviteNew invite)
        {
            _invites.Add(invite);
        }

        private void buttonAcceptClick(object sender, RoutedEventArgs e)
        {
            ChatroomInviteNew invite;
            if (((DependencyObject)sender).TryGetPresentedParent(out invite))
            {
                _userData.JoinChat(invite._id);
                _invites.Remove(invite);
            }

            CheckIfShouldClose();
        }

        private void buttonRejectClick(object sender, RoutedEventArgs e)
        {
            ChatroomInviteNew invite;
            if (((DependencyObject)sender).TryGetPresentedParent(out invite))
                _invites.Remove(invite);
            CheckIfShouldClose();
        }

        private void CheckIfShouldClose()
        {
            if (_invites.Count != 0)
                return;
            Close();
        }

        private void myUserControl_Activated(object sender, EventArgs e)
        {
            this.StopFlashingWindow();
        }
    }
}
