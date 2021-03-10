using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static i_ConVerificationSystem.Structs.CrossSects.DesignCrossSectSurf;
using static i_ConVerificationSystem.Structs.VerificationResult;
using static i_ConVerificationSystem.Structs.OGExtensions;
using static i_ConVerificationSystem.Structs.OGExtensions.DesignCrossSectSurf_OGExtension;
using i_ConVerificationSystem.Verification;

namespace i_ConVerificationSystem.Structs
{
    class WVerificationResultItem
    {
        private static WVerificationResultItem instance = new WVerificationResultItem();
        public static WVerificationResultItem Instance
        {
            get
            {
                return instance;
            }
        }
        public void Clear()
        {
            instance = new WVerificationResultItem();
        }

        /// <summary>
        /// 照査が実行されているか
        /// </summary>
        /// <returns></returns>
        public bool HasVerificationResult()
        {
            return wtvrPairs.Any();
        }

        /// <summary>
        /// NGがあるか
        /// </summary>
        /// <returns></returns>
        public bool HasError()
        {
            foreach (var item in wtvrPairs)
            {
                if (item.Value.Item1.HasError(item.Value.Item2)) return true;
            }
            return false;
        }

        /// <summary>
        /// NGの件数を返答する
        /// </summary>
        /// <returns></returns>
        public int GetErrorCount()
        {
            int errCount = 0;
            foreach (var item in wtvrPairs)
            {
                var v = new WVerificationResultItems(item.Value.Item1, item.Value.Item2);
                errCount += v.GetErrorCount();
            }
            return errCount;
        }

        /// <summary>
        /// OK(確認)の件数を返答する
        /// </summary>
        /// <returns></returns>
        public int GetOK_CCount()
        {
            int okcCount = 0;
            foreach (var item in wtvrPairs)
            {
                var v = new WVerificationResultItems(item.Value.Item1, item.Value.Item2);
                okcCount += v.GetOK_CCount();
            }
            return okcCount;
        }

        public Dictionary<Tuple<string, string>, Tuple<TotalResult_Width, CrossSect_OGExtension>> wtvrPairs { get; set; }
        private WVerificationResultItem()
        {
            wtvrPairs = new Dictionary<Tuple<string, string>, Tuple<TotalResult_Width, CrossSect_OGExtension>>();
        }

        private void IsExistsKey(string ali, string sta)
        {
            var isExists = (from T in wtvrPairs where T.Key.Item1 == ali && T.Key.Item2 == sta select T).Any();

            if (isExists == false)
            {
                wtvrPairs.Add(new Tuple<string, string>(ali, sta), new Tuple<TotalResult_Width, CrossSect_OGExtension>(new TotalResult_Width(), new CrossSect_OGExtension()));
            }
        }

        public void Update(string aliName, string sta, TotalResult_Width wtvr, CrossSect_OGExtension ogcs)
        {
            IsExistsKey(aliName, sta);
            wtvrPairs[new Tuple<string, string>(aliName, sta)] = new Tuple<TotalResult_Width, CrossSect_OGExtension>(wtvr, ogcs);
        }
    }

    public class WVerificationResultItems
    {
        private TotalResult_Width _wTotalResult { get; set; }
        private CrossSect_OGExtension _ogcs { get; set; }
        public WVerificationResultItems(TotalResult_Width wTotalResult, CrossSect_OGExtension ogcs)
        {
            _wTotalResult = wTotalResult;
            _ogcs = ogcs;
        }
        
        public List<WVRIViewItem> GetViewItemList()
        {
            var retList = new List<WVRIViewItem>();
            foreach (var dcss in _ogcs.dcssList.OrderBy(T => T))
            {
                var resultJ = new WVRIViewItem();
                if (dcss.group1 == GroupCode.None)
                {
                    //単線の判定結果
                    resultJ.gCode = $"{dcss.group1}";
                    resultJ.side = dcss.side;
                    resultJ.rNum = dcss.cspList.Max(row => row.roadPositionNo).ToString();
                    resultJ.name_J = CommonMethod.GetName_JFromGroupNameCode(dcss.name_J);
                    resultJ.rWidth = dcss.cspList.First().roadWidth;
                    resultJ.resultType = dcss.result is null ? VerifyResultType.SKIP : dcss.result.ResultType;
                    resultJ.message = dcss.result is null ? string.Empty : dcss.result.Message;
                }
                else
                {
                    //グループ1での判定結果
                    resultJ.gCode = $"G1{dcss.group1}";
                    //片側でまとまっているならそのまま使う。中央帯などまたいでいるならOther
                    resultJ.side = (from T in _ogcs.dcssList
                                    where T.group1 == dcss.group1 &&
                                    T.side != dcss.side
                                    select T).Any() ? DCSSSide.Other : dcss.side;
                    resultJ.rNum = string.Join("+", (from T in _ogcs.dcssList
                                                     where T.group1 == dcss.group1
                                                     select T.cspList.Max(row => row.roadPositionNo)).ToList());
                    resultJ.name_J = CommonMethod.GetName_JFromGroupNameCode(dcss.group1Name);
                    resultJ.rWidth = (from T in _ogcs.dcssList
                                      where T.group1 == dcss.group1
                                      select T.cspList.First().roadWidth).Sum();
                    resultJ.resultType = dcss.group1Result is null ? VerifyResultType.SKIP : dcss.group1Result.ResultType;
                    resultJ.message = dcss.group1Result is null ? string.Empty : dcss.group1Result.Message;
                }
                retList.Add(resultJ);

                if (dcss.group2 != GroupCode.None)
                {
                    var resultG2 = new WVRIViewItem()
                    {
                        gCode = $"G2{dcss.group2}",
                        //片側でまとまっているならそのまま使う。中央帯などまたいでいるならOther
                        side = (from T in _ogcs.dcssList
                                where T.group2 == dcss.group2 &&
                                T.side != dcss.side
                                select T).Any() ? DCSSSide.Other : dcss.side,
                        rNum = string.Join("+", (from T in _ogcs.dcssList
                                                 where T.group2 == dcss.group2
                                                 select T.cspList.Max(row => row.roadPositionNo)).ToList()),
                        name_J = CommonMethod.GetName_JFromGroupNameCode(dcss.group2Name),
                        rWidth = (from T in _ogcs.dcssList
                                  where T.group2 == dcss.group2
                                  select T.cspList.First().roadWidth).Sum(),
                        resultType = dcss.group2Result is null ? VerifyResultType.SKIP : dcss.group2Result.ResultType,
                        message = dcss.group2Result is null ? string.Empty : dcss.group2Result.Message
                    };
                    retList.Add(resultG2);
                }
            }

            return retList.Distinct(new CompareWVRIViewItem()).ToList(); 
        }

        public VerifyResultType GetTotalVerifyResult()
        {
            return _wTotalResult.GetTotalResult(_ogcs);
        }

        /// <summary>
        /// NGの件数を返答する
        /// </summary>
        /// <returns></returns>
        public int GetErrorCount()
        {
            var v = GetViewItemList();
            int errCount = 0;
            foreach (var item in v)
            {
                if (item.resultType == VerifyResultType.NG) errCount++;
            }
            return errCount;
        }

        /// <summary>
        /// OK(確認)の件数を返答する
        /// </summary>
        /// <returns></returns>
        public int GetOK_CCount()
        {
            var v = GetViewItemList();
            int okcCount = 0;
            foreach (var item in v)
            {
                if (item.resultType == VerifyResultType.OK_C) okcCount++;
            }
            return okcCount;
        }

        public class WVRIViewItem : IEquatable<WVRIViewItem>
        {
            public string gCode { get; set; }
            public DCSSSide side { get; set; }
            public string rNum { get; set; }
            public string name_J { get; set; }
            public decimal rWidth { get; set; }
            public VerifyResultType resultType { get; set; }
            public string message { get; set; }

            public bool Equals(WVRIViewItem other)
            {
                return gCode == other.gCode &&
                    side == other.side &&
                    rNum == other.rNum &&
                    name_J == other.name_J;
            }

            public override int GetHashCode()
            {
                return gCode.GetHashCode() ^ side.GetHashCode() ^ rNum.GetHashCode() ^ name_J.GetHashCode();
            }
        }

        public class CompareWVRIViewItem : IEqualityComparer<WVRIViewItem>
        {
            public bool Equals(WVRIViewItem x, WVRIViewItem y)
            {
                return x.gCode == y.gCode &&
                    x.side == y.side &&
                    x.rNum == y.rNum &&
                    x.name_J == y.name_J;
            }

            public int GetHashCode(WVRIViewItem obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
