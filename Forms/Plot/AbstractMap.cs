using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using static i_ConVerificationSystem.Structs.CrossSects;
using System.Linq;
using static i_ConVerificationSystem.Structs.OGExtensions;
using static i_ConVerificationSystem.Structs.CrossSects.DesignCrossSectSurf;

namespace i_ConVerificationSystem.Forms
{
    partial class AbstractMap : UserControl
    {
        private bool PlotAreaZoomed { get; set; }
        private double PlotViewRate = 0.75;

        public AbstractMap()
        {
            InitializeComponent();
        }


        /// <summary>
        /// イニシャライズ
        /// </summary>
        public void InitializePlot()
        {
            this.Chart_Plotarea.Series.Clear();
            this.Chart_Plotarea.BackColor = Color.White;
        }

        /// <summary>
        /// グラフのX、Y軸の調整（プロットはしない）
        /// </summary>
        /// <param name="csList"></param>
        public void FinalizePlot(List<CrossSect_OGExtension> csList)
        {
            var pList = CreatePlainGraphData(csList);

            var minValx = (from T in pList select T.Item2).Min();
            var maxValx = (from T in pList select T.Item2).Max();
            var minValy = (from T in pList select T.Item3).Min();
            var maxValy = (from T in pList select T.Item3).Max();

            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "0";
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisX.Minimum = Math.Round(minValx / 100, 0, MidpointRounding.AwayFromZero) * 100 - 100;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisX.Maximum = Math.Round(maxValx / 100, 0, MidpointRounding.AwayFromZero) * 100 + 100;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisX.Interval = 100;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisX.MajorGrid.Enabled = true;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisX.MajorGrid.LineColor = Color.Gray;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisX.MinorGrid.Enabled = false;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisX.MinorGrid.LineColor = Color.Gray;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisX.LabelStyle.ForeColor = Color.Black;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisX.Title = "累加距離標[m]";
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisX.TitleAlignment = StringAlignment.Center;

            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisY.Minimum = Math.Round(minValy / 5) * 5 - 5;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisY.Maximum = Math.Round(maxValy / 5) * 5 + 5;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisY.Interval = 5;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisY.MajorGrid.Enabled = true;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisY.MajorGrid.LineColor = Color.Gray;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisY.MinorGrid.Enabled = false;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisY.MinorGrid.LineColor = Color.Gray;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisY.LabelStyle.ForeColor = Color.Black;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisY.Title = "中心点からの距離[m]";
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisY.TitleAlignment = StringAlignment.Center;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisY.IsReversed = true;
        }

        public void PlotPlainGraph(List<CrossSect_OGExtension> csList, string seriesName)
        {
            var pList = CreatePlainGraphData(csList);
            var reqCount = GetRequire2AreaList(csList).Count();
            var maxItemCount = pList.Max(row => row.Item1);
            var sList = new List<int>();

            for (int i = 0; i < maxItemCount + reqCount * 2 + 2; i++)
            {
                Chart_Plotarea.Series.Add($"{seriesName}{i}".ToString());
                Chart_Plotarea.Series[$"{seriesName}{i}"].ChartType = SeriesChartType.Line;
                Chart_Plotarea.Series[$"{seriesName}{i}"].BorderWidth = 2;
                Chart_Plotarea.Series[$"{seriesName}{i}"].IsVisibleInLegend = false;
                Chart_Plotarea.Series[$"{seriesName}{i}"].IsValueShownAsLabel = false;
                Chart_Plotarea.Series[$"{seriesName}{i}"].Color = Color.DarkCyan;
            }

            //デバッグ
            //int plotNum = 0;
            foreach (var item in pList)
            {
                //if (item.Item1 != plotNum) continue;
                //System.Diagnostics.Debug.WriteLine(item.ToString());

                //概略図のため線形は考慮しない
                this.Chart_Plotarea.Series[$"{seriesName}{item.Item1}"].Points.AddXY(item.Item2, item.Item3);
            }
        }

        /// <summary>
        /// NGになった標準幅員を赤色表示する
        /// </summary>
        /// <param name="ngCsList"></param>
        /// <param name="seriesName"></param>
        public void PlotNGPlainGraph(List<CrossSect_OGExtension> ngCsList, string seriesName, Color brushColor)
        {
            var pList = CreatePlainGraphData(ngCsList);
            var reqCount = GetRequire2AreaList(ngCsList).Count();
            var maxItemCount = ngCsList.Select(csRow => csRow.dcssList.Count()).Max();
            var sList = new List<int>();

            for (int i = 0; i < maxItemCount + reqCount * 2; i++)
            {
                Chart_Plotarea.Series.Add($"{seriesName}{i}");
                Chart_Plotarea.Series[$"{seriesName}{i}"].ChartType = SeriesChartType.Line;
                Chart_Plotarea.Series[$"{seriesName}{i}"].BorderWidth = 2;
                Chart_Plotarea.Series[$"{seriesName}{i}"].IsVisibleInLegend = false;
                Chart_Plotarea.Series[$"{seriesName}{i}"].IsValueShownAsLabel = false;
                Chart_Plotarea.Series[$"{seriesName}{i}"].Color = brushColor;
            }

            foreach (var item in pList)
            {
                //概略図のため線形は考慮しない
                this.Chart_Plotarea.Series[$"{seriesName}{item.Item1}"].Points.AddXY(item.Item2, item.Item3);
            }
        }

        /// <summary>
        /// プロット用座標点が2枠必要な構成番号を取得する
        /// </summary>
        /// <param name="csList"></param>
        /// <returns></returns>
        private List<Tuple<DCSSSide, int>> GetRequire2AreaList(List<CrossSect_OGExtension> csList)
        {
            var retList = new List<Tuple<DCSSSide, int>>();
            foreach (var cs in csList)
            {
                foreach (var dcss in cs.dcssList)
                {
                    var fhItem = (from T in dcss.cspList where T.code.Contains("FH") select T);
                    if (fhItem.Any())
                    {
                        //ex. LFHxnx'であるとき、交差点などでの路線の分岐点であるためその分の枠を準備する
                        //R5、L1などサイドと構成番号を組み合わせたキー
                        var reqPos = new Tuple<DCSSSide, int>(dcss.side, dcss.cspList.Max(row => row.roadPositionNo));
                        if (!(retList.Contains(reqPos))) retList.Add(reqPos);
                    }
                    else continue;
                }
            }

            return retList;
        }

        /// <summary>
        /// 平面図グラフデータを作成
        /// </summary>
        /// <param name="csList"></param>
        private List<Tuple<int, double, double>> CreatePlainGraphData(List<CrossSect_OGExtension> csList)
        {
            var retPList = new List<Tuple<int, double, double>>();
            var reqList = GetRequire2AreaList(csList);
            //int leftSideMaxItemCount = csList.Select(csRow => csRow.dcssList.Where(row => row.side == DCSSSide.Left).Count()).Max();
            var itemTable = CreatePlainGraphItemTable(csList);
            int maxLSideNum = itemTable.Where(row => row.Item1 == DCSSSide.Left).Max(row => row.Item4) + 1;

            foreach (var cs in csList)
            {
                var pList = new List<Tuple<int, double, double>>();
                bool isLSideFirstItem = true;
                bool isRSideFirstItem = true;
                foreach (var dcss in cs.dcssList)
                {
                    var maxPos = dcss.cspList.Max(row => row.roadPositionNo);
                    var isReq2Area = (from T in reqList where T.Item1 == dcss.side && T.Item2 == maxPos select T).Any();
                    var fhItem = (from T in dcss.cspList where T.code.Contains("FH") select T).Any();

                    var itemList = (from T in itemTable
                                    where T.Item1 == dcss.side &&
                                    T.Item2 == dcss.name &&
                                    T.Item3 == false
                                    select T).ToList();

                    var rWidth = dcss.cspList.First().roadWidth;

                    if (isReq2Area)
                    {
                        if (fhItem)
                        {
                            var minPosX = dcss.cspList.Where(row => row.roadPositionNo != 0).Min(mRow => Math.Abs(mRow.roadPositionX));
                            var maxPosX = dcss.cspList.Where(row => row.roadPositionNo != 0).Max(mRow => Math.Abs(mRow.roadPositionX));

                            var posX1 = -minPosX + cs.clOffset;
                            if (isLSideFirstItem == false)
                            {
                                posX1 = (decimal)pList.Last().Item3 - minPosX;
                            }
                            else isLSideFirstItem = false;
                            var posX2 = posX1 - maxPosX;

                            if (dcss.side == DCSSSide.Right)
                            {
                                posX1 = minPosX + cs.clOffset;
                                if (isRSideFirstItem == false)
                                {
                                    posX1 = (decimal)pList.Last().Item3 + minPosX;
                                }
                                else isRSideFirstItem = false;
                                posX2 = posX1 + maxPosX;
                            }

                            foreach (var item in itemList)
                            {
                                var recordNum = item.Item4;
                                if (dcss.side == DCSSSide.Right) recordNum += maxLSideNum;
                                bool isAlready = (from T in pList where T.Item1 == recordNum select T).Any();
                                if (isAlready) continue;
                                else
                                {
                                    pList.Add(new Tuple<int, double, double>(recordNum, (double)cs.sta, (double)posX1));
                                    pList.Add(new Tuple<int, double, double>(recordNum + 1, (double)cs.sta, (double)posX2));
                                    break;
                                }
                            }
                        }
                        else
                        {
                            var posX1 = -rWidth + cs.clOffset;
                            if (isLSideFirstItem == false)
                            {
                                posX1 = (decimal)pList.Last().Item3 - rWidth;
                            }
                            else isLSideFirstItem = false;
                            var posX2 = posX1;

                            if (dcss.side == DCSSSide.Right)
                            {
                                posX1 = rWidth + cs.clOffset;
                                if (isRSideFirstItem == false)
                                {
                                    posX1 = (decimal)pList.Last().Item3 + rWidth;
                                }
                                else isRSideFirstItem = false;
                                posX2 = posX1;
                            }
                            foreach (var item in itemList)
                            {
                                var recordNum = item.Item4;
                                if (dcss.side == DCSSSide.Right) recordNum += maxLSideNum;
                                bool isAlready = (from T in pList where T.Item1 == recordNum select T).Any();
                                if (isAlready) continue;
                                else
                                {
                                    pList.Add(new Tuple<int, double, double>(recordNum, (double)cs.sta, (double)posX1));
                                    pList.Add(new Tuple<int, double, double>(recordNum + 1, (double)cs.sta, (double)posX2));
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        var posX1 = -rWidth + cs.clOffset;
                        if (isLSideFirstItem == false)
                        {
                            posX1 = (decimal)pList.Last().Item3 - rWidth;
                        }
                        else isLSideFirstItem = false;
                        if (dcss.side == DCSSSide.Right)
                        {
                            posX1 = rWidth + cs.clOffset;
                            if (isRSideFirstItem == false)
                            {
                                posX1 = (decimal)pList.Last().Item3 + rWidth;
                            }
                            else isRSideFirstItem = false;
                        }
                        foreach (var item in itemList)
                        {
                            var recordNum = item.Item4;
                            if (dcss.side == DCSSSide.Right) recordNum += maxLSideNum;
                            bool isAlready = (from T in pList where T.Item1 == recordNum select T).Any();
                            if (isAlready) continue;
                            else
                            {
                                pList.Add(new Tuple<int, double, double>(recordNum, (double)cs.sta, (double)posX1));
                                break;
                            }
                        }
                    }
                }
                retPList.AddRange(pList);
            }

            return retPList;
        }

        private List<Tuple<DCSSSide, string, bool, int>> CreatePlainGraphItemTable(List<CrossSect_OGExtension> csList)
        {
            //平面図対比表を作る
            var reqList = GetRequire2AreaList(csList);
            //Side, name, isFH, num
            var retList = new List<Tuple<DCSSSide, string, bool, int>>();

            //1.Carriageway, 2.Carriageway, 3.Carryageway, 4.Roadshoulder, 5.Other
            //1.Carriageway, 2.Carriageway, 3.Roadshoulder, 4.Other
            //1.Carriageway, 2.Carriageway, 3.Roadshoulder, 4.Other, 5.Other
            //-> 1.Carriageway, 2.Carriageway, 3.Carryageway, 4.Roadshoulder, 5.Other, 6.Other こうしたい
            //L,Rも考慮

            foreach (var cs in csList)
            {
                int idx = 0;
                bool isFirstTimeRSide = true;

                foreach (var dcss in cs.dcssList)
                {
                    if (dcss.side == DCSSSide.Right && isFirstTimeRSide)
                    {
                        isFirstTimeRSide = false;
                        idx = 0;
                    }

                    var maxPos = dcss.cspList.Max(row => row.roadPositionNo);
                    var addCount = (from T in reqList where T.Item1 == dcss.side && T.Item2 < maxPos select T).Count();
                    var isReq2Area = (from T in reqList where T.Item1 == dcss.side && T.Item2 == maxPos select T).Any();

                    //登録済みなら次へ
                    var isAlready = (from T in retList
                                     where T.Item1 == dcss.side &&
                                     T.Item2 == dcss.name &&
                                     T.Item3 == false &&
                                     T.Item4 == idx + addCount
                                     select T).Any();
                    if (isAlready)
                    {
                        idx++;
                        continue;
                    }

                    var hasItemCount = (from T in cs.dcssList where T.side == dcss.side && T.name == dcss.name select T).Count();
                    var alreadyItemCount = (from T in retList where T.Item1 == dcss.side && T.Item2 == dcss.name && T.Item3 == false select T).Count();

                    if (hasItemCount <= alreadyItemCount)
                    {
                        idx++;
                        continue;
                    }

                    //番号が使われていたら最大値+1を使う
                    var isAlreadyUsedNum = (from T in retList
                                            where T.Item1 == dcss.side &&
                                            T.Item4 == idx + addCount
                                            select T).Any();

                    //登録処理
                    int recordNum = 0;
                    
                    if (isAlreadyUsedNum)
                    {
                        //MAXを取り端がFHでもOK
                        recordNum = retList.Where(row => row.Item1 == dcss.side).Max(row => row.Item4) + 1;
                    }
                    else
                    {
                        recordNum = idx + addCount;
                    }

                    var addItem = new Tuple<DCSSSide, string, bool, int>(dcss.side, dcss.name, false, recordNum);
                    retList.Add(addItem);
                    if (isReq2Area)
                    {
                        var addItemFH = new Tuple<DCSSSide, string, bool, int>(dcss.side, dcss.name, true, recordNum + 1);
                        retList.Add(addItemFH);
                    }

                    idx++;
                }
            }

            return retList;
        }

        private void Chart_Plotarea_DoubleClick(object sender, EventArgs e)
        {
            double maVal = this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisX.Maximum;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisX.Interval = 100;
            this.Chart_Plotarea.ChartAreas["ChartArea1"].AxisX.ScaleView.Size = PlotAreaZoomed ? double.NaN : maVal * PlotViewRate;
            this.PlotAreaZoomed = !this.PlotAreaZoomed;

            //var chartArea = this.Chart_Plotarea.ChartAreas["ChartArea1"];
            //chartArea.CursorX.AutoScroll = true;
            //chartArea.AxisX.ScaleView.Zoomable = true;
            //chartArea.AxisX.ScaleView.SizeType = DateTimeIntervalType.Number;
            //int position = 0;
            //int size = 2;
            //chartArea.AxisX.ScaleView.Zoom(position, size);
        }

        private void ClearPlotArea()
        {
            this.Chart_Plotarea.Series.Clear();
            this.Chart_Plotarea.Series.Add("S1");
            this.Chart_Plotarea.Series["S1"].ChartType = SeriesChartType.Line;

        }
    }
}
