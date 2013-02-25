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
            var password = passwordBoxPassword.Password;
            var username = textBoxUsername.Text;
            passwordBoxPassword.Clear();

            session.BeginLogin(username, password, LogInCompleted);
        }

        private void LogInCompleted(SocketSession session, bool isSuccess)
        {
            this.InvokeUI(delegate
            {
                if (isSuccess)
                {
                    IsLoggedIn = true;
                    SocketSession = session;
                    Close();
                }
                else
                {
                    textBoxResult.Foreground = Brushes.Red;
                    textBoxResult.Text = "Invalid username or password";
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
