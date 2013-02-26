using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using RestSharp;

namespace PointGaming.Desktop
{
    public partial class App : Application
    {
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss zzz";

        public static bool IsShuttingDown = false;

        public static readonly string SettingsPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar
            + PointGaming.Desktop.Properties.Settings.Default.UserFolder;

        static App()
        {
            try
            {
                var di = new DirectoryInfo(SettingsPath);
                if (!di.Exists)
                    di.Create();
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not create user settings folder!  Details: " + e.Message);
            }
        }

        public static void LogLine(string message)
        {
            Console.WriteLine(DateTime.Now.ToString(DateTimeFormat) + " " + message);
        }
    }

    static class UIExtensionMethods
    {
        public static void BeginInvokeUI(this Control c, Action a)
        {
            try
            {
                c.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate()
                {
                    if (App.IsShuttingDown) return;
                    a();
                });
            }
            catch { }
        }
        public static void InvokeUI(this Control c, Action a)
        {
            if (Thread.CurrentThread.ManagedThreadId == HomeWindow.GuiThreadId)
            {
                a();
            }
            else
            {
                AutoResetEvent areSelect = new AutoResetEvent(false);
                try
                {
                    c.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        try
                        {
                            if (App.IsShuttingDown) return;
                            a();
                        }
                        finally
                        {
                            areSelect.Set();
                        }
                    });
                }
                catch
                {
                    areSelect.Set();
                }
                areSelect.WaitOne();
            }
        }




        public static bool TryGetRowItem<T>(this DataGrid d, MouseButtonEventArgs e, out T rowItem)
        {
            var element = d.InputHitTest(e.GetPosition(d));
            return TryGetX(element, out rowItem);
        }

        private static bool TryGetX<T>(IInputElement element, out T friend)
        {
            friend = default(T);

            DataGridRow row;
            if (!TryGetRow((DependencyObject)element, out row))
                return false;

            friend = (T)row.Item;
            return true;
        }

        private static bool TryGetRow(DependencyObject element, out DataGridRow row)
        {
            row = null;
            while (element != null)
            {
                if (element is DataGridRow)
                {
                    row = (DataGridRow)element;
                    return true;
                }
                element = VisualTreeHelper.GetParent(element);
            }
            return false;
        }
        
        public static T FindDescendant<T>(this DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) return default(T);
            int numberChildren = VisualTreeHelper.GetChildrenCount(obj);
            if (numberChildren == 0) return default(T);

            for (int i = 0; i < numberChildren; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                {
                    return (T)(object)child;
                }
            }

            for (int i = 0; i < numberChildren; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                var potentialMatch = FindDescendant<T>(child);
                if (potentialMatch != default(T))
                {
                    return potentialMatch;
                }
            }

            return default(T);
        }


        public static bool IsOk(this RestResponseBase response)
        {
            return response.StatusCode >= System.Net.HttpStatusCode.OK && (int)response.StatusCode <= 209;
        }

        #region Window Flashing API Stuff
 
        private const UInt32 FLASHW_STOP = 0; //Stop flashing. The system restores the window to its original state.
        private const UInt32 FLASHW_CAPTION = 1; //Flash the window caption.
        private const UInt32 FLASHW_TRAY = 2; //Flash the taskbar button.
        private const UInt32 FLASHW_ALL = 3; //Flash both the window caption and taskbar button.
        private const UInt32 FLASHW_TIMER = 4; //Flash continuously, until the FLASHW_STOP flag is set.
        private const UInt32 FLASHW_TIMERNOFG = 12; //Flash continuously until the window comes to the foreground.
 
        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public UInt32 cbSize; //The size of the structure in bytes.
            public IntPtr hwnd; //A Handle to the Window to be Flashed. The window can be either opened or minimized.
            public UInt32 dwFlags; //The Flash Status.
            public UInt32 uCount; // number of times to flash the window
            public UInt32 dwTimeout; //The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.
        }
 
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);
 
        #endregion
 
        public static void FlashWindow(this Window win, UInt32 count = UInt32.MaxValue)
        {
            WindowInteropHelper h = new WindowInteropHelper(win);
 
            FLASHWINFO info = new FLASHWINFO
            {
                hwnd = h.Handle,
                dwFlags = FLASHW_ALL | FLASHW_TIMER,
                uCount = count,
                dwTimeout = 1000
            };
 
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            FlashWindowEx(ref info);
        }
 
        public static void StopFlashingWindow(this Window win)
        {
            WindowInteropHelper h = new WindowInteropHelper(win);
 
            FLASHWINFO info = new FLASHWINFO();
            info.hwnd = h.Handle;
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            info.dwFlags = FLASHW_STOP;
            info.uCount = UInt32.MaxValue;
            info.dwTimeout = 1000;
 
            FlashWindowEx(ref info);
        }
    }
}
