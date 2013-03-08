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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;


namespace PointGaming.Desktop
{
    public partial class ClosableTab : TabItem
    {
        //readonly ClosableTabHeader _closableTabTabHeader;
        public event CancelEventHandler Closing;

        // Constructor
        public ClosableTab()
        {
            InitializeComponent();
            // Create an instance of the usercontrol
            //_closableTabTabHeader = new ClosableTabHeader();
            //_closableTabTabHeader.Text = "";
            // Assign the usercontrol to the tab header
            //this.Header = _closableTabTabHeader;

            // Attach to the ClosableHeader events
            // (Mouse Enter/Leave, Button Click, and Label resize)
            //ClosableHeader.button_close.MouseEnter +=
            //   new MouseEventHandler(button_close_MouseEnter);
            //ClosableHeader.button_close.MouseLeave +=
            //   new MouseEventHandler(button_close_MouseLeave);
            //ClosableHeader.button_close.Click +=
            //   new RoutedEventHandler(button_close_Click);
        }
        //public static readonly DependencyProperty ClosableTitleProperty = DependencyProperty.Register(
        //    "ClosableTitle", typeof(string), typeof(ClosableTab));
        //public string ClosableTitle
        //{
        //    get { return this.GetValue(ClosableTitleProperty) as string; }
        //    set { this.SetValue(ClosableTitleProperty, value); }
        //}
        internal void OnTabHeaderCloseClick()
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
    }
}
