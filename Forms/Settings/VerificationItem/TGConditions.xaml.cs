using i_ConVerificationSystem.Forms.Gradient;
using i_ConVerificationSystem.Forms.Settings.Base;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace i_ConVerificationSystem.Forms.Settings.VerificationItem
{
    /// <summary>
    /// TGConditions.xaml の相互作用ロジック
    /// </summary>
    public partial class TGConditions : UserControl
    {
        public TGConditions()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 閉じるボタンがクリックされた場合に発生させるルーティングイベントの識別子
        /// </summary>
        public static readonly RoutedEvent DecisionButtonClickEvent = EventManager.RegisterRoutedEvent("DecisionButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TGConditions));
        public static readonly RoutedEvent CancelButtonClickEvent = EventManager.RegisterRoutedEvent("CancelButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TGConditions));

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
                var tgip = new TGInputParameter();
                tgip.IsEditable = IsEditable;
                tgip.sig.IsEditable = IsEditable;
                tgip.alignmentName = item.Item1;
                tgip.sig.selXAlignment = XMLLoader.Instance.GetAlignmentFromName(item.Item1);
                tgip.LoadConditions();
                //wHost.Child = wcip;
                tp.Header = item.Item1;
                tp.Content = tgip;
                tcAlignments.Items.Add(tp);
            }

            tcAlignments.SelectedIndex = 0;
        }

        private void DecisionButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (TabItem_Extensions tc in tcAlignments.Items)
            {
                var p = tc.Content as TGInputParameter;
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
