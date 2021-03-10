using i_ConVerificationSystem.Forms.Settings.Base;
using i_ConVerificationSystem.JSON;
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
using static i_ConVerificationSystem.Structs.OGExtensions;

namespace i_ConVerificationSystem.Forms.Settings.StdWidthComposition
{
    /// <summary>
    /// StdConditions.xaml の相互作用ロジック
    /// </summary>
    public partial class StdAlignments : UserControl
    {
        public StdAlignments()
        {
            InitializeComponent();
        }

        ///// <summary>
        ///// 閉じるボタンがクリックされた場合に発生させるルーティングイベントの識別子
        ///// </summary>
        //public static readonly RoutedEvent CloseButtonClickEvent = EventManager.RegisterRoutedEvent("CloseButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StdAlignments));

        ///// <summary>
        ///// イベント
        ///// </summary>
        //public event RoutedEventHandler CloseButtonClick
        //{
        //    add { AddHandler(CloseButtonClickEvent, value); }
        //    remove { RemoveHandler(CloseButtonClickEvent, value); }
        //}

        ///// <summary>
        ///// ボタンの表示非表示
        ///// </summary>
        //[Browsable(true)]
        //public Visibility ButtonVisibility
        //{
        //    get { return wp.Visibility; }
        //    set { wp.Visibility = value; }
        //}

        ///// <summary>
        ///// テンプレートが適用された際に発生するイベントのハンドラ
        ///// 閉じるボタンがクリックされた際のイベントハンドラを登録する
        ///// </summary>
        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();

        //    //Button decisionButton = base.GetTemplateChild("DecisionButton") as Button;
        //    //Button cancelButton = base.GetTemplateChild("CancelButton") as Button;
        //    DecisionButton.Click += new RoutedEventHandler(closeButton_Click);
        //    CancelButton.Click += new RoutedEventHandler(closeButton_Click);
        //}

        ///// <summary>
        ///// 閉じるボタンがクリックされた際のイベントハンドラ
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void closeButton_Click(object sender, RoutedEventArgs e)
        //{
        //    //ClosePageEventを発生させる
        //    RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent, this));

        //    e.Handled = true;
        //}

        /// <summary>
        /// 編集可不可
        /// </summary>
        [Browsable(true)]
        public bool IsEditable { get; set; }

        /// <summary>
        /// リフレッシュ
        /// </summary>
        public void RefreshConditions()
        {
            tcAlignments.Items.Clear();

            var AlignmentList = XMLLoader.Instance.GetAlignmentTupleList();

            foreach (var item in AlignmentList)
            {
                //初期表示用
                var tp = new TabItem_Extensions();
                var ogmlv = new OGMapListView();
                ogmlv.IsEditable = IsEditable;
                ogmlv.CreateAlignmentTabs(item.Item2);

                tp.Header = item.Item1;
                tp.Content = ogmlv;
                tcAlignments.Items.Add(tp);
            }

            tcAlignments.SelectedIndex = 0;
        }

        /// <summary>
        /// 指定タブのCSリストを返答
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        public List<CrossSect_OGExtension> GetCsFromName(string targetName)
        {
            var retVal = new List<CrossSect_OGExtension>();
            foreach (var item in tcAlignments.Items)
            {
                var tp = item as TabItem_Extensions;
                if (tp.Header.ToString() == targetName)
                {
                    var ogmlv = tp.Content as OGMapListView;
                    //retVal = ogmlv.ogcsList.Value;
                    retVal = ogmlv.GetListViewItem();
                    break;
                }
            }
            
            return retVal;
        }


        public (string, List<CrossSect_OGExtension>) GetSelectedOgcsListAndName()
        {
            if (tcAlignments.Items.Count == 0) return ("", new List<CrossSect_OGExtension>());

            var tp = tcAlignments.SelectedItem as TabItem_Extensions;
            var ogmlv = tp.Content as OGMapListView;

            //return (tp.Header.ToString(), ogmlv.ogcsList.Value);
            return (tp.Header.ToString(), ogmlv.GetListViewItem());
        }


        public void SetOgcsList(string targetName, List<CrossSect_OGExtension> ogcsList)
        {
            foreach (var item in tcAlignments.Items)
            {
                var tp = item as TabItem_Extensions;
                if (tp.Header.ToString() == targetName)
                {
                    var ogmlv = tp.Content as OGMapListView;
                    ogmlv.RefreshOGMap(ogcsList);
                    break;
                }
            }
        }

        ///// <summary>
        ///// 確定ボタンクリック
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void DecisionButton_Click(object sender, RoutedEventArgs e)
        //{
        //    foreach (TabItem_Extensions tc in tcAlignments.Items)
        //    {
        //        var p = tc.Content as StdTabs;
        //        if (!(p is null))
        //        {
        //            p.SaveConditions();
        //        }
        //    }

        //    //closeButton_Click(null, null);
        //}

        ///// <summary>
        ///// キャンセルボタンクリック
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void CancelButton_Click(object sender, RoutedEventArgs e)
        //{
        //    //closeButton_Click(null, null);
        //}
    }
}
