using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PointGaming.Desktop.Chat;
using PointGaming.Desktop.POCO;
using RestSharp;
using SocketIOClient;
using SocketIOClient.Messages;

namespace PointGaming.Desktop.GameRoom
{
    public class GameRoomSession : ChatroomSession
    {
        private readonly UserDataManager _userData = HomeWindow.UserData;

        private readonly Lobby.LobbySession _lobbySession;
        public readonly Lobby.GameRoomItem GameRoom;

        public string GameId { get { return _lobbySession.GameId; } }
        public string GameRoomId { get { return GameRoom.Id; } }

        public GameRoomSession(ChatManager manager, Lobby.LobbySession lobbySession, Lobby.GameRoomItem gameRoom)
            : base(manager)
        {
            _lobbySession = lobbySession;
            GameRoom = gameRoom;
        }

        public override Type GetUserControlType()
        {
            return typeof(GameRoomTab);
        }

        public override IChatroomTab GetNewUserControl()
        {
            return new GameRoomTab();
        }

        public void SetGameRoomSettings(POCO.GameRoomPoco poco)
        {
            RestResponse<GameRoomSinglePoco> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = Properties.Settings.Default.GameRooms + "/" + GameRoom.Id + "?auth_token=" + _userData.PgSession.AuthToken;
                var client = new RestClient(url);
                var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
                var root = new GameRoomSinglePoco { game_room = poco };
                request.AddBody(root);
                response = (RestResponse<GameRoomSinglePoco>)client.Execute<GameRoomSinglePoco>(request);
            }, delegate
            {
                if (!response.IsOk())
                {
                    MessageDialog.Show(_userData.GetChatWindow(), "Failed to set Settings", "Sorry, failed to set settings.\r\nDetails: " + response.ErrorMessage);
                }
            });
        }

        public void OnUpdate(GameRoomPoco poco)
        {
            // lobby should have handled it
        }
        public void OnDestroy(GameRoomPoco poco)
        {
            _manager.ChatWindow.CloseTab(typeof(GameRoomTab), ChatroomId);
        }
    }
}
