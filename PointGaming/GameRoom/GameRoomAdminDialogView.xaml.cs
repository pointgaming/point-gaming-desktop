using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PointGaming.GameRoom
{
    public partial class GameRoomAdminDialog : Window
    {

        private Point startPoint;
        private bool isDragging;
        
        public GameRoomAdminDialog()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;
            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance && isDragging)
            {
                ListBox listBox = sender as ListBox;
                ListBoxItem listBoxItem =
                    FindAnchestor<ListBoxItem>((DependencyObject)e.OriginalSource);
                if (listBox.Name == membersListBox.Name)
                {
                    membersListBox.AllowDrop = false;
                    adminsListBox.AllowDrop = true;
                }
                else if (listBox.Name == adminsListBox.Name)
                {
                    membersListBox.AllowDrop = true;
                    adminsListBox.AllowDrop = false;
                }
                PgUser contact = (PgUser)listBox.ItemContainerGenerator.
                    ItemFromContainer(listBoxItem);
                DataObject dragData = new DataObject("myFormat", contact);
                DragDrop.DoDragDrop(listBoxItem, dragData, DragDropEffects.Move);
                isDragging = false;
            }
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
            isDragging = true;
        }

        private void ListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("myFormat") ||
                sender == e.Source)
            {
                //e.Effects = DragDropEffects.None;
            }
        }

        private void membersListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myFormat"))
            {
                PgUser member = e.Data.GetData("myFormat") as PgUser;
                MoveMemberFromTo(adminsListBox, membersListBox, member);
                isDragging = false;
            }
        }

        private void adminsListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myFormat"))
            {
                PgUser member = e.Data.GetData("myFormat") as PgUser;
                MoveMemberFromTo(membersListBox, adminsListBox, member);
                isDragging = false;
            }
        }

        private void MoveMemberFromTo(ListBox originListBox, ListBox targetListBox, PgUser member)
        {
            var targetItems = targetListBox.Items;
            PgUser[] newTargetItems = new PgUser[targetItems.Count + 1];
            targetItems.CopyTo(newTargetItems, 0);
            newTargetItems[newTargetItems.Length - 1] = member;
            targetListBox.ItemsSource = newTargetItems;

            var originItems = originListBox.Items;
            var newOriginItems = new PgUser[originItems.Count - 1];
            for (int i = 0, j = 0; i < originItems.Count; i++)
                if (i != originItems.IndexOf(member))
                    newOriginItems[j++] = (PgUser)originItems[i];
            originListBox.ItemsSource = newOriginItems;
        }
    }
}
