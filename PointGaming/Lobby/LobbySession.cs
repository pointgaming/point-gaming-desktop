using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.ComponentModel;
using PointGaming.Chat;
using PointGaming.GameRoom;
using PointGaming.POCO;
using RestSharp;
using SocketIOClient;
using SocketIOClient.Messages;
using System.Windows.Threading;


namespace PointGaming.Lobby
{
    public class LobbySession : ChatroomSessionBase
    {
        private const int DefaultMaxGameRoomMemberCount = 50;
        private const int MinRoomCount = 100;
        public readonly HomeTab.LauncherInfo GameInfo;
        private readonly UserDataManager _userData = UserDataManager.UserData;

        public GameRoomItemCollection GameRoomManager;

        private readonly ObservableCollection<GameRoomItem> _joinedGameRooms = new ObservableCollection<GameRoomItem>();
        public ObservableCollection<GameRoomItem> JoinedGameRooms { get { return _joinedGameRooms; } }

        private ObservableCollection<GameRoomItem> _activeGames = new ObservableCollection<GameRoomItem>();
        public ObservableCollection<GameRoomItem> ActiveGames { get { return this._activeGames; } }

        public event Action<LobbySession> LoadGameRoomsComplete;
        public event Action<GameRoomItem> DisplayToggled;

        private LobbyWindow _window;
        public LobbyWindow Window { get { return _window; } }

        public LobbySession(SessionManager manager, HomeTab.LauncherInfo gameInfo)
            : base(manager)
        {
            GameInfo = gameInfo;
            GameRoomManager = new GameRoomItemCollection();
        }

        public override void ShowControl(bool shouldActivate)
        {
            if (_window == null)
            {
                _window = new LobbyWindow();
                _window.Init(this);
            }

            _window.ShowNormal(shouldActivate);
        }

        public void LoadGameRooms()
        {
            RestResponse<GameRoomListPoco> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = _userData.PgSession.GetWebAppFunction("", "/game_rooms", "game_id=" + GameInfo.Id);
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                response = (RestResponse<GameRoomListPoco>)client.Execute<GameRoomListPoco>(request);
            }, delegate
            {
                if (response.IsOk())
                {
                    var gameRooms = response.Data.game_rooms;
                    LoadGameRooms(gameRooms);

                    FillAdvertisedGames(gameRooms);
                }
            });
        }

        private void FillAdvertisedGames(List<GameRoomPoco> gameRooms)
        {
            foreach (var item in gameRooms)
            {
                if (item.is_advertising == true)
                    this.ActiveGames.Add(new GameRoomItem(item));
            }
        }

        private void UpdateActiveGames(GameRoomItem gameRoom)
        {
            var isPresentInList = false;
            var index = 0;
            foreach (var item in ActiveGames)
                if (item.Id == gameRoom.Id)
                {
                    isPresentInList = true;
                    index = ActiveGames.IndexOf(item);
                    break;
                }
            if (gameRoom.IsAdvertising && !isPresentInList)
                ActiveGames.Add(gameRoom);
            else if (gameRoom.IsAdvertising && isPresentInList)
            {
                ActiveGames[index] = gameRoom;
            }
            else if (!gameRoom.IsAdvertising && isPresentInList)
                ActiveGames.Remove(gameRoom);
        }

        private class ListInvoker
        {
            private IEnumerator<Action> _actions;
            private DispatcherTimer _timer;
            public ListInvoker(IEnumerable<Action> actions)
            {
                _actions = actions.GetEnumerator();
            }
            public void Start()
            {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(30);
                _timer.Tick += _timer_Tick;
                _timer.Start();
            }

            void _timer_Tick(object sender, EventArgs e)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (_actions.MoveNext())
                        ((Action)_actions.Current).BeginInvokeUI();
                    else
                    {
                        _timer.Stop();
                        return;
                    }
                }
            }
        }

        private void LoadGameRooms(List<GameRoomPoco> gameRooms)
        {
            var actions = LoadGameRooms2(gameRooms);
            ListInvoker li = new ListInvoker(actions);
            li.Start();
        }

        private IEnumerable<Action> LoadGameRooms2(List<GameRoomPoco> gameRooms)
        {
            foreach (var gameRoom in gameRooms)
            {
                if (gameRoom.owner == null) continue;

                var item = new GameRoomItem(gameRoom);
                yield return delegate { GameRoomManager.Add(item); };
            }

            yield return delegate
            {
                var call = LoadGameRoomsComplete;
                if (call != null)
                    call(this);
            };

            for (int i = 0; i < MinRoomCount; i++ )
            {
                var position = i + 1;
                var fake = CreateFakeRoom(position);
                yield return delegate { GameRoomManager.Add(fake); };
            }
        }

        public void ToggleDisplay(GameRoomItem item)
        {
            var call = DisplayToggled;
            if (call != null)
                call(item);
        }
        
        public void ShowUndecidedMatches(string id)
        {
            _manager.ShowUndecidedMatches(id);
        }

        public void ReportMatchWinner(Match match, Action<Match> onCompleted)
        {
            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = _userData.PgSession.GetWebAppFunction("/api", "/matches/" + match.Id + ".json");
                var client = new RestClient(url);
                var request = new RestRequest(Method.PUT);
                response = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
            }, delegate
            {
                if (!response.IsOk())
                {
                    string msg = response.Data.errors == null ? response.StatusCode.ToString() : string.Concat(response.Data.errors);
                    MessageDialog.Show(_window, "Failed to report winner", msg);
                    onCompleted(null);
                }
                else
                {
                    onCompleted(match);
                }
            });
        }

        public void RequestUndecidedMatches(Action<List<MatchPoco>> onCompleted)
        {
            RestResponse<List<MatchPoco>> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = _userData.PgSession.GetWebAppFunction("/api", "/matches");
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
                response = (RestResponse<List<MatchPoco>>)client.Execute<List<MatchPoco>>(request);
            }, delegate
            {
                if (response.IsOk() && response.Data != null)
                {
                    if (onCompleted != null)
                        onCompleted(response.Data);
                }
                else
                {
                    // fail silently
                    onCompleted(new List<MatchPoco>(0));
                }
            });
        }

        public void CreateRoomAt(int position, string description, Action<string> onCreated, bool takeover = false)
        {
            RestResponse<GameRoomPoco> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = _userData.PgSession.GetWebAppFunction("/api", "/game_rooms");
                var client = new RestClient(url);
                var request = new RestRequest(Method.POST) { RequestFormat = RestSharp.DataFormat.Json };
                var poco = new GameRoomPoco
                {
                    position = position,
                    description = description,
                    takeover_position = takeover,
                    max_member_count = DefaultMaxGameRoomMemberCount,
                    is_advertising = false,
                    game_id = GameInfo.Id,
                    betting_type = "1v1",
                    betting = true
                };
                var root = new GameRoomSinglePoco { game_room = poco };
                request.AddBody(root);
                response = (RestResponse<GameRoomPoco>)client.Execute<GameRoomPoco>(request);
            }, delegate
            {
                if (response.IsOk() && response.Data != null)
                {
                    var gameRoom = response.Data;
                    if (onCreated != null)
                        onCreated(gameRoom._id);
                }
                else
                {
                    string reason = String.IsNullOrEmpty(response.ErrorMessage) ? response.Content : response.ErrorMessage;
                    MessageDialog.Show(_window, "Join Failed", reason);
                }
            });
        }

        public void TakeOverRoomAt(GameRoomItem item, Action<string> onCreated)
        {
            RestResponse<GameRoomPoco> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = _userData.PgSession.GetWebAppFunction("/api", "/game_rooms/" + item.Id + "/take_over");
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
                response = (RestResponse<GameRoomPoco>)client.Execute<GameRoomPoco>(request);
            }, delegate
            {
                if (response.IsOk() && response.Data != null)
                {
                    var gameRoom = response.Data;
                    if (onCreated != null)
                        onCreated(gameRoom._id);
                }
                else
                {
                    string reason = String.IsNullOrEmpty(response.ErrorMessage) ? response.Content : response.ErrorMessage;
                    MessageDialog.Show(_window, "Join Failed", reason);
                }
            });
        }

        public static void LookupGameRoom(UserDataManager userData, string id, Action<GameRoomItem> onLookupResponse)
        {
            RestResponse<GameRoomSinglePoco> response = null;
            userData.PgSession.BeginAndCallback(delegate
            {
                var url = userData.PgSession.GetWebAppFunction("/api", "/game_rooms/" + id, "include_bets=true");
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                response = (RestResponse<GameRoomSinglePoco>)client.Execute<GameRoomSinglePoco>(request);
            }, delegate
            {
                if (response.IsOk())
                {
                    var gameRoom = response.Data.game_room;
                    var item = new GameRoomItem(gameRoom);
                    if (onLookupResponse != null)
                        onLookupResponse(item);
                }
            });
        }

        public void OnGameRoomNew(GameRoomPoco poco)
        {
            var item = new GameRoomItem(poco);
            GameRoomManager.Add(item);
        }
        public void OnGameRoomUpdate(GameRoomPoco poco)
        {
            GameRoomItem room;
            if (!GameRoomManager.TryGetItemById(poco._id, out room))
                return;

            if (room.Position != poco.position)
                GameRoomManager.Move(room, poco.position);

            room.Update(poco);
            UpdateActiveGames(room);
        }
        public void OnGameRoomDestroy(GameRoomPoco poco)
        {
            GameRoomItem item;
            if (!GameRoomManager.TryGetItemById(poco._id, out item))
                return;
            GameRoomManager.Remove(item);
            ActiveGames.Remove(item);
        }
        
        public void GameRoomJoining(GameRoomSession gameRoomSession)
        {
            gameRoomSession.SessionStateChanged += gameRoomSession_SessionStateChanged;
        }

        void gameRoomSession_SessionStateChanged(object sender, EventArgs e)
        {
            var gameRoomSession = sender as GameRoomSession;
            if (gameRoomSession.State == ChatroomState.Connected)
                JoinedGameRooms.Add(gameRoomSession.GameRoom);
            else
                JoinedGameRooms.Remove(gameRoomSession.GameRoom);
        }


        public class GameRoomItemCollection
        {
            private readonly ObservableCollection<GameRoomItem> _items = new ObservableCollection<GameRoomItem>();
            public ObservableCollection<GameRoomItem> AllGameRooms { get { return _items; } }
                        
            public bool TryGetItemById(string id, out GameRoomItem item)
            {
                item = null;
                foreach (var cur in _items)
                {
                    if (id == cur.Id)
                    {
                        item = cur;
                        break;
                    }
                }
                return item != null;
            }

            public void Add(GameRoomItem item)
            {
                bool isAdded = false;
                var position = item.Position;
                for (int i = 0; i < _items.Count; i++)
                {
                    var oldItem = _items[i];
                    if (oldItem.Position == position)
                    {
                        if (item.Id == null)
                        {
                            isAdded = true;
                            break;
                        }
                        else if (oldItem.Id == null)
                        {
                            _items[i] = item;
                            isAdded = true;
                            CleanEnd();
                            break;
                        }
                        // else add it later
                    }
                    else if (oldItem.Position > position)
                    {
                        _items.Insert(i, item);
                        isAdded = true;
                        break;
                    }
                }
                if (!isAdded)
                {
                    _items.Add(item);
                    CleanEnd();
                }
            }
            
            public bool Remove(GameRoomItem item)
            {
                var index = IndexOf(item);
                var shouldRemove = index >= 0;
                if (shouldRemove)
                {
                    HandleRemove(item, index);
                    CleanEnd();
                }
                return shouldRemove;
            }
            
            public void Move(GameRoomItem room, int position)
            {
                var oldIndex = IndexOf(room);
                HandleRemove(room, oldIndex);

                room.Position = position;
                Add(room);
                CleanEnd();
            }

            public int IndexOf(GameRoomItem item)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var cur = _items[i];
                    if (cur == item)
                        return i;
                }
                return -1;
            }

            public int Count { get { return _items.Count; } }

            private int GetCountAtPosition(int position)
            {
                int countAtPosition = 0;
                for (int i = 0; i < _items.Count; i++)
                {
                    var oldItem = _items[i];
                    if (oldItem.Position == position)
                        countAtPosition++;
                    else if (oldItem.Position > position)
                        break;
                }
                return countAtPosition;
            }

            private void HandleRemove(GameRoomItem room, int oldIndex)
            {
                var position = room.Position;
                int countAtPosition = GetCountAtPosition(position);
                if (countAtPosition == 1 && position <= MinRoomCount)
                {
                    var fake = CreateFakeRoom(position);
                    _items[oldIndex] = fake;
                }
                else
                {
                    _items.RemoveAt(oldIndex);
                }
            }

            private void CleanEnd()
            {
                var isInitialized = _items.Count >= MinRoomCount;
                if (!isInitialized)
                    return;

                // remove any fake rooms at position > MinRoomCount... unless its the last room
                for (int i = _items.Count - 2; i >= 0; i--)
                {
                    var cur = _items[i];
                    if (cur.Position <= MinRoomCount)
                        break;

                    if (cur.Id == null)
                        _items.RemoveAt(i);
                }

                var lastItem = _items[_items.Count - 1];
                var isFake = lastItem.Id == null;
                if (!isFake)
                {
                    // append a fake room
                    var position = lastItem.Position + 1;
                    if (position < MinRoomCount)
                        position = MinRoomCount;

                    var fake = CreateFakeRoom(position);
                    _items.Add(fake);
                }
                else
                {
                    var roomBefore = _items[_items.Count - 2];
                    var roomBeforeIsFake = roomBefore.Id == null;
                    if (roomBeforeIsFake && roomBefore.Position == MinRoomCount)
                    {
                        // edge case, remove the last room because the one before it should be the last fake room
                        var index = _items.Count - 1;
                        _items.RemoveAt(index);
                    }
                    else
                    {
                        // fix the position
                        var position = roomBefore.Position + 1;
                        if (position < MinRoomCount)
                            position = MinRoomCount;
                        lastItem.Position = position;
                    }
                }
            }
        }

        private static GameRoomItem CreateFakeRoom(int position)
        {
            var fakeRoom = new GameRoomItem
            {
                Position = position,
                Description = "",
                MaxMemberCount = DefaultMaxGameRoomMemberCount,
            };
            return fakeRoom;
        }
    }
}
