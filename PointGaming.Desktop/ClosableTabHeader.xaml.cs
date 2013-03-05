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
    }

    public class ClosableTab : TabItem
    {
        readonly ClosableTabHeader _closableTabTabHeader;
        public event CancelEventHandler Closing;

        // Constructor
        public ClosableTab()
        {
            // Create an instance of the usercontrol
            _closableTabTabHeader = new ClosableTabHeader();
            // Assign the usercontrol to the tab header
            this.Header = _closableTabTabHeader;

            // Attach to the ClosableHeader events
            // (Mouse Enter/Leave, Button Click, and Label resize)
            _closableTabTabHeader.button_close.MouseEnter +=
               new MouseEventHandler(button_close_MouseEnter);
            _closableTabTabHeader.button_close.MouseLeave +=
               new MouseEventHandler(button_close_MouseLeave);
            _closableTabTabHeader.button_close.Click +=
               new RoutedEventHandler(button_close_Click);
            _closableTabTabHeader.label_TabTitle.SizeChanged +=
               new SizeChangedEventHandler(label_TabTitle_SizeChanged);
        }
        /// <summary>
        /// Property - Set the Title of the Tab
        /// </summary>
        public string Title
        {
            set
            {
                ((ClosableTabHeader)this.Header).label_TabTitle.Content = value;
            }
        }

        // Override OnSelected - Show the Close Button
        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            ((ClosableTabHeader)this.Header).button_close.Visibility = Visibility.Visible;
        }

        // Override OnUnSelected - Hide the Close Button
        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);
            ((ClosableTabHeader)this.Header).button_close.Visibility = Visibility.Hidden;
        }

        // Override OnMouseEnter - Show the Close Button
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            ((ClosableTabHeader)this.Header).button_close.Visibility = Visibility.Visible;
        }

        // Override OnMouseLeave - Hide the Close Button (If it is NOT selected)
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!this.IsSelected)
            {
                ((ClosableTabHeader)this.Header).button_close.Visibility = Visibility.Hidden;
            }
        }

        // Button MouseEnter - When the mouse is over the button - change color to Red
        void button_close_MouseEnter(object sender, MouseEventArgs e)
        {
            ((ClosableTabHeader)this.Header).button_close.Foreground = Brushes.Red;
        }
        // Button MouseLeave - When mouse is no longer over button - change color back to black
        void button_close_MouseLeave(object sender, MouseEventArgs e)
        {
            ((ClosableTabHeader)this.Header).button_close.Foreground = Brushes.Black;
        }
        // Button Close Click - Remove the Tab - (or raise
        // an event indicating a "CloseTab" event has occurred)
        void button_close_Click(object sender, RoutedEventArgs e)
        {
            var closing = Closing;
            if (closing != null)
            {
                var args = new CancelEventArgs(false);
                closing(this, args);
                if (args.Cancel)
                    return;
            }
            
            ((TabControl)this.Parent).Items.Remove(this);
        }
        // Label SizeChanged - When the Size of the Label changes
        // (due to setting the Title) set position of button properly
        void label_TabTitle_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((ClosableTabHeader)this.Header).button_close.Margin = new Thickness(
               ((ClosableTabHeader)this.Header).label_TabTitle.ActualWidth + 5, 3, 4, 0);
        }
    }
}
