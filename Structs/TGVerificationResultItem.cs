using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace i_ConVerificationSystem.Structs
{
    public class TGVerificationResultItem
    {
        private static TGVerificationResultItem instance = new TGVerificationResultItem();
        public static TGVerificationResultItem Instance
        {
            get
            {
                return instance;
            }
        }
        public void Clear()
        {
            instance = new TGVerificationResultItem();
        }

        /// <summary>
        /// 照査が実行されているか
        /// </summary>
        /// <returns></returns>
        public bool HasVerificationResult()
        {
            return tgvrPairs.Any();
        }

        /// <summary>
        /// エラーがあるか
        /// </summary>
        /// <returns></returns>
        public bool HasError()
        {
            foreach (var item in tgvrPairs)
            {
                if (item.Value.HasError)
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
            foreach (var item in tgvrPairs)
            {
                errCount += item.Value.ErrorCount;
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
            foreach (var item in tgvrPairs)
            {
                okcCount += item.Value.OK_CCount;
            }

            return okcCount;
        }

        public Dictionary<string, TGVerificationResultItems> tgvrPairs { get; set; }

        public class TGVerificationResultItems
        {
            private XElement selXAlignment { get; set; }
            public string alignmentName { get; set; }
            public TG_VerificationResult VR_StraightLineTransverseGradient { get; set; }
            public List<TG_VerificationResult> VR_SidewalkCrownList { get; set; }
            public List<TG_MOG_VerificationResult> VR_MaximumOnesidedGradientList { get; set; }
            public List<TG_RSG_VerificationResult> VR_RoadShoulderGradientList { get; set; }

            /// <summary>
            /// エラーがあるか
            /// </summary>
            public bool HasError
            {
                get
                {
                    if (VR_StraightLineTransverseGradient.HasError)
                    {
                        return true;
                    }
                    foreach (var item in VR_SidewalkCrownList)
                    {
                        if (item.HasError)
                        {
                            return true;
                        }
                    }
                    foreach (var item in VR_MaximumOnesidedGradientList)
                    {
                        if (item.HasError)
                        {
                            return true;
                        }
                    }
                    foreach (var item in VR_RoadShoulderGradientList)
                    {
                        if (item.HasError)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            /// <summary>
            /// OK(確認)があるか
            /// </summary>
            public bool HasOK_C
            {
                get
                {
                    if (VR_StraightLineTransverseGradient.HasOK_C)
                    {
                        return true;
                    }
                    foreach (var item in VR_SidewalkCrownList)
                    {
                        if (item.HasOK_C)
                        {
                            return true;
                        }
                    }
                    foreach (var item in VR_MaximumOnesidedGradientList)
                    {
                        if (item.HasOK_C)
                        {
                            return true;
                        }
                    }
                    foreach (var item in VR_RoadShoulderGradientList)
                    {
                        if (item.HasOK_C)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            /// <summary>
            /// エラーの数を返答
            /// </summary>
            public int ErrorCount
            {
                get
                {
                    int errCount = 0;
                    if (VR_StraightLineTransverseGradient.HasError)
                    {
                        errCount++;
                    }
                    foreach (var item in VR_SidewalkCrownList)
                    {
                        if (item.HasError)
                        {
                            errCount++;
                        }
                    }
                    foreach (var item in VR_MaximumOnesidedGradientList)
                    {
                        if (item.HasError)
                        {
                            errCount++;
                        }
                    }
                    foreach (var item in VR_RoadShoulderGradientList)
                    {
                        if (item.HasError)
                        {
                            errCount++;
                        }
                    }

                    return errCount;
                }
            }

            /// <summary>
            /// OK(確認)の数を返答
            /// </summary>
            public int OK_CCount
            {
                get
                {
                    int okcCount = 0;
                    if (VR_StraightLineTransverseGradient.HasOK_C)
                    {
                        okcCount++;
                    }
                    foreach (var item in VR_SidewalkCrownList)
                    {
                        if (item.HasOK_C)
                        {
                            okcCount++;
                        }
                    }
                    foreach (var item in VR_MaximumOnesidedGradientList)
                    {
                        if (item.HasOK_C)
                        {
                            okcCount++;
                        }
                    }
                    foreach (var item in VR_RoadShoulderGradientList)
                    {
                        if (item.HasOK_C)
                        {
                            okcCount++;
                        }
                    }

                    return okcCount;
                }
            }
        }
        
        public class TG_VerificationResult : VerificationResult
        {
            public string stdValue { get; set; } = "-";
            public string designValue { get; set; } = "-";
        }
        public class TG_MOG_VerificationResult : TG_VerificationResult
        {
            public string radiusItem { get; set; } = "-";
        }
        public class TG_RSG_VerificationResult : VerificationResult
        {
            public string radiusItem { get; set; } = "-";
            public string isRequireChangingGradient { get; set; } = "-";
            public string isChangingGradient { get; set; } = "-";
            public string changingGradientPosition { get; set; } = "-";
            public string onesidedRoadGradient { get; set; } = "-";
            public string roadshoulderGradient { get; set; } = "-";
        }

        private TGVerificationResultItem()
        {
            tgvrPairs = new Dictionary<string, TGVerificationResultItems>();
        }

        private void IsExistsKey(string ali)
        {
            if (!(tgvrPairs.ContainsKey(ali)))
            {
                tgvrPairs.Add(ali, new TGVerificationResultItems()
                {
                    alignmentName = ali
                });
            }
        }

        public void Update(string aliName, TGVerificationResultItems tgvr)
        {
            IsExistsKey(aliName);

            tgvrPairs[aliName] = tgvr;
        }

        public void UpdateForSTG(string aliName, TG_VerificationResult stgVR)
        {
            IsExistsKey(aliName);

            tgvrPairs[aliName].VR_StraightLineTransverseGradient = stgVR;
        }

        public void UpdateForSC(string aliName, List<TG_VerificationResult> scVRList)
        {
            IsExistsKey(aliName);

            tgvrPairs[aliName].VR_SidewalkCrownList = scVRList;
        }

        public void UpdateForMOG(string aliName, List<TG_MOG_VerificationResult> mogVRList)
        {
            IsExistsKey(aliName);

            tgvrPairs[aliName].VR_MaximumOnesidedGradientList = mogVRList;
        }

        public void UpdateForRSG(string aliName, List<TG_RSG_VerificationResult> rsgVRList)
        {
            IsExistsKey(aliName);

            tgvrPairs[aliName].VR_RoadShoulderGradientList = rsgVRList;
        }
    }
}
