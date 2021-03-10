using i_ConVerificationSystem.Extensions.EnumExtension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static i_ConVerificationSystem.Forms.Gradient.OGInputParameter.OGInputParameters;
using static i_ConVerificationSystem.Structs.CrossSects.DesignCrossSectSurf;
using static i_ConVerificationSystem.Structs.OGExtensions.DesignCrossSectSurf_OGExtension;

namespace i_ConVerificationSystem.Structs
{
    public class OGExtensions : CrossSects
    {
        public class CrossSect_OGExtension : CrossSect
        {
            public List<DesignCrossSectSurf_OGExtension> dcssList { get; set; } = new List<DesignCrossSectSurf_OGExtension>();
            public bool isStd { get; set; } = false;

            public bool ValidateForSave(FHPosition fhp)
            {
                bool validateOK = true;

                int fhCount = dcssList.Where(row => row.IsFHPosition).Count();
                int eprCount = dcssList.Where(row => row.IsEndPointRoadway).Count();

                switch (fhp)
                {
                    case FHPosition.Center:
                        if (fhCount != 0)
                        {
                            validateOK = false;
                        }
                        if (eprCount != 2)
                        {
                            validateOK = false;
                        }
                        break;
                    case FHPosition.Both:
                        if (2 < fhCount)
                        {
                            validateOK = false;
                        }
                        if (eprCount != 2)
                        {
                            validateOK = false;
                        }
                        break;
                    case FHPosition.Left:
                    case FHPosition.Right:
                        if (1 < fhCount)
                        {
                            validateOK = false;
                        }
                        if (eprCount != 1)
                        {
                            validateOK = false;
                        }
                        break;
                    default:
                        break;
                }

                return validateOK;
            }

            /// <summary>
            /// 車線数を取得
            /// </summary>
            /// <returns></returns>
            public int GetRoadCount()
            {
                var carriagewayItems = (from T in dcssList
                                        where (T.name_J == Name_JItems.Carriageway || T.name_J == Name_JItems.AdditionalLane ||
                                               (T.group1Name == Name_JItems.Carriageway && T.group1 != GroupCode.None) ||
                                               (T.group1Name == Name_JItems.AdditionalLane && T.group1 != GroupCode.None)) &&
                                               0 < T.cspList.First().roadWidth &&
                                               T.isTarget
                                        select T);

                if (carriagewayItems.Any())
                {
                    //Noneの区別ができないためGroupbyでは無理そう
                    int carriagewayCount = 0;
                    var countedDcssList = new List<DesignCrossSectSurf_OGExtension>();
                    foreach (var cItem in carriagewayItems)
                    {
                        if (cItem.group1 == GroupCode.None)
                        {
                            carriagewayCount++;
                        }
                        else
                        {
                            var isCounted = (from T in countedDcssList
                                             where T.group1 == cItem.group1
                                             select T).Any();
                            if (!isCounted)
                            {
                                carriagewayCount++;
                                countedDcssList.Add(cItem);
                            }
                        }
                    }

                    return carriagewayCount;
                }
                else return 0;
            }

            /// <summary>
            /// 総幅員を返答
            /// </summary>
            /// <returns></returns>
            public string GetTotalWidthString()
            {
                decimal totalWidth = 0;
                foreach (var dcss in dcssList)
                {
                    if (dcss.isTarget == false) continue;
                    totalWidth += dcss.cspList.First().roadWidth;
                }
                return Math.Round(totalWidth, 3, MidpointRounding.AwayFromZero).ToString();
            }

            /// <summary>
            /// 幅員が0以上である車道一つの幅員を返答する
            /// </summary>
            /// <param name="side"></param>
            /// <returns></returns>
            public decimal GetRoadwayWidth(DesignCrossSectSurf.DCSSSide side)
            {
                return dcssList.Where(row => (row.name_J == Name_JItems.Carriageway || row.group1Name == Name_JItems.Carriageway || row.group2Name == Name_JItems.Carriageway) &&
                                             0 < row.cspList.First().roadWidth && row.side == side)
                               .Select(row => row.cspList.First().roadWidth).First();
            }

            /// <summary>
            /// 左車線の路肩幅員を返答
            /// </summary>
            /// <returns></returns>
            public double GetRoadShoulderWidth(DCSSSide side)
            {
                return (double)dcssList.Where(row => row.side == side &&
                                                     (row.name_J == Name_JItems.Roadshoulder || 
                                                     (row.group1Name == Name_JItems.Roadshoulder && row.group1 != GroupCode.None) || 
                                                     (row.group2Name == Name_JItems.Roadshoulder && row.group2 != GroupCode.None)) &&
                                                     row.isTarget)
                                       .Select(row => row.cspList.First()).Sum(row => row.roadWidth);
            }

            public List<DesignCrossSectSurf_OGExtension> GetOnesidedTargetDCSSList()
            {
                var retVal = (from T in this.dcssList where T.IsFHPosition || T.IsEndPointRoadway select T).ToList();

                return retVal;
            }

            public CrossSect_OGExtension GetCSFromSta(List<CrossSect_OGExtension> csList, decimal startSta)
            {
                return (from T in csList 
                        where Math.Round(T.sta, 3, MidpointRounding.AwayFromZero) == Math.Round(startSta, 3, MidpointRounding.AwayFromZero)
                        select T).FirstOrDefault();
            }

            /// <summary>
            /// FH位置から見た幅員
            /// </summary>
            /// <param name="dcssList"></param>
            /// <returns></returns>
            public (decimal, decimal) GetRoadwaysWidthFromFHPosition()
            {
                return GetRoadwaysWidthFromFHPosition(dcssList);
            }

            /// <summary>
            /// FH位置から見た幅員
            /// </summary>
            /// <param name="dcssList"></param>
            /// <returns></returns>
            public (decimal, decimal) GetRoadwaysWidthFromFHPosition(List<DesignCrossSectSurf_OGExtension> dcssList)
            {
                //最大2件である前提
                var fhList = (from T in dcssList where T.IsFHPosition select T).ToList();
                //fh0or1：ep1or2 fh2:ep2
                var epList = (from T in dcssList where T.IsEndPointRoadway select T).ToList();

                Func<double, double, decimal> calcF = null;
                calcF = (i1, i2) =>
                {
                    return Math.Abs((decimal)(i1 - i2));
                };

                decimal retLeft, retRight;
                
                if (fhList.Count() == 2 && epList.Count() == 2)
                {
                    retLeft = calcF(fhList[0].GetWidthByCenter(DCSSSide.Left), epList[0].GetWidthByCenter(DCSSSide.Left));
                    retRight = calcF(fhList[1].GetWidthByCenter(DCSSSide.Right), epList[1].GetWidthByCenter(DCSSSide.Right));
                }
                else
                {
                    if (epList.Count() != 0)
                    {
                        retLeft = calcF((fhList.Count() == 0 ? 0 : fhList[0].GetWidthByCenter(DCSSSide.Left)), epList[0].GetWidthByCenter(DCSSSide.Left));
                        retRight = calcF((fhList.Count() == 0 ? 0 : fhList[0].GetWidthByCenter(DCSSSide.Right)), epList.Count() == 1 ? epList[0].GetWidthByCenter(DCSSSide.Right) : epList[1].GetWidthByCenter(DCSSSide.Right));
                    }
                    else
                    {
                        retLeft = 0;
                        retRight = 0;
                    }
                }

                return (retLeft, retRight);
            }

            /// <summary>
            /// 指定Sideの車道の片勾配を取得する
            /// </summary>
            /// <param name="side"></param>
            /// <returns></returns>
            public decimal GetCarriagewayGradient(DCSSSide side)
            {
                //適当な車道を取る
                var cItem = (from T in dcssList
                             where T.side == side &&
                                   T.isTarget &&
                                   0 < T.cspList.First().roadWidth &&
                                   T.name_J == Name_JItems.Carriageway
                             select T).FirstOrDefault();
                var cGradient = Math.Round((cItem.cspList[1].roadHight - cItem.cspList[0].roadHight) / (cItem.cspList[0].roadWidth) * 100, 3, MidpointRounding.AwayFromZero);
                if (side == DCSSSide.Right)
                {
                    cGradient = -cGradient;
                }

                return cGradient;
            }

            /// <summary>
            /// 路肩折れの判定
            /// </summary>
            /// <returns>路肩折れあり、路肩折れの場所（路肩、路肩側帯）、勾配</returns>
            public (bool, Name_JItems, decimal) IsChangingGradientInRoadShoulder(DCSSSide side)
            {
                //指定Sideの路肩を使って路肩折れを判定する
                //路肩か路肩側帯を取る
                var lList = (from T in dcssList
                             where T.side == side &&
                                   T.isTarget &&
                                   0 < T.cspList.First().roadWidth &&
                                   (T.name_J == Name_JItems.Roadshoulder || T.name_J == Name_JItems.MarginalStrip)
                             select T).ToList();

                ////適当な車道を取る
                var cGradient = GetCarriagewayGradient(side);

                bool isChangingGradient = false;
                Name_JItems changePoint = Name_JItems.None;
                decimal retGradient = 0.0M;

                foreach (var lRoadshoulder in lList)
                {
                    var rsGradient = Math.Round((lRoadshoulder.cspList[1].roadHight - lRoadshoulder.cspList[0].roadHight) / (lRoadshoulder.cspList[0].roadWidth) * 100, 3, MidpointRounding.AwayFromZero);
                    if (side == DCSSSide.Right)
                    {
                        rsGradient = -rsGradient;
                    }
                    if (cGradient != rsGradient)
                    {
                        isChangingGradient = true;
                        changePoint = lRoadshoulder.name_J;
                        retGradient = rsGradient;
                    }
                }

                return (isChangingGradient, changePoint, retGradient);
            }

            public override bool Equals(object obj)
            {
                if (obj == null || this.GetType() != obj.GetType()) return false;

                var other = (CrossSect_OGExtension)obj;
                if (this.dcssList.Count != other.dcssList.Count) return false;
                foreach (var dcss in this.dcssList)
                {
                    if (!(other.dcssList.Contains(dcss, new CompareDesignCrossSectSurf_OGExtension()))) return false;
                }
                return true;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        /// <summary>
        /// 比較用クラス
        /// </summary>
        public class CompareCrossSect_OGExtension : IEqualityComparer<CrossSect_OGExtension>
        {
            public bool Equals(CrossSect_OGExtension x, CrossSect_OGExtension y)
            {
                if (object.ReferenceEquals(x, y)) return true;
                return x.Equals(y);
            }

            public int GetHashCode(CrossSect_OGExtension obj)
            {
                return obj.GetHashCode();
            }
        }

        public class DesignCrossSectSurf_OGExtension : DesignCrossSectSurf
        {
            public Name_JItems name_J { get; set; } = Name_JItems.None;
            public GroupCode group1 { get; set; } = GroupCode.None;
            public Name_JItems group1Name { get; set; } = Name_JItems.None;
            public GroupCode group2 { get; set; } = GroupCode.None;
            public Name_JItems group2Name { get; set; } = Name_JItems.None;
            public bool isTarget { get; set; } = true;
            public VerificationResult result { get; set; } = new VerificationResult();
            public VerificationResult group1Result { get; set; } = new VerificationResult();
            public VerificationResult group2Result { get; set; } = new VerificationResult();
            public VerificationResult.VerifyResultType totalResult
            {
                get
                {
                    if (result is null && group1Result is null && group2Result is null) return VerificationResult.VerifyResultType.SKIP;

                    if (group1 == GroupCode.None)
                    {
                        if (!(result is null) && result.ResultType == VerificationResult.VerifyResultType.NG) return VerificationResult.VerifyResultType.NG;
                    }
                    else
                    {
                        if (!(group1Result is null) && group1Result.ResultType == VerificationResult.VerifyResultType.NG) return VerificationResult.VerifyResultType.NG;
                    }

                    if (group2 != GroupCode.None)
                    {
                        if (!(group2Result is null) && group2Result.ResultType == VerificationResult.VerifyResultType.NG) return VerificationResult.VerifyResultType.NG;
                    }

                    return VerificationResult.VerifyResultType.OK;
                }
            }

            public enum GroupCode
            {
                [Description("-")]
                None = 0,
                [Description("Group1")]
                Group1,
                [Description("Group2")]
                Group2,
                [Description("Group3")]
                Group3,
                [Description("Group4")]
                Group4,
                [Description("Group5")]
                Group5
            }

            public class GroupCodeProvider : EnumSourceProvider<GroupCode> { }

            /// <summary>
            /// 日本語名、グループ名Enum
            /// アイテム更新時はOGMapとCommonMethod.GetName_JFromGroupNameCodeを更新すること
            /// </summary>
            public enum Name_JItems
            {
                /// <summary>
                /// 初期状態
                /// </summary>
                None = 0,
                /// <summary>
                /// 中央帯
                /// </summary>
                CenterStrip,
                /// <summary>
                /// 中央分離帯
                /// </summary>
                CenterSprit,
                /// <summary>
                /// 中央帯側帯
                /// </summary>
                CenterMarginalStrip,
                /// <summary>
                /// 車道
                /// </summary>
                Carriageway,
                /// <summary>
                /// 付加車線
                /// </summary>
                AdditionalLane,
                /// <summary>
                /// 路肩
                /// </summary>
                Roadshoulder,
                /// <summary>
                /// 路肩側帯
                /// </summary>
                MarginalStrip,
                /// <summary>
                /// 右側路肩
                /// </summary>
                RoadshoulderR,
                /// <summary>
                /// 植樹帯
                /// </summary>
                PlantingLane,
                /// <summary>
                /// 歩道
                /// </summary>
                Sidewalk,
                /// <summary>
                /// 自転車歩行者道
                /// </summary>
                Sidepath,
                /// <summary>
                /// 自転車道
                /// </summary>
                Cycletrack,
                /// <summary>
                /// 自転車通行帯
                /// </summary>
                Bikelane,
                /// <summary>
                /// 停車帯
                /// </summary>
                StoppingArea,
                /// <summary>
                /// その他
                /// </summary>
                Other
            }

            /// <summary>
            /// 中心点からの幅員を取得
            /// </summary>
            /// <returns></returns>
            public double GetWidthByCenter(DCSSSide side)
            {
                if (IsFHPosition)
                {
                    //プレフィックス
                    var prefixStr = side == DCSSSide.Left ? "L" : "R";
                    var fhStr = "FH";
                    var startWithStr = $"{prefixStr}{fhStr}";
                    //1個しか取れないはず
                    var pos = (from T in cspList
                               where T.code.StartsWith(startWithStr)
                               select T.roadPositionX);
                    if (pos.Any())
                    {
                        return (double)pos.First();
                    }
                    else
                    {
                        //サイド指定で取れないならFHだけで探す
                        pos = (from T in cspList
                               where T.code.Contains(fhStr)
                               select T.roadPositionX);
                        if (pos.Any())
                        {
                            return (double)pos.First();
                        }
                        else
                        {
                            //それでも取れないなら中央から離れている路線端を返答する
                            pos = (from T in cspList
                                   orderby Math.Abs(T.roadPositionX) descending
                                   select T.roadPositionX);
                            if (pos.Any())
                            {
                                return (double)pos.First();
                            }
                            else
                            {
                                return 0;
                            }
                        }
                    }
                }
                else
                {
                    //車道縁なら端の測点を取る
                    return (double)(from T in cspList
                                    orderby Math.Abs(T.roadPositionX) descending
                                    select T.roadPositionX).First();
                }
            }

            /// <summary>
            /// FH位置
            /// </summary>
            public bool IsFHPosition { get; set; }

            /// <summary>
            /// 片勾配すりつけ計算における車道縁
            /// </summary>
            public bool IsEndPointRoadway { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null || this.GetType() != obj.GetType()) return false;

                var other = (DesignCrossSectSurf_OGExtension)obj;
                if (this.cspList.Count != other.cspList.Count) return false;
                foreach (var csp in this.cspList)
                {
                    if (!(other.cspList.Contains(csp, new CompareCrossSectPnt()))) return false;
                }
                return true;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        /// <summary>
        /// 比較用クラス
        /// </summary>
        public class CompareDesignCrossSectSurf_OGExtension : IEqualityComparer<DesignCrossSectSurf_OGExtension>
        {
            public bool Equals(DesignCrossSectSurf_OGExtension x, DesignCrossSectSurf_OGExtension y)
            {
                if (object.ReferenceEquals(x, y)) return true;
                return x.Equals(y);
            }

            public int GetHashCode(DesignCrossSectSurf_OGExtension obj)
            {
                return obj.GetHashCode();
            }
        }


        /// <summary>
        /// 幅員構成の照査結果
        /// </summary>
        public abstract class TotalResult
        {
            public abstract VerificationResult.VerifyResultType GetTotalResult();
        }

        public class TotalResult_WidthComposition : TotalResult
        {
            //照査項目
            public VerificationResult CarriagewayLaneCount { get; set; }
            public VerificationResult CenterLane { get; set; }
            public VerificationResult RoadShoulder { get; set; }
            public VerificationResult MarginalStrip { get; set; }
            public VerificationResult StoppingLane { get; set; }
            public VerificationResult SideWalkAndBicycleLane { get; set; }
            public VerificationResult PlantingLane { get; set; }

            public override VerificationResult.VerifyResultType GetTotalResult()
            {
                //NG,OKの2パターンのみ使用する
                var retVal = VerificationResult.VerifyResultType.OK;
                //自身のインスタンスのプロパティを全てチェックし、NGが含まれていれば総合結果NGとなる
                var results = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)?.ToList();
                foreach (var result in results)
                {
                    var resVal = result.GetValue(this);
                    if (resVal == null) continue;
                    if (!(resVal is VerificationResult other)) continue;
                    if (other.ResultType == VerificationResult.VerifyResultType.NG) retVal = VerificationResult.VerifyResultType.NG;
                }

                return retVal;
            }

            /// <summary>
            /// エラーの数を返答
            /// </summary>
            /// <returns></returns>
            public int GetErrorCount()
            {
                int errCount = 0;

                var results = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)?.ToList();
                foreach (var result in results)
                {
                    var resVal = result.GetValue(this);
                    if (resVal == null) continue;
                    if (!(resVal is VerificationResult other)) continue;
                    if (other.ResultType == VerificationResult.VerifyResultType.NG)
                    {
                        errCount++;
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

                var results = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)?.ToList();
                foreach (var result in results)
                {
                    var resVal = result.GetValue(this);
                    if (resVal == null) continue;
                    if (!(resVal is VerificationResult other)) continue;
                    if (other.ResultType == VerificationResult.VerifyResultType.OK_C)
                    {
                        okcCount++;
                    }
                }
                return okcCount;

            }
        }

        public class TotalResult_Width : TotalResult
        {
            //照査項目と判定はDesignCrossSectSurf_Extensionに持つ
            public VerificationResult.VerifyResultType GetTotalResult(CrossSect_OGExtension cs)
            {
                //NG,OKの2パターンのみ
                var retVal = VerificationResult.VerifyResultType.OK;

                //DCSSExのresultを全てチェックし、NGが含まれていれば総合結果NGとなる
                foreach (var item in cs.dcssList)
                {
                    if (item.result.ResultType == VerificationResult.VerifyResultType.NG) retVal = VerificationResult.VerifyResultType.NG;
                    if (item.group1Result.ResultType == VerificationResult.VerifyResultType.NG) retVal = VerificationResult.VerifyResultType.NG;
                    if (item.group2Result.ResultType == VerificationResult.VerifyResultType.NG) retVal = VerificationResult.VerifyResultType.NG;
                }

                return retVal;
            }

            /// <summary>
            /// NGがあるか
            /// </summary>
            /// <param name="cs"></param>
            /// <returns></returns>
            public bool HasError(CrossSect_OGExtension cs)
            {
                foreach (var item in cs.dcssList)
                {
                    if (item.result.HasError) return true;
                    if (item.group1Result.HasError) return true;
                    if (item.group2Result.HasError) return true;
                }
                return false;
            }

            public override VerificationResult.VerifyResultType GetTotalResult()
            {
                //プロパティから判別できないため常にNG
                return VerificationResult.VerifyResultType.NG;
            }
        }
    }
}
