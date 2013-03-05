using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PointGaming.Desktop.HomeTab
{
    public partial class LobbyChoice : UserControl, INotifyPropertyChanged
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
        public LobbyChoice()
        {
            InitializeComponent();
        }

        private ImageSource _image;
        public ImageSource Image
        {
            get { return _image; }
            set
            {
                if (value == _image)
                    return;
                _image = value;
                NotifyChanged("Image");
            }
        }

        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                if (value == _displayName)
                    return;
                _displayName = value;
                NotifyChanged("DisplayName");
            }
        }

        private double _displayHeight;
        public double DisplayHeight
        {
            get { return _displayHeight; }
            set
            {
                if (value == _displayHeight)
                    return;
                _displayHeight = value;
                NotifyChanged("DisplayHeight");
            }
        }
    }
}
