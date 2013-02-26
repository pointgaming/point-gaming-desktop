﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
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

        private bool _isClosing;
        private readonly List<Window> _childWindows = new List<Window>();

        private SocketSession _socketSession;
        private Chat.ChatManager _chatManager;

        public static UserDataManager UserDataManager = new UserDataManager();

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

            if (!_windowClosing)
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
                this.BeginInvokeUI(ShowLogInWindow);
            }
        }

        private void ShowLogInWindow()
        {
            var lw = new LoginWindow();
            _childWindows.Add(lw);
            lw.ShowDialog();

            if (lw.IsLoggedIn)
            {
                _socketSession = lw.SocketSession;
                friendTabInstance.OnAuthorized(_socketSession);
                _chatManager = new Chat.ChatManager();
                _chatManager.OnAuthorized(_socketSession);
            }
            else
            {
                CloseProgram();
            }
        }

        public void LogOut()
        {
            if (_socketSession == null)
                return;

            UserDataManager.LoggedOut();

            var childWindows = new List<Window>(_childWindows);
            foreach (var window in childWindows)
                window.Close();
            _childWindows.Clear();

            friendTabInstance.LoggedOut();
            _chatManager = null;

            _socketSession.Begin(_socketSession.Logout);
            _socketSession = null;

            if (!_windowClosing)
                ShowLogInWindow();
        }

        public void AddChildWindow(Window window)
        {
            _childWindows.Add(window);
        }
        public void RemoveChildWindow(Window window)
        {
            _childWindows.Remove(window);
        }

        private bool _windowClosing;

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _windowClosing = true;
            LogOut();
        }

        public void ChatWith(HomeTab.FriendUiData friend)
        {
            _chatManager.ChatWith(friend);
        }
    }

}
