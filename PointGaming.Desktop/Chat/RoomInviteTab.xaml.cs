using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PointGaming.Desktop.POCO;

namespace PointGaming.Desktop.Chat
{
    public partial class RoomInviteTab : UserControl, ITabWithId
    {
        private ChatWindow _chatWindow;
        private UserDataManager _userData = HomeWindow.UserData;

        public const string TabId = "@%%^&^#&)):hgdf";
        public string Id { get { return TabId; } }

        private readonly ObservableCollection<ChatroomInviteNew> _invites = new ObservableCollection<ChatroomInviteNew>();
        public ObservableCollection<ChatroomInviteNew> Invites { get { return _invites; } }

        public RoomInviteTab()
        {
            InitializeComponent();
        }

        public void Init(ChatWindow window)
        {
            _chatWindow = window;
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
            _chatWindow.CloseTab(typeof(RoomInviteTab), Id);
        }
    }
}
