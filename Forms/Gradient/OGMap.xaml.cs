using i_ConVerificationSystem.JSON;
using i_ConVerificationSystem.Structs;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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
using static i_ConVerificationSystem.Forms.Gradient.OGInputParameter;
using static i_ConVerificationSystem.Structs.CrossSects;
using static i_ConVerificationSystem.Structs.CrossSects.DesignCrossSectSurf;
using static i_ConVerificationSystem.Structs.OGExtensions;
using static i_ConVerificationSystem.Structs.OGExtensions.DesignCrossSectSurf_OGExtension;
using static i_ConVerificationSystem.Structs.VerificationResult;

namespace i_ConVerificationSystem.Forms.Gradient
{
    /// <summary>
    /// OGMap.xaml の相互作用ロジック
    /// </summary>
    public partial class OGMap : UserControl
    {
        public OGMap()
        {
            InitializeDataTable();
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 編集可不可
        /// </summary>
        [Browsable(true)]
        public bool IsEditable 
        {
            get { return stdGrid.IsEnabled; }
            set { stdGrid.IsEnabled = value; }
        }

        public ObservableCollection<OGDataGridItems> _DispItems { get; set; } = new ObservableCollection<OGDataGridItems>();
        private CrossSect_OGExtension cs { get; set; }
        private enum ColumnHeaderIdx
        {
            Code = 0,
            LRInfo,
            Target,
            RoadPNumber,
            ElementName,
            ElementName_JP,
            ElementName_JP_Modify,
            RoadWidth,
            RoadWidthGroupIdx,
            RoadWidthGroupName,
            RoadWidthGroupIdx2,
            RoadWidthGroupName2,
            FHPosition,
            EndPosition,
            VerificationResult
        }

        public class OGDataGridItems : INotifyPropertyChanged
        {
            public string Code { get; set; }
            public DCSSSide Side { get; set; }
            public ReactiveProperty<bool> IsTarget { get; set; } = new ReactiveProperty<bool>(true);
            public string Number { get; set; }
            public string Name { get; set; }
            public string Name_J { get; set; }
            public ReactiveProperty<Name_JItems> Name_J_Modify { get; set; } = new ReactiveProperty<Name_JItems>();
            public string Width { get; set; }
            public ReactiveProperty<DesignCrossSectSurf_OGExtension.GroupCode> Group1 { get; set; } = new ReactiveProperty<GroupCode>();
            public ReactiveProperty<Name_JItems> Group1Name { get; set; } = new ReactiveProperty<Name_JItems>();
            public ReactiveProperty<DesignCrossSectSurf_OGExtension.GroupCode> Group2 { get; set; } = new ReactiveProperty<GroupCode>();
            public ReactiveProperty<Name_JItems> Group2Name { get; set; } = new ReactiveProperty<Name_JItems>();
            public VerificationResult WVerificationResult { get; set; }
            public VerificationResult WG1VerificationResult { get; set; }
            public VerificationResult WG2VerificationResult { get; set; }
            public VerifyResultType WResultType
            {
                get
                {
                    if (WVerificationResult is null && WG1VerificationResult is null && WG2VerificationResult is null) return VerifyResultType.SKIP;
                    if (IsTarget.Value == false) return VerifyResultType.SKIP;

                    if (Group1.Value == GroupCode.None)
                    {
                        //単線
                        //グループ2と合わせて評価
                        if (Group2.Value == GroupCode.None)
                        {
                            return WVerificationResult.ResultType;
                        }
                        else
                        {
                            if (WVerificationResult.ResultType == VerifyResultType.NG || WG2VerificationResult.ResultType == VerifyResultType.NG) return VerifyResultType.NG;
                            if (WVerificationResult.ResultType == VerifyResultType.OK_C) return WVerificationResult.ResultType;
                            if (WG2VerificationResult.ResultType == VerifyResultType.OK_C) return WG2VerificationResult.ResultType;
                            return WVerificationResult.ResultType;
                        }
                    }
                    else
                    {
                        //グループ1あり
                        //グループ2と合わせて評価
                        if (Group2.Value == GroupCode.None)
                        {
                            return WG1VerificationResult.ResultType;
                        }
                        else
                        {
                            if (WG1VerificationResult.ResultType == VerifyResultType.NG || WG2VerificationResult.ResultType == VerifyResultType.NG) return VerifyResultType.NG;
                            if (WG1VerificationResult.ResultType == VerifyResultType.OK_C) return WG1VerificationResult.ResultType;
                            if (WG2VerificationResult.ResultType == VerifyResultType.OK_C) return WG2VerificationResult.ResultType;
                            return WG1VerificationResult.ResultType;
                        }
                    }
                }
            }
            public string WMessage { get { return WVerificationResult is null ? "" : WVerificationResult.Message; } }
            public ReactiveProperty<bool> IsFHPosition { get; set; } = new ReactiveProperty<bool>(false);
            public ReactiveProperty<bool> IsEndPosition { get; set; } = new ReactiveProperty<bool>(false);

#pragma warning disable CS0067 // イベント 'OGMap.OGDataGridItems.PropertyChanged' は使用されていません。
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067 // イベント 'OGMap.OGDataGridItems.PropertyChanged' は使用されていません。
        }

        private void InitializeDataTable()
        {
            _DispItems.Clear();
        }

        public void DrawWC(CrossSect_OGExtension argCs)
        {
            this.cs = argCs;
            InitializeDataTable();

            //var odcss = argCs.dcssList.OrderBy(T => T.cspList.OrderBy(T1 => T1), new DesignCrossSectSurf_OGExtension());
            var odcss = argCs.dcssList.OrderBy(T => T);
            foreach (var dcss in odcss)
            {
                var csp = dcss.cspList.OrderByDescending(s => s.roadPositionNo).FirstOrDefault();
                var dgvRow = new OGDataGridItems();

                dgvRow.Code = csp.code;
                dgvRow.Side = dcss.side;
                dgvRow.IsTarget.Value = dcss.isTarget;
                dgvRow.Number = csp.roadPositionNo.ToString();
                dgvRow.Name = dcss.name;
                dgvRow.Name_J = ConvertEN2JP(dcss.name);
                dgvRow.Width = $"{csp.GetRoadWidthString()}m";
                dgvRow.IsFHPosition.Value = dcss.IsFHPosition;
                dgvRow.IsEndPosition.Value = dcss.IsEndPointRoadway;
                dgvRow.WVerificationResult = dcss.result;
                dgvRow.WG1VerificationResult = dcss.group1Result;
                dgvRow.WG2VerificationResult = dcss.group2Result;

                dgvRow.Name_J_Modify.Value = dcss.name_J == Name_JItems.None ? GetComboboxIndexByName(dgvRow.Name_J) : dcss.name_J;
                dgvRow.Group1Name.Value = dcss.group1Name == Name_JItems.None ? GetComboboxIndexByName(dgvRow.Name_J) : dcss.group1Name;
                dgvRow.Group2Name.Value = dcss.group2Name == Name_JItems.None ? GetComboboxIndexByName(dgvRow.Name_J) : dcss.group2Name;
                dgvRow.Group1.Value = dcss.group1;
                dgvRow.Group2.Value = dcss.group2;

                _DispItems.Add(dgvRow);
            }
            PlotTransverse();
        }

        private void PlotTransverse()
        {
            TPlotArea.PlotTransverse(GetCrossSect_Extension());
        }

        /// <summary>
        /// 和英変換
        /// </summary>
        /// <param name="enStr"></param>
        /// <returns></returns>
        private string ConvertEN2JP(string enStr)
        {
            if (EN2JPDictionary.ContainsKey(enStr)) return EN2JPDictionary[enStr];
            else return enStr;
        }

        /// <summary>
        /// 日本語名でコンボボックスのインデックスを指定する
        /// </summary>
        /// <param name="jpStr"></param>
        /// <returns></returns>
        private Name_JItems GetComboboxIndexByName(string jpStr)
        {
            foreach (Name_JItemsToComboBox item in Name_JComboItems.Items)
            {
                if (item._Label == jpStr) return item._Value;
            }

            //見つからなかった場合は最終項目を返答する
            return Name_JItems.Other;
        }

        /// <summary>
        /// グループIDでコンボボックスのインデックスを指定する
        /// </summary>
        /// <param name="gID"></param>
        /// <returns></returns>
        private DesignCrossSectSurf_OGExtension.GroupCode GetComboboxIndexByGroupID(string gID)
        {
            foreach (GroupCodeToComboBox item in GroupCodeComboItems.Items)
            {
                if (item._Label == gID) return item._Value;
            }
            
            return DesignCrossSectSurf_OGExtension.GroupCode.None;
        }

        /// <summary>
        /// 英和辞書
        /// </summary>
        private Dictionary<string, string> EN2JPDictionary { get; }
        = new Dictionary<string, string>()
        {
            {"Carriageway","車道"},
            {"CenterStrip","中央帯"},
            {"RoadShoulder","路肩"},
            {"StoppingLane","停車帯"},
            {"SideWalk","歩道"},
            {"PlantingZone","植樹帯"},
            {"FrontageRoad","副道"},
            {"Track","軌道敷"},
            {"Separator","中央分離帯"},
            {"MarginalStrip","中央帯側帯"},
            {"SubBase","路床"},
            {"SubGrade","路体"},
            {"Excavation","床掘(掘削)"},
            {"SlopeFill","法面（盛土）"},
            {"SlopeCut","法面（切土）"},
            {"BermFill","小段（盛土）"},
            {"BermCut","小段（切土）"},
            {"RetainingWall","擁壁"},
            {"Drainage","側溝"},
            {"Pavement","舗装"},
            {"Other","その他"}
        };

        /// <summary>
        /// 画面入力値を取得
        /// </summary>
        /// <returns></returns>
        public CrossSect_OGExtension GetCrossSect_Extension()
        {
            var dgvCs = GetDGVCrossSect_Extension();
            var retVal = this.cs;

            foreach (var retDcss in retVal.dcssList)
            {
                foreach (var dgvDcss in dgvCs.dcssList)
                {
                    var retCsp = retDcss.cspList.OrderByDescending(s => s.roadPositionNo).FirstOrDefault();
                    var dgvCsp = dgvDcss.cspList.OrderByDescending(s => s.roadPositionNo).FirstOrDefault();

                    if (retCsp.code == dgvCsp.code)
                    {
                        retDcss.isTarget = dgvDcss.isTarget;
                        retDcss.name_J = dgvDcss.name_J;
                        retDcss.group1 = dgvDcss.group1;
                        retDcss.group1Name = dgvDcss.group1Name;
                        retDcss.group2 = dgvDcss.group2;
                        retDcss.group2Name = dgvDcss.group2Name;
                        retDcss.IsFHPosition = dgvDcss.IsFHPosition;
                        retDcss.IsEndPointRoadway = dgvDcss.IsEndPointRoadway;
                    }
                }
            }

            return retVal;
        }


        public bool ApplyStdSettingsFromOtherSetting(CrossSect_OGExtension cs, OGInputParameters.FHPosition fhp)
        {
            var isNotApplied = false;
            foreach (var copyItem in cs.dcssList)
            {
                bool isApplied = false;
                foreach (var orgItem in _DispItems)
                {
                    if (orgItem.Side == copyItem.side &&
                        orgItem.Number == $"{copyItem.cspList.Last().roadPositionNo}" &&
                        orgItem.Name == copyItem.name)
                    {
                        isApplied = true;

                        //同じサイドで同じ構成番号ならコピー。幅員、項目名も見ていいかも。
                        //幅員が0mなら強制的にチェック対象外としてもいいかも
                        orgItem.IsTarget.Value = copyItem.isTarget;
                        orgItem.Name_J_Modify.Value = copyItem.name_J;
                        orgItem.Group1.Value = copyItem.group1;
                        orgItem.Group1Name.Value = copyItem.group1Name;
                        orgItem.Group2.Value = copyItem.group2;
                        orgItem.Group2Name.Value = copyItem.group2Name;
                        orgItem.IsFHPosition.Value = copyItem.IsFHPosition;
                        orgItem.IsEndPosition.Value = copyItem.IsEndPointRoadway;
                    }
                }

                if (isApplied == false)
                {
                    //適用できなかった項目があった場合
                    isNotApplied = true;
                }
            }
            if (cs.dcssList.Count() < _DispItems.Count())
            {
                //コピー元よりコピー先の要素が多い場合は適用できない項目が必ずある
                isNotApplied = true;
            }

            var endPointItem = (from T in cs.dcssList
                               where T.IsEndPointRoadway
                               select T).FirstOrDefault();
            var endPointPos = Name_JItems.Carriageway;
            if (endPointItem != null)
            {
                //Group1で判定
                endPointPos = endPointItem.group1Name;
            }
            ApplyFHPosition(fhp, endPointPos);
            PlotTransverse();

            return isNotApplied;
        }

        /// <summary>
        /// FH位置と車道端の自動提案
        /// </summary>
        /// <param name="fhp"></param>
        public void ApplyFHPosition(OGInputParameters.FHPosition fhp, Name_JItems endPointPos)
        {
            Action<DCSSSide, Name_JItems> action = null;
            action = (s, n) =>
            {
                //指定サイドの中心から離れている要素に車道端チェックを入れる
                var w = (from T in _DispItems
                         where T.Side == s &&
                         (T.Name_J_Modify.Value == n ||
                          T.Group1Name.Value == n ||
                          T.Group2Name.Value == n)
                         orderby T.Number descending
                         select T).FirstOrDefault();
                if (w != null)
                {
                    w.IsEndPosition.Value = true;
                }
            };

            Func<DCSSSide, DCSSSide> func = null;
            func = (s) =>
            {
                //FH位置を返答する
                var prefix = s == DCSSSide.Left ? "L" : "R";
                var FHPrefix = "FH";

                var w = (from T in cs.dcssList
                         where (from T1 in T.cspList
                                where T1.code.Contains($"{prefix}{FHPrefix}")
                                select T1).Any()
                         select T).FirstOrDefault();
                if (w == null)
                {
                    w = (from T in cs.dcssList
                         where (from T1 in T.cspList
                                where T1.code.Contains($"{FHPrefix}")
                                select T1).Any()
                         select T).FirstOrDefault();
                }

                if (w != null)
                {
                    var fhPos = (from T in _DispItems
                                 where w.cspList.OrderByDescending(o => o.roadPositionNo).First().code == T.Code
                                 select T).FirstOrDefault();
                    if (fhPos != null)
                    {
                        //見つかるはず
                        fhPos.IsFHPosition.Value = true;
                    }
                    return w.side;
                }
                else
                {
                    return DCSSSide.Other;
                }
            };

            foreach (var item in _DispItems)
            {
                //車道中心なら全て外す
                item.IsFHPosition.Value = false;
                //念の為に車道縁も外す
                item.IsEndPosition.Value = false;
            }

            if (fhp != OGInputParameters.FHPosition.Center)
            {
                var lFHPos = func(DCSSSide.Left);
                var rFHPos = func(DCSSSide.Right);
            }
            switch (fhp)
            {
                case OGInputParameters.FHPosition.Center:
                case OGInputParameters.FHPosition.Both:
                    //車道端
                    action(DCSSSide.Left, endPointPos);
                    action(DCSSSide.Right, endPointPos);
                    break;
                case OGInputParameters.FHPosition.Left:
                    //車道端
                    action(DCSSSide.Right, endPointPos);
                    break;
                case OGInputParameters.FHPosition.Right:
                    //車道端
                    action(DCSSSide.Left, endPointPos);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 画面入力値を取得
        /// </summary>
        /// <returns></returns>
        private CrossSect_OGExtension GetDGVCrossSect_Extension()
        {
            //var dt = (DataTable)this.dataGridView1.ItemsSource;
            var dcssList = new List<DesignCrossSectSurf_OGExtension>();

            foreach (var rowItem in _DispItems)
            {
                var dcss = new DesignCrossSectSurf_OGExtension();
                dcss.isTarget = rowItem.IsTarget.Value;
                dcss.name_J = rowItem.Name_J_Modify.Value;
                dcss.group1 = rowItem.Group1.Value;
                dcss.group1Name = rowItem.Group1Name.Value;
                dcss.group2 = rowItem.Group2.Value;
                dcss.group2Name = rowItem.Group2Name.Value;
                dcss.IsFHPosition = rowItem.IsFHPosition.Value;
                dcss.IsEndPointRoadway = rowItem.IsEndPosition.Value;
                dcss.cspList = new List<CrossSects.CrossSectPnt>()
                {
                    new CrossSects.CrossSectPnt()
                    {
                        code = rowItem.Code
                    }
                };

                dcssList.Add(dcss);
            }

            return new CrossSect_OGExtension()
            {
                dcssList = dcssList
            };
        }

        private void c_dataGridScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            // Add MouseWheel support for the datagrid scrollviewer.
            c_dataGrid.AddHandler(MouseWheelEvent, new RoutedEventHandler(DataGridMouseWheelHorizontal), true);
        }

        private void DataGridMouseWheelHorizontal(object sender, RoutedEventArgs e)
        {
            MouseWheelEventArgs eargs = (MouseWheelEventArgs)e;
            double x = (double)eargs.Delta;
            double y = c_dataGridScrollViewer.VerticalOffset;
            c_dataGridScrollViewer.ScrollToVerticalOffset(y - x);
        }

        private void cmbName_J_DropDownClosed(object sender, EventArgs e)
        {
            if (IsLoaded == false) return;
            if (((OGDataGridItems)((FrameworkElement)sender).DataContext).Group1.Value == GroupCode.None)
            {
                ((OGDataGridItems)((FrameworkElement)sender).DataContext).Group1Name.Value =
                    ((OGDataGridItems)((FrameworkElement)sender).DataContext).Name_J_Modify.Value;
            }
            //再描画
            PlotTransverse();
        }

        private void cmbName_J_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded == false) return;
            if (((OGDataGridItems)((FrameworkElement)sender).DataContext).Group1.Value == GroupCode.None)
            {
                ((OGDataGridItems)((FrameworkElement)sender).DataContext).Group1Name.Value =
                    ((OGDataGridItems)((FrameworkElement)sender).DataContext).Name_J_Modify.Value;
            }
            //再描画
            PlotTransverse();
        }

        private void IsTarget_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoaded == false) return;
            //再描画
            PlotTransverse();
        }
    }

    /// <summary>
    /// 日本語名、グループ名用コンボボックスクラス
    /// </summary>
    public class Name_JItemsToComboBox
    {
        public string _Label { get; set; }
        public Name_JItems _Value { get; set; }
    }

    /// <summary>
    /// グループコード用コンボボックスクラス
    /// </summary>
    public class GroupCodeToComboBox
    {
        public string _Label { get; set; }
        public DesignCrossSectSurf_OGExtension.GroupCode _Value { get; set; }
    }
}
