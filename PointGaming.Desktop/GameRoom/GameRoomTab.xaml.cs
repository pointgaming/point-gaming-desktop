using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PointGaming.Desktop.HomeTab;
using PointGaming.Desktop.POCO;
using PointGaming.Desktop.Chat;

namespace PointGaming.Desktop.GameRoom
{
    public partial class GameRoomTab : UserControl, IWeakEventListener, IChatroomTab, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged(string propertyName)
        {
            var changedCallback = PropertyChanged;
            if (changedCallback == null)
                return;
            var args = new PropertyChangedEventArgs(propertyName);
            changedCallback(this, args);
        }

        private ChatWindow _chatWindow;
        private GameRoomSession _gameRoomSession;
        private UserDataManager _userData = HomeWindow.UserData;
        private AutoScroller _autoScroller;
        private FlowDocument _descriptionDocument;

        public string Id { get { return _gameRoomSession.ChatroomId; } }

        public GameRoomTab()
        {
            InitializeComponent();
            flowDocumentLog.Document = new FlowDocument();
            UpdateChatFont();
            _autoScroller = new AutoScroller(flowDocumentLog);
            PropertyChangedEventManager.AddListener(Properties.Settings.Default, this, "PropertyChanged");

            _descriptionDocument = new FlowDocument();
            _descriptionDocument.SetPointGamingDefaults();
            flowDocumentDescription.Document = _descriptionDocument;
            
            var p = new Paragraph();
            p.Inlines.Add(new Bold(new Run("Description: ")));
            Chat.ChatTabCommon.Format("5 vs 5 Dust 2 No Scrubs\r\nWill ban for being bad\r\nNo 8 digs\r\n\r\n\r\n\r\n\r\n\r\n", p.Inlines);
            _descriptionDocument.Blocks.Add(p);
        }

        void GameRoom_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Owner")
            {
                UpdateOwner();
            }
        }

        private void UpdateOwner()
        {
            IsOwner = _gameRoomSession.GameRoom.Owner.Equals(_userData.User);
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            this.BeginInvokeUI(UpdateChatFont);
            return true;
        }

        private void UpdateChatFont()
        {
            flowDocumentLog.Document.Background = Brushes.White;
            flowDocumentLog.Document.PagePadding = new Thickness(2);
            flowDocumentLog.Document.FontFamily = new FontFamily(Properties.Settings.Default.ChatFontFamily + ", " + flowDocumentLog.Document.FontFamily);
            flowDocumentLog.Document.FontSize = Properties.Settings.Default.ChatFontSize;
        }

        public void Init(ChatWindow window, ChatroomSession gameRoomSession)
        {
            _chatWindow = window;
            _gameRoomSession = (GameRoomSession)gameRoomSession;
            _gameRoomSession.ReceivedMessage += ReceivedMessage;
            listBoxMembership.ItemsSource = _gameRoomSession.Membership;

            Binding b = new Binding("DescriptionDocument");
            b.Source = _gameRoomSession.GameRoom;
            flowDocumentDescription.SetBinding(FlowDocumentScrollViewer.DocumentProperty, b);

            _gameRoomSession.GameRoom.PropertyChanged += GameRoom_PropertyChanged;
            UpdateOwner();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxInput.Focus();
        }

        private void buttonSendInput_Click(object sender, RoutedEventArgs e)
        {
            SendInput();
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
            string send, remain;
            if (!ChatTabCommon.FilterMessage(textBoxInput.Text, out send, out remain))
                return;
            textBoxInput.Text = remain;

            _gameRoomSession.SendMessage(send);
        }

        private void ReceivedMessage(UserBase fromUser, string message)
        {
            AppendUserMessage(fromUser.username, message);
        }
        
        private void AppendUserMessage(string username, string message)
        {
            var time = DateTime.Now;

            string timeString = time.ToString("HH:mm");

            _autoScroller.PreAppend();

            var p = new Paragraph();
            p.Inlines.Add(new Run(timeString + " "));
            p.Inlines.Add(new Bold(new Run(username + ": ")));
            ChatTabCommon.Format(message, p.Inlines);
            flowDocumentLog.Document.Blocks.Add(p);

            _autoScroller.PostAppend();
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item;
            if (((DependencyObject)sender).TryGetParent(out item))
            {
                var pgUser = (PgUser)item.DataContext;
                _userData.ChatWith(pgUser);
            }
        }

        #region drag & drop source
        private Point _previewMouseLeftButtonDownPosition;
        private ListBoxItem _previewMouseLeftButtonDownItem;
        private PgUser _previewMouseLeftButtonDownUser;
        private bool _previewMouseLeftButtonDownCaptured;
        private DataObject _previewMouseLeftButtonDownDragData;
        private void listBoxMembership_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ResetDrag();
            ListBoxItem item;
            PgUser pgUser;
            if (listBoxMembership.TryGetItemAndItem(e, out item, out pgUser))
            {
                _previewMouseLeftButtonDownItem = item;
                _previewMouseLeftButtonDownUser = pgUser;
                _previewMouseLeftButtonDownPosition = e.GetPosition(null);
                _previewMouseLeftButtonDownCaptured = listBoxMembership.CaptureMouse();
            }
        }

        private void listBoxMembership_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResetDrag();
        }

        private void ResetDrag()
        {
            _previewMouseLeftButtonDownCaptured = false;
            listBoxMembership.ReleaseMouseCapture();
            _previewMouseLeftButtonDownDragData = null;
        }

        private void listBoxMembership_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_previewMouseLeftButtonDownCaptured || _previewMouseLeftButtonDownDragData != null)
                return;

            Vector diff = _previewMouseLeftButtonDownPosition - e.GetPosition(null);

            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            _previewMouseLeftButtonDownDragData = new DataObject(typeof(PgUser).FullName, _previewMouseLeftButtonDownUser);
            DragDrop.DoDragDrop(_previewMouseLeftButtonDownItem, _previewMouseLeftButtonDownDragData, DragDropEffects.Link);
        }
        #endregion

        #region drag & drop sink
        private void TabPreviewDragQuery(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (CanHandleDrop(e))
            {
                e.Effects = DragDropEffects.Link;
                e.Handled = true;
            }
        }
        private bool CanHandleDrop(DragEventArgs e)
        {
            return e.Data.GetDataPresent(typeof(PgUser).FullName) && _previewMouseLeftButtonDownDragData == null;
        }
        private void TabPreviewDrop(object sender, DragEventArgs e)
        {
            if (!CanHandleDrop(e))
                return;

            if (e.Data.GetDataPresent(typeof(PgUser).FullName))
            {
                PgUser anotherUser = e.Data.GetData(typeof(PgUser).FullName) as PgUser;
                _gameRoomSession.Invite(anotherUser);
                e.Handled = true;
            }
        }
        #endregion

        private void hyperLinkRoomInfoClick(object sender, RoutedEventArgs e)
        {
            var dialog = new GameRoomAdminDialog();
            dialog.Description = _gameRoomSession.GameRoom.Description;
            dialog.Password = _gameRoomSession.GameRoom.Password;
            dialog.IsAdvertising = _gameRoomSession.GameRoom.IsAdvertising;
            dialog.Owner = _chatWindow;
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                var poco = new
                {
                    _id = _gameRoomSession.GameRoom.Id,
                    description = dialog.Description,
                    is_advertising = dialog.IsAdvertising,
                    password = dialog.Password,
                };
                _gameRoomSession.SetGameRoomSettings(poco);
            }
        }

        private void buttonProposeABet_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new BetProposalDialog();
            dialog.Owner = _chatWindow;
            dialog.SetBetOperands(new PgUser { Username = "Mr.Apple" }, new PgUser { Username = "Mr.Banana" });
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                MessageBox.Show(_chatWindow, "Add Proposal", "Todo add a proposal");
            }
        }

        private void KickMemberClick(object sender, RoutedEventArgs e)
        {
            MessageDialog.Show(_chatWindow, "Todo", "Todo: kick");
        }

        private void BanMemberClick(object sender, RoutedEventArgs e)
        {
            MessageDialog.Show(_chatWindow, "Todo", "Todo: ban");
        }

        private void PromoteToOwnerMemberClick(object sender, RoutedEventArgs e)
        {
            var poco = new
            {
                _id = _gameRoomSession.GameRoom.Id,
                owner_id = _contextMenuUser.Id,
            };
            _gameRoomSession.SetGameRoomSettings(poco);
        }

        private PgUser _contextMenuUser;
        private void listBoxMembership_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item;
            PgUser pgUser;
            if (listBoxMembership.TryGetItemAndItem(e, out item, out pgUser))
            {
                _contextMenuUser = pgUser;
                IsSelfMemberClick = _contextMenuUser.Equals(_userData.User);
            }
        }

        private bool _isOwner;
        public bool IsOwner
        {
            get { return _isOwner; }
            set
            {
                if (value == _isOwner)
                    return;
                _isOwner = value;
                NotifyChanged("IsOwner");
            }
        }

        private bool _isSelfMemberClick;
        public bool IsSelfMemberClick
        {
            get { return _isSelfMemberClick; }
            set
            {
                if (value == _isSelfMemberClick)
                    return;
                _isSelfMemberClick = value;
                NotifyChanged("IsSelfMemberClick");
            }
        }
    }
}
