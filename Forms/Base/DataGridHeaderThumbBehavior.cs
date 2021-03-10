using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace i_ConVerificationSystem.Forms.Base
{
    public class DataGridHeaderThumbBehavior
    {
        public static readonly DependencyProperty SyncColumnProperty = DependencyProperty.RegisterAttached("SyncColumn", typeof(DataGridColumn), typeof(DataGridHeaderThumbBehavior), new PropertyMetadata(PropertyCallback));

        public static DataGridColumn GetSyncColumn(Thumb obj)
        {
            return (DataGridColumn)obj.GetValue(SyncColumnProperty);
        }

        public static void SetSyncColumn(Thumb obj, DataGridColumn value)
        {
            obj.SetValue(SyncColumnProperty, value);
        }

        private static void PropertyCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Thumb source = (Thumb)obj;

            DragDeltaEventHandler eventHandler = (object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e) =>
            {
                DataGridColumn target = GetSyncColumn(source);
                target.Width = new DataGridLength(target.ActualWidth + e.HorizontalChange);
            };

            if (args.OldValue != null)
                source.DragDelta -= eventHandler;
            if (args.NewValue != null)
                source.DragDelta += eventHandler;
        }
    }

}
