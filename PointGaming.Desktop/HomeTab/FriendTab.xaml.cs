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

        private SocketSession _session;

        public FriendTab()
        {
            InitializeComponent();
        }

        public void OnAuthorized(SocketSession session)
        {
            _session = session;

            session.OnThread("friends", OnFriends);
            session.OnThread("friend_status_changed", OnFriendStatusChanged);

            session.EmitLater("friends", null);
            CheckPendingFriendRequest();
        }

        public void LoggedOut()
        {
            _session = null;

            _friends.Clear();
            _friendLookup.Clear();
        }

        public void CheckPendingFriendRequest()
        {
            List<FriendRequestResponse> friendRequestApiResponse = null;
            bool isSuccess = false;
            _session.BeginAndCallback(delegate
            {
                isSuccess = TryFriendRequestApiResponse(out friendRequestApiResponse);
            }, delegate
            {
                if (isSuccess)
                {
                    foreach (var item in friendRequestApiResponse)
                    {
                        IncomingFriendRequest(item.user._id, item.user.username);
                    }
                }
            });
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
            
            _session.Begin(delegate
            {
                List<FriendRequestResponse> friendRequestApiResponse;
                if (TryFriendRequestApiResponse(out friendRequestApiResponse))
                {
                    foreach (var item in friendRequestApiResponse)
                    {
                        if (item.user.username == source.Username)
                        {
                            RespondToFriendRequest(item, isAccepted);
                            break;
                        }
                    }
                }
            });
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
                _session.Emit("friends", null);
        }

        private void buttonAddFriend_Click(object sender, RoutedEventArgs e)
        {
            RequestFriend();
        }

        private void RequestFriend()
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

            _session.Begin(delegate
            {
                var apiResponse = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
                var isSuccess = apiResponse.Data.success;

                if (!isSuccess)
                {
                    App.LogLine("Error requesting friend: " + apiResponse.Data.message);
                    if (apiResponse.Data.message == "User not found")
                    {
                        this.BeginInvokeUI(delegate
                        {
                            MessageDialog.Show(HomeWindow.Home, "User not found", "User '" + username + "' not found");
                        });
                    }
                    else if (apiResponse.Data.message == "You are already friends with that user")
                    {
                        this.BeginInvokeUI(delegate
                        {
                            MessageDialog.Show(HomeWindow.Home, "Already friends", "Already friends with '" + username + "'");
                        });
                    }
                }
            });
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
            _session.Begin(delegate { DeleteFriend(_session, _rightClickedFriend);});
        }

        private static void DeleteFriend(SocketSession session, FriendUiData friend)
        {
            var request = new RestRequest(Method.DELETE);
            
            var baseUrl = Properties.Settings.Default.Unfriend + friend.Id + "?auth_token=" + Persistence.AuthToken;
            var client = new RestClient(baseUrl);
            var apiResponse = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
            var status = apiResponse.Data.success;

            if (!status)
                App.LogLine("Failed to delete friend: " + apiResponse.Data.message);

            session.EmitLater("friends", null);
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
                App.LogLine(ex.Message);
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
                RequestFriend();
                e.Handled = true;
                return;
            }
        }

    }
}
