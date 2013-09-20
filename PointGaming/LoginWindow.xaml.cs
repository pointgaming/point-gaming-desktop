using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PointGaming.POCO;
using RestSharp;
using System.Threading;
using System.Net;
using System.Diagnostics;

namespace PointGaming
{
    public partial class LoginWindow : Window
    {
        public bool IsLoggedIn;
        public SocketSession SocketSession;
        public static LoginWindow Instance;

        public LoginWindow()
        {
            Instance = this;
            HomeWindow.GuiThreadId = Thread.CurrentThread.ManagedThreadId;

            InitializeComponent();

            App.LoggedOut();
        }

        private bool RunUpdate()
        {
            var updateFileName = "PoingGaming.msp";
            var tempPath = System.IO.Path.GetTempPath() + "\\" + updateFileName;
            try
            {
                using (WebClient Client = new WebClient())
                {
                    Client.DownloadFile(App.Settings.WebServerUrl + "/system/desktop_client/PointGaming.msp", tempPath);

                    var processName = Process.GetCurrentProcess().ProcessName;

                    var dllLocation = typeof(App).Assembly.Location;
                    var dllFileInfo = new System.IO.FileInfo(dllLocation);
                    var runAfterUpdate = dllFileInfo.Name;

                    var dirInfo = App.ExecutableDirectoryInfo;
                    var executableInfo = dirInfo.GetFiles("PointGaming.Update.exe")[0];

                    Process updateInvoker = new Process();
                    updateInvoker.StartInfo.FileName = executableInfo.FullName;
                    updateInvoker.StartInfo.Arguments = BuildArguments(processName, updateFileName, runAfterUpdate);
                    updateInvoker.StartInfo.UseShellExecute = false;
                    updateInvoker.StartInfo.RedirectStandardOutput = true;
                    updateInvoker.Start();
                    Close();

                    return true;
                    // ... update installer installs update, then restarts the desktop client
                }
            }
            catch (Exception e)
            {
                App.LogLine(e.Message);
                // todo handle error
            }
            return false;
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
            SocketSession = new SocketSession();
            SocketSession.SetThreadQueuerForCurrentThread(this.BeginInvokeUI);

            if (App.Settings.UpdateAutomatic)
                CheckForUpdate();
            else
                UseRememberedLogin();
        }

        private void CheckForUpdate()
        {
            SetWork("Checking for updates...", Brushes.Black, false);
            RestResponse<PgVersion> response = null;

            SocketSession.BeginAndCallback(delegate
            {
                var url = App.Settings.WebServerUrl + "/desktop_client/version";
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

                    bool isQuitForUpdate = false;
                    if (latestVersionV > assemblyVersion)
                        isQuitForUpdate = RunUpdate();

                    if (!isQuitForUpdate)
                        UseRememberedLogin();
                }
                else
                {
                    UseRememberedLogin();
                }
            });
        }

        private void UseRememberedLogin()
        {
            SetWork("Welcome.  Please login to continue.", Brushes.Black, true);

            string lastUsername = App.Settings.Username;
            if (string.IsNullOrWhiteSpace(lastUsername))
            {
                textBoxUsername.Text = "";
                textBoxUsername.Focus();
            }
            else
            {
                textBoxUsername.Text = lastUsername;
                passwordBoxPassword.Focus();

                string lastPasswordEncrypted = App.Settings.Password;
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

        public void ProgramaticallyLogIn(string username, string password)
        {
            textBoxUsername.Text = username;
            passwordBoxPassword.Password = password;
            LogIn();
        }

        private void LogIn()
        {
            var username = textBoxUsername.Text.Trim();
            var password = passwordBoxPassword.Password.Trim();

            if (username == "")
            {
                SetWork("Enter username", Brushes.Red, true);
                textBoxUsername.Focus();
                return;
            }
            if (password == "")
            {
                SetWork("Enter password", Brushes.Red, true);
                passwordBoxPassword.Focus();
                return;
            }

            DoLogin(username, password);
        }

        private void DoLogin(string username, string password)
        {
            var workMessage = "Logging in...";

            SetWork(workMessage, Brushes.Black, false);

            passwordBoxPassword.Clear();

            DateTime timeout = DateTime.Now + App.Settings.LogInTimeout;

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

                    var userData = new UserDataManager(SocketSession);
                    var homeWindow = new HomeWindow();
                    SocketSession.SetThreadQueuerForCurrentThread(HomeWindow.Home.BeginInvokeUI);
                    homeWindow.Init();
                    Hide();
                    homeWindow.Show();

                    App.Settings.Username = username;
                    if (checkBoxRememberPassword.IsChecked == true)
                        App.Settings.Password = password.Encrypt();
                    else
                        App.Settings.Password = "";
                    App.Settings.Save();

                    Close();

                    foreach (var action in _loginSuccessActions)
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Login success action threw Exception: " + e.Message);
                            Console.WriteLine(e.StackTrace);
                        }
                    }
                }
                else
                {
                    var message = "Invalid username or password";
                    if (DateTime.Now >= timeout)
                        message = "Try again later.  No response within timeout period.";

                    SetWork(message, Brushes.Red, true);
                    passwordBoxPassword.Focus();
                }
                _loginSuccessActions.Clear();
            });
        }

        private readonly List<Action> _loginSuccessActions = new List<Action>();

        private void SetWork(string workMessage, Brush textColor, bool isEnabled)
        {
            labelResult.Foreground = textColor;
            labelResult.Content = workMessage;
            gridControls.IsEnabled = isEnabled;
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
            Instance = null;
            if (!IsLoggedIn)
            {
                App.IsShuttingDown = true;
            }
        }

        private void buttonQuit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        internal void OnLoginSuccess(Action action)
        {
            _loginSuccessActions.Add(action);
        }
    }
}
