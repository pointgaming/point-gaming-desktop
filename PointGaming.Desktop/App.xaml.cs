using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Documents;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Interop;
using System.Text;
using RestSharp;

namespace PointGaming.Desktop
{
    public partial class App : Application
    {
        public static TextBox DebugBox;

        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss zzz";

        public static bool IsShuttingDown = false;

        private static readonly string ApplicationSettingsPath;
        public static string LoginSettingsPath {get {
            return ApplicationSettingsPath
                + Path.DirectorySeparatorChar
                + PointGaming.Desktop.Properties.Settings.Default.Username;
        }}

        private static StreamWriter _logWriter;

        static App()
        {
            try
            {
                Assembly currentAssem = typeof(App).Assembly;
                object[] attribs = currentAssem.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
                string company = ((AssemblyCompanyAttribute)attribs[0]).Company;

                ApplicationSettingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    + Path.DirectorySeparatorChar
                    + company
                    + Path.DirectorySeparatorChar
                    + currentAssem.ManifestModule.Name;

                LoggedOut();
                
                StartConsole();
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not create user settings folder!  Details: " + e.Message);
            }
        }

        public static void LoggedOut()
        {
            OpenLogFile(ApplicationSettingsPath);
        }

        public static void LoggedIn()
        {
            OpenLogFile(LoginSettingsPath);
        }

        private static string _lastLogFolder = null;

        private static void OpenLogFile(string folder)
        {
            if (folder == _lastLogFolder)
                return;
            _lastLogFolder = folder;

            try
            {
                var di = new DirectoryInfo(folder);
                if (!di.Exists)
                    di.Create();

                var fs = File.Open(folder + "\\debug.log", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                fs.Seek(0, SeekOrigin.End);
                var logWriter = new StreamWriter(fs, Encoding.UTF8);

                var logWriterOld = _logWriter;
                _logWriter = logWriter;
                if (logWriterOld != null)
                    logWriterOld.Close();
            }
            catch { }
        }

        public static void LogLine(string message)
        {
            Console.WriteLine(DateTime.Now.ToString(DateTimeFormat) + " " + message);
        }



        private static void StartConsole()
        {
            var t = new Thread(ConsoleThread)
            {
                Name = "MainWindow.Console",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            t.Start();
        }

        private static void ConsoleThread()
        {
            var ms = new SynchronizedMemoryStream();
            var sw = new StreamWriter(ms);

            Console.SetOut(sw);

            while (!App.IsShuttingDown)
            {

                Thread.Sleep(250);
                string text;

                lock (ms.WriteSynch)
                {
                    sw.Flush();
                    long end = ms.Position;
                    ms.Position = 0;

                    text = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)end);
                }

                if (text.Length > 0 && _logWriter != null)
                {
                    _logWriter.Write(text);
                    _logWriter.Flush();
                }

                if (DebugBox != null)
                {
                    HomeWindow.Home.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<string>(delegate(string s)
                    {
                        if (DebugBox == null)
                            return;
                        DebugBox.AppendText(s);
                        if (!DebugBox.IsKeyboardFocused)
                            DebugBox.ScrollToEnd();
                    }), text);
                }
            }
        }


        class SynchronizedMemoryStream : MemoryStream
        {
            public readonly object WriteSynch = new object();
            public override void Write(byte[] buffer, int offset, int count)
            {
                lock (WriteSynch)
                {
                    base.Write(buffer, offset, count);
                }
            }
            public override void WriteByte(byte value)
            {
                lock (WriteSynch)
                {
                    base.WriteByte(value);
                }
            }
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
            DataGridRow row;
            var element = d.InputHitTest(e.GetPosition(d));
            return TryGetX(element, out row, out rowItem);
        }
        public static bool TryGetRowAndItem<T>(this DataGrid d, MouseButtonEventArgs e, out DataGridRow row, out T rowItem)
        {
            var element = d.InputHitTest(e.GetPosition(d));
            return TryGetX(element, out row, out rowItem);
        }
        public static bool TryGetItemAndItem<T>(this ListBox d, MouseButtonEventArgs e, out ListBoxItem item, out T itemItem)
        {
            var element = d.InputHitTest(e.GetPosition(d));
            return TryGetX(element, out item, out itemItem);
        }

        private static bool TryGetX<T>(IInputElement element, out ListBoxItem item, out T friend)
        {
            friend = default(T);

            if (!TryGetParent((DependencyObject)element, out item))
                return false;

            friend = (T)item.DataContext;
            return true;
        }

        private static bool TryGetX<T>(IInputElement element, out DataGridRow row, out T friend)
        {
            friend = default(T);

            if (!TryGetParent((DependencyObject)element, out row))
                return false;

            friend = (T)row.Item;
            return true;
        }

        public static bool TryGetParent<T>(this DependencyObject element, out T parent) where T : DependencyObject
        {
            parent = default(T);

            if (element != null)
                element = VisualTreeHelper.GetParent(element);

            while (element != null)
            {
                if (element is T)
                {
                    parent = (T)element;
                    return true;
                }
                element = VisualTreeHelper.GetParent(element);
            }
            return false;
        }

        public static bool TryGetPresentedParent<T>(this DependencyObject element, out T parent) where T : class
        {
            parent = default(T);
            ContentPresenter presenter;
            if (element.TryGetParent(out presenter))
            {
                parent = presenter.Content as T;
                if (parent != null)
                    return true;

                return TryGetPresentedParent((DependencyObject)presenter, out parent);
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

        public static void SetPointGamingDefaults(this FlowDocument doc)
        {
            doc.Background = System.Windows.Media.Brushes.White;
            doc.PagePadding = new Thickness(2);
            var textBlock = new TextBlock();
            doc.FontFamily = textBlock.FontFamily;
            doc.FontSize = textBlock.FontSize;
        }
    }
}
