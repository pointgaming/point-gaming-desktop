using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PointGaming.Desktop.POCO;
using RestSharp;

namespace PointGaming.Desktop
{
    public partial class LoginWindow : Window
    {
        public bool IsLoggedIn;
        public SocketSession SocketSession;

        public LoginWindow()
        {
            InitializeComponent();
            Owner = HomeWindow.Home;
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
            }
        }

        private void LogIn()
        {
            textBoxResult.Foreground = Brushes.Black;
            textBoxResult.Text = "Logging in...";
            gridControls.IsEnabled = false;
            SocketSession session = new SocketSession();
            session.AddThreadQueuerForCurrentThread(HomeWindow.Home.BeginInvokeUI);
            var password = passwordBoxPassword.Password;
            var username = textBoxUsername.Text;
            passwordBoxPassword.Clear();

            DateTime timeout = DateTime.Now + Properties.Settings.Default.LogInTimeout;

            bool isSuccess = false;
            session.BeginAndCallback(delegate {
                isSuccess = session.Login(username, password, timeout);
            }, delegate {
                if (isSuccess) 
                {
                    IsLoggedIn = true;
                    SocketSession = session;
                    Close();
                }
                else
                {
                    var message = "Invalid username or password";
                    if (DateTime.Now >= timeout)
                        message = "Try again later.  No response within timeout period.";
                    textBoxResult.Foreground = Brushes.Red;
                    textBoxResult.Text = message;
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
    }
}
