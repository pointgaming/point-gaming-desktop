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
            bool isSuccess = session.Login(textBoxUsername.Text, passwordBoxPassword.Password);
            if (isSuccess)
            {
                IsLoggedIn = true;
                SocketSession = session;
                Close();
            }
            else
            {
                MessageBox.Show("Invalid Username or Password");
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
