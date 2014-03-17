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

namespace PointGaming.HomeTab
{
    /// <summary>
    /// Логика взаимодействия для NotificationsBox.xaml
    /// </summary>
    public partial class NotificationsBox : Window
    {
        public NotificationsBox()
        {
            InitializeComponent();
            this.NotificationsContentBox.Document = new FlowDocument();
            UpdateFont();
        }

        public void AddMessage(string messageText)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(messageText);
            this.NotificationsContentBox.Document.Blocks.Add(paragraph);
        }

        private void UpdateFont()
        {
            this.NotificationsContentBox.Document.Background = Brushes.White;
            this.NotificationsContentBox.Document.PagePadding = new Thickness(2);
            this.NotificationsContentBox.Document.FontFamily = new FontFamily(UserDataManager.UserData.Settings.ChatFontFamily + ", " + this.NotificationsContentBox.Document.FontFamily);
            this.NotificationsContentBox.Document.FontSize = UserDataManager.UserData.Settings.ChatFontSize;
        }
    }
}
