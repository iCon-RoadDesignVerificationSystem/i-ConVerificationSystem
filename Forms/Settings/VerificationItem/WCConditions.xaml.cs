using i_ConVerificationSystem.Forms.Settings.Base;
using i_ConVerificationSystem.Forms.WidthComposition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace i_ConVerificationSystem.Forms.Settings.VerificationItem
{
    /// <summary>
    /// WCConditions.xaml の相互作用ロジック
    /// </summary>
    public partial class WCConditions : UserControl
    {
        public WCConditions()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 閉じるボタンがクリックされた場合に発生させるルーティングイベントの識別子
        /// </summary>
        public static readonly RoutedEvent DecisionButtonClickEvent = EventManager.RegisterRoutedEvent("DecisionButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WCConditions));
        public static readonly RoutedEvent CancelButtonClickEvent = EventManager.RegisterRoutedEvent("CancelButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WCConditions));

        /// <summary>
        /// イベント
        /// </summary>
        public event RoutedEventHandler DecisionButtonClick
        {
            add { AddHandler(DecisionButtonClickEvent, value); }
            remove { RemoveHandler(DecisionButtonClickEvent, value); }
        }

        public event RoutedEventHandler CancelButtonClick
        {
            add { AddHandler(CancelButtonClickEvent, value); }
            remove { RemoveHandler(CancelButtonClickEvent, value); }
        }

        /// <summary>
        /// ボタンの表示非表示
        /// </summary>
        [Browsable(true)]
        public Visibility ButtonVisibility
        {
            get { return wp.Visibility; }
            set { wp.Visibility = value; }
        }

        /// <summary>
        /// 編集可不可
        /// </summary>
        [Browsable(true)]
        public bool IsEditable { get; set; } = true;

        /// <summary>
        /// テンプレートが適用された際に発生するイベントのハンドラ
        /// 閉じるボタンがクリックされた際のイベントハンドラを登録する
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public void RefreshConditions()
        {
            tcAlignments.Items.Clear();

            var AlignmentList = XMLLoader.Instance.GetAlignmentTupleList();

            foreach (var item in AlignmentList)
            {
                var tp = new TabItem_Extensions();
                //var wHost = new WindowsFormsHost();
                var wcip = new WCInputParameter();
                wcip.IsEditable = IsEditable;
                wcip.sib.IsEditable = IsEditable;
                wcip.selXAlignment = item.Item2;
                wcip.sib.selXAlignment = item.Item2;
                wcip.LoadConditions();
                //wHost.Child = wcip;
                tp.Header = item.Item1;
                tp.Content = wcip;
                tcAlignments.Items.Add(tp);
            }

            tcAlignments.SelectedIndex = 0;
        }


        public List<WCInputParameter> GetWCInputParameters()
        {
            if (tcAlignments.Items.Count == 0) return new List<WCInputParameter>();

            var retVal = new List<WCInputParameter>();
            foreach (var item in tcAlignments.Items)
            {
                var tp = item as TabItem_Extensions;
                var wcip = tp.Content as WCInputParameter;
                retVal.Add(wcip);
            }

            return retVal;
        }


        public WCInputParameter GetSelectedWCInputParameter()
        {
            if (tcAlignments.Items.Count == 0) return null;

            var tp = tcAlignments.SelectedItem as TabItem_Extensions;
            var wcip = tp.Content as WCInputParameter;
            return wcip;
        }

        private void DecisionButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (TabItem_Extensions tc in tcAlignments.Items)
            {
                var p = tc.Content as WCInputParameter;
                if (!(p is null))
                {
                    p.SaveConditions();
                }
            }

            RaiseEvent(new RoutedEventArgs(DecisionButtonClickEvent, this));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CancelButtonClickEvent, this));
        }
    }
}
