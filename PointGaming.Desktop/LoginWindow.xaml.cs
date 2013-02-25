using System.Windows;
using System.Windows.Input;
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
            SocketSession session = new SocketSession();
            var password = passwordBoxPassword.Password;
            var username = textBoxUsername.Text;
            passwordBoxPassword.Clear();

            bool isSuccess = session.Login(username, password);
            if (isSuccess)
            {
                IsLoggedIn = true;
                SocketSession = session;
                Close();
            }
            else
            {
                MessageBox.Show(this, "Invalid username or password");
            }
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
