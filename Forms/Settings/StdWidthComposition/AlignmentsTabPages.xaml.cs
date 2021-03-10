using i_ConVerificationSystem.Forms.Settings.Base;
using i_ConVerificationSystem.Forms.Settings.StdWidthComposition;
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
using System.Xml.Linq;
using static i_ConVerificationSystem.Structs.OGExtensions;

namespace i_ConVerificationSystem.Forms.Settings.StdWidthComposition
{
    /// <summary>
    /// AlignmentsTabPages.xaml の相互作用ロジック
    /// </summary>
    public partial class AlignmentsTabPages : UserControl
    {
        public List<Tuple<string, XElement>> AlignmentList { get; set; }
        public Dictionary<string, List<CrossSect_OGExtension>> stdOGCsDictionary { get; set; }

        /// <summary>
        /// 中心線形セット分タブを表示
        /// </summary>
        public AlignmentsTabPages()
        {
            foreach (var item in AlignmentList)
            {
                var tp = new TabItem_Extensions();
                var tc = new StdTabControl();
                tc.ShowAlignmentTabPages(stdOGCsDictionary[item.Item1]);
                tp.Header = item.Item1;
                tp.Content = tc;
                AlignmentsTabControl.Items.Add(tp);
            }
            AlignmentsTabControl.SelectedIndex = 0;

            InitializeComponent();
        }
    }
}
