using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace PointGaming.Desktop
{
    public partial class ClosableTabHeader : UserControl
    {
        public ClosableTabHeader()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(ClosableTabHeader));
        public string Text
        {
            get { return this.GetValue(TextProperty) as string; }
            set { this.SetValue(TextProperty, value); }
        }



        //// Override OnSelected - Show the Close Button
        //protected override void OnSelected(RoutedEventArgs e)
        //{
        //    base.OnSelected(e);
        //    _closableHeader.button_close.Visibility = Visibility.Visible;
        //}

        //// Override OnUnSelected - Hide the Close Button
        //protected override void OnUnselected(RoutedEventArgs e)
        //{
        //    base.OnUnselected(e);
        //    _closableHeader.button_close.Visibility = Visibility.Hidden;
        //}

        // Override OnMouseEnter - Show the Close Button
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            button_close.Visibility = Visibility.Visible;
        }

        // Override OnMouseLeave - Hide the Close Button (If it is NOT selected)
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            button_close.Visibility = Visibility.Hidden;
        }

        // Button MouseEnter - When the mouse is over the button - change color to Red
        void button_close_MouseEnter(object sender, MouseEventArgs e)
        {
            button_close.Foreground = Brushes.Red;
        }
        // Button MouseLeave - When mouse is no longer over button - change color back to black
        void button_close_MouseLeave(object sender, MouseEventArgs e)
        {
            button_close.Foreground = Brushes.Black;
        }
        // Button Close Click - Remove the Tab - (or raise
        // an event indicating a "CloseTab" event has occurred)
        void button_close_Click(object sender, RoutedEventArgs e)
        {
            var parent = FindParentControl<ClosableTab>(this);
            parent.OnTabHeaderCloseClick();
        }

        /// <summary>
        /// Find a specific parent object type in the visual tree
        /// </summary>
        public static T FindParentControl<T>(DependencyObject outerDepObj) where T : DependencyObject
        {
            DependencyObject dObj = VisualTreeHelper.GetParent(outerDepObj);
            if (dObj == null)
                return null;

            if (dObj is T)
                return dObj as T;

            while ((dObj = VisualTreeHelper.GetParent(dObj)) != null)
            {
                if (dObj is T)
                    return dObj as T;
            }

            return null;
        }
    }
}
