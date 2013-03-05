using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PointGaming.Desktop.POCO;
using RestSharp;
using SocketIOClient;
using SocketIOClient.Messages;

namespace PointGaming.Desktop.HomeTab
{
    public partial class LobbyListTab : UserControl
    {
        private SocketSession _session = HomeWindow.Home.SocketSession;

        public LobbyListTab()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RestResponse<GameList> response = null;
            _session.BeginAndCallback(delegate
            {
                var url = Properties.Settings.Default.Games + "?auth_token=" + _session.AuthToken;
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                response = (RestResponse<GameList>)client.Execute<GameList>(request);
            }, delegate
            {
                if (response.IsOk())
                {
                    var games = response.Data.games;

                    foreach (var game in games)
                    {
                        AddGame(game);
                    }
                }
            });
        }

        private void AddGame(GamePoco game)
        {
            var assembly = typeof(LauncherInfo).Assembly;
            var defaultUri = "pack://application:,,,/" + assembly.GetName().Name + ";component/PointGaming.ico";
            var source = new ImageSourceConverter().ConvertFromString(defaultUri) as ImageSource;

            LobbyChoice choice = new LobbyChoice
            {
                Image = source,
                DisplayName = game.name,
                Height = 30,
                DisplayHeight = 15,
                Margin = new Thickness(0, 2, 0, 2),
            };
            stackPanelChoices.Children.Add(choice);
        }
    }
}
