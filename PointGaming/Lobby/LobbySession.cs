using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using PointGaming.Chat;
using PointGaming.GameRoom;
using PointGaming.POCO;
using RestSharp;
using SocketIOClient;
using SocketIOClient.Messages;

namespace PointGaming.Lobby
{
    public class LobbySession : ChatroomSessionBase
    {
        private const int DefaultMaxGameRoomMemberCount = 50;
        private const int MinRoomCount = 100;
        public readonly HomeTab.LauncherInfo GameInfo;
        private readonly UserDataManager _userData = HomeWindow.UserData;

        public Dictionary<string, GameRoomItem> GameRoomLookup = new Dictionary<string, GameRoomItem>();

        private readonly ObservableCollection<GameRoomItem> _allGameRooms = new ObservableCollection<GameRoomItem>();
        public ObservableCollection<GameRoomItem> AllGameRooms { get { return _allGameRooms; } }

        private readonly ObservableCollection<GameRoomItem> _joinedGameRooms = new ObservableCollection<GameRoomItem>();
        public ObservableCollection<GameRoomItem> JoinedGameRooms { get { return _joinedGameRooms; } }

        public event Action<LobbySession> LoadGameRoomsComplete;
        public event Action<GameRoomItem> DisplayToggled;

        private LobbyWindow _window;
        public LobbyWindow Window { get { return _window; } }
        
        public LobbySession(SessionManager manager, HomeTab.LauncherInfo gameInfo)
            : base(manager)
        {
            GameInfo = gameInfo;
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
            FillEmptyRooms();

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
                    foreach (var gameRoom in gameRooms)
                    {
                        if (gameRoom.owner == null) continue;

                        var item = new GameRoomItem(gameRoom);
                        AddRoom(item);
                    }
                    var call = LoadGameRoomsComplete;
                    if (call != null)
                        call(this);
                }
            });
        }

        public void ToggleDisplay(GameRoomItem item)
        {
            var call = DisplayToggled;
            if (call != null)
                call(item);
        }
        
        private void FillEmptyRooms()
        {
            while (_allGameRooms.Count < MinRoomCount)
            {
                var position = _allGameRooms.Count + 1;
                SetFakeRoomAt(position);
            }
        }

        private void SetFakeRoomAt(int position)
        {
            var fakeRoom = new GameRoomItem { Position = position, Description = "", MaxMemberCount = DefaultMaxGameRoomMemberCount, };
            int index = position - 1;
            if (index < _allGameRooms.Count)
                _allGameRooms[index] = fakeRoom;
            else
                _allGameRooms.Add(fakeRoom);
        }

        public void CreateRoomAt(int position, string description, Action<string> onCreated, bool takeover = false)
        {
            RestResponse<GameRoomSinglePoco> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = _userData.PgSession.GetWebAppFunction("", "/game_rooms");
                var client = new RestClient(url);
                var request = new RestRequest(Method.POST) { RequestFormat = RestSharp.DataFormat.Json };
                var poco = new GameRoomPoco {
                    position = position,
                    description = description,
                    takeover_position = takeover,
                    max_member_count = DefaultMaxGameRoomMemberCount,
                    is_advertising = false,
                    game_id = GameInfo.Id,
                    betting_type = "1v1"
                };
                var root = new GameRoomSinglePoco { game_room = poco };
                request.AddBody(root);
                response = (RestResponse<GameRoomSinglePoco>)client.Execute<GameRoomSinglePoco>(request);
            }, delegate
            {
                if (response.IsOk() && response.Data != null)
                {
                    var gameRoom = response.Data.game_room;
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

        private void AddRoom(GameRoomItem room)
        {
            var position = room.Position;
            while (position >= _allGameRooms.Count)// if position is 100
                SetFakeRoomAt(_allGameRooms.Count + 1);// then create a fake room at 101
            _allGameRooms[position - 1] = room;
            GameRoomLookup[room.Id] = room;
        }

        private void DeleteRoom(GameRoomItem room)
        {
            var position = room.Position;
            SetFakeRoomAt(position);

            bool isLastEmpty = _allGameRooms[_allGameRooms.Count - 1].Id == null;
            if (isLastEmpty)
            {
                int lastMinRoomIndex = MinRoomCount - 1;
                for (int i = _allGameRooms.Count - 2; i >= lastMinRoomIndex; i--)
                {
                    var isEmpty = _allGameRooms[i].Id == null;
                    if (!isEmpty)
                        break;
                    _allGameRooms.RemoveAt(i + 1);
                }
            }
        }
        
        public void OnGameRoomNew(GameRoomPoco poco)
        {
            var item = new GameRoomItem(poco);
            AddRoom(item);
        }
        public void OnGameRoomUpdate(GameRoomPoco poco)
        {
            GameRoomItem room;
            if (!GameRoomLookup.TryGetValue(poco._id, out room))
                return;

            if (room.Position != poco.position)
            {
                SetFakeRoomAt(room.Position);
                room.Position = poco.position;
                var index = poco.position - 1;
                if (_allGameRooms.Count == index)
                    _allGameRooms.Add(room);
                else
                    _allGameRooms[index] = room;
            }

            room.Update(poco);
        }
        public void OnGameRoomDestroy(GameRoomPoco poco)
        {
            GameRoomItem item;
            if (!GameRoomLookup.TryGetValue(poco._id, out item))
                return;

            DeleteRoom(item);
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
    }
}
