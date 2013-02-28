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
        private System.Windows.Controls.FlowDocumentScrollViewer _textbox;
        private ScrollViewer _scrollViewer;

        public AutoScroller(System.Windows.Controls.FlowDocumentScrollViewer textbox)
        {
            _textbox = textbox;
            textbox.Loaded += new RoutedEventHandler(textbox_Loaded);
        }

        void textbox_Loaded(object sender, RoutedEventArgs e)
        {
            _scrollViewer = _textbox.FindDescendant<ScrollViewer>();
            if (_scrollViewer != null)
                _scrollViewer.ScrollChanged += ScrollChanged;
        }

        private bool _isAtEnd = true;
        private double lastVerticalOffset = 0;
        void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _isAtEnd = e.ExtentHeight - (e.VerticalOffset + e.ViewportHeight) <= 1.0;

            if (!_isAtEnd && e.VerticalOffset == lastVerticalOffset)
                _scrollViewer.ScrollToEnd();
            lastVerticalOffset = e.VerticalOffset;
        }

        private bool _wasAtEnd;
        public void PreAppend()
        {
            _wasAtEnd = _isAtEnd;
        }

        public void PostAppend()
        {
            if (_wasAtEnd && _scrollViewer!= null)
                _scrollViewer.ScrollToEnd();
        }
    }
}
