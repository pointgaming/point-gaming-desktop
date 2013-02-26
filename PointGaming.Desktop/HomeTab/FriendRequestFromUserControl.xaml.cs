using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PointGaming.Desktop.HomeTab
{
    public delegate void FriendRequestCanceled(FriendRequestFromUserControl source);

    public partial class FriendRequestFromUserControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event FriendRequestCanceled FriendRequestCanceled;

        private void NotifyChanged(string propertyName)
        {
            var changedCallback = PropertyChanged;
            if (changedCallback == null)
                return;
            var args = new PropertyChangedEventArgs(propertyName);
            changedCallback(this, args);
        }

        public FriendRequestFromUserControl()
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

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            var answeredCallback = FriendRequestCanceled;
            if (answeredCallback == null)
                return;
            answeredCallback(this);
        }
    }
}
