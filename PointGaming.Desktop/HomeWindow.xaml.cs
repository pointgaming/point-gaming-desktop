using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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

        private readonly List<Window> _childWindows = new List<Window>();

        public SocketSession SocketSession;
        private Chat.ChatManager _chatManager;

        public HomeWindow()
        {
            InitializeComponent();
            Home = this;
            new HomeWindowBoundsPersistor().Load(this);
        }

        public void Init(SocketSession session)
        {
            SocketSession = session;
            
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
                Header = "Bitcoin",
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

            _chatManager = new Chat.ChatManager();
            _chatManager.Init(SocketSession);
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            UpdateMinimizeToTray();
        }

        public void UpdateMinimizeToTray()
        {
            bool isEnabled = Properties.Settings.Default.MinimizeToTray;
            if (!isEnabled && !IsVisible)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }
            taskbarIcon.Visibility = isEnabled ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            ShowInTaskbar = !isEnabled;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (Properties.Settings.Default.MinimizeToTray)
            {
                if (WindowState == WindowState.Minimized)
                    this.Hide();
            }

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
        public void ChatWith(PgUser friend)
        {
            _chatManager.ChatWith(friend);
        }
        public void JoinChat(string id)
        {
            _chatManager.JoinChatroom(id);
        }

        public Chat.ChatWindow GetChatWindow()
        {
            return _chatManager.ChatWindow;
        }

        public void LogOut(bool shouldShowLogInWindow)
        {
            if (SocketSession == null)
                return;

            taskbarIcon.Visibility = System.Windows.Visibility.Collapsed;
            
            var childWindows = new List<Window>(_childWindows);
            foreach (var window in childWindows)
                window.Close();
            _childWindows.Clear();

            App.DebugBox = null;

            Hide();

            SocketSession.Data.LoggedOut();
            SocketSession.Begin(SocketSession.Logout);

            SocketSession = null;
            _chatManager = null;

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
            _windowClosing = true;
            LogOut(false);
            new HomeWindowBoundsPersistor().Save(this);
        }

        private class HomeWindowBoundsPersistor : WindowBoundsPersistor
        {
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
    }
}


