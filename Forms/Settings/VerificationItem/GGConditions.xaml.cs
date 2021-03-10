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
using System.Xml.Linq;

namespace i_ConVerificationSystem.Forms.Settings.VerificationItem
{
    /// <summary>
    /// GGConditions.xaml の相互作用ロジック
    /// </summary>
    public partial class GGConditions : UserControl
    {
        //public List<Tuple<string, XElement>> AlignmentList { get; set; }

        public GGConditions()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 閉じるボタンがクリックされた場合に発生させるルーティングイベントの識別子
        /// </summary>
        public static readonly RoutedEvent DecisionButtonClickEvent = EventManager.RegisterRoutedEvent("DecisionButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GGConditions));
        public static readonly RoutedEvent CancelButtonClickEvent = EventManager.RegisterRoutedEvent("CancelButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GGConditions));

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
        /// リフレッシュ
        /// </summary>
        public void RefreshConditions()
        {
            tcAlignments.Items.Clear();

            var AlignmentList = XMLLoader.Instance.GetAlignmentTupleList();

            foreach (var item in AlignmentList)
            {
                var tp = new TabItem_Extensions();
                //var wHost = new WindowsFormsHost();
                var ggip = new GGInputParameter();
                ggip.IsEditable = IsEditable;
                ggip.sib.IsEditable = IsEditable;
                ggip.alignmentName = item.Item1;
                ggip.sib.selXAlignment = XMLLoader.Instance.GetAlignmentFromName(item.Item1);
                ggip.LoadConditions();
                //wHost.Child = wcip;
                tp.Header = item.Item1;
                tp.Content = ggip;
                tcAlignments.Items.Add(tp);
            }

            tcAlignments.SelectedIndex = 0;
        }

        /// <summary>
        /// 確定ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecisionButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (TabItem_Extensions tc in tcAlignments.Items)
            {
                var p = tc.Content as GGInputParameter;
                if (!(p is null))
                {
                    p.SaveConditions();
                }
            }

            RaiseEvent(new RoutedEventArgs(DecisionButtonClickEvent, this));
        }

        /// <summary>
        /// キャンセルボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CancelButtonClickEvent, this));
        }
    }
}
