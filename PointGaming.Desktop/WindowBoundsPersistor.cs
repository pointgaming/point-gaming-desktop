using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace PointGaming.Desktop
{
    public abstract class WindowBoundsPersistor
    {
        private Rect _bounds;
        private Window _window;
        public WindowBoundsPersistor(Window window) 
        {
            _window = window;
            _window.LocationChanged += LocationChanged;
            _window.SizeChanged += SizeChanged;
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

            SetBounds(rect, desktopInfo);
            Properties.Settings.Default.Save();
        }
        public void Load()
        {
            string oldDesktopInfo;
            _bounds = GetBounds(out oldDesktopInfo);
            var desktopInfo = GetDesktopInfo();

            if (desktopInfo != oldDesktopInfo)
                return;

            _window.Left = _bounds.Left;
            _window.Top = _bounds.Top;
            _window.Width = _bounds.Width;
            _window.Height = _bounds.Height;
            _window.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        private string GetDesktopInfo()
        {
            return System.Windows.SystemParameters.VirtualScreenWidth + "x" + System.Windows.SystemParameters.VirtualScreenHeight;
        }

        protected abstract Rect GetBounds(out string oldDesktopInfo);
        protected abstract void SetBounds(Rect r, string desktopInfo);
    }
}
