using i_ConVerificationSystem.Verification;
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
using static i_ConVerificationSystem.Structs.VerificationResult;

namespace i_ConVerificationSystem.Forms.Plot
{
    /// <summary>
    /// TPlotTest.xaml の相互作用ロジック
    /// </summary>
    public partial class TransversePlot : UserControl
    {
        public TransversePlot()
        {
            InitializeComponent();
        }

        public void PlotTransverse(CrossSect_OGExtension cs)
        {
            var points = new List<Tuple<Point, Point, string, string, VerifyResultType>>();

            if (cs.dcssList.Any() && cs.dcssList[0].cspList.Any())
            {
                //ベース高さを取る。画面の中心点が0,0
                var baseHeight = cs.dcssList[0].cspList[0].roadHight;
                foreach (var dcss in cs.dcssList)
                {
                    if (dcss.isTarget == false) continue;
                    for (int i = 0; i < dcss.cspList.Count - 1; i++)
                    {
                        var bX = (double)(dcss.cspList[i].roadPositionX + cs.clOffset) * 50;
                        var bY = (double)(dcss.cspList[i].roadHight + cs.fhOffset - baseHeight);
                        bY = bY + (bY - Math.Truncate(bY)) * 100;
                        var bP = new Point(bX, bY);
                        var aX = (double)(dcss.cspList[i + 1].roadPositionX + cs.clOffset) * 50;
                        var aY = (double)(dcss.cspList[i + 1].roadHight + cs.fhOffset - baseHeight);
                        aY = aY + (aY - Math.Truncate(aY)) * 100;
                        var aP = new Point(aX, aY);

                        var rWidth = dcss.cspList[i].GetWidthStringBetweenRoad(dcss.cspList[i + 1].roadPositionX);
                        if (rWidth == "0" || bX == aX)
                        {
                            points.Add(new Tuple<Point, Point, string, string, VerifyResultType>(bP, aP, null, rWidth, VerifyResultType.SKIP));
                        }
                        else
                        {
                            points.Add(new Tuple<Point, Point, string, string, VerifyResultType>(bP, aP, CommonMethod.GetName_JFromGroupNameCode(dcss.name_J), rWidth, dcss.totalResult));
                        }
                    }
                }
            }

            xView.Points = points;
            xView.TotalWidth = cs.GetTotalWidthString();
            xView.InvalidateVisual();
        }
    }
}
