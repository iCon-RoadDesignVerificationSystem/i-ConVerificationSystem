using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace i_ConVerificationSystem.Forms.Settings.StdWidthComposition
{
    public class PageCloseEventArgs : RoutedEventArgs
    {
        private StdTabItem _closedItem = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="routedEvent"></param>
        /// <param name="originalSource"></param>
        /// <param name="closedItem"></param>
        public PageCloseEventArgs(RoutedEvent routedEvent, Object originalSource, StdTabItem closedItem)
            : base(routedEvent,originalSource)
        {
            _closedItem = closedItem;
        }

        /// <summary>
        /// 閉じられたStdTabItem
        /// </summary>
        public StdTabItem ClosedItem
        {
            get { return _closedItem; }
            set { _closedItem = value; }
        }
    }
}
