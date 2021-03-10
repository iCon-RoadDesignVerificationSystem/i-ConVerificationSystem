using i_ConVerificationSystem.Structs.LandXML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static i_ConVerificationSystem.Structs.CrossSects.DesignCrossSectSurf;
using static i_ConVerificationSystem.Structs.OGExtensions;

namespace i_ConVerificationSystem.Verification
{
    class SlopeListClass
    {
        /// <summary>
        /// SlopeListがTrueである場合の片勾配すりつけ区間のリストを返答
        /// </summary>
        /// <param name="csList"></param>
        /// <param name="sltg"></param>
        /// <param name="selXAlignment"></param>
        /// <returns></returns>
        public List<Superelevation> GetConvertedSlopeValueList(List<CrossSect_OGExtension> csList, decimal sltg, XElement selXAlignment)
        {
            var svsList = GetSlopeValueStructList(csList, sltg, selXAlignment);
            var sList = ConvertSlopeValueStruct2Superelevation(svsList);
            return sList;
        }

        /// <summary>
        /// SlopeListがTrueである場合の片勾配すりつけ要素の特定と計算用リストの返答
        /// </summary>
        /// <param name="csList"></param>
        /// <param name="sltg"></param>
        /// <param name="selXAlignment"></param>
        /// <returns></returns>
        private List<SlopeValueStruct> GetSlopeValueStructList(List<CrossSect_OGExtension> csList, decimal sltg, XElement selXAlignment)
        {
            //すりつけ率を計算、qLeftとqRightをそれぞれ作る。
            //作った上記らを使ってすりつけ要素の判定をする
            //qLeft(Right)を作るには指定Staの幅員が必要

            decimal qLeft, qRight, nqLeft, nqRight;

            var spList = XMLLoader.Instance.GetSlopeList(selXAlignment);
            var isSingleLane = XMLLoader.Instance.IsSingleLaneRoad(selXAlignment);
            var csProvider = new CrossSect_OGExtension();

            List<SlopeValueStruct> retList = new List<SlopeValueStruct>();

            for (int i = 0; i < spList.SlopeValues.Count() - 2; i++)
            {
                //1件目は必ずNoneとなり破棄される
                var svs = new SlopeValueStruct(spList.SlopeValues[i]);
                var nSvs = new SlopeValueStruct(spList.SlopeValues[i + 1]);
                svs.staCs = csProvider.GetCSFromSta(csList, svs.sta);
                svs.iLeft = isSingleLane ? svs.gradientSingleLane : svs.gradientLeft;
                svs.iRight = isSingleLane ? svs.gradientSingleLane : svs.gradientRight;
                var (widthFHLeft, widthFHRight) = svs.staCs.GetRoadwaysWidthFromFHPosition();

                var nCs = csProvider.GetCSFromSta(csList, spList.SlopeValues[i + 1].sta);
                var nILeft = isSingleLane ? spList.SlopeValues[i + 1].gradientSingleLane : spList.SlopeValues[i + 1].gradientLeft;
                var nIRight = isSingleLane ? spList.SlopeValues[i + 1].gradientSingleLane : spList.SlopeValues[i + 1].gradientRight;
                var (nWidthFHLeft, nWidthFHRight) = nCs.GetRoadwaysWidthFromFHPosition();
                nSvs.staCs = nCs;
                nSvs.iLeft = nILeft;
                nSvs.iRight = nIRight;

                var nnCs = csProvider.GetCSFromSta(csList, spList.SlopeValues[i + 2].sta);
                var nnILeft = isSingleLane ? spList.SlopeValues[i + 2].gradientSingleLane : spList.SlopeValues[i + 2].gradientLeft;
                var nnIRight = isSingleLane ? spList.SlopeValues[i + 2].gradientSingleLane : spList.SlopeValues[i + 2].gradientRight;

                qLeft = CommonMethod.GetOnesidedGradientRate(widthFHLeft, svs.iLeft, nILeft, svs.staCs.sta, nCs.sta);
                qRight = CommonMethod.GetOnesidedGradientRate(widthFHRight, svs.iRight, nIRight, svs.staCs.sta, nCs.sta);
                svs.LeftDeltaI = nILeft - svs.iLeft;
                svs.RightDeltaI = nIRight - svs.iRight;

                nqLeft = CommonMethod.GetOnesidedGradientRate(nWidthFHLeft, nILeft, nnILeft, nCs.sta, nnCs.sta);
                nqRight = CommonMethod.GetOnesidedGradientRate(nWidthFHRight, nIRight, nnIRight, nCs.sta, nnCs.sta);
                nSvs.LeftDeltaI = nnILeft - nSvs.iLeft;
                nSvs.RightDeltaI = nnIRight - nSvs.iRight;

                this.GetSuperelevationType(qLeft, nqLeft, nILeft, nIRight, sltg, nSvs, true, svs.iLeft == nILeft, nILeft == nnILeft);
                this.GetSuperelevationType(qRight, nqRight, nILeft, nIRight, sltg, nSvs, false, svs.iRight == nIRight, nIRight == nnIRight);
                nSvs.sType = this.GetSuperelevationType(nSvs);
                retList.Add(nSvs);

                if (i == spList.SlopeValues.Count() - 3)
                {
                    //最後の判定をしているとき、SlopeValueの最後の要素をEndofRunoutとするか判定する
                    if (nnILeft == -sltg && nnIRight == sltg)
                    {
                        var finalSvs = new SlopeValueStruct(spList.SlopeValues[i + 2]);
                        finalSvs.staCs = nnCs;
                        finalSvs.iLeft = nnILeft;
                        finalSvs.iRight = nnIRight;
                        finalSvs.sType = SuperelevationType.EndofRunoutSta;
                        retList.Add(finalSvs);
                    }
                }
            }

            return retList;
        }

        /// <summary>
        /// 片勾配すりつけ要素の取得
        /// </summary>
        /// <param name="q"></param>
        /// <param name="nq"></param>
        /// <param name="ogip"></param>
        /// <param name="s"></param>
        /// <param name="isLeft"></param>
        /// <param name="sg">今回は同じ勾配率か</param>
        /// <param name="nSg">次回は同じ勾配率か</param>
        private void GetSuperelevationType(decimal q, decimal nq, decimal nILeft, decimal nIRight, decimal sltg, SlopeValueStruct s, bool isLeft, bool sg, bool nSg)
        {
            //if (q == nq)
            //{
            //    if (isLeft) s.sLType = SuperelevationType.None;
            //    else s.sRType = SuperelevationType.None;
            //    return;
            //}

            if (sg && !nSg && nILeft == -sltg && nIRight == sltg)
            {
                if (isLeft) s.sLType = SuperelevationType.BeginRunoutSta;
                else s.sRType = SuperelevationType.BeginRunoutSta;
                return;
            }

            if (!sg && nSg && nILeft == -sltg && nIRight == sltg)
            {
                if (isLeft) s.sLType = SuperelevationType.EndofRunoutSta;
                else s.sRType = SuperelevationType.EndofRunoutSta;
                return;
            }

            if (!sg && !nSg && nILeft == nIRight && Math.Abs(nILeft) == sltg)
            {
                if (isLeft) s.sLType = SuperelevationType.ReverseCrown;
                else s.sRType = SuperelevationType.ReverseCrown;
                return;
            }

            if (!sg && nSg && (isLeft ? nILeft != -sltg : nIRight != sltg))
            {
                if (isLeft) s.sLType = SuperelevationType.FullSuperSta;
                else s.sRType = SuperelevationType.FullSuperSta;
                return;
            }

            if (sg && !nSg && (isLeft ? nILeft != -sltg : nIRight != sltg))
            {
                if (isLeft) s.sLType = SuperelevationType.RunoffSta;
                else s.sRType = SuperelevationType.RunoffSta;
                return;
            }

            if (isLeft) s.sLType = SuperelevationType.None;
            else s.sRType = SuperelevationType.None;
            return;
        }

        /// <summary>
        /// 指定Staの片勾配すりつけ要素を判定
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private SuperelevationType GetSuperelevationType(SlopeValueStruct s)
        {
            if (s.sLType == SuperelevationType.ReverseCrown || s.sRType == SuperelevationType.ReverseCrown) return SuperelevationType.ReverseCrown;
            if (s.sLType == SuperelevationType.BeginRunoutSta || s.sRType == SuperelevationType.BeginRunoutSta) return SuperelevationType.BeginRunoutSta;
            if (s.sLType == SuperelevationType.EndofRunoutSta || s.sRType == SuperelevationType.EndofRunoutSta) return SuperelevationType.EndofRunoutSta;
            if (s.sLType == SuperelevationType.FullSuperSta || s.sRType == SuperelevationType.FullSuperSta) return SuperelevationType.FullSuperSta;
            if (s.sLType == SuperelevationType.RunoffSta || s.sRType == SuperelevationType.RunoffSta) return SuperelevationType.RunoffSta;
            if (s.sLType == s.sRType) return s.sLType;
            return SuperelevationType.None;
        }

        /// <summary>
        /// 片勾配すりつけ要素
        /// </summary>
        public enum SuperelevationType
        {
            None = 0,
            BeginRunoutSta,
            EndofRunoutSta,
            ReverseCrown,
            FullSuperSta,
            RunoffSta
        }

        /// <summary>
        /// 片勾配すりつけ要素特定用クラス
        /// </summary>
        public class SlopeValueStruct : SlopeList.SlopeValue
        {
            public decimal iLeft { get; set; } = 0;
            public decimal iRight { get; set; } = 0;
            public SuperelevationType sLType { get; set; } = SuperelevationType.None;
            public SuperelevationType sRType { get; set; } = SuperelevationType.None;
            public CrossSect_OGExtension staCs { get; set; }
            public SuperelevationType sType { get; set; } = SuperelevationType.None;
            public decimal LeftDeltaI { get; set; } = 0;
            public decimal RightDeltaI { get; set; } = 0;

            /// <summary>
            /// コンストラクタ（ダウンキャスト）
            /// </summary>
            /// <param name="sp"></param>
            public SlopeValueStruct(SlopeList.SlopeValue sp)
            {
                base.gradientLeft = sp.gradientLeft;
                base.gradientRight = sp.gradientRight;
                base.gradientSingleLane = sp.gradientSingleLane;
                base.sta = sp.sta;
            }

            /// <summary>
            /// 一つの車道幅員に対する片勾配すりつけ率を返答
            /// </summary>
            /// <param name="other"></param>
            /// <param name="isSingleLane"></param>
            /// <param name="isLeft"></param>
            /// <returns></returns>
            public decimal GetOnesidedGradientValue(SlopeValueStruct other, bool isSingleLane, bool isLeft)
            {
                if (isSingleLane)
                {
                    return this.staCs.GetRoadwayWidth(DCSSSide.Right) * Math.Abs(this.gradientSingleLane - other.gradientSingleLane) / (this.sta - other.sta);
                }
                else if (isLeft)
                {
                    return this.staCs.GetRoadwayWidth(DCSSSide.Left) * Math.Abs(this.gradientLeft - other.gradientLeft) / (this.sta - other.sta);
                }
                else
                {
                    return this.staCs.GetRoadwayWidth(DCSSSide.Right) * Math.Abs(this.gradientRight - other.gradientRight) / (this.sta - other.sta);
                }
            }
        }

        private List<Superelevation> ConvertSlopeValueStruct2Superelevation(List<SlopeValueStruct> sList)
        {
            bool during = true;
            decimal latestEndofRunoffSta = decimal.MinValue;
            decimal latestLElevation = 0.0M, latestRElevation = 0.0M;
            int i = 0;

            Func<List<SlopeValueStruct>, int, List<Superelevation>> func = null;
            func = (targetS, currentIndex) =>
            {
                var fList = new List<Superelevation>();
                bool fssAdded = false;
                bool rcAdded = false;
                bool rc2ItemAdded = false; 
                var fs = new Superelevation();

                while (i < targetS.Count())
                {
                    switch (targetS[i].sType)
                    {
                        case SuperelevationType.None:
                            break;
                        case SuperelevationType.BeginRunoutSta:
                            fs.BeginRunoutSta = targetS[i].sta;
                            latestLElevation = targetS[i].iLeft;
                            latestRElevation = targetS[i].iRight;
                            break;
                        case SuperelevationType.EndofRunoutSta:
                            fs.EndofRunoutSta = targetS[i].sta;
                            break;
                        case SuperelevationType.ReverseCrown:
                            if (rc2ItemAdded)
                            {
                                fList.AddRange(func(targetS, i));
                            }
                            else
                            {
                                if (rcAdded)
                                {
                                    fs.SetForEndSideReverseCrown(targetS[i].sta);
                                    latestLElevation = targetS[i].iLeft;
                                    latestRElevation = targetS[i].iRight;
                                    rc2ItemAdded = true;
                                }
                                else
                                {
                                    fs.SetForBeginSideReverseCrown(targetS[i].sta);
                                    latestLElevation = targetS[i].iLeft;
                                    latestRElevation = targetS[i].iRight;
                                    rcAdded = true;
                                }
                            }

                            break;
                        case SuperelevationType.FullSuperSta:
                            if (fssAdded)
                            {
                                fList.AddRange(func(targetS, i));
                            }
                            else
                            {
                                fs.FullSuperSta = targetS[i].sta;
                                if (Math.Abs(targetS[i].iLeft) < Math.Abs(targetS[i].iRight))
                                {
                                    fs.FullSuperelev = targetS[i].iRight;
                                }
                                else
                                {
                                    fs.FullSuperelev = targetS[i].iLeft;
                                }
                                fs.LeftDeltaI = Math.Abs(latestLElevation - targetS[i].iLeft);
                                fs.RightDeltaI = Math.Abs(latestRElevation - targetS[i].iRight);
                                fs.staStart = targetS[i].sta;
                                latestLElevation = targetS[i].iLeft;
                                latestRElevation = targetS[i].iRight;

                                fssAdded = true;
                            }
                            break;
                        case SuperelevationType.RunoffSta:
                            fs.RunoffSta = targetS[i].sta;
                            fs.staEnd = targetS[i].sta;
                            latestLElevation = targetS[i].iLeft;
                            latestRElevation = targetS[i].iRight;
                            break;
                        default:
                            break;
                    }

                    i++;
                }

                if (fs.HasBeginSideReverseCrown || fs.HasEndSideReverseCrown)
                {
                    fs.AdverseSE = Superelevation.EAdverseSE.adverse;
                }
                else
                {
                    fs.AdverseSE = Superelevation.EAdverseSE.nonAdverse;
                }
                fList.Add(fs);

                return fList;
            };

            var retList = new List<Superelevation>();
            while (during)
            {
                //funcカウンタのリセット
                i = 0;
                var superE = new Superelevation();
                var beginS = (from T in sList
                              where latestEndofRunoffSta < T.sta &&
                                    T.sType == SuperelevationType.BeginRunoutSta
                              orderby T.sta ascending
                              select T).FirstOrDefault();
                if (beginS == null)
                {
                    //取り終わったら変換終了
                    during = false;
                    continue;
                }

                var endS = (from T in sList
                            where latestEndofRunoffSta < T.sta &&
                                  T.sType == SuperelevationType.EndofRunoutSta
                            orderby T.sta ascending
                            select T).FirstOrDefault();
                decimal endSta = decimal.MaxValue;
                if (endS != null)
                {
                    //slopeListが最後まで定義されない可能性あり
                    endSta = endS.sta;
                }

                //BeginRunoutSta - EndofRunoutSta間のSlopeValueStructを取る
                var betweenS = (from T in sList
                                where beginS.sta <= T.sta &&
                                      T.sta <= endSta
                                select T).ToList();

                retList.AddRange(func(betweenS, 0));
                latestEndofRunoffSta = endSta;
            }

            retList = (from T in retList
                       orderby T.staStart ascending
                       select T).ToList();

            return retList;
        }
    }
}
