using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static i_ConVerificationSystem.Structs.OnesidedGradientVerificationResult;
using static i_ConVerificationSystem.Structs.OnesidedGradientVerificationResult.OGVerificationResult;
using static i_ConVerificationSystem.Structs.VerificationResult;

namespace i_ConVerificationSystem.Structs
{
    public class OGVerificationResultItem
    {
        private static OGVerificationResultItem instance = new OGVerificationResultItem();
        public static OGVerificationResultItem Instance
        {
            get
            {
                return instance;
            }
        }
        public void Clear()
        {
            instance = new OGVerificationResultItem();
        }
        public Dictionary<string, List<OnesidedGradientVerificationResult>> ogvrPairs { get; set; }

        /// <summary>
        /// 照査が実行されているか
        /// </summary>
        /// <returns></returns>
        public bool HasVerificationResult()
        {
            return ogvrPairs.Any();
        }

        /// <summary>
        /// エラーがあるか
        /// </summary>
        /// <returns></returns>
        public bool HasError()
        {
            foreach (var item in ogvrPairs)
            {
                foreach (var vr in item.Value)
                {
                    if (vr.beginPoint.HasError() ||
                        vr.endPoint.HasError())
                    {
                        return true;
                    }
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
            foreach (var item in ogvrPairs)
            {
                foreach (var vr in item.Value)
                {
                    errCount += vr.beginPoint.GetErrorCount();
                    errCount += vr.endPoint.GetErrorCount();
                }
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
            foreach (var item in ogvrPairs)
            {
                foreach (var vr in item.Value)
                {
                    okcCount += vr.beginPoint.GetOK_CCount();
                    okcCount += vr.endPoint.GetOK_CCount();
                }
            }
            return okcCount;
        }

        private OGVerificationResultItem()
        {
            ogvrPairs = new Dictionary<string, List<OnesidedGradientVerificationResult>>();
        }

        private void IsExistsKey(string ali)
        {
            if (!(ogvrPairs.ContainsKey(ali))){
                ogvrPairs.Add(ali, new List<OnesidedGradientVerificationResult>());
            }
        }

        public void Update(string aliName, List<OnesidedGradientVerificationResult> ogvrList)
        {
            IsExistsKey(aliName);
            ogvrPairs[aliName] = ogvrList;
        }

        public List<OnesidedGradientVerificationResult> GetOGVRList(string aliName)
        {
            IsExistsKey(aliName);
            return ogvrPairs[aliName];
        }
    }

    public class OGVerificationResultItems
    {
        private OGVerificationResult _ogvr { get; set; }
        private int _vrNum { get; set; }
        private bool _isBeginPoint { get; set; }
        public OGVerificationResultItems(int vrNum, bool isBeginPoint, OGVerificationResult ogvr)
        {
            _ogvr = ogvr;
            _vrNum = vrNum;
            _isBeginPoint = isBeginPoint;
        }
        public OGVerificationResultItems()
        {
            _ogvr = new OGVerificationResult();
            _vrNum = 0;
            _isBeginPoint = true;
        }

        public int vrNum { get { return _vrNum; } }
        public string point { get { return _isBeginPoint ? "起点側" : "終点側"; } }
        public string beginSta { get { return $"{Math.Round(_ogvr.beginSta, 3, MidpointRounding.AwayFromZero)}m"; } }
        public string endSta { get { return $"{Math.Round(_ogvr.endSta, 3, MidpointRounding.AwayFromZero)}m"; } }
        public string ls { get { return $"{Math.Round(_ogvr.Ls, 3, MidpointRounding.AwayFromZero)}m"; } }
        //public string requireGradient4Drainage { get { return _ogvr.RequireGradient4Drainage ? "要" : "不要"; } }
        public string straight2MaximumOnesidedGradientRate { get { return _ogvr.Straight2MaximumOnesidedGradientRate; } }
        public string changePoint2MaximumOnesidedGradientRate { get { return _ogvr.ChangePoint2MaximumOnesidedGradientRate; } }
        public string maximumOnesidedGradientRate2MaximumOnesidedGradientRate { get { return _ogvr.MaximumOnesidedGradientRate2MaximumOnesidedGradientRate; } }
        public string hasMitigationArea { get { return _ogvr.HasMitigationArea ? "あり" : "なし"; } }
        public OnesidedGradientShapeEnum onesidedGradientShape { get { return _ogvr.OnesidedGradientShape; } }
        public VerifyResultType vror_ResultType { get { return _ogvr.VR_OnesidedRate.ResultType; } }
        public string vror_Message { get { return _ogvr.VR_OnesidedRate.Message; } }
        public VerifyResultType vrg4d_ResultType { get { return _ogvr.VR_Gradient4Drainage.ResultType; } }
        public string vrg4d_Message { get { return _ogvr.VR_Gradient4Drainage.Message; } }
        public VerifyResultType vrma_ResultType { get { return _ogvr.VR_MitigationArea.ResultType; } }
        public string vrma_Message { get { return _ogvr.VR_MitigationArea.Message; } }
        public VerifyResultType vrtg_ResultType { get { return _ogvr.VR_TransverseGradient.ResultType; } }
        public string vrtg_Message { get { return _ogvr.VR_TransverseGradient.Message; } }
        public VerifyResultType vrcu_ResultType { get { return _ogvr.VR_Curves.ResultType; } }
        public string vrcu_Message { get { return _ogvr.VR_Curves.Message; } }
        public VerifyResultType vrtg0_ResultType { get { return _ogvr.VR_TransverseGradient0Point.ResultType; } }
        public string vrtg0_Message { get { return _ogvr.VR_TransverseGradient0Point.Message; } }
        public VerifyResultType vrs2c_ResultType { get { return _ogvr.VR_Strike2Curve.ResultType; } }
        public string vrs2c_Message { get { return _ogvr.VR_Strike2Curve.Message; } }
    }
}
