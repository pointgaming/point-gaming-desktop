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

            session.OnThread("friend_status_changed", OnFriendStatusChanged);
            session.OnThread("friend_request_created", OnFriendRequestCreated);
            session.OnThread("friend_request_destroyed", OnFriendRequestDestroyed);

            GetFriends();
            GetFriendRequestsTo();
            GetFriendRequestsFrom();
        }

        public void LoggedOut()
        {
            _session = null;

            _friends.Clear();
            _friendLookup.Clear();
            stackPanelFriendRequestsTo.Children.Clear();
            stackPanelFriendRequestsFrom.Children.Clear();
        }

        public void GetFriendRequestsTo()
        {
            List<FriendRequest> requests = null;
            bool isSuccess = false;
            _session.BeginAndCallback(delegate
            {
                var friendsRequestCall = Properties.Settings.Default.FriendRequests + "?auth_token=" + Persistence.AuthToken;
                var friendClient = new RestClient(friendsRequestCall);
                var fRequest = new RestRequest(Method.GET);
                var response = (RestResponse<FriendRequestRoot>)
                    friendClient.Execute<FriendRequestRoot>(fRequest);
                requests = response.Data.friend_requests;
                isSuccess = response.IsOk();

            }, delegate
            {
                if (isSuccess)
                {
                    foreach (var item in requests)
                        FriendRequestToReceived(item);
                }
            });
        }

        public void GetFriendRequestsFrom()
        {
            List<FriendRequest> requests = null;
            bool isSuccess = false;
            _session.BeginAndCallback(delegate
            {
                var friendsRequestCall = Properties.Settings.Default.FriendRequests + "?sent=1&auth_token=" + Persistence.AuthToken;
                var friendClient = new RestClient(friendsRequestCall);
                var fRequest = new RestRequest(Method.GET);
                var response = (RestResponse<FriendRequestRoot>)
                    friendClient.Execute<FriendRequestRoot>(fRequest);
                requests = response.Data.friend_requests;
                isSuccess = response.IsOk();
            }, delegate
            {
                if (isSuccess)
                {
                    foreach (var item in requests)
                        FriendRequestFromReceived(item);
                }
            });
        }

        private void LogError(string checkpendingfriendrequestItemUsernameIsNull)
        {
            App.LogLine(checkpendingfriendrequestItemUsernameIsNull);
        }

        private void FriendRequestToReceived(FriendRequest friendRequest)
        {
            if (AlreadyHaveFriendRequestTo(friendRequest._id))
                return;

            var control = new FriendRequestToUserControl();
            control.FriendRequestId = friendRequest._id;
            control.Username = friendRequest.from_user.username;
            control.UserId = friendRequest.from_user._id;
            control.FriendRequestToAnswered += FriendRequestToAnswered;
            stackPanelFriendRequestsTo.Children.Add(control);
        }

        private void FriendRequestFromReceived(FriendRequest friendRequest)
        {
            if (AlreadyHaveFriendRequestFrom(friendRequest._id))
                return;

            var control = new FriendRequestFromUserControl();
            control.FriendRequestId = friendRequest._id;
            control.Username = friendRequest.to_user.username;
            control.UserId = friendRequest.to_user._id;
            control.FriendRequestCanceled += FriendRequestFromCanceled;
            stackPanelFriendRequestsFrom.Children.Add(control);
        }
        
        private bool AlreadyHaveFriendRequestTo(string friendRequestId)
        {
            foreach (var item in stackPanelFriendRequestsTo.Children)
            {
                var request = item as FriendRequestToUserControl;
                if (request == null)
                    continue;

                if (request.FriendRequestId == friendRequestId)
                    return true;
            }
            return false;
        }

        private bool AlreadyHaveFriendRequestFrom(string friendRequestId)
        {
            foreach (var item in stackPanelFriendRequestsFrom.Children)
            {
                var request = item as FriendRequestFromUserControl;
                if (request == null)
                    continue;

                if (request.FriendRequestId == friendRequestId)
                    return true;
            }
            return false;
        }

        private void FriendRequestToAnswered(FriendRequestToUserControl source, bool isAccepted)
        {
            string id = source.FriendRequestId;
            _session.Begin(delegate
            {
                var apiCall = Properties.Settings.Default.FriendRequests + id + "?auth_token=" + Persistence.AuthToken;
                var client = new RestClient(apiCall);
                var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };

                var friendPutRequest = new FriendRequestResponse { action = isAccepted ? "accept" : "reject" };
                var friendPutRootObject = new FriendRequestResponseRoot { friend_request = friendPutRequest };
                request.AddBody(friendPutRootObject);
                client.Execute<ApiResponse>(request);
            });
        }

        private void FriendRequestFromCanceled(FriendRequestFromUserControl source)
        {
            string id = source.FriendRequestId;
            _session.Begin(delegate
            {
                var apiCall = Properties.Settings.Default.FriendRequests + id + "?auth_token=" + Persistence.AuthToken;
                var client = new RestClient(apiCall);
                var request = new RestRequest(Method.DELETE);
                client.Execute<ApiResponse>(request);
            });
        }

        private void buttonAddFriend_Click(object sender, RoutedEventArgs e)
        {
            RequestFriend();
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

        private void RequestFriend()
        {
            var username = textBoxAddFriendUsername.Text.Trim();
            if (string.IsNullOrWhiteSpace(username))
                return;

            textBoxAddFriendUsername.Text = "";

            var friendRequest = new InitialFriendRequest { username = username };
            var friendRequestRootObject = new InitialFriendRequestRoot { friend_request = friendRequest };

            var request = new RestRequest(Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddBody(friendRequestRootObject);

            var friendsRequestApiCall = Properties.Settings.Default.FriendRequests + "?auth_token=" + Persistence.AuthToken;
            var client = new RestClient(friendsRequestApiCall);

            _session.Begin(delegate
            {
                var apiResponse = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);

                if (!apiResponse.IsOk())
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
            var baseUrl = Properties.Settings.Default.Friends + friend.Id + "?auth_token=" + Persistence.AuthToken;
            var client = new RestClient(baseUrl);
            client.Execute<ApiResponse>(request);
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

        private void OnFriendRequestCreated(IMessage data)
        {
            var friendRequest = data.Json.GetFirstArgAs<FriendRequest>();
            if (friendRequest.to_user._id == Persistence.loggedInUserId)
            {
                FriendRequestToReceived(friendRequest);
            }
            else if (friendRequest.from_user._id == Persistence.loggedInUserId)
            {
                FriendRequestFromReceived(friendRequest);
            }
        }

        private void OnFriendRequestDestroyed(IMessage data)
        {
            var friendRequest = data.Json.GetFirstArgAs<FriendRequest>();

            foreach (var item in stackPanelFriendRequestsTo.Children)
            {
                var request = item as FriendRequestToUserControl;
                if (request == null)
                    continue;

                if (request.FriendRequestId == friendRequest._id)
                {
                    stackPanelFriendRequestsTo.Children.Remove((UIElement)item);
                    break;
                }
            }

            foreach (var item in stackPanelFriendRequestsFrom.Children)
            {
                var request = item as FriendRequestFromUserControl;
                if (request == null)
                    continue;

                if (request.FriendRequestId == friendRequest._id)
                {
                    stackPanelFriendRequestsFrom.Children.Remove((UIElement)item);
                    break;
                }
            }
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
        
        private void FriendRemoved(FriendStatus friendStatus)
        {
            FriendUiData friendUiData;
            if (_friendLookup.TryGetValue(friendStatus._id, out friendUiData))
                RemoveFriendFromList(friendUiData);
        }

        private void FriendAdded(FriendStatus friendStatus)
        {
            User friend = new User();
            friend._id = friendStatus._id;
            friend.username = friendStatus.username;
            friend.status = FriendStatusOffline;
            AddOrUpdateFriend(friend);

            foreach (var item in stackPanelFriendRequestsFrom.Children)
            {
                var request = item as FriendRequestFromUserControl;
                if (request == null)
                    continue;
                if (request.Username == friendStatus.username)
                    stackPanelFriendRequestsFrom.Children.Remove((UIElement)item);
            }
        }

        private void GetFriends()
        {
            RestResponse<FriendList> response = null;
            _session.BeginAndCallback(delegate
            {
                var friendsRequestCall = Properties.Settings.Default.Friends + "?auth_token=" + Persistence.AuthToken;
                var friendClient = new RestClient(friendsRequestCall);
                var fRequest = new RestRequest(Method.GET);
                response = (RestResponse<FriendList>)friendClient.Execute<FriendList>(fRequest);
            }, delegate
            {
                if (response.IsOk())
                {
                    var friends = response.Data.friends;
                    RemoveOldFriends(friends);

                    foreach (var item in friends)
                        AddOrUpdateFriend(item);
                }
            });
        }

        private void AddOrUpdateFriend(User friend)
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

        private void RemoveOldFriends(List<User> newFriends)
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
    }
}
