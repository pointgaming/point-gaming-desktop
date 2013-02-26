using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PointGaming.Desktop.HomeTab
{
    public delegate void FriendRequestAnswered(FriendRequestUserControl source, bool isAccepted);

    public partial class FriendRequestUserControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event FriendRequestAnswered FriendRequestAnswered;

        private void NotifyChanged(string propertyName)
        {
            var changedCallback = PropertyChanged;
            if (changedCallback == null)
                return;
            var args = new PropertyChangedEventArgs(propertyName);
            changedCallback(this, args);
        }

        public FriendRequestUserControl()
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
            var answeredCallback = FriendRequestAnswered;
            if (answeredCallback == null)
                return;
            answeredCallback(this, isAccepted);
        }
    }
}
