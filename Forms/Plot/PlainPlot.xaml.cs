using i_ConVerificationSystem.JSON;
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
using System.Xml.Linq;
using static i_ConVerificationSystem.Structs.OGExtensions;

namespace i_ConVerificationSystem.Forms.Plot
{
    /// <summary>
    /// PlotTest.xaml の相互作用ロジック
    /// </summary>
    public partial class PlainPlot : Window
    {
        public XElement alignmentXe { get; set; }

        public PlainPlot()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var aMap = new AbstractMap();
            var allCsList = XMLLoader.Instance.GetCrossSectsOG(alignmentXe, true);
            var alignmentName = XMLLoader.Instance.GetAlignmentName(alignmentXe);
            var compareProvider = new XMLLoader.CrossSectCompareOG();

            //csを全件取得する
            var appliedCsList = AppSettingsManager.Instance.GetCrossSect_OGList_ApplyAllCS(alignmentName, allCsList);

            //標準幅員ごとに出力
            var orgOgcsList = XMLLoader.Instance.GetCrossSectsOG(alignmentXe, false);
            //var stdWCList = AppSettingsManager.Instance.GetCrossSect_OGList(alignmentName, orgOgcsList);

            aMap.InitializePlot();

            foreach (var resOgcs in orgOgcsList)
            {
                var seriesName = resOgcs.name;
                var stdPlotList = (from T in appliedCsList where compareProvider.Equals(T, resOgcs) select T).ToList();

                aMap.PlotPlainGraph(stdPlotList, $"STD-{seriesName}");
            }

            aMap.FinalizePlot(appliedCsList);

            //幅員チェック結果
            var wvrNGOgcsList = (from T in WVerificationResultItem.Instance.wtvrPairs
                               where T.Key.Item1 == alignmentName &&
                                     T.Value.Item1.GetTotalResult(T.Value.Item2) == VerificationResult.VerifyResultType.NG
                               select T.Value.Item2).ToList();
            foreach (var resOgcs in wvrNGOgcsList)
            {
                var seriesName = resOgcs.name;
                var ngList = (from T in appliedCsList where compareProvider.Equals(T, resOgcs) select T).ToList();

                aMap.PlotNGPlainGraph(ngList, $"W-{seriesName}", System.Drawing.Color.Red);
            }

            //幅員構成チェック結果
            var wcvrNGOgcsList = (from T in WCVerificationResultItem.Instance.wctvrPairs
                                  where T.Key.Item1 == alignmentName &&
                                        T.Value.Item1.GetTotalResult() == VerificationResult.VerifyResultType.NG
                                  select T.Value.Item2).ToList();
            foreach (var resOgcs in wcvrNGOgcsList)
            {
                var seriesName = resOgcs.name;
                var ngList = (from T in appliedCsList where compareProvider.Equals(T, resOgcs) select T).ToList();

                aMap.PlotNGPlainGraph(ngList, $"WC-{seriesName}", System.Drawing.Color.Orange);
            }

            wHost.Child = aMap;
        }
    }
}
