using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using PointGaming.Desktop.POCO;
using SocketIOClient;
using SocketIOClient.Messages;

namespace PointGaming.Desktop
{
    public partial class ChatWindow : Window
    {
        private Client _chatSocket;
        private OutgoingMessages _outgoing;
        private ReceivedMessages _received;
        private AuthEmit _authEmit;
        private ApiResponse _apiResponse;
        private string _otherUsername;

        public ChatWindow()
        {
            InitializeComponent();
        }

        public void Init(string otherUsername)
        {
            _otherUsername = otherUsername;
            Title = otherUsername;
        }

        private bool _once;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            if (!_once)
            {
                _once = true;
                ConnectSocket();
            }
        }

        private void ConnectSocket()
        {
            _chatSocket = new Client(Properties.Settings.Default.SocketIoUrl);
            _chatSocket.On("connect", OnConnect);
            _chatSocket.On("message", OnMessage);
            _chatSocket.On("auth_resp", OnAuthResponse);
            _chatSocket.Connect();
        }

        private void OnAuthResponse(IMessage message)
        {
            _apiResponse = new ApiResponse();
            _apiResponse = message.Json.GetFirstArgAs<ApiResponse>();
        }

        private void OnMessage(IMessage message)
        {
            this.InvokeUI(delegate
            {
                _received = message.Json.GetFirstArgAs<ReceivedMessages>();
                AppendUserMessage(_received.username, _received.message);
            });
        }

        private void OnConnect(IMessage message)
        {
            try
            {
                this.InvokeUI(delegate
                {
                    _authEmit = new AuthEmit { auth_token = Persistence.AuthToken };
                    AppendLine("Connected");
                    AppendLine("Chat with " + _otherUsername + " started!");
                    _chatSocket.Emit("auth", _authEmit);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void textBoxInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isShiftDown = e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift);

            if (e.Key == Key.Enter && !isShiftDown)
            {
                SendInput();
                e.Handled = true;
                return;
            }
        }

        private void SendInput()
        {
            var message = textBoxInput.Text;
            textBoxInput.Text = "";
            
            AppendUserMessage(Persistence.loggedInUsername, message);

            _outgoing = new OutgoingMessages { user = _otherUsername, message = message };
            _chatSocket.Emit("message", _outgoing);
        }

        private void AppendUserMessage(string username, string message)
        {
            var p = new Paragraph();
            p.Inlines.Add(new Bold(new Run(username + ": ")));
            p.Inlines.Add(new Run(message));
            richTextBoxLog.Document.Blocks.Add(p);
        }

        private void AppendLine(string line)
        {
            var p = new Paragraph();
            p.Inlines.Add(new Run(line));
            richTextBoxLog.Document.Blocks.Add(p);
        }

        private void buttonSendInput_Click(object sender, RoutedEventArgs e)
        {
            SendInput();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _chatSocket.Close();
        }
    }
}
