using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;



namespace PointGaming
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly Dictionary<string, PropertyChangedEventArgs> eventArgsCache = new Dictionary<string, PropertyChangedEventArgs>();
 
        protected void OnPropertyChanged(string propertyName)
        {
            var args = GetArgNamed(propertyName);
            OnPropertyChanged(args);
        }

        private PropertyChangedEventArgs GetArgNamed(string propertyName)
        {
            PropertyChangedEventArgs args;
            if (!eventArgsCache.TryGetValue(propertyName, out args))
            {
                args = new PropertyChangedEventArgs(propertyName);
                eventArgsCache.Add(propertyName, args);
            }
            return args;
        }
 
        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, args);
        }

        protected void SetProperty<T>(ref T field, T value, Expression<Func<T>> expression)
        {
            if (field == null && value == null)
                return;
            if (field != null && field.Equals(value))
                return;
            field = value;
            OnPropertyChanged(expression);
        }

        protected void SetProperty<T>(object synch, ref T field, T value, Expression<Func<T>> expression)
        {
            lock (synch)
            {
                if (field == null && value == null)
                    return;
                if (field != null && field.Equals(value))
                    return;
                field = value;
            }
            OnPropertyChanged(expression);
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> propertySelector)
        {
            var myName = GetMemberName<T>(propertySelector);
            if (!string.IsNullOrEmpty(myName))
                OnPropertyChanged(myName);
        }
        
        protected string GetMemberName<T>(Expression<Func<T>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                return null;
            return memberExpression.Member.Name;
        }
    }
}