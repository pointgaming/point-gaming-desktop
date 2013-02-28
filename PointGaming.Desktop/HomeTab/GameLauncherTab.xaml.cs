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

namespace PointGaming.Desktop.HomeTab
{
    public partial class GameLauncherTab : UserControl
    {
        private readonly ObservableCollection<LauncherInfo> _launchers = new ObservableCollection<LauncherInfo>();
        public ObservableCollection<LauncherInfo> Launchers { get { return _launchers; } }

        public GameLauncherTab()
        {
            InitializeComponent();

            var settingsList = Properties.Settings.Default.LaunchList;
            if (settingsList == null)
                settingsList = new System.Collections.Specialized.StringCollection();

            foreach (var launcherString in settingsList)
            {
                var launcher = LauncherInfo.FromString(launcherString);
                launcher.PropertyChanged += launcher_PropertyChanged;
                _launchers.Add(launcher);
            }

            if (_launchers.Count == 0)
            {
                var launcher = new LauncherInfo("Firefox", "C:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe", "http://www.shadowstats.com/charts/monetary-base-money-supply");
                launcher.PropertyChanged += launcher_PropertyChanged;
                _launchers.Add(launcher);
                launcher = new LauncherInfo("Internet Explorer", "C:\\Program Files\\Internet Explorer\\iexplore.exe", "http://www.shadowstats.com/charts/monetary-base-money-supply");
                launcher.PropertyChanged += launcher_PropertyChanged;
                _launchers.Add(launcher);
                launcher = new LauncherInfo("Windows Registry", "C:\\Windows\\regedit.exe", "");
                launcher.PropertyChanged += launcher_PropertyChanged;
                _launchers.Add(launcher);
                launcher = new LauncherInfo("Notepad", "C:\\Windows\\notepad.exe", "C:\\test.txt");
                launcher.PropertyChanged += launcher_PropertyChanged;
                _launchers.Add(launcher);
            }

            _launchers.CollectionChanged += _launchers_CollectionChanged;
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
                settingsList.Add(item.ToString());
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
            _launchers.Remove(_rightClickLauncher);
            e.Handled = true;
        }

        private void dataGridLauncher_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                LauncherInfo launcher;
                if (dataGridLauncher.TryGetRowItem(e, out launcher))
                {
                    launcher.Launch();
                }
            }
        }
    }

}
