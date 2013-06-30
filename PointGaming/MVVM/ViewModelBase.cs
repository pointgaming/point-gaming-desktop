using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PointGaming
{
    class ViewModelBase : INotifyPropertyChanged
    {
        private readonly Dictionary<string, PropertyChangedEventArgs> eventArgsCache;
 
        protected ViewModelBase()
        {
            eventArgsCache = new Dictionary<string, PropertyChangedEventArgs>();
        }
 
        public event PropertyChangedEventHandler PropertyChanged;
 
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventArgs args;
            if (!eventArgsCache.TryGetValue(propertyName, out args))
            {
                args = new PropertyChangedEventArgs(propertyName);
                eventArgsCache.Add(propertyName, args);
            }
 
            OnPropertyChanged(args);
        }
 
        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, args);
        }
    }
}