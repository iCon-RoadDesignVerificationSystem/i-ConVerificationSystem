using i_ConVerificationSystem.Forms.Gradient;
using i_ConVerificationSystem.Forms.Settings.Base;
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
    /// TGVRTabs.xaml の相互作用ロジック
    /// </summary>
    public partial class TGVRTabs : UserControl
    {
        public TGVRTabs()
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

            foreach (var item in AlignmentList)
            {
                var tp = new TabItem_Extensions();
                var tgvr_c = new TGVerificationResult();
                if (TGVerificationResultItem.Instance.tgvrPairs.ContainsKey(item.Item1))
                {
                    var tgvr = TGVerificationResultItem.Instance.tgvrPairs[item.Item1];
                    tgvr_c.RefreshDataGrid(tgvr);
                }

                tp.Header = item.Item1;
                tp.Content = tgvr_c;
                tcAlignments.Items.Add(tp);
            }

            tcAlignments.SelectedIndex = 0;
        }
    }
}
