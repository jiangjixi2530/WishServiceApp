using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace JT100.Wish.Component
{
    public class VisualTreeProvider
    {
        public static T FindFirstChild<T>(DependencyObject reference, string childName)
        {
            if (reference != null)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(reference);
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(reference, i);
                    if (child.GetType() == typeof(T))
                    {
                        var frameworkElement = child as FrameworkElement;
                        if (frameworkElement.Name.Equals(childName))
                        {
                            return (T)Convert.ChangeType(child, typeof(T));
                        }
                    }
                    else
                    {
                        var result = FindFirstChild<T>(child, childName);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            return default(T);
        }
    }
}
