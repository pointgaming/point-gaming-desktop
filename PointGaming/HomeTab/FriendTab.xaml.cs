using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PointGaming.POCO;
using RestSharp;
using SocketIOClient;
using SocketIOClient.Messages;

namespace PointGaming.HomeTab
{
    public partial class FriendTab : UserControl
    {
        private const string FriendStatusOffline = "offline";
        private const string FriendStatusOnline = "online";
        private const string FriendStatusAdded = "added";
        private const string FriendStatusRemoved = "removed";

        private static readonly List<string> ChatAvailableStatuses = new List<string>(new[] { FriendStatusOnline });

        private UserDataManager _userData = HomeWindow.UserData;
        public ObservableCollection<PgUser> Friends { get { return _userData.Friends; } }

        public FriendTab()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            _userData.PgSession.OnThread("friend_status_changed", OnFriendStatusChanged);
            _userData.PgSession.OnThread("friend_request_created", OnFriendRequestCreated);
            _userData.PgSession.OnThread("friend_request_destroyed", OnFriendRequestDestroyed);

            GetFriends();
            GetFriendRequestsTo();
            GetFriendRequestsFrom();
        }
        
        private PgUser _rightClickedFriend;

        private void dataGridFriends_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                PgUser friend;
                if (dataGridFriends.TryGetRowItem(e, out friend))
                {
                    _rightClickedFriend = friend;
                }
            }
        }

        private void UnfriendClick(object sender, RoutedEventArgs e)
        {
            PgUser friend = _rightClickedFriend;
            Unfriend(friend);
        }

        private void dataGridFriends_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PgUser friend;
            if (dataGridFriends.TryGetRowItem(e, out friend))
            {
                if (CanChatWith(friend))
                {
                    _userData.ChatWith(friend);
                }
            }
        }

        private bool CanChatWith(PgUser friend)
        {
            var status = friend.Status;
            return ChatAvailableStatuses.Contains(status);
        }

        #region friend status
        private void OnFriendStatusChanged(IMessage data)
        {
            try
            {
                var friendStatus = data.Json.GetFirstArgAs<FriendStatus>();
                switch (friendStatus.status)
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
            PgUser friendData;
            if (_userData.TryGetFriend(friendStatus._id, out friendData))
            {
                friendData.Status = friendStatus.status;
            }
        }

        private void FriendRemoved(FriendStatus friendStatus)
        {
            PgUser friendUiData;
            if (_userData.TryGetFriend(friendStatus._id, out friendUiData))
                _userData.RemoveFriend(friendUiData);
        }

        private void FriendAdded(FriendStatus friendStatus)
        {
            UserWithStatus friend = new UserWithStatus();
            friend._id = friendStatus._id;
            friend.username = friendStatus.username;
            friend.status = FriendStatusOffline;
            AddOrUpdateFriend(friend);

            var todoList = new List<UIElement>();
            foreach (var item in stackPanelFriendRequestsFrom.Children)
            {
                var request = item as FriendRequestFromUserControl;
                if (request == null)
                    continue;
                if (request.Username == friendStatus.username)
                    todoList.Add((UIElement)item);
            }
            foreach (var item in todoList)
                stackPanelFriendRequestsFrom.Children.Remove(item);
        }

        private void GetFriends()
        {
            RestResponse<FriendList> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var friendsRequestCall = _userData.PgSession.GetWebApiV1Function("/friends");
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

        private void AddOrUpdateFriend(UserWithStatus friend)
        {
            PgUser old;
            if (_userData.TryGetFriend(friend._id, out old))
            {
                old.Status = friend.status;
                old.Username = friend.username;
            }
            else
            {
                var newFriend = _userData.GetPgUser(friend);
                newFriend.Status = friend.status;
                _userData.AddFriend(newFriend);
            }
        }

        private void RemoveOldFriends(List<UserWithStatus> newFriends)
        {
            var newData = new Dictionary<string, PgUser>(newFriends.Count);
            foreach (var item in newFriends)
                newData.Add(item.username, null);

            var removes = new List<PgUser>();
            foreach (var item in Friends)
                if (!newData.ContainsKey(item.Username))
                    removes.Add(item);
            foreach (var item in removes)
                _userData.RemoveFriend(item);
        }
        #endregion

        #region friend request
        #region friend request general
        private void OnFriendRequestCreated(IMessage data)
        {
            var friendRequest = data.Json.GetFirstArgAs<FriendRequest>();
            if (friendRequest.to_user._id == _userData.User.Id)
            {
                FriendRequestToReceived(friendRequest);
            }
            else if (friendRequest.from_user._id == _userData.User.Id)
            {
                FriendRequestFromReceived(friendRequest);
            }
        }

        private void OnFriendRequestDestroyed(IMessage data)
        {
            var friendRequest = data.Json.GetFirstArgAs<FriendRequest>();
            IFriendRequestUserControl control;
            if (TryGetFriendRequestUserControl(stackPanelFriendRequestsTo.Children, friendRequest._id, out control))
                stackPanelFriendRequestsTo.Children.Remove((UIElement)control);
            if (TryGetFriendRequestUserControl(stackPanelFriendRequestsFrom.Children, friendRequest._id, out control))
                stackPanelFriendRequestsFrom.Children.Remove((UIElement)control);
        }

        private static bool HaveFriendRequestUserControl(UIElementCollection children, string id)
        {
            IFriendRequestUserControl control;
            return TryGetFriendRequestUserControl(children, id, out control);
        }

        private static bool TryGetFriendRequestUserControl(UIElementCollection children, string id, out IFriendRequestUserControl control)
        {
            foreach (var item in children)
            {
                var request = item as IFriendRequestUserControl;
                if (request == null)
                    continue;

                if (request.FriendRequestId == id)
                {
                    control = request;
                    return true;
                }
            }
            control = null;
            return false;
        }
        #endregion

        #region friend request from
        public void GetFriendRequestsFrom()
        {
            List<FriendRequest> requests = null;
            bool isSuccess = false;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var friendsRequestCall = _userData.PgSession.GetWebApiV1Function("/friend_requests", "sent=1");
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

        private void FriendRequestFromReceived(FriendRequest friendRequest)
        {
            if (HaveFriendRequestUserControl(stackPanelFriendRequestsFrom.Children, friendRequest._id))
                return;

            var control = new FriendRequestFromUserControl();
            control.FriendRequestId = friendRequest._id;
            control.Username = friendRequest.to_user.username;
            control.UserId = friendRequest.to_user._id;
            control.FriendRequestCanceled += FriendRequestFromCanceled;
            stackPanelFriendRequestsFrom.Children.Add(control);
        }
        private void FriendRequestFromCanceled(FriendRequestFromUserControl source)
        {
            string id = source.FriendRequestId;
            _userData.PgSession.Begin(delegate
            {
                var apiCall = _userData.PgSession.GetWebApiV1Function("/friend_requests/" + id);
                var client = new RestClient(apiCall);
                var request = new RestRequest(Method.DELETE);
                client.Execute<ApiResponse>(request);
            });
        }
        #endregion

        #region friend request to
        public void GetFriendRequestsTo()
        {
            List<FriendRequest> requests = null;
            bool isSuccess = false;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var friendsRequestCall = _userData.PgSession.GetWebApiV1Function("/friend_requests");
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
        private void FriendRequestToReceived(FriendRequest friendRequest)
        {
            if (HaveFriendRequestUserControl(stackPanelFriendRequestsTo.Children, friendRequest._id))
                return;

            var control = new FriendRequestToUserControl();
            control.FriendRequestId = friendRequest._id;
            control.Username = friendRequest.from_user.username;
            control.UserId = friendRequest.from_user._id;
            control.FriendRequestToAnswered += FriendRequestToAnswered;
            stackPanelFriendRequestsTo.Children.Add(control);
        }
        private void FriendRequestToAnswered(FriendRequestToUserControl source, bool isAccepted)
        {
            string id = source.FriendRequestId;
            _userData.PgSession.Begin(delegate
            {
                var apiCall = _userData.PgSession.GetWebApiV1Function("/friend_requests/" + id);
                var client = new RestClient(apiCall);
                var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };

                var friendPutRequest = new FriendRequestResponse { action = isAccepted ? "accept" : "reject" };
                var friendPutRootObject = new FriendRequestResponseRoot { friend_request = friendPutRequest };
                request.AddBody(friendPutRootObject);
                client.Execute<ApiResponse>(request);
            });
        }

        
        #endregion

        #region create friend request
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

            RequestFriend(username);
        }

        private void RequestFriend(string username)
        {
            _userData.Friendship.RequestFriend(username, OnRequestFriend);
        }

        private void OnRequestFriend(string errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                App.LogLine("Error requesting friend: " + errorMessage);
                if (errorMessage == "User not found")
                {
                    this.BeginInvokeUI(delegate
                    {
                        MessageDialog.Show(HomeWindow.Home, "User not found", errorMessage);
                    });
                }
                else if (errorMessage == "You are already friends with that user")
                {
                    this.BeginInvokeUI(delegate
                    {
                        MessageDialog.Show(HomeWindow.Home, "Already friends", errorMessage);
                    });
                }
            }
        }
        #endregion

        #region unfriend
        private void Unfriend(PgUser friend)
        {
            _userData.PgSession.Begin(delegate
            {
                var request = new RestRequest(Method.DELETE);
                var friendsUrl = _userData.PgSession.GetWebApiV1Function("/friends/" + friend.Id);
                var client = new RestClient(friendsUrl);
                client.Execute<ApiResponse>(request);
            });
        }
        #endregion
        #endregion friend request


        #region drag & drop source
        private Point _previewMouseLeftButtonDownPosition;
        private DataGridRow _previewMouseLeftButtonDownRow;
        private PgUser _previewMouseLeftButtonDownUser;
        private bool _previewMouseLeftButtonDownCaptured;
        private DataObject _previewMouseLeftButtonDownDragData;
        private void dataGridFriends_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ResetDrag();
            DataGridRow row;
            PgUser friend;
            if (dataGridFriends.TryGetRowAndItem(e, out row, out friend))
            {
                _previewMouseLeftButtonDownRow = row;
                _previewMouseLeftButtonDownUser = friend;
                _previewMouseLeftButtonDownPosition = e.GetPosition(null);
                _previewMouseLeftButtonDownCaptured = dataGridFriends.CaptureMouse();
            }
        }

        private void dataGridFriends_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResetDrag();
        }

        private void ResetDrag()
        {
            _previewMouseLeftButtonDownCaptured = false;
            dataGridFriends.ReleaseMouseCapture();
            _previewMouseLeftButtonDownDragData = null;
        }

        private void dataGridFriends_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_previewMouseLeftButtonDownCaptured || _previewMouseLeftButtonDownDragData != null)
                return;

            Vector diff = _previewMouseLeftButtonDownPosition - e.GetPosition(null);

            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            _previewMouseLeftButtonDownDragData = new DataObject(typeof(PgUser).FullName, _previewMouseLeftButtonDownUser);
            DragDrop.DoDragDrop(_previewMouseLeftButtonDownRow, _previewMouseLeftButtonDownDragData, DragDropEffects.Link); 
        }
        #endregion

        #region drag & drop sink
        private void dataGridFriends_DragQuery(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (CanHandleDrop(e))
            {
                e.Effects = DragDropEffects.Link;
                e.Handled = true;
            }
        }

        private bool CanHandleDrop(DragEventArgs e)
        {
            return e.Data.GetDataPresent(typeof(PgUser).FullName) && _previewMouseLeftButtonDownDragData == null;
        }

        private void dataGridFriends_Drop(object sender, DragEventArgs e)
        {
            if (!CanHandleDrop(e))
                return;

            if (e.Data.GetDataPresent(typeof(PgUser).FullName))
            {
                PgUser user = e.Data.GetData(typeof(PgUser).FullName) as PgUser;
                if (_userData.IsFriend(user.Id))
                    RequestFriend(user.Username);
                else
                {
                    foreach (DataGridRow row in dataGridFriends.Items)
                    {
                        var rowUser = (PgUser)row.Item;
                        if (user.Equals(rowUser))
                        {
                            dataGridFriends.SelectedItem = row;
                            dataGridFriends.ScrollIntoView(row);
                            break;
                        }
                    }
                }
                e.Handled = true;
            }
        }
        #endregion

    }
}
