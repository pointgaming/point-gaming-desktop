using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace PointGaming.Settings
{
    public class WindowBoundsPersistor
    {
        private Rect _bounds;
        private Window _window;
        private string _windowName;
        public WindowBoundsPersistor(Window window, string windowName) 
        {
            _window = window;
            _window.LocationChanged += LocationChanged;
            _window.SizeChanged += SizeChanged;
            _windowName = windowName;
        }

        private void LocationChanged(object sender, EventArgs e)
        {
            if (_window.WindowState != WindowState.Normal)
                return;
            _bounds.X = _window.Left;
            _bounds.Y = _window.Top;
        }
        private void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_window.WindowState != WindowState.Normal)
                return;
            _bounds.Width = _window.Width;
            _bounds.Height = _window.Height;
        }

        public void Save()
        {
            var rect = new Rect(_bounds.Left, _bounds.Top, _bounds.Width, _bounds.Height);
            var desktopInfo = GetDesktopInfo();
            var ras = new WindowPersistInfo
            {
                Name = _windowName,
                Left = rect.Left,
                Top = rect.Top,
                Width = rect.Width,
                Height = rect.Height,
                ScreenWidth = desktopInfo.Width,
                ScreenHeight = desktopInfo.Height,
            };

            var saved = LoadAll();
            saved[_windowName] = ras;
            SaveAll(saved);
        }
        public void Load()
        {
            var saved = LoadAll();
            WindowPersistInfo ras;
            if (!saved.TryGetValue(_windowName, out ras))
                return;
            
            _bounds = new Rect(ras.Left, ras.Top, ras.Width, ras.Height);
            var desktopInfo = GetDesktopInfo();

            if (desktopInfo != new Size(ras.ScreenWidth, ras.ScreenHeight))
                return;

            _window.Left = _bounds.Left;
            _window.Top = _bounds.Top;
            _window.Width = _bounds.Width;
            _window.Height = _bounds.Height;
            _window.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        private static Dictionary<string, WindowPersistInfo> LoadAll()
        {
            var saved = new Dictionary<string, WindowPersistInfo>();
            var savedList = UserDataManager.UserData.Settings.WindowBounds;
            foreach (var item in savedList)
                saved[item.Name] = item;
            return saved;
        }

        private static void SaveAll(Dictionary<string, WindowPersistInfo> sizes)
        {
            var nsizes = new List<WindowPersistInfo>(sizes.Values);
            if (UserDataManager.UserData != null)
            {
                UserDataManager.UserData.Settings.WindowBounds = nsizes;
                UserDataManager.UserData.Settings.Save();
            }
        }

        private Size GetDesktopInfo()
        {
            return new Size
            {
                Width = System.Windows.SystemParameters.VirtualScreenWidth,
                Height = System.Windows.SystemParameters.VirtualScreenHeight
            };
        }
    }

    public class WindowPersistInfo
    {
        public string Name { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double ScreenWidth { get; set; }
        public double ScreenHeight { get; set; }
    }
}
