﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace PointGaming
{
    public class WindowTreeManager
    {
        private Window _window;

        public Window Self { get { return _window; } }

        private WindowTreeManager _parent;
        public WindowTreeManager Parent
        {
            get { return _parent; }
            set
            {
                if (_parent != null)
                    _parent.ChildRemoved(this);
                _parent = value;
                if (_parent != null)
                    _parent.ChildAdded(this);
            }
        }
        private List<WindowTreeManager> _children = new List<WindowTreeManager>();
        private Settings.WindowBoundsPersistor _windowBoundsPersistor;
        private bool _autoClosing;


        public void AddChild(Window window, bool autoClosing = true)
        {
            new WindowTreeManager(window, this, autoClosing);
        }


        public WindowTreeManager(Window window, WindowTreeManager parent, bool autoClosing = true)
        {
            Parent = parent;
            _window = window;
            _autoClosing = autoClosing;
            
            var windowName = window.GetType().Name;
            _windowBoundsPersistor = new Settings.WindowBoundsPersistor(window, windowName);
            _windowBoundsPersistor.Load();

            window.Closing += SelfClosing;
        }

        private void SelfClosing(object sender, CancelEventArgs e)
        {
            if (!_autoClosing)
                return;

            ManualClosing();
        }

        public void ManualClosing()
        {
            _windowBoundsPersistor.Save();
            CloseChildren();

            if (Parent != null)
                Parent.ChildRemoved(this);
        }

        private void CloseChildren()
        {
            var needToClose = new List<WindowTreeManager>(_children);
            _children.Clear();
            foreach (var item in needToClose)
            {
                try
                {
                    item._window.Close();// I'm not sure if this throws an exception if it wasn't ever shown or if it was already closed
                }
                catch { }
            }
        }

        public void ChildAdded(WindowTreeManager child)
        {
            if (!_children.Contains(child))
                _children.Add(child);
        }

        public void ChildRemoved(WindowTreeManager child)
        {
            _children.Remove(child);
        }
    }
}
