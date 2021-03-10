using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace i_ConVerificationSystem.Forms.Base
{
    public class HorizontalScrollSyncBehavior
    {
        public static readonly DependencyProperty SyncElementProperty = DependencyProperty.RegisterAttached("SyncElement", typeof(ScrollViewer), typeof(HorizontalScrollSyncBehavior), new PropertyMetadata(PropertyCallback));

        public static ScrollViewer GetSyncElement(ScrollViewer obj)
        {
            return (ScrollViewer)obj.GetValue(SyncElementProperty);
        }

        public static void SetSyncElement(ScrollViewer obj, ScrollViewer value)
        {
            obj.SetValue(SyncElementProperty, value);
        }

        private static void PropertyCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ScrollViewer source = (ScrollViewer)obj;

            ScrollChangedEventHandler eventHandler = (object sender, ScrollChangedEventArgs e) =>
            {
                ScrollViewer target = GetSyncElement(source);
                target.ScrollToHorizontalOffset(source.HorizontalOffset);
            };

            if (args.OldValue != null)
            {
                ScrollViewer scroll = (ScrollViewer)args.OldValue;
                source.ScrollChanged -= eventHandler;
            }
            if (args.NewValue != null)
            {
                ScrollViewer scroll = (ScrollViewer)args.NewValue;
                source.ScrollChanged += eventHandler;
            }
        }
    }
}
