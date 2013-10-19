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
using Newtonsoft.Json;

namespace PointGaming.HomeTab
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LauncherInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged(string propertyName)
        {
            var changedCallback = PropertyChanged;
            if (changedCallback == null)
                return;
            var args = new PropertyChangedEventArgs(propertyName);
            changedCallback(this, args);
        }

        private string _id;
        [JsonProperty]
        public string Id
        {
            get { return _id; }
            set
            {
                if (value == _id)
                    return;
                _id = value;
                NotifyChanged("Id");
            }
        }

        private bool _isOfficialGame;
        [JsonProperty]
        public bool IsOfficialGame
        {
            get { return _isOfficialGame; }
            set
            {
                if (value == _isOfficialGame)
                    return;
                _isOfficialGame = value;
                NotifyChanged("IsOfficialGame");
            }
        }

        private string _displayName;
        [JsonProperty]
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                if (value == _displayName)
                    return;
                if (_displayName != null && _isOfficialGame)
                    throw new Exception("Display name is read only");

                _displayName = value;
                NotifyChanged("DisplayName");
            }
        }

        private string _filePath;
        [JsonProperty]
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (value == _filePath)
                    return;
                _filePath = value;
                _fileIcon = GetIcon(value, DisplayName);
                NotifyChanged("FilePath");
                NotifyChanged("FileIcon");
            }
        }

        private string _arguments;
        [JsonProperty]
        public string Arguments
        {
            get { return _arguments; }
            set
            {
                if (value == _arguments)
                    return;
                _arguments = value;
                NotifyChanged("Arguments");
            }
        }

        private int _playerCount;
        public int PlayerCount
        {
            get { return _playerCount; }
            set
            {
                if (value == _playerCount)
                    return;
                _playerCount = value;
                NotifyChanged("PlayerCount");
            }
        }

        private ImageSource _fileIcon;
        public ImageSource FileIcon { get { return _fileIcon; } }


        public LauncherInfo()
        {
        }

        public LauncherInfo(LauncherInfo other)
        {
            CopyFrom(other);
        }

        public LauncherInfo(POCO.GamePoco poco)
        {
            Id = poco._id;
            DisplayName = poco.name;
            PlayerCount = poco.player_count;

            IsOfficialGame = true;
            FilePath = "";
            Arguments = "";
        }

        public LauncherInfo(string displayName, string filePath, string arguments)
        {
            Id = "user_" + Guid.NewGuid().ToString().Replace("-", "");
            DisplayName = displayName;
            PlayerCount = 0;

            IsOfficialGame = false;
            FilePath = filePath;
            Arguments = arguments;
        }


        public LauncherInfo CopyFrom(LauncherInfo other)
        {
            Id = other.Id;
            DisplayName = other.DisplayName;
            PlayerCount = other.PlayerCount;

            IsOfficialGame = other.IsOfficialGame;
            FilePath = other.FilePath;
            Arguments = other.Arguments;

            return this;
        }

        public void Update(LauncherInfo other)
        {
            if (Id != other.Id)
                throw new Exception("Ids don't match");
            if (IsOfficialGame != other.IsOfficialGame)
                throw new Exception("IsOfficialGame don't match");

            DisplayName = other.DisplayName;
            PlayerCount = other.PlayerCount;

            if (!IsOfficialGame)
            {
                FilePath = other.FilePath;
                Arguments = other.Arguments;
            }
        }

        public void Launch()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                MessageDialog.Show(HomeWindow.Home, "Launcher not Setup", "Right click on the game and choose edit.  Then set the executable.");
                return;
            }
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(FilePath, Arguments);
                info.UseShellExecute = false;
                Process.Start(info);
            }
            catch (Exception e)
            {
                MessageDialog.Show(HomeWindow.Home, "Error Launching", e.Message);
            }
        }

        public static ImageSource GetIcon(string filePath, string appDisplayName = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return GetDefaultIcon(appDisplayName);

            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(filePath);
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            new Int32Rect(0, 0, icon.Width, icon.Height),
                            BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                return GetDefaultIcon(appDisplayName);
            }
        }

        private static ImageSource GetDefaultIcon(string appDisplayName)
        {
            var assembly = typeof(LauncherInfo).Assembly;
            var defaultUri = "pack://application:,,,/" + assembly.GetName().Name + ";component/PointGaming.ico";

            switch (appDisplayName)
            {
                case "Quake Live":
                    defaultUri = "pack://application:,,,/" + assembly.GetName().Name + ";component/Resources/Logos/quake-live-logo.png";
                    break;
                case "StarCraft II":
                    defaultUri = "pack://application:,,,/" + assembly.GetName().Name + ";component/Resources/Logos/SC2 Logo.png";
                    break;
                case "League of Legends":
                    defaultUri = "pack://application:,,,/" + assembly.GetName().Name + ";component/Resources/Logos/League_of_Legends.png";
                    break;
                case "Counter-Strike: Go":
                    defaultUri = "pack://application:,,,/" + assembly.GetName().Name + ";component/Resources/Logos/counter strike logo.jpg";
                    break;
                case "Counter-Strike: Source":
                    defaultUri = "pack://application:,,,/" + assembly.GetName().Name + ";component/Resources/Logos/counter strike logo.jpg";
                    break;
                case "ShootMania":
                    defaultUri = "pack://application:,,,/" + assembly.GetName().Name + ";component/Resources/Logo/ShootMania.jpg";
                    break;
            }

            var source = new ImageSourceConverter().ConvertFromString(defaultUri) as ImageSource;
            return source;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
