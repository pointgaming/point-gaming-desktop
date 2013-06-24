using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;
using PointGaming.POCO;
using RestSharp;
using SocketIOClient;
using SocketIOClient.Messages;

namespace PointGaming.HomeTab
{
    public partial class GameLauncherTab : UserControl
    {
        private UserDataManager _userData = HomeWindow.UserData;

        public ObservableCollection<LauncherInfo> Launchers { get { return _userData.Launchers; } }

        public GameLauncherTab()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var settingsList = Properties.Settings.Default.LaunchList;
            if (settingsList == null)
                settingsList = new System.Collections.Specialized.StringCollection();

            foreach (var launcherString in settingsList)
            {
                try
                {
                    var launcher = LauncherInfo.FromJson(launcherString);
                    AddOrUpdate(launcher);
                }
                catch { }
            }

            Launchers.CollectionChanged += _launchers_CollectionChanged;

            RestResponse<GameList> response = null;
            _userData.PgSession.BeginAndCallback(delegate
            {
                var url = _userData.PgSession.GetWebApiV1Function("/games");
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
                        var li = new LauncherInfo(game);
                        AddOrUpdate(li);
                    }
                }
            });
        }

        private void AddOrUpdate(LauncherInfo li)
        {
            foreach (var item in Launchers)
            {
                if (item.Id == li.Id)
                {
                    item.Update(li);
                    return;
                }
            }

            li.PropertyChanged += launcher_PropertyChanged;
            Launchers.Add(li);
        }

        void launcher_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveLauncherList();
        }

        void _launchers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveLauncherList();
        }

        private void SaveLauncherList()
        {
            var settingsList = new System.Collections.Specialized.StringCollection();
            foreach (var item in Launchers)
            {
                settingsList.Add(item.ToJson());
            }
            Properties.Settings.Default.LaunchList = settingsList;
            Properties.Settings.Default.Save();
        }

        private LauncherInfo _rightClickLauncher;
        private void dataGridLauncher_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                LauncherInfo launcher;
                if (dataGridLauncher.TryGetRowItem(e, out launcher))
                {
                    _rightClickLauncher = launcher;
                }
            }
        }

        private void AddLauncherClick(object sender, RoutedEventArgs e)
        {
            var editor = new LauncherEditorDialog();
            editor.Owner = HomeWindow.Home;
            editor.Title = "Add Launcher";
            var result = true == editor.ShowDialog();
            if (result)
            {
                var launcher = editor.Launcher;
                Launchers.Add(launcher);
                launcher.PropertyChanged += launcher_PropertyChanged;
            }
        }
        private void EditLauncherClick(object sender, RoutedEventArgs e)
        {
            var editor = new LauncherEditorDialog();
            editor.Owner = HomeWindow.Home;
            editor.Title = "Edit Launcher";
            editor.Launcher = _rightClickLauncher;
            var result = true == editor.ShowDialog();
            if (result)
                _rightClickLauncher.CopyFrom(editor.Launcher);
        }
        private void RemoveLauncherClick(object sender, RoutedEventArgs e)
        {
            if (_rightClickLauncher.IsOfficialGame)
                return;
            Launchers.Remove(_rightClickLauncher);
            e.Handled = true;
        }

        private void dataGridLauncher_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LauncherInfo launcher;
            if (dataGridLauncher.TryGetRowItem(e, out launcher))
                JoinLobby(launcher);
        }

        private void JoinLobbyClick(object sender, RoutedEventArgs e)
        {
            var launcher = ((FrameworkElement)sender).DataContext as LauncherInfo;
            JoinLobby(launcher);
        }

        private void JoinLobby(LauncherInfo launcher)
        {
            if (!launcher.IsOfficialGame)
                return;
            _userData.JoinChat(Chat.ChatManager.PrefixGameLobby + launcher.Id);
        }

        private void LaunchExecutableClick(object sender, RoutedEventArgs e)
        {
            var launcher = ((FrameworkElement)sender).DataContext as LauncherInfo;
            launcher.Launch();
        }
    }

}
