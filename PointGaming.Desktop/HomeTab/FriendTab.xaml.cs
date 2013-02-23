using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PointGaming.Desktop.POCO;
using RestSharp;
using SocketIOClient;
using SocketIOClient.Messages;

namespace PointGaming.Desktop.HomeTab
{
    /// <summary>
    /// Interaction logic for FriendTab.xaml
    /// </summary>
    public partial class FriendTab : UserControl
    {
        private const string FriendStatusOffline = "Offline";
        private const string FriendStatusOnline = "Online";
        private static readonly List<string> ChatAvailableStatuses = new List<string>(new[] { FriendStatusOnline });


        private Client _friendsSocket;
        private AuthEmit _authEmit;
        private ApiResponse _apiResponse;

        private readonly ObservableCollection<FriendUiData> _friends = new ObservableCollection<FriendUiData>();
        public ObservableCollection<FriendUiData> Friends { get { return _friends; } }
        private Dictionary<string, FriendUiData> _friendLookup = new Dictionary<string, FriendUiData>();

        public class FriendUiData : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private void NotifyChanged(string propertyName)
            {
                var changedCallback = PropertyChanged;
                if (changedCallback == null)
                    return;
                var args = new PropertyChangedEventArgs(propertyName);
                changedCallback(this, args);
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
            private string _status;
            public string Status
            {
                get { return _status; }
                set
                {
                    if (value == _status)
                        return;
                    _status = value;
                    NotifyChanged("Status");
                }
            }
        }


        public FriendTab()
        {
            InitializeComponent();
        }

        public void CheckPendingFriendRequest()
        {
            List<FriendRequestResponse> friendRequestApiResponse;
            if (TryFriendRequestApiResponse(out friendRequestApiResponse))
            {
                foreach (var item in friendRequestApiResponse)
                {
                    if (item.user.username == null)
                    {
                        LogError("CheckPendingFriendRequest: item.username is null");
                        continue;
                    }

                    IncomingFriendRequest(item.user.username);
                }
            }
        }

        private void LogError(string checkpendingfriendrequestItemUsernameIsNull)
        {
            App.LogLine(checkpendingfriendrequestItemUsernameIsNull);
        }

        private static bool TryFriendRequestApiResponse(out List<FriendRequestResponse> pendingFriendRequests)
        {
            var friendsRequestCall = Properties.Settings.Default.FriendRequest + Persistence.AuthToken;
            var friendClient = new RestClient(friendsRequestCall);
            var fRequest = new RestRequest(Method.GET);
            var response = (RestResponse<FriendRequestsCollectionRootObject>)
                friendClient.Execute<FriendRequestsCollectionRootObject>(fRequest);
            var status = response.Data.success;
            pendingFriendRequests = response.Data.friend_requests;
            return status;
        }

        private void IncomingFriendRequest(string username)
        {
            if (AlreadyHaveRequest(username))
                return;

            var control = new FriendRequestUserControl();
            control.Username = username;
            control.FriendRequestAnswered += UserAnsweredFriendRequest;
            stackPanelFriendRequests.Children.Add(control);
        }

        private bool AlreadyHaveRequest(string username)
        {
            foreach (var item in stackPanelFriendRequests.Children)
            {
                var request = item as FriendRequestUserControl;
                if (request == null)
                    continue;

                if (request.Username == username)
                    return true;
            }
            return false;
        }

        private void UserAnsweredFriendRequest(FriendRequestUserControl source, bool isAccepted)
        {
            stackPanelFriendRequests.Children.Remove(source);

            List<FriendRequestResponse> friendRequestApiResponse;
            if (TryFriendRequestApiResponse(out friendRequestApiResponse))
            {
                foreach (var item in friendRequestApiResponse)
                {
                    if (item.user.username == source.Username)
                        RespondToFriendRequest(item, isAccepted);
                }
            }
        }

        private void RespondToFriendRequest(FriendRequestResponse friendRequest, bool isAccepted)
        {
            var apiCall = Properties.Settings.Default.FriendRequestsBaseUrl + friendRequest._id + "?auth_token=" + Persistence.AuthToken;
            var client = new RestClient(apiCall);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            
            // todo dean gores 2013-02: notify server developer that I added the reject action
            var friendPutRequest = new FriendPutRequest { action = isAccepted ? "accept" : "reject" };
            var friendPutRootObject = new FriendPutRootObject { friend_request = friendPutRequest };
            request.AddBody(friendPutRootObject);

            var acceptResponse = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
            var isSuccess = acceptResponse.Data.success;

            if (isSuccess && isAccepted)
            {
                _friendsSocket.Emit("friends", null);
            }
        }

        private void buttonAddFriend_Click(object sender, RoutedEventArgs e)
        {
            AddFriend();
        }

        private void AddFriend()
        {
            var username = textBoxAddFriendUsername.Text.Trim();
            if (string.IsNullOrWhiteSpace(username))
                return;

            textBoxAddFriendUsername.Text = "";

            var friendRequest = new FriendRequest { username = username };
            var friendRequestRootObject = new FriendRequestRootObject { friend_request = friendRequest };

            var request = new RestRequest(Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddBody(friendRequestRootObject);

            var friendsRequestApiCall = Properties.Settings.Default.FriendRequest + Persistence.AuthToken;
            var client = new RestClient(friendsRequestApiCall);
            var apiResponse = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
            var isSuccess = apiResponse.Data.success;

            if (isSuccess)
            {
                MessageBox.Show("Friend Request Sent!");
            }
            else
            {
                MessageBox.Show("Error: " + apiResponse.Data.message);
            }
        }

        private string _clickedFriendUsername;

        private void dataGridFriends_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                FriendUiData friend;
                if (dataGridFriends.TryGetRowItem(e, out friend))
                {
                    var username = friend.Username;
                    _clickedFriendUsername = username;
                    // todo dean gores 2013-02: show menu to delete friend
                    DeleteFriend(_clickedFriendUsername);
                }
            }
        }

        private void DeleteFriend(string username)
        {
            var user = new User {username = username};

            var userRootObject = new UserRootObject();
            userRootObject.user = user;

            var request = new RestRequest(Method.DELETE);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddBody(userRootObject);

            var baseUrl = Properties.Settings.Default.Friends + Persistence.AuthToken;
            var client = new RestClient(baseUrl);
            var apiResponse = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
            var status = apiResponse.Data.success;

            if (status)
            {
                MessageBox.Show("Friend Deleted Successfully!");
            }
            else
            {
                MessageBox.Show(apiResponse.Data.message);
            }

            _friendsSocket.Emit("friends", null);
        }

        private void dataGridFriends_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FriendUiData friend;
            if (dataGridFriends.TryGetRowItem(e, out friend))
            {
                if (CanChatWith(friend))
                {
                    var chatWindow = new ChatWindow();
                    var username = friend.Username;
                    chatWindow.Init(username);
                    HomeWindow.Home.AddChildWindow(chatWindow);
                    chatWindow.Show();
                }
            }
        }

        private bool CanChatWith(FriendUiData friend)
        {
            var status = friend.Status;
            return true;
            return ChatAvailableStatuses.Contains(status);
        }

        public void LoggedIn()
        {
            _friendsSocket = new Client(Properties.Settings.Default.SocketIoUrl);
            
            _friendsSocket.On("connect", new UIInvoker(this, OnConnect).Invoke);
            _friendsSocket.On("auth_resp", new UIInvoker(this, OnAuthResponse).Invoke);
            _friendsSocket.On("friend_signed_out", new UIInvoker(this, OnFriendSignedOut).Invoke);
            _friendsSocket.On("new_friend_request", new UIInvoker(this, OnNewFriendRequest).Invoke);
            _friendsSocket.On("new_friend", new UIInvoker(this, OnNewFriend).Invoke);
            _friendsSocket.On("friends", new UIInvoker(this, OnFriends).Invoke);

            _friendsSocket.Opened += _friendsSocket_Opened;

            _friendsSocket.Connect();
        }

        void _friendsSocket_Opened(object sender, EventArgs e)
        {
            App.LogLine("friends socket opened!");
        }

        public void LoggedOut()
        {
            _friendsSocket.Close();
            _friendsSocket = null;
    
            _friends.Clear();
            _friendLookup.Clear();
        }

        private class UIInvoker
        {
            private readonly Control _control;
            private readonly Action<IMessage> _action;

            public UIInvoker(Control control, Action<IMessage> action)
            {
                _control = control;
                _action = action;
            }

            public void Invoke(IMessage message)
            {
                _control.InvokeUI(() => _action(message));
            }
        }
        
        private void OnFriends(IMessage data)
        {
            var fro = data.Json.GetFirstArgAs<FriendResponseRootObject>();
            if (fro.success)
            {
                RemoveOldFriends(fro.friends);

                foreach (var item in fro.friends)
                    AddOrUpdateFriend(item);
            }
        }

        private void AddOrUpdateFriend(Friend friend)
        {
            var username = friend.username;
            var status = friend.status;
            FriendUiData old;
            if (_friendLookup.TryGetValue(username, out old))
            {
                old.Status = status;
            }
            else
            {
                var newFriend = new FriendUiData
                {
                    Username = username,
                    Status = status
                };
                _friendLookup[username] = newFriend;
                _friends.Add(newFriend);
            }
        }

        private void RemoveOldFriends(List<Friend> newFriends)
        {
            var newData = new Dictionary<string, FriendUiData>(newFriends.Count);
            foreach (var item in newFriends)
                newData.Add(item.username, null);

            var removes = new List<FriendUiData>();
            foreach (var item in _friends)
                if (!newData.ContainsKey(item.Username))
                    removes.Add(item);
            foreach (var item in removes)
            {
                _friends.Remove(item);
                _friendLookup.Remove(item.Username);
            }
        }

        private void OnNewFriend(IMessage data)
        {
            var friendStatus = data.Json.GetFirstArgAs<FriendStatus>();
            MessageBox.Show("You have a new friend '" + friendStatus.username + "'.");
            _friendsSocket.Emit("friends", null);
        }

        private void OnNewFriendRequest(IMessage data)
        {
            var friendStatus = data.Json.GetFirstArgAs<FriendStatus>();
            var username = friendStatus.username;
            IncomingFriendRequest(username);
        }

        private void OnFriendSignedOut(IMessage data)
        {
            try
            {
                var friendStatus = data.Json.GetFirstArgAs<FriendStatus>();
                FriendUiData friendData;
                if (_friendLookup.TryGetValue(friendStatus.username, out friendData))
                {
                    friendData.Status = FriendStatusOffline;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OnAuthResponse(IMessage data)
        {
            try
            {
                _apiResponse = new ApiResponse();
                _apiResponse = data.Json.GetFirstArgAs<ApiResponse>();

                _friendsSocket.Emit("friends", null);
                CheckPendingFriendRequest();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OnConnect(IMessage message)
        {
            try
            {
                _authEmit = new AuthEmit {auth_token = Persistence.AuthToken};
                _friendsSocket.Emit("auth", _authEmit);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void buttonAddFriend_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddFriend();
                e.Handled = true;
                return;
            }
        }
    }
}
