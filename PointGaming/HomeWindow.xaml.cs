﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using PointGaming.POCO;
using SocketIOClient;
using SocketIOClient.Messages;
using RestSharp;


namespace PointGaming
{
    public partial class HomeWindow : Window
    {
        public static int GuiThreadId;
        public static HomeWindow Home;

        public WindowTreeManager WindowTreeManager;
        private HomeTab.PaymentTab _paymentTab;
        
        public HomeWindow()
        {
            InitializeComponent();
            Home = this;
            WindowTreeManager = new WindowTreeManager(this, null, false);
        }

        public void Init()
        {            
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

            _paymentTab = new HomeTab.PaymentTab();
            tab = new TabItem
            {
                Header = "Better",
                Content = _paymentTab,
            };
            tabControlMain.Items.Add(tab);

            tab = new TabItem
            {
                Header = "Debug",
                Content = new HomeTab.DebugTab(),
            };
            tabControlMain.Items.Add(tab);

            var b = new System.Windows.Data.Binding("Username");
            b.Mode = System.Windows.Data.BindingMode.OneWay;
            b.Source = UserDataManager.UserData.User;
            menuItemUsername.SetBinding(MenuItem.HeaderProperty, b);

            UserDataManager.UserData.StartChat();
        }

        private void ProfileClick(object sender, EventArgs e)
        {
            UserDataManager.UserData.User.ViewProfile();
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
                
        public void LogOut(bool showLoginWindow, bool isShutDown, bool isFromWindowClosingEvent)
        {
            if (UserDataManager.UserData == null)
                return;

            _allowClose = true;

            taskbarIcon.Visibility = System.Windows.Visibility.Collapsed;

            WindowTreeManager.ManualClosing();
            
            _paymentTab.LoggingOut();

            App.DebugBox = null;

            Hide();

            UserDataManager.UserData.LoggedOut();
            UserDataManager.UserData = null;

            if (showLoginWindow)
            {
                var lw = new LoginWindow();
                lw.Show();
            }

            if (isShutDown)
                App.IsShuttingDown = true;

            if (!isFromWindowClosingEvent)
                Close();

            if (isShutDown)
                WaitForThreadsToDie();
        }

        private static void WaitForThreadsToDie()
        {
            var now = DateTime.UtcNow;
            while ((DateTime.UtcNow - now).TotalMilliseconds < 200)
                Thread.Sleep(50);
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_allowClose)
            {
                e.Cancel = true;
                WindowState = System.Windows.WindowState.Minimized;
                return;
            }

            LogOut(false, true, true);
        }
        private void ExitClick(object sender, RoutedEventArgs e)
        {
            _allowClose = true;
            Close();
        }
        private bool _allowClose = false;

        private void homeWindow_SourceInitialized(object sender, EventArgs e)
        {
            //GlassExtender.ExtendGlassFrame(this, new Thickness(-1));
        }

        private void LogOutClick(object sender, RoutedEventArgs e)
        {
            App.Settings.Password = "";
            App.Settings.Save();
            LogOut(true, false, false);
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void signedInAsLabel_GotMouseCapture(object sender, MouseEventArgs e)
        {

        }

        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            Settings.SettingsWindow.ShowDialog(WindowTreeManager, null);
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


