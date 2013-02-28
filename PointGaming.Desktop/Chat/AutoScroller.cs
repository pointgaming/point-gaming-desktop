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
using PointGaming.Desktop.HomeTab;
using PointGaming.Desktop.POCO;

namespace PointGaming.Desktop.Chat
{
    public class AutoScroller
    {
        private System.Windows.Controls.Primitives.TextBoxBase _textbox;

        public AutoScroller(System.Windows.Controls.Primitives.TextBoxBase textbox)
        {
            _textbox = textbox;
            ScrollViewer s = textbox.FindDescendant<ScrollViewer>();
            if (s != null)
            {
                s.ScrollChanged += ScrollChanged;
            }
        }

        private bool _isAtEnd = true;
        private double lastVerticalOffset = 0;
        void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _isAtEnd = e.ExtentHeight - (e.VerticalOffset + e.ViewportHeight) <= 1.0;

            if (!_isAtEnd && e.VerticalOffset == lastVerticalOffset)
                _textbox.ScrollToEnd();
            lastVerticalOffset = e.VerticalOffset;
        }

        private bool _wasAtEnd;
        public void PreAppend()
        {
            _wasAtEnd = _isAtEnd;
        }

        public void PostAppend()
        {
            if (_wasAtEnd)
                _textbox.ScrollToEnd();
        }
    }
}
