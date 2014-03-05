using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PointGaming.Chat;
using PointGaming.POCO;
using RestSharp;
using SocketIOClient;
using SocketIOClient.Messages;
using System.Collections.ObjectModel;

namespace PointGaming.GameRoom
{
    public class GameRoomSession : ChatroomSessionBase
    {
        private readonly UserDataManager _userData = UserDataManager.UserData;

        private readonly Lobby.LobbySession _lobbySession;
        public readonly Lobby.GameRoomItem GameRoom;

        private Match _myMatch = new Match();
        public Match MyMatch { get { return _myMatch; } }
        public readonly ObservableCollection<Bet> RoomBets = new ObservableCollection<Bet>();

        private GameRoomWindow _window;
        public GameRoomWindow Window { get { return _window; } }

        public string GameId { get { return GameRoom.GameId; } }
        public string GameRoomId { get { return GameRoom.Id; } }

        public GameRoomSession(SessionManager manager, Lobby.LobbySession lobbySession, Lobby.GameRoomItem gameRoom)
            : base(manager)
        {
            _lobbySession = lobbySession;
            GameRoom = gameRoom;
            _lobbySession.GameRoomJoining(this);
            _lobbySession.DisplayToggled += _lobbySession_DisplayToggled;
        }

        void _lobbySession_DisplayToggled(Lobby.GameRoomItem obj)
        {
            if (obj != GameRoom)
                return;
            if (_window.IsVisible)
                _window.Hide();
            else
                ShowControl(true);
        }

        public override void ShowControl(bool shouldActivate)
        {
            if (_window == null)
            {
                GameRoomWindowModelView modelView = new GameRoomWindowModelView();
                modelView.Init(_manager, this);

                _window = new GameRoomWindow();
                _window.DataContext = modelView;
                _window.Owner = _lobbySession.Window;
                _window.Closing += _window_Closing;

                LoadBets();
                LoadMatch();
            }

            _window.WindowTreeManager.Parent = _lobbySession.Window.WindowTreeManager;
            _window.ShowNormal(shouldActivate);
        }

        void _window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _window = null;
            _lobbySession.DisplayToggled -= _lobbySession_DisplayToggled;
        }

        public void ShowLobby(bool shouldActivate)
        {
            _lobbySession.ShowControl(shouldActivate);
        }

        public void LoadBets()
        {
            if (GameRoom.Bets.Length > 0) return;

            RestResponse<List<BetPoco>> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = _userData.PgSession.GetWebAppFunction("/api", "/game_rooms/" + GameRoom.Id + "/bets", "include_matches=true");
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                response = (RestResponse<List<BetPoco>>)client.Execute<List<BetPoco>>(request);
            }, delegate
            {
                if (response.IsOk())
                {
                    if (response.Data != null)
                    {
                        var bets = response.Data;
                        foreach (BetPoco bet in bets)
                        {
                            Match match = new Match(bet.match);
                            Bet item = new Bet(_userData, match, bet);
                            RoomBets.Add(item);
                        }
                    }
                }
                else
                {
                    MessageDialog.Show(_window, "Failed to get Bets", "Sorry, failed to get bet list.\r\nDetails: " + response.ErrorMessage);
                }
            });
        }

        public void SetGameRoomSettings(object poco)
        {
            var url = _userData.PgSession.GetWebAppFunction("/api", "/game_rooms/" + GameRoom.Id);
            var root = new { game_room = poco };
            RestResponse<GameRoomPoco> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                response = SocketSession.Rest<GameRoomPoco>(url, Method.PUT, root);
            }, delegate
            {
                if (response.IsOk())
                {
                    GameRoom.Description = response.Data.description;
                    GameRoom.IsAdvertising = response.Data.is_advertising;
                    GameRoom.Password = response.Data.password;
                    GameRoom.IsBetting = response.Data.betting;
                    GameRoom.BettingType = response.Data.betting_type;
                    GameRoom.IsTeamBotPlaced = response.Data.is_team_bot_placed;
                }
                else
                {
                    var errorMessage = String.IsNullOrEmpty(response.Content) ? response.ErrorMessage : response.Content;
                    MessageDialog.Show(_window, "Failed to set Settings", "Sorry, failed to set settings.\r\nDetails: " + errorMessage);
                }
            });
        }

        public void OnUpdate(GameRoomPoco poco)
        {
            // lobby should have handled it
        }
        public void OnDestroy(GameRoomPoco poco)
        {
            if (_window != null)
                _window.Close();
        }

        #region match
        public void LoadMatch()
        {
            if (string.IsNullOrWhiteSpace(GameRoom.MatchId))
                return;

            var url = _userData.PgSession.GetWebApiV1Function("/matches/" + GameRoom.MatchId + ".json", "include_bets=true");
            RestResponse<MatchAndBetsPoco> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                response = SocketSession.Rest<MatchAndBetsPoco>(url, Method.GET, null);
            }, delegate
            {
                if (!response.IsOk())
                    return;

                MyMatch.Update(_userData, response.Data.match);
                foreach (var betPoco in response.Data.bets)
                {
                    Bet bet = new Bet(_userData, MyMatch, betPoco);
                    RoomBets.Add(bet);
                }
            });
        }

        public void CreateMatch(Match m)
        {
            MyMatch.IsEditable = false;

            var url = _userData.PgSession.GetWebAppFunction("", "/game_rooms/" + GameRoom.Id + "/matches.json");
            var poco = new //POCO.MatchPoco
            {
                game_id = m.GameId,
                betting = m.IsBetting,
                
                player_1_name = m.Player1.ShortDescription,
                player_1_type = m.Player1.PocoType,
                player_1_id = m.Player1.Id,

                player_2_name = m.Player2.ShortDescription,
                player_2_type = m.Player2.PocoType,
                player_2_id = m.Player2.Id,

                map = m.Map,
            };
            var root = new { match = poco };

            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                response = SocketSession.Rest<ApiResponse>(url, Method.POST, root);
            }, delegate
            {
                if (!response.IsOk())
                {
                    MyMatch.IsEditable = true;
                    MessageDialog.Show(_window, "Failed to create match", "Sorry, failed to create match.\r\nDetails: " + response.ErrorMessage);
                }
            });
        }
        public void UpdateMatch(Match m)
        {
            MyMatch.IsEditable = false;

            var url = _userData.PgSession.GetWebApiV1Function("/matches/" + m.Id + ".json");
            var poco = new //POCO.MatchPoco
            {
                game_id = m.GameId,
                betting = m.IsBetting,

                player_1_name = m.Player1.ShortDescription,
                player_1_type = m.Player1.PocoType,
                player_1_id = m.Player1.Id,

                player_2_name = m.Player2.ShortDescription,
                player_2_type = m.Player2.PocoType,
                player_2_id = m.Player2.Id,

                map = m.Map,
            };
            var root = new { match = poco };

            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                response = SocketSession.Rest<ApiResponse>(url, Method.PUT, root);
            }, delegate
            {
                if (!response.IsOk())
                {
                    MyMatch.IsEditable = true;
                    MessageDialog.Show(_window, "Failed to update match", "Sorry, failed to update match.\r\nDetails: " + response.ErrorMessage);
                }
            });
        }
        public void StartMatch()
        {
            MyMatch.IsEditable = false;

            var url = _userData.PgSession.GetWebApiV1Function("/matches/" + MyMatch.Id + "/start.json");
            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                response = SocketSession.Rest<ApiResponse>(url, Method.PUT, null);
            }, delegate
            {
                if (!response.IsOk())
                {
                    MyMatch.IsEditable = true;
                    MessageDialog.Show(_window, "Failed to start match", "Sorry, failed to start match.\r\nDetails: " + response.ErrorMessage);
                }
            });
        }
        public void CancelMatch()
        {
            MyMatch.IsEditable = false;

            var url = _userData.PgSession.GetWebApiV1Function("/matches/" + MyMatch.Id + "/cancel.json");
            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                response = SocketSession.Rest<ApiResponse>(url, Method.PUT, null);
            }, delegate
            {
                if (!response.IsOk())
                {
                    MyMatch.IsEditable = true;
                    MessageDialog.Show(_window, "Failed to cancel match", "Sorry, failed to cancel match.\r\nDetails: " + response.ErrorMessage);
                }
            });
        }
        public void FinishMatch(IBetOperand winner)
        {
            MyMatch.IsEditable = false;

            var url = _userData.PgSession.GetWebApiV1Function("/matches/" + MyMatch.Id + ".json");
            var poco = new
            {
                winner_id = winner.Id,
                winner_type = winner.PocoType,
            };
            var root = new { match = poco };

            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                response = SocketSession.Rest<ApiResponse>(url, Method.PUT, root);
            }, delegate
            {
                if (!response.IsOk())
                {
                    MyMatch.IsEditable = true;
                    MessageDialog.Show(_window, "Failed to finish match", "Sorry, failed to finish match.\r\nDetails: " + response.ErrorMessage);
                }
            });
        }

        public void OnMatchNew(MatchPoco poco)
        {
            MyMatch.Update(_userData, poco);
            MyMatch.IsEditable = true;
        }
        public void OnMatchUpdate(MatchPoco poco)
        {
            MyMatch.Update(_userData, poco);
            MyMatch.IsEditable = true;
            if (poco.state == "canceled")
            {
                CleanBets(poco.match_hash);
            }
        }
        #endregion

        #region bets
        public void CreateBet(Bet bet)
        {
            var poco = bet.ToPoco();

            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = _userData.PgSession.GetWebAppFunction("/api", "/game_rooms/" + GameRoom.Id + "/bets");
                var client = new RestClient(url);
                var request = new RestRequest(Method.POST) { RequestFormat = RestSharp.DataFormat.Json };
                var root = new { bet = poco };
                request.AddBody(root);
                response = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
            }, delegate
            {
                if (response.IsOk())
                {
                    //if (TryGetBetById(poco, out bet))
                        //bet.SetOutcome(outcome);
                }
                else
                {
                    string msg = response.Data.errors == null ? response.StatusCode.ToString() : string.Concat(response.Data.errors);
                    MessageDialog.Show(_window, "Failed to create bet", msg);
                }
            });
        }

        public void AcceptBet(Bet bet) 
        {
            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = _userData.PgSession.GetWebAppFunction("/api", "/game_rooms/" + GameRoom.Id + "/bets/" + bet.Id + ".json");
                var client = new RestClient(url);
                var request = new RestRequest(Method.PUT);
                response = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
            }, delegate
            {
                if (!response.IsOk())
                {
                    string msg = response.Data.errors == null ? response.StatusCode.ToString() : string.Concat(response.Data.errors);
                    MessageDialog.Show(_window, "Failed to accept bet", msg);
                }
            });
        }

        public void CancelBet(Bet bet) 
        {
            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = _userData.PgSession.GetWebAppFunction("/api", "/game_rooms/" + GameRoom.Id + "/bets/" + bet.Id + ".json");
                var client = new RestClient(url);
                var request = new RestRequest(Method.DELETE);
                response = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
            }, delegate
            {
                if (response.IsOk())
                {
                    if (MyMatch.Id == bet.MyMatch.Id && bet.MyMatch.State == MatchState.created)
                    {
                        _myMatch = new Match();
                    }
                }
                else
                {
                    string msg = response.Data.errors == null ? response.StatusCode.ToString() : string.Concat(response.Data.errors);
                    MessageDialog.Show(_window, "Failed to delete bet", msg);
                }
            });
        }

        private void CleanBets(string matchHash)
        {
            var removes = new List<Bet>();
            foreach (var item in RoomBets)
            {
                if (item.MatchHash == matchHash)
                    removes.Add(item);
            }
            foreach (var item in removes)
                RoomBets.Remove(item);
        }
        public void OnBetNew(BetPoco poco)
        {
            Bet bet = new Bet(_userData, MyMatch, poco);
            RoomBets.Add(bet);
            _myMatch = new Match();
        }
        public void OnBetTakerNew(BetPoco poco)
        {
            var acceptedBy = _userData.GetPgUser(new UserBase{ _id = poco.taker_id, username = poco.taker_username});

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
        #endregion
    }
}
