using i_ConVerificationSystem.Forms.Settings.Base;
using i_ConVerificationSystem.Forms.WidthComposition;
using i_ConVerificationSystem.Structs;
using Reactive.Bindings;
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
    /// WCVRListView.xaml の相互作用ロジック
    /// </summary>
    public partial class WCVRListView : UserControl
    {
        public WCVRListView()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ReactiveProperty<List<string>> listItems { get; set; } = new ReactiveProperty<List<string>>();

        /// <summary>
        /// リフレッシュ
        /// </summary>
        /// <param name="alignmentName"></param>
        public void RefreshVerificationResult(string alignmentName)
        {
            listItems.Value = new List<string>();

            foreach (var stdWC in WCVerificationResultItem.Instance.wctvrPairs)
            {
                var stdTp = new TabItem_Extensions();
                var wcvr_c = new WCVerificationResult();
                if (stdWC.Key.Item1 == alignmentName)
                {
                    var wcvri = new WCVerificationResultItems(stdWC.Value.Item1);
                    wcvr_c.RefreshDataGrid(wcvri);
                    stdTp.Header = stdWC.Value.Item2.name;
                    stdTp.Content = wcvr_c;
                    listItems.Value.Add(stdWC.Value.Item2.name);
                    tcAlignments.Items.Add(stdTp);
                }
            }

            tcAlignments.SelectedIndex = 0;
        }

        private void lstWCVR_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstWCVR.SelectedItems == null) return;
            var selTabHeaderString = (string)lstWCVR.SelectedItem;
            ShowAnyTab(selTabHeaderString);
        }

        /// <summary>
        /// 指定タブを開く
        /// </summary>
        /// <param name="tabHeaderString"></param>
        private void ShowAnyTab(string tabHeaderString)
        {
            foreach (var item in tcAlignments.Items)
            {
                var tp = item as TabItem_Extensions;
                if (!(tp is null) && tp.Header.ToString() == tabHeaderString)
                {
                    tcAlignments.SelectedItem = tp;
                    break;
                }
            }
        }
    }
}
