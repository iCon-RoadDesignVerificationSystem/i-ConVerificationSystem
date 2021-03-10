using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualBasic;

namespace i_ConVerificationSystem.Forms.Base
{

    public class DataGridSelectAllStyleBehavior
    {
        public static readonly DependencyProperty SelectAllStyleTemplateProperty = DependencyProperty.RegisterAttached("SelectAllStyleTemplate", typeof(ControlTemplate), typeof(DataGridSelectAllStyleBehavior), new PropertyMetadata(PropertyCallback));

        public static ControlTemplate GetSelectAllStyleTemplate(DataGrid obj)
        {
            return (ControlTemplate)obj.GetValue(SelectAllStyleTemplateProperty);
        }

        public static void SetSelectAllStyleTemplate(DataGrid obj, ControlTemplate value)
        {
            obj.SetValue(SelectAllStyleTemplateProperty, value);
        }

        private static void PropertyCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DataGrid source = (DataGrid)obj;

            RoutedEventHandler eventHandler = (System.Object sender, System.Windows.RoutedEventArgs e) =>
            {
                Button b = FindButton(source);
                if (b != null)
                {
                    b.Template = GetSelectAllStyleTemplate(source);
                }
            };

            if (args.OldValue != null)
                source.Loaded -= eventHandler;
            if (args.NewValue != null)
                source.Loaded += eventHandler;
        }

        private static Button FindButton(FrameworkElement ele)
        {
            if (ele == null)
                return null/* TODO Change to default(_) if this is not a reference type */;
            if (ele.GetType() == typeof(Button))
                return (Button)ele;
            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(ele) - 1; i++)
            {
                Button child = FindButton((FrameworkElement)VisualTreeHelper.GetChild(ele, i));
                if (child != null)
                    return child;
            }
            return null/* TODO Change to default(_) if this is not a reference type */;
        }
    }

}
