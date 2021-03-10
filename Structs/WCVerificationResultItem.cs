using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static i_ConVerificationSystem.Structs.OGExtensions;
using static i_ConVerificationSystem.Structs.VerificationResult;

namespace i_ConVerificationSystem.Structs
{
    public class WCVerificationResultItem
    {
        private static WCVerificationResultItem instance = new WCVerificationResultItem();
        public static WCVerificationResultItem Instance
        {
            get
            {
                return instance;
            }
        }
        public void Clear()
        {
            instance = new WCVerificationResultItem();
        }

        /// <summary>
        /// 照査が実行されているか
        /// </summary>
        /// <returns></returns>
        public bool HasVerificationResult()
        {
            return wctvrPairs.Any();
        }

        /// <summary>
        /// エラーがあるか
        /// </summary>
        /// <returns></returns>
        public bool HasError()
        {
            foreach (var item in wctvrPairs)
            {
                if (item.Value.Item1.GetTotalResult() == VerifyResultType.NG)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// エラーの数を返答
        /// </summary>
        /// <returns></returns>
        public int GetErrorCount()
        {
            int errCount = 0;
            foreach (var item in wctvrPairs)
            {
                errCount += item.Value.Item1.GetErrorCount();
            }
            return errCount;
        }

        /// <summary>
        /// OK(確認)の数を返答
        /// </summary>
        /// <returns></returns>
        public int GetOK_CCount()
        {
            int okcCount = 0;
            foreach (var item in wctvrPairs)
            {
                okcCount += item.Value.Item1.GetOK_CCount();
            }
            return okcCount;
        }

        public Dictionary<Tuple<string, string>, Tuple<TotalResult_WidthComposition, CrossSect_OGExtension>> wctvrPairs { get; set; }
        private WCVerificationResultItem()
        {
            wctvrPairs = new Dictionary<Tuple<string, string>, Tuple<TotalResult_WidthComposition, CrossSect_OGExtension>>();
        }

        private void IsExistsKey(string ali, string sta)
        {
            var isExists = (from T in wctvrPairs where T.Key.Item1 == ali && T.Key.Item2 == sta select T).Any();

            if (isExists == false)
            {
                wctvrPairs.Add(new Tuple<string, string>(ali, sta), new Tuple<TotalResult_WidthComposition, CrossSect_OGExtension>(new TotalResult_WidthComposition(), new CrossSect_OGExtension()));
            }
        }

        public void Update(string aliName, string sta, TotalResult_WidthComposition wctvr, CrossSect_OGExtension ogcs)
        {
            IsExistsKey(aliName, sta);
            wctvrPairs[new Tuple<string,string>(aliName, sta)] = new Tuple<TotalResult_WidthComposition, CrossSect_OGExtension>(wctvr, ogcs);
        }
    }

    public class WCVerificationResultItems
    {
        private TotalResult_WidthComposition _wcTotalResult { get; set; }
        public WCVerificationResultItems(TotalResult_WidthComposition wcTotalResult)
        {
            _wcTotalResult = wcTotalResult;
        }

        public List<WCVRIViewItem> GetViewItemList()
        {
            return new List<WCVRIViewItem>()
            {
                new WCVRIViewItem()
                {
                    title = "車線数",
                    resultType = _wcTotalResult.CarriagewayLaneCount.ResultType,
                    message = _wcTotalResult.CarriagewayLaneCount.Message,
                },
                new WCVRIViewItem()
                {
                    title = "中央帯および中央帯側帯",
                    resultType = _wcTotalResult.CenterLane.ResultType,
                    message = _wcTotalResult.CenterLane.Message
                },
                new WCVRIViewItem()
                {
                    title = "路肩",
                    resultType = _wcTotalResult.RoadShoulder.ResultType,
                    message = _wcTotalResult.RoadShoulder.Message
                },
                new WCVRIViewItem()
                {
                    title = "路肩側帯",
                    resultType = _wcTotalResult.MarginalStrip.ResultType,
                    message = _wcTotalResult.MarginalStrip.Message
                },
                new WCVRIViewItem()
                {
                    title = "停車帯",
                    resultType = _wcTotalResult.StoppingLane.ResultType,
                    message = _wcTotalResult.StoppingLane.Message
                },
                new WCVRIViewItem()
                {
                    title = "歩行者・自転車通行空間",
                    resultType = _wcTotalResult.SideWalkAndBicycleLane.ResultType,
                    message = _wcTotalResult.SideWalkAndBicycleLane.Message
                },
                new WCVRIViewItem()
                {
                    title = "植樹帯",
                    resultType = _wcTotalResult.PlantingLane.ResultType,
                    message = _wcTotalResult.PlantingLane.Message
                }
            };
        }

        public class WCVRIViewItem
        {
            public string title { get; set; }
            public VerifyResultType resultType { get; set; }
            public string message { get; set; }
        }
    }
}
