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
    /// OGVRTabs.xaml の相互作用ロジック
    /// </summary>
    public partial class OGVRTabs : UserControl
    {
        public OGVRTabs()
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
                var ogvr_c = new OGVerificationResult();
                if (OGVerificationResultItem.Instance.ogvrPairs.ContainsKey(item.Item1))
                {
                    var ogvr = OGVerificationResultItem.Instance.ogvrPairs[item.Item1];
                    ogvr_c.RefreshDataGrid(ogvr);
                }

                tp.Header = item.Item1;
                tp.Content = ogvr_c;
                tcAlignments.Items.Add(tp);
            }

            tcAlignments.SelectedIndex = 0;
        }
    }
}
