using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace PointGaming
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

        public static readonly DependencyProperty ShouldFlashProperty = DependencyProperty.Register(
            "ShouldFlash", typeof(bool), typeof(ClosableTabHeader));
        public bool ShouldFlash
        {
            get { return (this.GetValue(ShouldFlashProperty) as bool?) == true; }
            set { this.SetValue(ShouldFlashProperty, value); }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            button_close.Visibility = Visibility.Visible;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            button_close.Visibility = Visibility.Hidden;
        }

        void button_close_MouseEnter(object sender, MouseEventArgs e)
        {
            button_close.Foreground = Brushes.Red;
        }
        void button_close_MouseLeave(object sender, MouseEventArgs e)
        {
            button_close.Foreground = Brushes.Black;
        }
        void button_close_Click(object sender, RoutedEventArgs e)
        {
            ClosableTab parent;
            if (this.TryGetParent(out parent))
                parent.OnTabHeaderCloseClick();
        }
    }
}
