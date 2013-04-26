using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using PointGaming.Desktop.POCO;
using SocketIOClient;
using SocketIOClient.Messages;
using RestSharp;


namespace PointGaming.Desktop
{
    public partial class HomeWindow : Window
    {
        public static int GuiThreadId;
        public static HomeWindow Home;
        public static UserDataManager UserData;

        private readonly List<Window> _childWindows = new List<Window>();

        private WindowBoundsPersistor _windowBoundsPersistor;

        public HomeWindow()
        {
            InitializeComponent();
            Home = this;
            _windowBoundsPersistor = new HomeWindowBoundsPersistor(this);
            _windowBoundsPersistor.Load();
        }

        public void Init(SocketSession session)
        {
            UserData = new UserDataManager(session);
            
            var tab = new TabItem
            {
                Header = "Friends",
                Content = new HomeTab.FriendTab(),
            };
            tabControlMain.Items.Add(tab);

            tab = new TabItem
            {
                Header = "Games",
                Content = new HomeTab.GameLauncherTab(),
            };
            tabControlMain.Items.Add(tab);

            tab = new TabItem
            {
                Header = "Points",
                Content = new HomeTab.PaymentTab(),
            };
            tabControlMain.Items.Add(tab);

            tab = new TabItem
            {
                Header = "Settings",
                Content = new HomeTab.SettingsTab(),
            };
            tabControlMain.Items.Add(tab);

            tab = new TabItem
            {
                Header = "Debug",
                Content = new HomeTab.DebugTab(),
            };
            tabControlMain.Items.Add(tab);

            UserData.StartChat();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (!this.IsVisible)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
            }
            else
            {
                this.Hide();
            }
        }

        public void AddChildWindow(Window window)
        {
            _childWindows.Add(window);
        }
        public void RemoveChildWindow(Window window)
        {
            _childWindows.Remove(window);
        }
        
        public void LogOut(bool shouldShowLogInWindow)
        {
            if (UserData == null)
                return;

            _shouldClose = true;

            taskbarIcon.Visibility = System.Windows.Visibility.Collapsed;
            
            var childWindows = new List<Window>(_childWindows);
            foreach (var window in childWindows)
                window.Close();
            _childWindows.Clear();

            App.DebugBox = null;

            Hide();

            UserData.LoggedOut();
            UserData = null;

            if (shouldShowLogInWindow)
            {
                var lw = new LoginWindow();
                lw.Show();
            }
            else
            {
                App.IsShuttingDown = true;
            }

            if (!_windowClosing)
                Close();
        }
        private bool _windowClosing;
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_shouldClose)
            {
                e.Cancel = true;
                WindowState = System.Windows.WindowState.Minimized;
                return;
            }

            _windowClosing = true;
            LogOut(false);
            _windowBoundsPersistor.Save();
        }
        private void ExitClick(object sender, RoutedEventArgs e)
        {
            _shouldClose = true;
            Close();
        }
        private bool _shouldClose = false;

        private class HomeWindowBoundsPersistor : WindowBoundsPersistor
        {
            public HomeWindowBoundsPersistor(Window window) : base(window) { }

            protected override Rect GetBounds(out string oldDesktopInfo)
            {
                var r = new Rect(
                    Properties.Settings.Default.HomeWindowBoundsLeft,
                    Properties.Settings.Default.HomeWindowBoundsTop,
                    Properties.Settings.Default.HomeWindowBoundsWidth,
                    Properties.Settings.Default.HomeWindowBoundsHeight
                );
                oldDesktopInfo = Properties.Settings.Default.HomeWindowBoundsDesktopInfo;
                return r;
            }
            protected override void SetBounds(Rect r, string desktopInfo)
            {
                Properties.Settings.Default.HomeWindowBoundsLeft = r.Left;
                Properties.Settings.Default.HomeWindowBoundsTop = r.Top;
                Properties.Settings.Default.HomeWindowBoundsWidth = r.Width;
                Properties.Settings.Default.HomeWindowBoundsHeight = r.Height;
                Properties.Settings.Default.HomeWindowBoundsDesktopInfo = desktopInfo;
            }
        }

        private void homeWindow_SourceInitialized(object sender, EventArgs e)
        {
            GlassExtender.ExtendGlassFrame(this, new Thickness(-1));
        }

    }

    public class GlassExtender
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("dwmapi.dll", PreserveSig = false)]
        static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
        [DllImport("dwmapi.dll", PreserveSig = false)]
        static extern bool DwmIsCompositionEnabled();
        struct MARGINS
        {
            public MARGINS(Thickness t)
            {
                Left = (int)t.Left;
                Right = (int)t.Right;
                Top = (int)t.Top;
                Bottom = (int)t.Bottom;
            }
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }

        public static void SetMinimizable(Window window, bool can)
        {
            const int GWL_STYLE = -16;
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            int windowLong = GetWindowLong(hwnd, GWL_STYLE);
            if (can)
                windowLong = (int)(windowLong | 0x00020000);
            else
                windowLong = (int)(windowLong & 0xfffdffff);
            SetWindowLong(hwnd, GWL_STYLE, windowLong);
        }
        public static void SetMaximizable(Window window, bool can)
        {
            const int GWL_STYLE = -16;
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            int windowLong = GetWindowLong(hwnd, GWL_STYLE);
            if (can)
                windowLong = (int)(windowLong | 0x00010000);
            else
                windowLong = (int)(windowLong & 0xfffeffff);
            SetWindowLong(hwnd, GWL_STYLE, windowLong);
        }

        public static bool ExtendGlassFrame(Window window, Thickness margin)
        {
            if (!DwmIsCompositionEnabled())
                return false;

            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            
            // Set the background to transparent from both the WPF and Win32 perspectives
            window.Background = Brushes.Transparent;
            HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = Colors.Transparent;

            MARGINS margins = new MARGINS(margin);
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
            return true;
        }
    }
}


