﻿using System;
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
            var url = Properties.Settings.Default.GameRooms + GameRoom.Id + "?auth_token=" + _userData.PgSession.AuthToken;
            var root = new { game_room = poco };
            RestResponse<GameRoomSinglePoco> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                response = SocketSession.Rest<GameRoomSinglePoco>(url, Method.PUT, root);
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

        #region match
        public void LoadMatch()
        {
            if (string.IsNullOrWhiteSpace(GameRoom.MatchId))
                return;

            var url = Properties.Settings.Default.Matches + GameRoom.MatchId + ".json?include_bets=true&auth_token=" + _userData.PgSession.AuthToken;

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

            var url = Properties.Settings.Default.GameRooms + GameRoom.Id + "/matches.json?auth_token=" + _userData.PgSession.AuthToken;
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
                    MessageDialog.Show(_userData.GetChatWindow(), "Failed to create match", "Sorry, failed to create match.\r\nDetails: " + response.ErrorMessage);
                }
            });
        }
        public void UpdateMatch(Match m)
        {
            MyMatch.IsEditable = false;

            var url = Properties.Settings.Default.Matches + m.Id + ".json?auth_token=" + _userData.PgSession.AuthToken;
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
                    MessageDialog.Show(_userData.GetChatWindow(), "Failed to update match", "Sorry, failed to update match.\r\nDetails: " + response.ErrorMessage);
                }
            });
        }
        public void StartMatch()
        {
            MyMatch.IsEditable = false;

            var url = Properties.Settings.Default.Matches + MyMatch.Id + "/start.json?auth_token=" + _userData.PgSession.AuthToken;
            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                response = SocketSession.Rest<ApiResponse>(url, Method.PUT, null);
            }, delegate
            {
                if (!response.IsOk())
                {
                    MyMatch.IsEditable = true;
                    MessageDialog.Show(_userData.GetChatWindow(), "Failed to start match", "Sorry, failed to start match.\r\nDetails: " + response.ErrorMessage);
                }
            });
        }
        public void CancelMatch()
        {
            MyMatch.IsEditable = false;

            var url = Properties.Settings.Default.Matches + MyMatch.Id + "/cancel.json?auth_token=" + _userData.PgSession.AuthToken;
            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                response = SocketSession.Rest<ApiResponse>(url, Method.PUT, null);
            }, delegate
            {
                if (!response.IsOk())
                {
                    MyMatch.IsEditable = true;
                    MessageDialog.Show(_userData.GetChatWindow(), "Failed to cancel match", "Sorry, failed to cancel match.\r\nDetails: " + response.ErrorMessage);
                }
            });
        }
        public void FinishMatch(IBetOperand winner)
        {
            MyMatch.IsEditable = false;

            var url = Properties.Settings.Default.Matches + MyMatch.Id + ".json?auth_token=" + _userData.PgSession.AuthToken;
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
                    MessageDialog.Show(_userData.GetChatWindow(), "Failed to finish match", "Sorry, failed to finish match.\r\nDetails: " + response.ErrorMessage);
                }
            });
        }

        public void OnMatchNew(MatchPoco poco)
        {
            MyMatch.Update(_userData, poco);
            MyMatch.IsEditable = true;
            CleanBets(poco.match_hash);
        }
        public void OnMatchUpdate(MatchPoco poco)
        {
            MyMatch.Update(_userData, poco);
            MyMatch.IsEditable = true;
            CleanBets(poco.match_hash);
        }
        #endregion

        #region bets
        public void CreateBet(Bet bet)
        {
            var poco = bet.ToPoco();

            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = Properties.Settings.Default.Matches + MyMatch.Id + "/bets?auth_token=" + _userData.PgSession.AuthToken;
                var client = new RestClient(url);
                var request = new RestRequest(Method.POST) { RequestFormat = RestSharp.DataFormat.Json };
                var root = new { bet = poco };
                request.AddBody(root);
                response = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
            }, delegate
            {
                if (!response.IsOk())
                {
                    MessageDialog.Show(_userData.GetChatWindow(), "Failed to create bet", 
                        string.Concat(response.Data.errors));
                }
            });
        }

        public void AcceptBet(Bet bet) 
        {
            RestResponse<ApiResponse> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = Properties.Settings.Default.Matches + MyMatch.Id + "/bets/" + bet.Id + ".json?auth_token=" + _userData.PgSession.AuthToken;
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
                var url = Properties.Settings.Default.Matches + MyMatch.Id + "/bets/" + bet.Id + ".json?auth_token=" + _userData.PgSession.AuthToken;
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