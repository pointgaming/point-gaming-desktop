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
        public void Save(Window window)
        {
            var rect = new Rect(window.Left, window.Top, window.Width, window.Height);
            var desktopInfo = GetDesktopInfo();

            SetBounds(rect, desktopInfo);
            Properties.Settings.Default.Save();
        }
        public void Load(Window window)
        {
            string oldDesktopInfo;
            var rect = GetBounds(out oldDesktopInfo);
            var desktopInfo = GetDesktopInfo();

            if (desktopInfo != oldDesktopInfo)
                return;

            window.Left = rect.Left;
            window.Top = rect.Top;
            window.Width = rect.Width;
            window.Height = rect.Height;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        private string GetDesktopInfo()
        {
            return System.Windows.SystemParameters.VirtualScreenWidth + "x" + System.Windows.SystemParameters.VirtualScreenHeight;
        }

        protected abstract Rect GetBounds(out string oldDesktopInfo);
        protected abstract void SetBounds(Rect r, string desktopInfo);
    }
}
