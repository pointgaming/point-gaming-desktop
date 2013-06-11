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
    public class LobbySession : ChatroomSession
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

        public LobbySession(ChatManager manager, HomeTab.LauncherInfo gameInfo)
            : base(manager)
        {
            GameInfo = gameInfo;
        }

        public void LoadGameRooms()
        {
            FillEmptyRooms();

            RestResponse<GameRoomListPoco> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = Properties.Settings.Default.GameRooms + "?game_id=" + GameInfo.Id + "&auth_token=" + _userData.PgSession.AuthToken;
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
        
        private void FillEmptyRooms()
        {
            while (_allGameRooms.Count < MinRoomCount)
            {
                var position = _allGameRooms.Count + 1;
                SetFakeRoomAt(position);
            }
            SetPlaceholderRoom();
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

        private void SetPlaceholderRoom()
        {
            _allGameRooms.Add(new GameRoomItem { Position = _allGameRooms.Count + 1});
        }

        public void CreateRoomAt(int position, string description, Action<string> onCreated)
        {
            RestResponse<GameRoomSinglePoco> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = Properties.Settings.Default.GameRooms + "?auth_token=" + _userData.PgSession.AuthToken;
                var client = new RestClient(url);
                var request = new RestRequest(Method.POST) { RequestFormat = RestSharp.DataFormat.Json };
                var poco = new GameRoomPoco {
                    position = position,
                    description = description,
                    max_member_count = DefaultMaxGameRoomMemberCount,
                    is_advertising = false,
                    game_id = GameInfo.Id,
                };
                var root = new GameRoomSinglePoco { game_room = poco };
                request.AddBody(root);
                response = (RestResponse<GameRoomSinglePoco>)client.Execute<GameRoomSinglePoco>(request);
            }, delegate
            {
                if (response.IsOk())
                {
                    var gameRoom = response.Data.game_room;
                    if (onCreated != null)
                        onCreated(gameRoom._id);
                }
                else
                {
                    string reason = String.IsNullOrEmpty(response.ErrorMessage) ? response.Content : response.ErrorMessage;
                    MessageDialog.Show(_userData.GetChatWindow(), "Join Failed", reason);
                }
            });
        }

        public static void LookupGameRoom(UserDataManager userData, string id, Action<GameRoomItem> onLookupResponse)
        {
            RestResponse<GameRoomSinglePoco> response = null;
            userData.PgSession.BeginAndCallback(delegate
            {
                var url = Properties.Settings.Default.GameRooms + id + "?auth_token=" + userData.PgSession.AuthToken;
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
            SetPlaceholderRoom();
        }

        public override Type GetUserControlType()
        {
            return typeof(LobbyTab);
        }

        public override IChatroomTab GetNewUserControl()
        {
            return new LobbyTab();
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
    }
}
