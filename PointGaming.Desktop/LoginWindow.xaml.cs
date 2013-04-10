﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PointGaming.Desktop.POCO;
using RestSharp;
using System.Threading;
using System.Net;
using System.Diagnostics;

namespace PointGaming.Desktop
{
    public partial class LoginWindow : Window
    {
        public bool IsLoggedIn;
        public SocketSession SocketSession;

        public LoginWindow()
        {
            HomeWindow.GuiThreadId = Thread.CurrentThread.ManagedThreadId;

            InitializeComponent();

            App.LoggedOut();

            SocketSession = new SocketSession();
            SocketSession.SetThreadQueuerForCurrentThread(this.BeginInvokeUI);

            RestResponse<PgVersion> response = null;

            SocketSession.BeginAndCallback(delegate
            {
                var url = "https://dev.pointgaming.com/desktop_client/version";
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                response = (RestResponse<PgVersion>)client.Execute<PgVersion>(request);
            }, delegate
            {
                if (response.IsOk())
                {
                    var latestVersion = response.Data.version;
                    var assemblyVersion = App.Version;
                    Version latestVersionV = new Version(latestVersion);
                    if (latestVersionV > assemblyVersion)
                        RunUpdate();
                }
            });
        }

        private void RunUpdate()
        {
            var updateFileName = "updater.msp";
            var tempPath = System.IO.Path.GetTempPath() + "\\" + updateFileName;
            try
            {
                using (WebClient Client = new WebClient())
                {
                    Client.DownloadFile("https://dev.pointgaming.com/desktop_client/updater.msp", tempPath);

                    var processName = Process.GetCurrentProcess().ProcessName;

                    var dllLocation = typeof(App).Assembly.Location;
                    var dllFileInfo = new System.IO.FileInfo(dllLocation);
                    var runAfterUpdate = dllFileInfo.Name;

                    var dirInfo = App.ExecutableDirectoryInfo;
                    var executableInfo = dirInfo.GetFiles("PointGaming.Desktop.Update.exe")[0];

                    Process updateInvoker = new Process();
                    updateInvoker.StartInfo.FileName = executableInfo.FullName;
                    updateInvoker.StartInfo.Arguments = BuildArguments(processName, updateFileName, runAfterUpdate);
                    updateInvoker.StartInfo.UseShellExecute = false;
                    updateInvoker.StartInfo.RedirectStandardOutput = true;
                    updateInvoker.Start();
                    Close();
                    // ... update installer installs update, then restarts the desktop client
                }
            }
            catch (Exception e)
            {
                App.LogLine(e.Message);
                // todo handle error
            }
        }

        private static string BuildArguments(params string[] arguments)
        {
            var result = "";
            foreach (var arg in arguments)
            {
                if (result.Length > 0)
                    result = result + " ";

                var a = arg.Replace("\"", "\"\"");
                result = result + "\"" + a + "\"";
            }
            return result;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string lastUsername = Properties.Settings.Default.Username;
            if (string.IsNullOrWhiteSpace(lastUsername))
            {
                textBoxUsername.Text = "";
                textBoxUsername.Focus();
            }
            else
            {
                textBoxUsername.Text = lastUsername;
                passwordBoxPassword.Focus();

                string lastPasswordEncrypted = Properties.Settings.Default.Password;
                if (!string.IsNullOrWhiteSpace(lastPasswordEncrypted))
                {
                    string lastPassword;
                    if (lastPasswordEncrypted.TryDecrypt(out lastPassword))
                    {
                        checkBoxRememberPassword.IsChecked = true;
                        passwordBoxPassword.Password = lastPassword;
                        LogIn();
                    }
                }
            }
        }

        private void LogIn()
        {
            var username = textBoxUsername.Text.Trim();
            var password = passwordBoxPassword.Password.Trim();

            if (username == "")
            {
                labelResult.Foreground = Brushes.Red;
                labelResult.Content = "Enter username";
                textBoxUsername.Focus();
                return;
            }
            if (password == "")
            {
                labelResult.Foreground = Brushes.Red;
                labelResult.Content = "Enter password";
                passwordBoxPassword.Focus();
                return;
            }

            labelResult.Foreground = Brushes.Black;
            labelResult.Content = "Logging in...";
            gridControls.IsEnabled = false;
            
            passwordBoxPassword.Clear();

            DateTime timeout = DateTime.Now + Properties.Settings.Default.LogInTimeout;

            bool isSuccess = false;
            SocketSession.BeginAndCallback(delegate
            {
                isSuccess = SocketSession.Login(username, password, timeout);
            }, delegate
            {
                if (isSuccess)
                {
                    IsLoggedIn = true;

                    App.LoggedIn(username);

                    var homeWindow = new HomeWindow();
                    SocketSession.SetThreadQueuerForCurrentThread(HomeWindow.Home.BeginInvokeUI);
                    homeWindow.Init(SocketSession);
                    Hide();
                    homeWindow.Show();

                    Properties.Settings.Default.Username = username;
                    if (checkBoxRememberPassword.IsChecked == true)
                        Properties.Settings.Default.Password = password.Encrypt();
                    else
                        Properties.Settings.Default.Password = "";
                    Properties.Settings.Default.Save();

                    Close();
                }
                else
                {
                    var message = "Invalid username or password";
                    if (DateTime.Now >= timeout)
                        message = "Try again later.  No response within timeout period.";
                    labelResult.Foreground = Brushes.Red;
                    labelResult.Content = message;
                    gridControls.IsEnabled = true;
                    passwordBoxPassword.Focus();
                }
            });
        }

        
        private void textBoxUsername_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                passwordBoxPassword.Password = "";
                passwordBoxPassword.Focus();
                return;
            }
        }

        private void passwordBoxPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                LogIn();
                return;
            }
        }

        private void buttonLogIn_Click(object sender, RoutedEventArgs e)
        {
            LogIn();
        }

        private void windowLogin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsLoggedIn)
            {
                App.IsShuttingDown = true;
            }
        }

        private void buttonQuit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
