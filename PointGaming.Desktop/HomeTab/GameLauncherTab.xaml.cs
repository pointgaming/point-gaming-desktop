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
using PointGaming.Desktop.POCO;
using RestSharp;
using SocketIOClient;
using SocketIOClient.Messages;

namespace PointGaming.Desktop.HomeTab
{
    public partial class GameLauncherTab : UserControl
    {
        private SocketSession _session = HomeWindow.Home.SocketSession;

        private readonly ObservableCollection<LauncherInfo> _launchers = new ObservableCollection<LauncherInfo>();
        public ObservableCollection<LauncherInfo> Launchers { get { return _launchers; } }

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

            if (_launchers.Count == 0)
            {
                var launcher = new LauncherInfo("Firefox", "C:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe", "http://www.shadowstats.com/charts/monetary-base-money-supply");
                AddOrUpdate(launcher);
                launcher = new LauncherInfo("Internet Explorer", "C:\\Program Files\\Internet Explorer\\iexplore.exe", "http://www.shadowstats.com/charts/monetary-base-money-supply");
                AddOrUpdate(launcher);
                launcher = new LauncherInfo("Windows Registry", "C:\\Windows\\regedit.exe", "");
                AddOrUpdate(launcher);
                launcher = new LauncherInfo("Notepad", "C:\\Windows\\notepad.exe", "C:\\test.txt");
                AddOrUpdate(launcher);
            }

            _launchers.CollectionChanged += _launchers_CollectionChanged;

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
                        var li = new LauncherInfo(game);
                        AddOrUpdate(li);
                    }
                }
            });
        }

        private void AddOrUpdate(LauncherInfo li)
        {
            foreach (var item in _launchers)
            {
                if (item.Id == li.Id)
                {
                    item.Update(li);
                    return;
                }
            }

            li.PropertyChanged += launcher_PropertyChanged;
            _launchers.Add(li);
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
            foreach (var item in _launchers)
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
                _launchers.Add(launcher);
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
            _launchers.Remove(_rightClickLauncher);
            e.Handled = true;
        }

        private void dataGridLauncher_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {}

        private void buttonJoinLobbyClick(object sender, RoutedEventArgs e)
        {
            var launcher = ((FrameworkElement)sender).DataContext as LauncherInfo;
            if (!launcher.IsOfficialGame)
            {
                MessageDialog.Show(HomeWindow.Home, "Lobby doesn't exist", "Lobby doesn't exist for " + launcher.DisplayName + "!");
                return;
            }
            HomeWindow.Home.JoinChat("lobby_" + launcher.Id);
        }

        private void buttonLaunchExecutableClick(object sender, RoutedEventArgs e)
        {
            var launcher = ((FrameworkElement)sender).DataContext as LauncherInfo;
            launcher.Launch();
        }
    }

}
