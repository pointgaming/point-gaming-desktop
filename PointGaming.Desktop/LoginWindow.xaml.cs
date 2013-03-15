using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PointGaming.Desktop.POCO;
using RestSharp;
using System.Threading;

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
            SocketSession session = new SocketSession();
            session.SetThreadQueuerForCurrentThread(this.BeginInvokeUI);
            passwordBoxPassword.Clear();

            DateTime timeout = DateTime.Now + Properties.Settings.Default.LogInTimeout;

            bool isSuccess = false;
            session.BeginAndCallback(delegate
            {
                isSuccess = session.Login(username, password, timeout);
            }, delegate
            {
                if (isSuccess)
                {
                    IsLoggedIn = true;
                    SocketSession = session;

                    App.LoggedIn(username);

                    var homeWindow = new HomeWindow();
                    session.SetThreadQueuerForCurrentThread(HomeWindow.Home.BeginInvokeUI);
                    homeWindow.Init(session);
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
