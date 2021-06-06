using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Windows.Controls;

namespace JT100.Wish.Core
{
    public class BaseControl : Control, IDisposable, INotifyPropertyChanged
    {
        private ConcurrentDictionary<string, object> values;
        public event PropertyChangedEventHandler PropertyChanged;
        public BaseControl()
        {
            values = new ConcurrentDictionary<string, object>();
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected string GetPropertyName(LambdaExpression lambda)
        {
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                UnaryExpression unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = lambda.Body as MemberExpression;
            }
            ConstantExpression constantExpression = memberExpression.Expression as ConstantExpression;
            PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;
            return propertyInfo.Name;

        }
        protected void SetValue<T>(Expression<Func<T>> property, T value)
        {
            if (property == null)
            {
                throw new ArgumentNullException("Property is null");
            }
            string propertyName = GetPropertyName(property);
            T valueInternal = GetValueInternal<T>(propertyName);
            if (!object.Equals(valueInternal, value))
            {
                values[propertyName] = value;
                OnPropertyChanged(propertyName);
            }
        }

        protected T GetValue<T>(Expression<Func<T>> property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("Property is null");
            }
            string propertyName = GetPropertyName(property);
            return GetValueInternal<T>(propertyName);
        }

        private T GetValueInternal<T>(string propertyName)
        {
            if (values.TryGetValue(propertyName, out object value))
            {
                return (T)value;
            }
            return default(T);
        }
        public virtual void Dispose()
        {
        }
    }
}
