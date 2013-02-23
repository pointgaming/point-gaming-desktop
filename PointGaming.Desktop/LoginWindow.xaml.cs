using System.Windows;
using System.Windows.Input;
using PointGaming.Desktop.POCO;
using RestSharp;

namespace PointGaming.Desktop
{
    public partial class LoginWindow : Window
    {
        public bool IsLoggedIn;

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
            var baseUrl = Properties.Settings.Default.BaseUrl; 
            var client = new RestClient(baseUrl);
            
            var request = new RestRequest("sessions", Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            string password = passwordBoxPassword.Password;
            string username = textBoxUsername.Text;
            request.AddBody(new UserLogin { username = username, password = password });

            var apiResponse = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
            var status = apiResponse.Data.success;

            if (status)
            {
                Persistence.AuthToken = apiResponse.Data.auth_token;
                Persistence.loggedInUsername = username;
                Properties.Settings.Default.Username = username;
                Properties.Settings.Default.Save();
                IsLoggedIn = true;
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
