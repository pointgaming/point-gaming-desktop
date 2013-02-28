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

        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                if (value == _displayName)
                    return;
                _displayName = value;
                NotifyChanged("DisplayName");
            }
        }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (value == _filePath)
                    return;
                _filePath = value;
                _fileIcon = GetIcon(value);
                NotifyChanged("FilePath");
                NotifyChanged("FileIcon");
            }
        }


        private string _arguments;
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

        public LauncherInfo(string displayName, string filePath, string arguments)
        {
            DisplayName = displayName;
            FilePath = filePath;
            Arguments = arguments;
            PlayerCount = 64;
        }

        public void CopyFrom(LauncherInfo other)
        {
            DisplayName = other.DisplayName;
            FilePath = other.FilePath;
            Arguments = other.Arguments;
            PlayerCount = other.PlayerCount;
        }

        public void Launch()
        {
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

        public static ImageSource GetIcon(string filePath)
        {
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
                var assembly = typeof(LauncherInfo).Assembly;
                var defaultUri = "pack://application:,,,/" + assembly.GetName().Name + ";component/PointGaming.ico";

                var source = new ImageSourceConverter().ConvertFromString(defaultUri) as ImageSource;
                return source;
            }
        }

        public override string ToString()
        {
            return DisplayName + "\t" + FilePath + "\t" + Arguments;
        }

        public static LauncherInfo FromString(string value)
        {
            try
            {
                var split = value.Split('\t');
                var li = new LauncherInfo(split[0], split[1], split[2]);
                return li;
            }
            catch
            {
                return new LauncherInfo("Failed to Import", "", value);
            }
        }
    }
}
