using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using PointGaming.Desktop.POCO;

namespace PointGaming.Desktop
{
    public partial class HomeWindow : Window
    {
        public static int GuiThreadId;
        public static HomeWindow Home;

        private bool _isClosing;
        private readonly List<Window> _childWindows = new List<Window>();

        public HomeWindow()
        {
            Home = this;
            GuiThreadId = Thread.CurrentThread.ManagedThreadId;

            InitializeComponent();
        }

        public void CloseProgram()
        {
            if (_isClosing)
                return;
            _isClosing = true;
            App.IsShuttingDown = true;

            foreach (var window in _childWindows)
                window.Close();

            Close();
        }

        private bool _once;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            if (!_once)
            {
                _once = true;
                this.BeginInvokeUI(ShowLogInWindow, true);
            }
        }

        private void ShowLogInWindow()
        {
            var lw = new LoginWindow();
            _childWindows.Add(lw);
            lw.ShowDialog();

            if (lw.IsLoggedIn)
            {
                LoggedIn();
            }
            else
            {
                CloseProgram();
            }
        }

        private void LoggedIn()
        {
            friendTabInstance.LoggedIn();
        }

        public void LoggedOut()
        {
            foreach (var window in _childWindows)
                window.Close();
            _childWindows.Clear();

            Persistence.AuthToken = String.Empty;
            Persistence.loggedInUsername = String.Empty;
            
            friendTabInstance.LoggedOut();

            ShowLogInWindow();
        }

        public void AddChildWindow(Window window)
        {
            _childWindows.Add(window);
        }
    }
}
