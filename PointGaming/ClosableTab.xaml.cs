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


namespace PointGaming
{
    public partial class ClosableTab : TabItem
    {
        public event CancelEventHandler Closing;

        public static readonly DependencyProperty ShouldFlashProperty = DependencyProperty.Register(
            "ShouldFlash", typeof(bool), typeof(ClosableTab));
        public bool ShouldFlash
        {
            get { return (this.GetValue(ShouldFlashProperty) as bool?) == true; }
            set { this.SetValue(ShouldFlashProperty, value); }
        }

        public ClosableTab()
        {
            InitializeComponent();
        }
        internal void OnTabHeaderCloseClick()
        {
            PerformClosing();
        }

        public void Close()
        {
            PerformClosing();
        }

        private void PerformClosing()
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
