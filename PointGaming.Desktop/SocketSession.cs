using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PointGaming.Desktop.POCO;
using SocketIOClient;
using SocketIOClient.Messages;
using RestSharp;

namespace PointGaming.Desktop
{    
    public class SocketSession
    {
        public Client MyClient;
        private AuthEmit _authEmit;
        private ApiResponse _apiResponse;

        private Action OnAuthorizedCallback;

        public bool Login(string username, string password)
        {
            bool isSuccess = false;
            try
            {
                var baseUrl = Properties.Settings.Default.BaseUrl;
                var client = new RestClient(baseUrl);

                var request = new RestRequest("sessions", Method.POST);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(new UserLogin { username = username, password = password });

                var apiResponse = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
                isSuccess = apiResponse.Data.success;

                if (isSuccess)
                {
                    Persistence.AuthToken = apiResponse.Data.auth_token;
                    Persistence.loggedInUsername = username;
                    Properties.Settings.Default.Username = username;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception e)
            {
                App.LogLine(e.Message);
            }

            return isSuccess;
        }

        public void Logout()
        {
            Disconnect();
            DestroySession();
        }

        private static void DestroySession()
        {
            try
            {
                var baseUrl = Properties.Settings.Default.BaseUrl + "sessions/destroy?auth_token=" + Persistence.AuthToken;
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.DELETE);
                var apiResponse = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
                var isSuccess = apiResponse.Data.success;
            }
            catch (Exception e)
            {
                App.LogLine(e.Message);
            }

            Persistence.AuthToken = String.Empty;
            Persistence.loggedInUsername = String.Empty;

            App.LogLine("Logged in session ended.");
        }

        public void ConnectSocket(Action OnAuthorized)
        {
            OnAuthorizedCallback = OnAuthorized;

            MyClient = new Client(Properties.Settings.Default.SocketIoUrl);
            MyClient.On("connect", OnConnect);
            MyClient.On("auth_resp", OnAuthResponse);

            App.LogLine("Client socket connecting...");
            MyClient.Connect();
        }

        private void Disconnect()
        {
            try
            {
                MyClient.Close();
                MyClient = null;
            }
            catch (Exception e)
            {
                App.LogLine(e.Message);
            }

            App.LogLine("Socket disconnected.");
        }

        private void OnConnect(IMessage message)
        {
            try
            {
                _authEmit = new AuthEmit { auth_token = Persistence.AuthToken };
                MyClient.Emit("auth", _authEmit);
            }
            catch (Exception e)
            {
                App.LogLine(e.Message);
            }

            App.LogLine("Client socket connected.");
        }
        
        private void OnAuthResponse(IMessage data)
        {
            try
            {
                _apiResponse = data.Json.GetFirstArgAs<ApiResponse>();
            }
            catch (Exception ex)
            {
                App.LogLine(ex.Message);
            }

            App.LogLine("Client socket authorized.");

            OnAuthorizedCallback();
        }
    }
}
