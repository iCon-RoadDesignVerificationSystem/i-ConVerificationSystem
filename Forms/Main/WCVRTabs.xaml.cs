using i_ConVerificationSystem.Forms.Settings.Base;
using i_ConVerificationSystem.Forms.WidthComposition;
using i_ConVerificationSystem.Structs;
using System;
using System.Collections.Generic;
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

namespace i_ConVerificationSystem.Forms.Main
{
    /// <summary>
    /// WCVRTabs.xaml の相互作用ロジック
    /// </summary>
    public partial class WCVRTabs : UserControl
    {
        public WCVRTabs()
        {
            InitializeComponent();
        }

        /// <summary>
        /// リフレッシュ
        /// </summary>
        public void RefreshVerificationResult()
        {
            tcAlignments.Items.Clear();

            var AlignmentList = XMLLoader.Instance.GetAlignmentTupleList();

            foreach (var ali in AlignmentList)
            {
                var aliTp = new TabItem_Extensions();
                //var tc = new TabControl();
                aliTp.Header = ali.Item1;
                var tc = new WCVRListView();

                tc.RefreshVerificationResult(ali.Item1);

                //foreach (var stdWC in WCVerificationResultItem.Instance.wctvrPairs)
                //{
                //    var stdTp = new TabItem_Extensions();
                //    var wcvr_c = new WCVerificationResult();
                //    if (stdWC.Key.Item1 == ali.Item1)
                //    {
                //        var wcvri = new WCVerificationResultItems(stdWC.Value.Item1);
                //        wcvr_c.RefreshDataGrid(wcvri);
                //        stdTp.Header = stdWC.Value.Item2.name;
                //        stdTp.Content = wcvr_c;
                //        tc.Items.Add(stdTp);
                //    }
                //}

                aliTp.Content = tc;
                tcAlignments.Items.Add(aliTp);
            }

            tcAlignments.SelectedIndex = 0;
        }
    }
}
