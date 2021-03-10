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
using System.Windows.Shapes;
using static i_ConVerificationSystem.Structs.OGExtensions;

namespace i_ConVerificationSystem.Forms.Main
{
    /// <summary>
    /// WVRWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class WVRWindow : Window
    {
        private string _alignmentName { get; set; }
        private List<CrossSect_OGExtension> _ogcsList { get; set; }

        public WVRWindow(string aliName, List<CrossSect_OGExtension> ogcsList)
        {
            InitializeComponent();

            _alignmentName = aliName;
            _ogcsList = ogcsList;
        }

        /// <summary>
        /// リフレッシュ
        /// </summary>
        public void RefreshVerificationResult()
        {
            if (_ogcsList == null || _alignmentName == null) return;

            foreach (var ogcs in _ogcsList)
            {
                var rItem = new WVerificationResult(ogcs.name);
                var resOgcs = (from T in WVerificationResultItem.Instance.wtvrPairs
                               where T.Key.Item1 == _alignmentName &&
                                     T.Key.Item2 == ogcs.sta.ToString()
                               select T.Value).FirstOrDefault();

                if (!(resOgcs is null))
                {
                    var wvri = new WVerificationResultItems(resOgcs.Item1, resOgcs.Item2);
                    rItem.RefreshDataGrid(wvri);
                    rItem.Margin = new Thickness(0, 0, 0, 10);
                    stPanel.Children.Add(rItem);
                }
            }
        }
    }
}
