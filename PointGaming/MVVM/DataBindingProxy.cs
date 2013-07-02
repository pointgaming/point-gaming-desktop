using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace PointGaming
{
    class DataBindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new DataBindingProxy();
        }
 
        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
 
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(DataBindingProxy), new UIPropertyMetadata(null));
    }
}
