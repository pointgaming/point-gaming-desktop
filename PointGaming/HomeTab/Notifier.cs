using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;

namespace PointGaming.HomeTab
{
    class Notifier
    {
        private static NotificationsBox _window;
        public static void NotifyBannedUser(PgUser user, string gameId)
        {
            var window = GetNotificationsWindow();
            var url =  UserDataManager.UserData.PgSession.GetWebAppFunction("/api", "/user_bans", "user_id=" + user.Id, "game_id=" + gameId);
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET) { RequestFormat = RestSharp.DataFormat.Json };
            var response = (RestResponse<UserBanPoco>)client.Execute<UserBanPoco>(request);

            var time = TimeString(response.Data.period);
            window.AddMessage(response.Data.start_time.ToString());
            var messageText = "You have been banned for " + time + " by admin " + response.Data.owner.username;
            window.AddMessage(messageText);
            window.Show();
        }

        private static string TimeString(string period)
        {
            string result = "";
            switch (period)
            {
                case "0.5":
                    result = "30 minutes";
                    break;
                case "24":
                    result = "24 hours";
                    break;
                case "48":
                    result = "48 hours";
                    break;
                case "168":
                    result = "1 week";
                    break;
                default:
                    result = "6 hours";
                    break;
            }
            return result;
        }

        private static NotificationsBox GetNotificationsWindow()
        {
            if (_window == null)
            {
                _window = new NotificationsBox();
                _window.Closing += OnClosing;
            }
            return _window;
        }

        private static void OnClosing(object sender, EventArgs e)
        {
            _window = null;
        }

        class UserBanPoco
        {
            public string _id { get; set; }
            public PointGaming.POCO.UserBase owner { get; set; }
            public DateTime start_time { get; set; }
            public string period { get; set; }
        }
    }
}
