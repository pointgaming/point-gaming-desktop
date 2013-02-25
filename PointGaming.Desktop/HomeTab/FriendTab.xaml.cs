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
    public partial class FriendTab : UserControl
    {
        private const string FriendStatusOffline = "offline";
        private const string FriendStatusOnline = "online";
        private const string FriendStatusAdded = "added";
        private const string FriendStatusRemoved = "removed";
        private const string FriendStatusInvited = "invited";

        private static readonly List<string> ChatAvailableStatuses = new List<string>(new[] { FriendStatusOnline });

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

            private string _id;
            public string Id
            {
                get { return _id; }
                set
                {
                    if (value == _id)
                        return;
                    _id = value;
                    NotifyChanged("Id");
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

                    IncomingFriendRequest(item.user._id, item.user.username);
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

        private void IncomingFriendRequest(string id, string username)
        {
            if (AlreadyHaveRequest(username))
                return;

            var control = new FriendRequestUserControl();
            control.Id = id;
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
            //RespondToFriendRequest(source.Id, isAccepted);

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

        private void RespondToFriendRequest(FriendRequestResponse frr, bool isAccepted)
        {
            var apiCall = Properties.Settings.Default.FriendRequestsBaseUrl + frr._id + "?auth_token=" + Persistence.AuthToken;
            var client = new RestClient(apiCall);
            var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
            
            var friendPutRequest = new FriendPutRequest { action = isAccepted ? "accept" : "reject" };
            var friendPutRootObject = new FriendPutRootObject { friend_request = friendPutRequest };
            request.AddBody(friendPutRootObject);

            var acceptResponse = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
            var isSuccess = acceptResponse.Data.success;

            if (isSuccess && isAccepted)
            {
                _client.Emit("friends", null);
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
                MessageBox.Show(HomeWindow.Home, "Friend Request Sent!");
            }
            else
            {
                MessageBox.Show(HomeWindow.Home, "Error: " + apiResponse.Data.message);
            }
        }

        private FriendUiData _rightClickedFriend;

        private void dataGridFriends_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                FriendUiData friend;
                if (dataGridFriends.TryGetRowItem(e, out friend))
                {
                    _rightClickedFriend = friend;
                }
            }
        }
        private void UnfriendClick(object sender, RoutedEventArgs e)
        {
            DeleteFriend(_rightClickedFriend);
        }

        private void DeleteFriend(FriendUiData friend)
        {
            var request = new RestRequest(Method.DELETE);
            
            var baseUrl = Properties.Settings.Default.Unfriend + friend.Id + "?auth_token=" + Persistence.AuthToken;
            var client = new RestClient(baseUrl);
            var apiResponse = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
            var status = apiResponse.Data.success;

            if (!status)
                MessageBox.Show(HomeWindow.Home, apiResponse.Data.message);

            _client.Emit("friends", null);
        }

        private void dataGridFriends_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FriendUiData friend;
            if (dataGridFriends.TryGetRowItem(e, out friend))
            {
                if (CanChatWith(friend))
                {
                    HomeWindow.Home.ChatWith(friend.Username);
                }
            }
        }

        private bool CanChatWith(FriendUiData friend)
        {
            var status = friend.Status;
            return ChatAvailableStatuses.Contains(status);
        }

        
        private Client _client;

        public void OnAuthorized(Client client)
        {
            _client = client;

            _client.On("friends", new UIInvoker(this, OnFriends).Invoke);
            _client.On("friend_status_changed", new UIInvoker(this, OnFriendStatusChanged).Invoke);

            _client.Emit("friends", null);
            CheckPendingFriendRequest();
        }

        private void OnFriendStatusChanged(IMessage data)
        {
            try
            {
                var friendStatus = data.Json.GetFirstArgAs<FriendStatus>();
                switch(friendStatus.status)
                {
                    case FriendStatusAdded:
                        FriendAdded(friendStatus);
                        break;
                    case FriendStatusRemoved:
                        FriendRemoved(friendStatus);
                        break;
                    case FriendStatusInvited:
                        FriendInviteReceived(friendStatus);
                        break;
                    default:
                        FriendStatusChanged(friendStatus);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(HomeWindow.Home, ex.Message);
            }
        }

        private void FriendStatusChanged(FriendStatus friendStatus)
        {
            FriendUiData friendData;
            if (_friendLookup.TryGetValue(friendStatus._id, out friendData))
            {
                friendData.Status = friendStatus.status;
            }
        }

        private void FriendInviteReceived(FriendStatus friendStatus)
        {
            var id = friendStatus._id;
            var username = friendStatus.username;
            IncomingFriendRequest(id, username);
        }

        private void FriendRemoved(FriendStatus friendStatus)
        {
            FriendUiData friendUiData;
            if (_friendLookup.TryGetValue(friendStatus._id, out friendUiData))
                RemoveFriendFromList(friendUiData);
        }

        private void FriendAdded(FriendStatus friendStatus)
        {
            Friend friend = new Friend();
            friend._id = friendStatus._id;
            friend.username = friendStatus.username;
            friend.status = FriendStatusOffline;
            AddOrUpdateFriend(friend);
            // todo dean gores 2013-02-25 maybe I should call something to get detailed friend info? (if we ever use that stuff)
        }

        public void LoggedOut()
        {
            _client = null;

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
            FriendUiData old;
            if (_friendLookup.TryGetValue(friend._id, out old))
            {
                old.Status = friend.status;
                old.Username = friend.username;
            }
            else
            {
                var newFriend = new FriendUiData
                {
                    Username = friend.username,
                    Status = friend.status,
                    Id = friend._id,
                };
                _friendLookup[friend._id] = newFriend;
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
                RemoveFriendFromList(item);
        }

        private void RemoveFriendFromList(FriendUiData item)
        {
            _friends.Remove(item);
            _friendLookup.Remove(item.Id);
        }

        private void textBoxAddFriendUsername_PreviewKeyDown(object sender, KeyEventArgs e)
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
