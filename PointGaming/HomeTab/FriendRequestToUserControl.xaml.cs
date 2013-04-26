using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PointGaming.HomeTab
{
    public interface IFriendRequestUserControl
    {
        string FriendRequestId { get; set; }
        string Username { get; set; }
        string UserId { get; set; }
    }

    public delegate void FriendRequestToAnswered(FriendRequestToUserControl source, bool isAccepted);

    public partial class FriendRequestToUserControl : UserControl, INotifyPropertyChanged, IFriendRequestUserControl
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event FriendRequestToAnswered FriendRequestToAnswered;

        private void NotifyChanged(string propertyName)
        {
            var changedCallback = PropertyChanged;
            if (changedCallback == null)
                return;
            var args = new PropertyChangedEventArgs(propertyName);
            changedCallback(this, args);
        }

        public FriendRequestToUserControl()
        {
            InitializeComponent();
        }

        private string _friendRequestId;
        public string FriendRequestId
        {
            get { return _friendRequestId; }
            set
            {
                if (value == _friendRequestId)
                    return;
                _friendRequestId = value;
                NotifyChanged("FriendRequestId");
            }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                if (value == _username)
                    return;
                _username = value;
                NotifyChanged("Username");
            }
        }

        private string _userId;
        public string UserId
        {
            get { return _userId; }
            set
            {
                if (value == _userId)
                    return;
                _userId = value;
                NotifyChanged("UserId");
            }
        }

        private void buttonAccept_Click(object sender, RoutedEventArgs e)
        {
            Answered(true);
        }

        private void buttonReject_Click(object sender, RoutedEventArgs e)
        {
            Answered(false);
        }

        private void Answered(bool isAccepted)
        {
            var answeredCallback = FriendRequestToAnswered;
            if (answeredCallback == null)
                return;
            answeredCallback(this, isAccepted);
        }
    }
}
