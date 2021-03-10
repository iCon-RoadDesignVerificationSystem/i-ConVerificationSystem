using i_ConVerificationSystem.Structs.JSON;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static i_ConVerificationSystem.Forms.Base.LandXMLStdInformation;
using static i_ConVerificationSystem.Forms.Gradient.GGInputParameter;
using static i_ConVerificationSystem.Forms.Gradient.OGInputParameter;
using static i_ConVerificationSystem.Forms.Gradient.TGInputParameter;
using static i_ConVerificationSystem.Forms.WidthComposition.WCInputParameter;
using static i_ConVerificationSystem.Structs.OGExtensions;
using static i_ConVerificationSystem.Structs.OGExtensions.DesignCrossSectSurf_OGExtension;

namespace i_ConVerificationSystem.JSON
{
    internal sealed class AppSettingsManager
    {
        /// <summary>
        /// Singletonインスタンス
        /// </summary>
        private static AppSettingsManager instance = new AppSettingsManager();

        public static AppSettingsManager Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private AppSettingsManager()
        {
            //初期表示用
            InitializeAppSettings();
        }

        private AppSettings appSettings { get; set; }

        /// <summary>
        /// ロード済みフラグ
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                return (!(JsonFilePath.Value is null));
            }
        }

        /// <summary>
        /// Jsonファイルパス
        /// </summary>
        public ReactiveProperty<string> JsonFilePath { get; private set; } = new ReactiveProperty<string>();

        /// <summary>
        /// 最終更新日時（ReactivePropertyで画面表示可能）
        /// </summary>
        public ReactiveProperty<DateTime?> UpdatedTime { get; private set; } = new ReactiveProperty<DateTime?>();

        /// <summary>
        /// 最終保存日時（ReactivePropertyで画面表示可能）
        /// </summary>
        public ReactiveProperty<DateTime?> SavedTime { get; private set; } = new ReactiveProperty<DateTime?>();

        /// <summary>
        /// 条件値ファイル保存要求
        /// </summary>
        public bool SaveRequire
        {
            get
            {
                if (UpdatedTime.Value is null) return false;
                if (UpdatedTime.Value < SavedTime.Value) return false;
                return true;
            }
        }

        /// <summary>
        /// 条件値をクリア（クリアボタン？）
        /// </summary>
        private void InitializeAppSettings()
        {
            //条件値をクリア
            appSettings = new AppSettings();
            JsonFilePath.Value = null;
            UpdatedTime.Value = null;
            SavedTime.Value = null;
        }

        public void ClearAppSettings()
        {
            InitializeAppSettings();
        }

        /// <summary>
        /// 設定ファイル読込
        /// </summary>
        /// <param name="jsonFilePath"></param>
        /// <returns></returns>
        public bool LoadJsonFile(string jsonFilePath)
        {
            ClearAppSettings();
            this.JsonFilePath.Value = jsonFilePath;
            var jsonStr = File.ReadAllText(jsonFilePath);
            var jParser = new JsonParser<AppSettings>();
            appSettings = jParser.DeselializeObj(jsonStr);

            return true;
        }

        /// <summary>
        /// 設定ファイル読込
        /// </summary>
        /// <returns></returns>
        public bool LoadJsonFile()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.FileName = "*.json";
                dialog.Filter = "JSONファイル|.json";
                dialog.FilterIndex = 1;
                dialog.Title = "条件値ファイルを開く";

                if (dialog.ShowDialog() == DialogResult.Cancel)
                {
                    return false;
                }
                else
                {
                    return LoadJsonFile(dialog.FileName);
                }
            }
        }

        /// <summary>
        /// 設定ファイル保存
        /// </summary>
        /// <returns></returns>
        public bool SaveJsonFile()
        {
            var jParser = new JsonParser<AppSettings>();
            var jsonStr = jParser.SelializeStr(appSettings);

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.InitialDirectory = JsonFilePath.Value;
                dialog.FileName = "*.json";
                dialog.Filter = "JSONファイル|.json";
                dialog.FilterIndex = 1;
                dialog.Title = "条件値ファイルを保存";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    JsonFilePath.Value = dialog.FileName;
                    File.WriteAllText(JsonFilePath.Value, jsonStr);
                    SavedTime.Value = DateTime.Now;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 初期処理
        /// </summary>
        /// <param name="alignmentName"></param>
        private void InitializeForGenerate(string alignmentName)
        {
            if (appSettings.alignments is null || appSettings.alignments.alignment is null)
            {
                //念のためNULLチェック
                appSettings.alignments = new AppSettings.Alignments();
                appSettings.alignments.alignment = new List<AppSettings.Alignment>();
            }

            var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();
            if (c is null)
            {
                c = new AppSettings.Alignment()
                {
                    name = alignmentName
                };
                //初出であれば新規追加
                appSettings.alignments.alignment.Add(c);
            }
        }

        /// <summary>
        /// 共通設定の保存
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <param name="rtt"></param>
        /// <param name="ds"></param>
        /// <param name="sltg"></param>
        public void GenerateAppSettingsForCommonSettings(string alignmentName, StdInformation std)
        {
            InitializeForGenerate(alignmentName);
            var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();

            c.commonsettings.type = $"{std.rtT.Item1}";
            c.commonsettings._class = $"{std.rtT.Item2}";
            c.commonsettings.ds = $"{std.ds}";
            c.commonsettings.sltg = $"{std.sltg}";
            c.commonsettings.interval = $"{std.interval}";
            UpdatedTime.Value = DateTime.Now;
        }

        ///// <summary>
        ///// 共通設定の保存
        ///// </summary>
        ///// <param name="alignmentName"></param>
        ///// <param name="rtt"></param>
        ///// <param name="ds"></param>
        ///// <param name="sltg"></param>
        //public void GenerateAppSettingsForCommonSettings(string alignmentName, Tuple<int, int> rtt, int ds)
        //{
        //    InitializeForGenerate(alignmentName);
        //    var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();

        //    c.commonsettings.type = rtt.Item1.ToString();
        //    c.commonsettings._class = rtt.Item2.ToString();
        //    c.commonsettings.ds = ds.ToString();
        //    UpdatedTime.Value = DateTime.Now;
        //}

        /// <summary>
        /// 共通設定の読込
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <returns></returns>
        public StdInformation DeselializeForCommonSettings(string alignmentName)
        {
            InitializeForGenerate(alignmentName);
            var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();
            if (c is null || c.commonsettings is null) throw new Exception("CommonSettingsの設定が見つかりませんでした。");

            var std = new StdInformation();
            try
            {
                std.rtT = new Tuple<int, int>(int.Parse(c.commonsettings.type), int.Parse(c.commonsettings._class));
                std.ds = int.Parse(c.commonsettings.ds);
                std.sltg = decimal.Parse(c.commonsettings.sltg);
                std.interval = int.TryParse(c.commonsettings.interval, out _) ?
                                    int.Parse(c.commonsettings.interval) : 0;
            }
            catch (Exception)
            {
                //Nothing
            }

            return std;
        }

        /// <summary>
        /// 幅員・幅員構成設定の保存
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <param name="wcip"></param>
        public void GenerateAppSettingsForWCSettings(string alignmentName, WCInputParams wcip)
        {
            InitializeForGenerate(alignmentName);
            var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();

            GenerateAppSettingsForCommonSettings(alignmentName, wcip.std);

            c.wcsettings.ptv = wcip.Ptv.ToString();
            c.wcsettings.lvmr = wcip.Lvmr.ToString();
            c.wcsettings.isbnp = wcip.IsBnp.ToString();
            c.wcsettings.isconnect41to31 = wcip.IsConnect41to31.ToString();
            c.wcsettings.isconnect41to32 = wcip.IsConnect41to32.ToString();
            c.wcsettings.rss = wcip.Rss.ToString();
            c.wcsettings.ispp = wcip.Ispp.ToString();
            c.wcsettings.qcycle = wcip.Qcycle.ToString();
            c.wcsettings.qpede = wcip.Qpede.ToString();
            c.wcsettings.ssft = wcip.Ssft.ToString();
            c.wcsettings.tg = wcip.Tg.ToString();
            c.wcsettings.islcp4 = wcip.Islcp4.ToString();
            UpdatedTime.Value = DateTime.Now;
        }

        /// <summary>
        /// 幅員・幅員構成設定の読込
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <returns></returns>
        public WCInputParams DeselializeForWCSettings(string alignmentName)
        {
            InitializeForGenerate(alignmentName);
            var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();
            if (c is null || c.wcsettings is null) throw new Exception("WCSettingsの設定が見つかりませんでした。");

            var std = DeselializeForCommonSettings(alignmentName);

            var wcip = new WCInputParams(
                std,
                long.Parse(c.wcsettings.ptv),
                decimal.Parse(c.wcsettings.lvmr),
                bool.Parse(c.wcsettings.isbnp),
                bool.Parse(c.wcsettings.isconnect41to31),
                bool.Parse(c.wcsettings.isconnect41to32),
                (RoadSideStandard)Enum.Parse(typeof(RoadSideStandard), c.wcsettings.rss),
                bool.Parse(c.wcsettings.ispp),
                bool.Parse(c.wcsettings.qcycle),
                bool.Parse(c.wcsettings.qpede),
                (StreetSideFacilitiesType)Enum.Parse(typeof(StreetSideFacilitiesType), c.wcsettings.ssft),
                (Topography)Enum.Parse(typeof(Topography), c.wcsettings.tg),
                bool.Parse(c.wcsettings.islcp4)
                );

            return wcip;
        }

        /// <summary>
        /// 横断勾配設定の保存
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <param name="tgip"></param>
        public void GenerateAppSettingsForTGSettings(string alignmentName, TGInputParameters tgip)
        {
            InitializeForGenerate(alignmentName);
            var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();

            GenerateAppSettingsForCommonSettings(alignmentName, tgip.si);

            c.tgsettings.rpt = tgip.RoadPavingType.ToString();
            c.tgsettings.spt = tgip.SidewalkPavingType.ToString();
            c.tgsettings.sltg = tgip.StraightLineTransverseGradient.ToString();
            c.tgsettings.isbf = tgip.IsBarrierFree.ToString();
            c.tgsettings.ismt = tgip.IsManyTraffic.ToString();
            c.tgsettings.issca = tgip.IsSnowyColdArea.ToString();
            c.tgsettings.isscoa = tgip.IsSnowyColdOtherArea.ToString();
            UpdatedTime.Value = DateTime.Now;
        }

        /// <summary>
        /// 横断勾配設定の読込
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <returns></returns>
        public TGInputParameters DeselializeForTGSettings(string alignmentName)
        {
            InitializeForGenerate(alignmentName);
            var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();
            if (c is null || c.tgsettings is null) throw new Exception("TGSettingsの設定が見つかりませんでした。");

            var std = DeselializeForCommonSettings(alignmentName);

            var tgip = new TGInputParameters();
            tgip.si = std;
            try
            {
                tgip.StraightLineTransverseGradient = decimal.Parse(c.tgsettings.sltg);
                tgip.IsBarrierFree = bool.Parse(c.tgsettings.isbf);
                tgip.IsManyTraffic = bool.Parse(c.tgsettings.ismt);
                tgip.IsSnowyColdArea = bool.Parse(c.tgsettings.issca);
                tgip.IsSnowyColdOtherArea = bool.Parse(c.tgsettings.isscoa);
                tgip.RoadPavingType = (RoadPavingType)Enum.Parse(typeof(RoadPavingType), c.tgsettings.rpt);
                tgip.SidewalkPavingType = (SidewalkPavingType)Enum.Parse(typeof(SidewalkPavingType), c.tgsettings.spt);
            }
            catch (Exception)
            {
                //Nothing
            }

            return tgip;
        }

        /// <summary>
        /// 積雪寒冷の度がはなはだしい地域または積雪寒冷地域のその他の地域に該当するか
        /// </summary>
        /// <returns></returns>
        public bool HasSnowyColdArea()
        {
            try
            {
                var conditions = (from T in appSettings.alignments.alignment select T);
                if (conditions is null) return false;

                foreach (var c in conditions)
                {
                    if (bool.Parse(c.tgsettings.issca) || bool.Parse(c.tgsettings.isscoa))
                    {
                        //1件でも積雪寒冷のチェックを入れていれば該当
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                //例外発生時は無しで返答する
                return false;
            }
        }

        /// <summary>
        /// 片勾配設定の保存
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <param name="ogip"></param>
        public void GenerateAppSettingsForOGSettings(string alignmentName, OGInputParameters ogip)
        {
            InitializeForGenerate(alignmentName);
            var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();

            GenerateAppSettingsForCommonSettings(alignmentName, ogip.si);

            c.ogsettings.fhp = ogip.FHP.ToString();
            c.tgsettings.sltg = ogip.StraightLineTransverseGradient.ToString();
            UpdatedTime.Value = DateTime.Now;
        }

        /// <summary>
        /// 片勾配設定の読込
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <returns></returns>
        public OGInputParameters DeselializeForOGSettings(string alignmentName)
        {
            InitializeForGenerate(alignmentName);
            var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();
            if (c is null || c.ogsettings is null) throw new Exception("OGSettingsの設定が見つかりませんでした。");

            var std = DeselializeForCommonSettings(alignmentName);

            var ogip = new OGInputParameters();
            ogip.si = std;

            if (c.tgsettings.sltg != null)
            {
                ogip.StraightLineTransverseGradient = decimal.Parse(c.tgsettings.sltg);
            }
            if (c.ogsettings.fhp != null)
            {
                ogip.FHP = (OGInputParameters.FHPosition)Enum.Parse(typeof(OGInputParameters.FHPosition), c.ogsettings.fhp);
            }

            return ogip;
        }

        /// <summary>
        /// 緩勾配設定の保存
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <param name="ggip"></param>
        public void GenerateAppSettingsForGGSettings(string alignmentName, GGInputParameters ggip) 
        {
            InitializeForGenerate(alignmentName);
            var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();

            GenerateAppSettingsForCommonSettings(alignmentName, ggip.si);

            c.ggsettings.bpn = ggip.bpn.ToString();
            c.ggsettings.bal = ggip.bAddLen.ToString();
            c.ggsettings.epn = ggip.epn.ToString();
            c.ggsettings.eal = ggip.eAddLen.ToString();
            UpdatedTime.Value = DateTime.Now;
        }

        /// <summary>
        /// 緩勾配設定の読込
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <returns></returns>
        public GGInputParameters DeselializeForGGSettings(string alignmentName)
        {
            InitializeForGenerate(alignmentName);
            var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();
            if (c is null || c.ggsettings is null) throw new Exception("GGSettingsの設定が見つかりませんでした。");

            var std = DeselializeForCommonSettings(alignmentName);

            var ggip = new GGInputParameters();
            ggip.si = std;
            try
            {
                ggip.bpn = int.Parse(c.ggsettings.bpn);
                ggip.bAddLen = decimal.Parse(c.ggsettings.bal);
                ggip.epn = int.Parse(c.ggsettings.epn);
                ggip.eAddLen = decimal.Parse(c.ggsettings.eal);
            }
            catch (Exception)
            {
                //Nothing
            }

            return ggip;
        }

        /// <summary>
        /// 標準幅員設定の保存（リスト）
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <param name="ogcsList"></param>
        public void GenerateAppSettingsForStd(string alignmentName, List<CrossSect_OGExtension> ogcsList, OGInputParameters.FHPosition fhp)
        {
            InitializeForGenerate(alignmentName);
            var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();
            //毎回作り直し
            c.stdwc = new List<AppSettings.Stdwc>();

            foreach (var ogcs in ogcsList)
            {
                GenerateAppSettingsForStd(alignmentName, ogcs);
            }

            c.ogsettings.fhp = fhp.ToString();

            UpdatedTime.Value = DateTime.Now;
        }

        /// <summary>
        /// 標準幅員設定の保存
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <param name="ogcs"></param>
        private void GenerateAppSettingsForStd(string alignmentName, CrossSect_OGExtension ogcs)
        {
            InitializeForGenerate(alignmentName);
            var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();
            var std = (from T in c.stdwc where T.name == ogcs.name select T).FirstOrDefault();
            if (std is null)
            {
                std = new AppSettings.Stdwc()
                {
                    name = ogcs.name,
                    isstd = ogcs.isStd.ToString(),
                    sta = ogcs.sta.ToString()
                };
                c.stdwc.Add(std);
            }
            //DCSSは全部作り直す
            std.dcss.Clear();

            foreach (var d in ogcs.dcssList)
            {
                var csp = d.cspList.OrderByDescending(s => s.roadPositionNo).FirstOrDefault();
                std.dcss.Add(new AppSettings.Dcss()
                {
                    code = csp.code,
                    g1 = d.group1.ToString(),
                    g2 = d.group2.ToString(),
                    g1name = d.group1Name.ToString(),
                    g2name = d.group2Name.ToString(),
                    name_j = d.name_J.ToString(),
                    istarget = d.isTarget.ToString(),
                    isepr = d.IsEndPointRoadway.ToString(),
                    isfh = d.IsFHPosition.ToString()
                });
            }
        }

        /// <summary>
        /// キー情報のみ持った標準幅員設定を読込
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <returns></returns>
        private List<CrossSect_OGExtension> DeselializeForStd(string alignmentName)
        {
            InitializeForGenerate(alignmentName);
            var c = (from T in appSettings.alignments.alignment where T.name == alignmentName select T).FirstOrDefault();
            if (c is null || c.stdwc is null) throw new Exception("StdSettingsの設定が見つかりませんでした。");

            var retCsList = new List<CrossSect_OGExtension>();

            foreach (var item in c.stdwc)
            {
                var cs = new CrossSect_OGExtension();
                cs.name = item.name;
                cs.isStd = bool.Parse(item.isstd);
                cs.sta = decimal.Parse(item.sta);

                foreach (var d in item.dcss)
                {
                    cs.dcssList.Add(new DesignCrossSectSurf_OGExtension()
                    {
                        cspList = new List<Structs.CrossSects.CrossSectPnt>()
                        {
                            new Structs.CrossSects.CrossSectPnt()
                            {
                                code = d.code
                            }
                        },
                        isTarget = bool.Parse(d.istarget),
                        group1 = (GroupCode)Enum.Parse(typeof(GroupCode), d.g1),
                        group2 = (GroupCode)Enum.Parse(typeof(GroupCode), d.g2),
                        group1Name = (Name_JItems)Enum.Parse(typeof(Name_JItems), d.g1name),
                        group2Name = (Name_JItems)Enum.Parse(typeof(Name_JItems), d.g2name),
                        name_J = (Name_JItems)Enum.Parse(typeof(Name_JItems), d.name_j),
                        IsEndPointRoadway = bool.Parse(d.isepr),
                        IsFHPosition = bool.Parse(d.isfh)
                    });
                }
                retCsList.Add(cs);
            }

            return retCsList;
        }

        /// <summary>
        /// 設定ファイルに定義したCSListを取得
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <param name="orgCsList"></param>
        /// <returns></returns>
        public List<CrossSect_OGExtension> GetCrossSect_OGList(string alignmentName, List<CrossSect_OGExtension> orgCsList, bool isGetAllCs = false)
        {
            var retList = new List<CrossSect_OGExtension>();

            try
            {
                var stdList = DeselializeForStd(alignmentName);
                if (stdList is null || stdList.Count() == 0) return orgCsList;

                foreach (var orgCs in orgCsList)
                {
                    //条件値にはnameのみ保存している
                    var stdCs = (from T in stdList where T.name == orgCs.name select T).FirstOrDefault();

                    if (stdCs == null)
                    {
                        if (isGetAllCs)
                        {
                            retList.Add(orgCs);
                        }
                    }
                    else
                    {
                        orgCs.isStd = stdCs.isStd;

                        foreach (var stdDcss in stdCs.dcssList)
                        {
                            foreach (var ordDcss in orgCs.dcssList)
                            {
                                var retCsp = ordDcss.cspList.OrderByDescending(s => s.roadPositionNo).FirstOrDefault();
                                var dgvCsp = stdDcss.cspList.OrderByDescending(s => s.roadPositionNo).FirstOrDefault();

                                if (retCsp.code == dgvCsp.code)
                                {
                                    ordDcss.isTarget = stdDcss.isTarget;
                                    ordDcss.name_J = stdDcss.name_J;
                                    ordDcss.group1 = stdDcss.group1;
                                    ordDcss.group1Name = stdDcss.group1Name;
                                    ordDcss.group2 = stdDcss.group2;
                                    ordDcss.group2Name = stdDcss.group2Name;
                                    ordDcss.IsFHPosition = stdDcss.IsFHPosition;
                                    ordDcss.IsEndPointRoadway = stdDcss.IsEndPointRoadway;
                                }
                            }
                        }

                        retList.Add(orgCs);
                    }

                    //foreach (var stdCs in stdList)
                    //{
                    //    if (stdCs.name != orgCs.name)
                    //    {
                    //        //標準外を取るために追加だけ
                    //        if (isGetAllCs && retList.Contains(orgCs, new CompareCrossSect_OGExtension()) == false)
                    //        {
                    //            retList.Add(orgCs);
                    //        }
                    //        continue;
                    //    }


                    //}
                }

                return retList;
            }
            catch (Exception)
            {
                return orgCsList;
            }
        }

        /// <summary>
        /// 標準幅員(issta=falseも含む)として設定したアイテムらstaで切り出し、その範囲内の未設定csに条件値を適用する
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <param name="orgCsList"></param>
        /// <returns></returns>
        public List<CrossSect_OGExtension> GetCrossSect_OGList_ApplyAllCS(string alignmentName, List<CrossSect_OGExtension> orgCsList)
        {
            var retList = new List<CrossSect_OGExtension>();

            try
            {
                var stdList = DeselializeForStd(alignmentName);
                if (stdList is null || stdList.Count() == 0) return orgCsList;

                int i = 0;
                while (i < stdList.Count)
                {
                    var stdCs = stdList[i];

                    //条件値のsta間を取得
                    var targetCsList = new List<CrossSect_OGExtension>();
                    if (stdList.Count - 1 == i)
                    {
                        //stdListのラストアイテム
                        targetCsList = (from T in orgCsList
                                        where stdCs.sta <= T.sta
                                        select T).ToList();
                    }
                    else
                    {
                        var nStdCs = stdList[i + 1];
                        targetCsList = (from T in orgCsList
                                        where stdCs.sta <= T.sta &&
                                              T.sta < nStdCs.sta
                                        select T).ToList();
                    }
                    foreach (var tarCs in targetCsList)
                    {
                        foreach (var stdDcss in stdCs.dcssList)
                        {
                            foreach (var ordDcss in tarCs.dcssList)
                            {
                                var retCsp = ordDcss.cspList.OrderByDescending(s => s.roadPositionNo).FirstOrDefault();
                                var dgvCsp = stdDcss.cspList.OrderByDescending(s => s.roadPositionNo).FirstOrDefault();

                                if (retCsp.code == dgvCsp.code)
                                {
                                    ordDcss.isTarget = stdDcss.isTarget;
                                    ordDcss.name_J = stdDcss.name_J;
                                    ordDcss.group1 = stdDcss.group1;
                                    ordDcss.group1Name = stdDcss.group1Name;
                                    ordDcss.group2 = stdDcss.group2;
                                    ordDcss.group2Name = stdDcss.group2Name;
                                    ordDcss.IsFHPosition = stdDcss.IsFHPosition;
                                    ordDcss.IsEndPointRoadway = stdDcss.IsEndPointRoadway;
                                }
                            }
                        }
                        retList.Add(tarCs);
                    }
                    i++;
                }

                return retList;
            }
            catch (Exception)
            {
                return orgCsList;
            }
        }
    }
}
