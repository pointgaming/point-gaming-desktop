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
    public partial class WindowBorder : UserControl, INotifyPropertyChanged
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

        public WindowBorder()
        {
            InitializeComponent();
        }

        private Visibility _minimizeVisibility;
        public Visibility MinimizeVisibility
        {
            get { return _minimizeVisibility; }
            set
            {
                if (value == _minimizeVisibility)
                    return;
                _minimizeVisibility = value;
                NotifyChanged("MinimizeVisibility");
            }
        }

        private Visibility _maximizeVisibility;
        public Visibility MaximizeVisibility
        {
            get { return _maximizeVisibility; }
            set
            {
                if (value == _maximizeVisibility)
                    return;
                _maximizeVisibility = value;
                NotifyChanged("MaximizeVisibility");
            }
        }

        private Visibility _closeVisibility;
        public Visibility CloseVisibility
        {
            get { return _closeVisibility; }
            set
            {
                if (value == _closeVisibility)
                    return;
                _closeVisibility = value;
                NotifyChanged("CloseVisibility");
            }
        }

        private bool _loadedOnce = false;
        private void MyLoaded(object sender, RoutedEventArgs e)
        {
            if (!_loadedOnce && Window != null)
            {
                _loadedOnce = true;
                Window.StateChanged += Window_StateChanged;
                MaximizeHelper.Help(Window);
            }
        }

        public static readonly DependencyProperty WindowProperty = DependencyProperty.Register(
            "Window", typeof(Window), typeof(WindowBorder));
        public Window Window
        {
            get { return this.GetValue(WindowProperty) as Window; }
            set { this.SetValue(WindowProperty, value); }
        }

        public static readonly DependencyProperty OtherContentProperty = DependencyProperty.Register(
            "OtherContent", typeof(Control), typeof(WindowBorder));
        public Control OtherContent
        {
            get { return this.GetValue(WindowProperty) as Control; }
            set { this.SetValue(WindowProperty, value); }
        }

        private void BorderMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_borderDownGrid == null)
                return;

            var newPosition = PointToScreen(e.GetPosition(null));
            var dPosition = newPosition - _borderDownPoint;
            _borderDownPoint = newPosition;

            var x = dPosition.X;
            var y = dPosition.Y;

            var grid = _borderDownGrid;

            using (var d = Dispatcher.DisableProcessing())
            {
                if (grid == gridC)
                {
                    Window.Left += x;
                    Window.Top += y;
                }
                else if (grid == gridTL)
                {
                    Window.Left += x;
                    Window.Width -= x;
                    Window.Top += y;
                    Window.Height -= y;
                }
                else if (grid == gridTR)
                {
                    Window.Width += x;
                    Window.Top += y;
                    Window.Height -= y;
                }
                else if (grid == gridBL)
                {
                    Window.Left += x;
                    Window.Width -= x;
                    Window.Height += y;
                }
                else if (grid == gridBR)
                {
                    Window.Width += x;
                    Window.Height += y;
                }
                else if (grid == gridT)
                {
                    Window.Top += y;
                    Window.Height -= y;
                }
                else if (grid == gridB)
                    Window.Height += y;
                else if (grid == gridL)
                {
                    Window.Left += x;
                    Window.Width -= x;
                }
                else if (grid == gridR)
                    Window.Width += x;
            }
        }

        private Grid _borderDownGrid = null;
        private Point _borderDownPoint;
        private void BorderMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var newPosition = e.GetPosition(null);

            _borderDownPoint = PointToScreen(newPosition);
            var element = VisualTreeHelper.HitTest(this, newPosition);
            if (element == null)
                return;

            var hit = element.VisualHit;
            if (!(hit is Grid))
            {
                if (hit == windowBorder && e.ClickCount > 1)
                    DoubleClickMaximizer();
                return;
            }

            var grid = (Grid)hit;
            List<Grid> areas = new List<Grid>(new[]
            {
                gridTL, gridT, gridTR,
                gridL, gridC, gridR,
                gridBL, gridB, gridBR,
            });
            if (!areas.Contains(grid))
                return;

            if (e.ClickCount > 1)
            {
                DoubleClickMaximizer();
                return;
            }

            _borderDownGrid = grid;

            Mouse.Capture(grid, CaptureMode.Element);
        }

        private void DoubleClickMaximizer()
        {
            if (MaximizeVisibility != System.Windows.Visibility.Visible)
                return;

            if (Window.WindowState == System.Windows.WindowState.Maximized)
                Window.WindowState = System.Windows.WindowState.Normal;
            else
                Window.WindowState = System.Windows.WindowState.Maximized;
            return;
        }

        private void BorderMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _borderDownGrid = null;
            Mouse.Capture(null);
        }

        private void BorderLostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _borderDownGrid = null;
            Mouse.Capture(null);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            using (var d = Dispatcher.DisableProcessing())
            {
                if (Window.WindowState == System.Windows.WindowState.Normal)
                {
                    windowBorder.BorderThickness = new Thickness(1);
                    windowBorder.CornerRadius = new CornerRadius(8);
                    SetResizeVisibility(System.Windows.Visibility.Visible);
                    OtherContent.Margin = new Thickness(6);
                }
                else if (Window.WindowState == System.Windows.WindowState.Maximized)
                {
                    windowBorder.BorderThickness = new Thickness(0);
                    windowBorder.CornerRadius = new CornerRadius(0);
                    SetResizeVisibility(System.Windows.Visibility.Collapsed);
                    OtherContent.Margin = new Thickness(0);
                }
            }
        }

        private void SetResizeVisibility(System.Windows.Visibility vis)
        {
            gridTL.Visibility = vis;
            gridTR.Visibility = vis;
            gridBL.Visibility = vis;
            gridBR.Visibility = vis;
            gridT.Visibility = vis;
            gridB.Visibility = vis;
            gridL.Visibility = vis;
            gridR.Visibility = vis;
        }


        private void MyMinimize(object sender, RoutedEventArgs e)
        {
            Window.WindowState = WindowState.Minimized;
        }

        private void MyMaximize(object sender, RoutedEventArgs e)
        {
            if (Window.WindowState == WindowState.Maximized)
                Window.WindowState = WindowState.Normal;
            else
                Window.WindowState = WindowState.Maximized;
        }

        private void MyClose(object sender, RoutedEventArgs e)
        {
            Window.Close();
        }
    }
}
