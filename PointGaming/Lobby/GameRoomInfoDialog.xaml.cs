using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SocketIOClient;
using SocketIOClient.Messages;
using PointGaming.POCO;

namespace PointGaming.Lobby
{
    public partial class GameRoomInfoDialog : Window
    {
        public SocketSession UserSession;

        public GameRoomInfoDialog()
        {
            InitializeComponent();
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void Show(GameRoomItem item, SocketSession session)
        {
            GameRoomInfoDialog info = new GameRoomInfoDialog
            {
                UserSession = session,
                Title = item.DisplayName
            };
            info.InitChatroomMembers();
            info.ShowDialog();
        }
        
        public String TakeOverAmount
        {
            get { return "19,000 points"; }
        }

        public String Avatar
        {
            get { return "http://forums.pointgaming.com/assets/logo-3b643498dc7635d6ce4598843b5fcf0e.png"; }
        }

        public void InitChatroomMembers()
        {
            // TODO: get chatroom members from REST API. the member list is only available via socket connection because members stored in different persistence layer; this forces
            // unnecessary socket communications just to inspect things like room members.
            UserSession.OnThread("Chatroom.Member.list", OnChatroomMemberList);
            UserSession.EmitLater("Chatroom.Member.getList", new Chatroom());
            UserSession.Emit("Chatroom.Member.getList", new Chatroom());
        }

        private void OnChatroomMemberList(IMessage message)
        {
            var received = message.Json.GetFirstArgAs<ChatroomMemberList>();
            var id = received._id;
        }
    }
}
