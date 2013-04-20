using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PointGaming.Desktop.Chat;
using PointGaming.Desktop.POCO;
using RestSharp;
using SocketIOClient;
using SocketIOClient.Messages;
using System.Collections.ObjectModel;

namespace PointGaming.Desktop.GameRoom
{
    public class GameRoomSession : ChatroomSession
    {
        private readonly UserDataManager _userData = HomeWindow.UserData;

        private readonly Lobby.LobbySession _lobbySession;
        public readonly Lobby.GameRoomItem GameRoom;

        public readonly Match MyMatch = new Match();
        public readonly ObservableCollection<Bet> RoomBets = new ObservableCollection<Bet>();

        public string GameId { get { return GameRoom.GameId; } }
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

        public void SetGameRoomSettings(object poco)
        {
            RestResponse<GameRoomSinglePoco> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = Properties.Settings.Default.GameRooms + "/" + GameRoom.Id + "?auth_token=" + _userData.PgSession.AuthToken;
                var client = new RestClient(url);
                var request = new RestRequest(Method.PUT) { RequestFormat = RestSharp.DataFormat.Json };
                var root = new { game_room = poco };
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

        public void CreateBet(Bet bet)
        {
            var poco = bet.ToPoco();
            poco.match_hash = MyMatch.MatchHash;

            RestResponse<BetPoco> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = Properties.Settings.Default.Matches + "/" + MyMatch.Id + "/bets?auth_token=" + _userData.PgSession.AuthToken;
                var client = new RestClient(url);
                var request = new RestRequest(Method.POST) { RequestFormat = RestSharp.DataFormat.Json };
                var root = new { bet = poco };
                request.AddBody(root);
                response = (RestResponse<BetPoco>)client.Execute<BetPoco>(request);
            }, delegate
            {
                if (!response.IsOk())
                {
                    MessageDialog.Show(_userData.GetChatWindow(), "Failed to create bet", response.ErrorMessage);
                }
            });
        }

        public void AcceptBet(Bet bet) 
        {
            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = Properties.Settings.Default.Matches + "/" + MyMatch.Id + "/bets" + bet.Id + "?auth_token=" + _userData.PgSession.AuthToken;
                var client = new RestClient(url);
                var request = new RestRequest(Method.PUT);
                response = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
            }, delegate
            {
                if (!response.IsOk())
                {
                    MessageDialog.Show(_userData.GetChatWindow(), "Failed to accept bet", response.ErrorMessage);
                }
            });
        }

        public void CancelBet(Bet bet) 
        {
            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = Properties.Settings.Default.Matches + "/" + MyMatch.Id + "/bets" + bet.Id + "?auth_token=" + _userData.PgSession.AuthToken;
                var client = new RestClient(url);
                var request = new RestRequest(Method.DELETE);
                response = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
            }, delegate
            {
                if (!response.IsOk())
                {
                    MessageDialog.Show(_userData.GetChatWindow(), "Failed to delete bet", response.ErrorMessage);
                }
            });
        }

        public void OnMatchNew(MatchPoco poco)
        {
            MyMatch.Update(_userData, poco);
            CleanBets(poco.match_hash);
        }
        public void OnMatchUpdate(MatchPoco poco)
        {
            MyMatch.Update(_userData, poco);
            CleanBets(poco.match_hash);
        }

        private void CleanBets(string matchHash)
        {
            var removes = new List<Bet>();
            foreach (var item in RoomBets)
            {
                if (item.MatchHash != matchHash)
                    removes.Add(item);
            }
            foreach (var item in removes)
                RoomBets.Remove(item);
        }
        public void OnBetNew(BetPoco poco)
        {
            Bet bet = new Bet(_userData, MyMatch, poco);
            RoomBets.Add(bet);
        }
        public void OnBetTakerNew(BetPoco poco)
        {
            var acceptedBy = _userData.GetPgUser(poco.taker);

            Bet bet;
            if (TryGetBetById(poco, out bet))
                bet.AcceptedBy(acceptedBy);
        }

        private bool TryGetBetById(BetPoco poco, out Bet bet)
        {
            bet = null;
            foreach (var item in RoomBets)
            {
                if (item.Id == poco._id)
                {
                    bet = item;
                    return true;
                }
            }
            return false;
        }

        public void OnBetUpdate(BetPoco poco)
        {
            var outcome = poco.outcome;
            Bet bet;
            if (TryGetBetById(poco, out bet))
                bet.SetOutcome(outcome);
        }
        public void OnBetDestroy(BetPoco poco)
        {
            Bet bet;
            if (TryGetBetById(poco, out bet))
                RoomBets.Remove(bet);
        }
    }
}
