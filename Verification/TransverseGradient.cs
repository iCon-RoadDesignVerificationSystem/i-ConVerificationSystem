using i_ConVerificationSystem.JSON;
using i_ConVerificationSystem.Structs;
using i_ConVerificationSystem.Structs.LandXML;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static i_ConVerificationSystem.Forms.Gradient.TGInputParameter;
using static i_ConVerificationSystem.Structs.CrossSects.DesignCrossSectSurf;
using static i_ConVerificationSystem.Structs.OGExtensions;
using static i_ConVerificationSystem.Structs.OGExtensions.DesignCrossSectSurf_OGExtension;
using static i_ConVerificationSystem.Structs.TGVerificationResultItem;
using static i_ConVerificationSystem.Verification.SlopeListClass;

namespace i_ConVerificationSystem.Verification
{
    class TransverseGradient
    {
        private XElement selXAlignment { get; set; }
        private string selAlignmentName
        {
            get
            {
                return XMLLoader.Instance.GetAlignmentName(selXAlignment);
            }
        }

        /// <summary>
        /// 直線部の横断勾配の標準値（単一定義であれば定義値と一致してるか、複数定義であればその範囲内であるか）
        /// {1.5M} -> 直線部の横断勾配が1.5%であればOK。それ以外はNG
        /// {3.0M, 5.0M} -> 直線部の横断勾配が3.0%以上5.0%以下であればOK。それ以外はNG
        /// 最大2アイテム
        /// </summary>
        private decimal[] StdNormalCrown;
        private decimal[] GetStdNormalCrown()
        {
            return StdNormalCrown;
        }
        private void SetStdNormalCrown(TGInputParameters tgip, List<CrossSect_OGExtension> csList)
        {
            var maxRCount = 1;
            foreach (var cs in csList)
            {
                var rCount = (int)Math.Floor((double)cs.GetRoadCount() / 2);
                //単線であった場合は1車線あるものとする
                if (rCount == 0) rCount = 1;

                if (maxRCount < rCount) maxRCount = rCount;
            }

            var snc = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.TG.StdNormalCrown;

            if (snc is null || maxRCount == 0)
            {
                StdNormalCrown = new decimal[] { 0.0M };
                return;
            }

            StdNormalCrown = (from T in snc
                              where T.IsOnesideOneLine == (maxRCount == 1) &&
                                    T.RoadPavingType == tgip.RoadPavingType
                              select T.STDCrown).FirstOrDefault();
        }

        /// <summary>
        /// SlopeListを使うか
        /// </summary>
        private bool IsUseSlopeList { get; set; }
        private void SetIsUseSlopeList()
        {
            IsUseSlopeList = XMLLoader.Instance.IsUseSlopeList(selXAlignment);
        }

        private decimal NormalCrown { get; set; }
        private void SetNormalCrown()
        {
            var nc = XMLLoader.Instance.GetNormalCrown(selXAlignment);
            NormalCrown = decimal.Parse(nc);
        }


        private const int CHANGING_ROADSHOULDER_DEP_NEST = 5;
        private enum ChangingRoadShoulderChkNum
        {
            IsWidthUpperStdVal = 0,
            IsSnowyColdArea,
            IsChangingGradientInRoadShoulder,
            IsAppropriateGradient,
            IsAppropriateChangingGradientPoint
        }

        private struct ChangingRoadShoulderChkResult
        {
            public VerificationResult vResult;
            public int resultCode;
        }

        private readonly List<ChangingRoadShoulderChkResult> rscrList = new List<ChangingRoadShoulderChkResult>()
        {
            //1.路肩幅員が基準値を超えているか
            //2.積雪寒冷の度がはなはだしい地域に該当するか
            //3.路肩折れがあるか
            //4.車道の片勾配に応じた路肩勾配となっているか（路肩折れなしであればスキップ）
            //5.路肩折れの位置は側帯側となっているか（路肩折れなしであればスキップ）
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "I-0045", ResultType = VerificationResult.VerifyResultType.OK },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true,true,true,true,true})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0037", ResultType = VerificationResult.VerifyResultType.NG },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true,true,true,true,false})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0038", ResultType = VerificationResult.VerifyResultType.NG },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true,true,true,false,true})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0039", ResultType = VerificationResult.VerifyResultType.NG },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true,true,true,false,false})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "I-0046", ResultType = VerificationResult.VerifyResultType.OK_C },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true,true,false,false,false})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "I-0045", ResultType = VerificationResult.VerifyResultType.OK },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true,false,true,true,true})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0037", ResultType = VerificationResult.VerifyResultType.NG },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true,false,true,true,false})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0038", ResultType = VerificationResult.VerifyResultType.NG },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true,false,true,false,true})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0039", ResultType = VerificationResult.VerifyResultType.NG },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true,false,true,false,false})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "I-0046", ResultType = VerificationResult.VerifyResultType.OK_C },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true,false,false,false,false})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "I-0047", ResultType = VerificationResult.VerifyResultType.OK_C },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false,true,true,true,true})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0040", ResultType = VerificationResult.VerifyResultType.NG },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false,true,true,true,false})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0041", ResultType = VerificationResult.VerifyResultType.NG },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false,true,true,false,true})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0042", ResultType = VerificationResult.VerifyResultType.NG },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false,true,true,false,false})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "I-0048", ResultType = VerificationResult.VerifyResultType.OK },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false,true,false,false,false})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "I-0049", ResultType = VerificationResult.VerifyResultType.OK_C },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false,false,true,true,true})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0043", ResultType = VerificationResult.VerifyResultType.NG },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false,false,true,true,false})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0044", ResultType = VerificationResult.VerifyResultType.NG },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false,false,true,false,true})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0045", ResultType = VerificationResult.VerifyResultType.NG },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false,false,true,false,false})},
            new ChangingRoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "I-0050", ResultType = VerificationResult.VerifyResultType.OK },
                                               resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false,false,false,false,false})},
        };

        /// <summary>
        /// 横断勾配の照査
        /// </summary>
        /// <param name="tgip"></param>
        /// <param name="stdCs"></param>
        /// <param name="csList"></param>
        public void IsCorrectTransverseGradient(XElement selXAlignment, TGInputParameters tgip)
        {
            if (tgip is null) return;

            this.selXAlignment = selXAlignment;
            var stdCsList = XMLLoader.Instance.GetCrossSectsOG(this.selXAlignment, false);
            stdCsList = AppSettingsManager.Instance.GetCrossSect_OGList(selAlignmentName, stdCsList, false);

            SetIsUseSlopeList();
            SetNormalCrown();
            SetStdNormalCrown(tgip, stdCsList);

            //直線部の横断勾配の判定
            CheckForStraightLineTransverseGradient(tgip);
            //歩道等の横断勾配のチェック
            CheckForSideWalks(tgip, stdCsList);

            //csを全件取得する
            var allCsList = XMLLoader.Instance.GetCrossSectsOG(this.selXAlignment, true);
            var appliedCsList = AppSettingsManager.Instance.GetCrossSect_OGList_ApplyAllCS(this.selAlignmentName, allCsList);

            //車道の片勾配のチェック
            CheckForCarriageway(tgip, appliedCsList);

            //路肩折れのチェック
            var rsgVRList = new List<TG_RSG_VerificationResult>();

            var cList = XMLLoader.Instance.GetCurves(selXAlignment);
            List<Superelevation> sList;
            if (IsUseSlopeList)
            {
                var slopeProvider = new SlopeListClass();
                sList = slopeProvider.GetConvertedSlopeValueList(appliedCsList, tgip.StraightLineTransverseGradient, selXAlignment);
            }
            else
            {
                sList = XMLLoader.Instance.GetSuperelevationList(selXAlignment);
            }

            foreach (var c in cList)
            {
                var rsgVr = new TG_RSG_VerificationResult();
                //曲線半径
                rsgVr.radiusItem = $"{Math.Round(c.radius, 3, MidpointRounding.AwayFromZero)}m";

                var targetCs = new CrossSect_OGExtension().GetCSFromSta(appliedCsList, c.BC);
                //csは取れるはず
                if (targetCs is null) continue;

                var rsgSltg = tgip.StraightLineTransverseGradient;
                var s = (from T in sList
                            where c.BC <= T.FullSuperSta &&
                                T.FullSuperSta <= T.RunoffSta &&
                                T.RunoffSta <= c.EC
                            select T).FirstOrDefault();
                if (s != null)
                {
                    //片勾配すりつけがあればそこの最大片勾配を使う
                    rsgSltg = s.FullSuperelev;
                    targetCs = new CrossSect_OGExtension().GetCSFromSta(appliedCsList, s.FullSuperSta);
                }

                //最大片勾配のstaに合致したcsを取ってそれの路肩折れを判定
                CheckForRoadShoulderGradient(tgip, targetCs, rsgSltg, rsgVr);
                rsgVRList.Add(rsgVr);
            }
            TGVerificationResultItem.Instance.UpdateForRSG(selAlignmentName, rsgVRList);
        }

        /// <summary>
        /// 直線部の横断勾配の判定
        /// </summary>
        /// <param name="tgip"></param>
        private void CheckForStraightLineTransverseGradient(TGInputParameters tgip)
        {
            var stgVR = new TG_VerificationResult();

            Func<bool> validFunc = null;

            validFunc = () =>
            {
                if (StdNormalCrown.Count() == 1)
                {
                    return (StdNormalCrown[0] == tgip.StraightLineTransverseGradient);
                }
                else
                {
                    return (StdNormalCrown[0] <= tgip.StraightLineTransverseGradient &&
                            tgip.StraightLineTransverseGradient <= StdNormalCrown[1]);
                }
            };

            stgVR.SetMsgParameter($"{tgip.StraightLineTransverseGradient}%");
            stgVR.SetMsgParameter($"{string.Join("%,", StdNormalCrown)}%");
            stgVR.designValue = $"{tgip.StraightLineTransverseGradient}%";
            stgVR.stdValue = $"{string.Join("%,", StdNormalCrown)}%";
            if (validFunc())
            {
                //OK
                stgVR.ResultType = VerificationResult.VerifyResultType.OK;
                stgVR.MsgCode = "I-0051";
            }
            else
            {
                //NG
                stgVR.ResultType = VerificationResult.VerifyResultType.NG;
                stgVR.MsgCode = "W-0046";
            }

            TGVerificationResultItem.Instance.UpdateForSTG(selAlignmentName, stgVR);
        }

        /// <summary>
        /// 歩道等の横断勾配の判定
        /// </summary>
        /// <param name="tgip"></param>
        /// <param name="cs"></param>
        private void CheckForSideWalks(TGInputParameters tgip, List<CrossSect_OGExtension> stdCsList)
        {
            //this.VR_SidewalkCrown = new VerificationResult[2];
            var scVRList = new List<TG_VerificationResult>();

            foreach (var stdCs in stdCsList)
            {
                var scVR = new TG_VerificationResult[2];
                scVR[0] = new TG_VerificationResult();
                scVR[1] = new TG_VerificationResult();

                var sideWalks = stdCs.dcssList.AsEnumerable().Where(row => ((row.name_J == Name_JItems.Sidewalk || row.group1Name == Name_JItems.Sidewalk || row.group2Name == Name_JItems.Sidewalk) ||
                                                                           (row.name_J == Name_JItems.Sidepath || row.group1Name == Name_JItems.Sidepath || row.group2Name == Name_JItems.Sidepath) ||
                                                                           (row.name_J == Name_JItems.Cycletrack || row.group1Name == Name_JItems.Cycletrack || row.group2Name == Name_JItems.Cycletrack)) &&
                                                                           row.isTarget &&
                                                                           0 < row.cspList.First().roadWidth)
                                                             .Select(row => row);

                var leftSw = sideWalks.Where(row => row.side == DCSSSide.Left).OrderByDescending(row => row.cspList.First().roadPositionNo).Select(row => row).FirstOrDefault();
                var rightSw = sideWalks.Where(row => row.side == DCSSSide.Right).OrderByDescending(row => row.cspList.First().roadPositionNo).Select(row => row).FirstOrDefault();

                Action<DesignCrossSectSurf_OGExtension, TG_VerificationResult, DCSSSide> action = null;
                action = (dcss, v, side) =>
                {
                    decimal g = GetGradient(dcss);
                    if (g <= 1.0M)
                    {
                        if (tgip.IsBarrierFree || tgip.IsManyTraffic)
                        {
                            if (tgip.SidewalkPavingType == SidewalkPavingType.WaterPermeableType)
                            {
                                v.ResultType = VerificationResult.VerifyResultType.OK;
                                v.MsgCode = "I-0033";
                            }
                            else
                            {
                                v.ResultType = VerificationResult.VerifyResultType.NG;
                                v.MsgCode = "W-0029";
                            }
                        }
                        else
                        {
                            if (tgip.SidewalkPavingType == SidewalkPavingType.WaterPermeableType)
                            {
                                v.ResultType = VerificationResult.VerifyResultType.OK_C;
                                v.MsgCode = "I-0034";
                            }
                            else
                            {
                                v.ResultType = VerificationResult.VerifyResultType.NG;
                                v.MsgCode = "W-0030";
                            }
                        }
                        v.SetMsgParameter($"{stdCs.name}:{(side == DCSSSide.Left ? "左" : "右")}", $"{Math.Round(g, 3, MidpointRounding.AwayFromZero)}%", $"{1.0M}%");
                        v.designValue = $"{Math.Round(g, 3, MidpointRounding.AwayFromZero)}%";
                        v.stdValue = $"{1.0M}%";
                    }
                    else if (g == 2.0M)
                    {
                        if (tgip.IsBarrierFree || tgip.IsManyTraffic)
                        {
                            if (tgip.SidewalkPavingType == SidewalkPavingType.WaterPermeableType)
                            {
                                v.ResultType = VerificationResult.VerifyResultType.OK_C;
                                v.MsgCode = "I-0035";
                            }
                            else
                            {
                                v.ResultType = VerificationResult.VerifyResultType.OK_C;
                                v.MsgCode = "I-0036";
                            }
                        }
                        else
                        {
                            if (tgip.SidewalkPavingType == SidewalkPavingType.WaterPermeableType)
                            {
                                v.ResultType = VerificationResult.VerifyResultType.OK_C;
                                v.MsgCode = "I-0037";
                            }
                            else
                            {
                                v.ResultType = VerificationResult.VerifyResultType.OK;
                                v.MsgCode = "I-0038";
                            }
                        }
                        v.SetMsgParameter($"{stdCs.name}:{(side == DCSSSide.Left ? "左" : "右")}", $"{Math.Round(g, 3, MidpointRounding.AwayFromZero)}%", $"{2.0M}%");
                        v.designValue = $"{Math.Round(g, 3, MidpointRounding.AwayFromZero)}%";
                        v.stdValue = $"{2.0M}%";
                    }
                    else
                    {
                        v.ResultType = VerificationResult.VerifyResultType.NG;
                        v.MsgCode = "W-0031";
                    }
                };

                if (leftSw is null)
                {
                    scVR[0] = new TG_VerificationResult()
                    {
                        //歩道無し
                        MsgCode = $"<{stdCs.name}:左>-",
                        ResultType = VerificationResult.VerifyResultType.SKIP
                    };
                }
                else
                {
                    action(leftSw, scVR[0], DCSSSide.Left);
                }
                if (rightSw is null)
                {
                    scVR[1] = new TG_VerificationResult()
                    {
                        //歩道無し
                        MsgCode = $"<{stdCs.name}:右>-",
                        ResultType = VerificationResult.VerifyResultType.SKIP
                    };
                }
                else
                {
                    action(rightSw, scVR[1], DCSSSide.Right);
                }

                scVRList.AddRange(scVR.ToList());
            }

            TGVerificationResultItem.Instance.UpdateForSC(selAlignmentName, scVRList);
        }

        /// <summary>
        /// 車道の片勾配のチェック
        /// </summary>
        /// <param name="tgip"></param>
        /// <param name="allCsList"></param>
        private void CheckForCarriageway(TGInputParameters tgip, List<CrossSect_OGExtension> allCsList)
        {
            var mogVRList = new List<TG_MOG_VerificationResult>();
            var csProvider = new CrossSect_OGExtension();

            var sog = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.TG.StdOnesidedGradient;
            var sogt4 = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.TG.StdOnesidedGradientForType4;
            var ssog = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.TG.StdStopOnesidedGradient;

            var fSOG = (from T in sog
                        where T.NormalCrown == tgip.StraightLineTransverseGradient &&
                              T.DesignSpeed == tgip.si.ds
                        select T).ToList();
            var fSSOG = (from T in ssog
                         where T.NormalCrown == tgip.StraightLineTransverseGradient &&
                               T.DesignSpeed == tgip.si.ds
                         select T).ToList();
            var fSOGT4 = (from T in sogt4
                          where T.NormalCrown == tgip.StraightLineTransverseGradient &&
                                T.DesignSpeed == tgip.si.ds
                          select T).ToList();

            if (fSOG is null || fSOG.Any() == false || fSSOG is null || fSSOG.Any() == false)
            {
                if (tgip.StraightLineTransverseGradient == 2.5M)
                {
                    //勾配2.5%はNEXCO設計要領の可能性
                    mogVRList = new List<TG_MOG_VerificationResult>()
                    {
                        new TG_MOG_VerificationResult(){ MsgCode = "I-0039", ResultType = VerificationResult.VerifyResultType.OK_C}
                    };
                }
                else
                {
                    mogVRList = new List<TG_MOG_VerificationResult>()
                    {
                        new TG_MOG_VerificationResult(){ MsgCode = "W-0032", ResultType = VerificationResult.VerifyResultType.NG}
                    };
                }
            }

            var cList = XMLLoader.Instance.GetCurves(this.selXAlignment);
            List<Superelevation> sList;
            if (IsUseSlopeList)
            {
                var slopeProvider = new SlopeListClass();
                sList = slopeProvider.GetConvertedSlopeValueList(allCsList, tgip.StraightLineTransverseGradient, selXAlignment);
            }
            else
            {
                sList = XMLLoader.Instance.GetSuperelevationList(this.selXAlignment);
            }

            foreach (var c in cList)
            {
                var pCs = csProvider.GetCSFromSta(allCsList, c.BC);
                var res = new TG_MOG_VerificationResult();
                //曲線半径
                res.radiusItem = $"{Math.Round(c.radius, 3, MidpointRounding.AwayFromZero)}m";
                var s = ExistsSuperelevationInCurve(c, sList);
                if (s != null)
                {
                    if (IsStoppingOnesidedGradient(c, fSSOG, tgip))
                    {
                        res.MsgCode = "W-0035";
                        res.SetMsgParameter(res.radiusItem);
                        res.ResultType = VerificationResult.VerifyResultType.NG;
                    }
                    else
                    {
                        var fs = (from T in fSOG
                                    where T.MinRadius <= c.radius &&
                                        c.radius < T.MaxRadius
                                    select T).FirstOrDefault();
                        if (fs == null)
                        {
                            //取れない場合
                            //IsStoppingOnesidedGradient判定をしていれば入らないはず？
                            res.MsgCode = "W-XXXX";
                            res.ResultType = VerificationResult.VerifyResultType.SKIP;
                        }
                        else
                        {
                            decimal ista = 0;
                            if (tgip.si.rtT.Item1 == 4 ||
                                tgip.IsSnowyColdArea ||
                                HasNotBicycleLanesType3Road(pCs, tgip))
                            {
                                ista = fs.CONVal2;
                            }
                            else if (tgip.IsSnowyColdOtherArea)
                            {
                                ista = fs.CONVal3;
                            }
                            else
                            {
                                ista = fs.CONVal1;
                            }

                            res.stdValue = $"{Math.Round(ista * (c.rot == Curve.Clockwise.cw ? 1 : -1), 3, MidpointRounding.AwayFromZero)}%";
                            res.designValue = $"{s.FullSuperelev}%";

                            if (s.FullSuperelev == ista * (c.rot == Curve.Clockwise.cw ? 1 : -1))
                            {
                                res.MsgCode = "I-0041";
                                res.SetMsgParameter(tgip.StraightLineTransverseGradient, tgip.si.ds, res.radiusItem, res.designValue);
                                res.ResultType = VerificationResult.VerifyResultType.OK;
                            }
                            else if (tgip.si.rtT.Item1 == 4)
                            {
                                ista = (from T in fSOGT4
                                        where T.MinRadius <= c.radius &&
                                                c.radius < T.MaxRadius
                                        select T.ConVal4).FirstOrDefault();
                                res.stdValue = $"{Math.Round(ista * (c.rot == Curve.Clockwise.cw ? 1 : -1), 3, MidpointRounding.AwayFromZero)}%";

                                if (s.FullSuperelev == ista * (c.rot == Curve.Clockwise.cw ? 1 : -1))
                                {
                                    res.MsgCode = "I-0040";
                                    res.SetMsgParameter(tgip.StraightLineTransverseGradient, tgip.si.ds, res.radiusItem, res.designValue);
                                    res.ResultType = VerificationResult.VerifyResultType.OK_C;
                                }
                                else
                                {
                                    res.MsgCode = "W-0033";
                                    res.SetMsgParameter(tgip.StraightLineTransverseGradient, tgip.si.ds, res.radiusItem, res.designValue);
                                    res.ResultType = VerificationResult.VerifyResultType.NG;
                                }
                            }
                            else
                            {
                                res.MsgCode = "W-0033";
                                res.SetMsgParameter(tgip.StraightLineTransverseGradient, tgip.si.ds, res.radiusItem, res.designValue);
                                res.ResultType = VerificationResult.VerifyResultType.NG;
                            }
                        }
                    }
                }
                else
                {
                    if (IsStoppingOnesidedGradient(c, fSSOG, tgip))
                    {
                        if (tgip.si.rtT.Item1 == 4)
                        {
                            //第4種でも特例値と標準値の判定が必要
                            res.MsgCode = "I-0044";
                            res.SetMsgParameter(res.radiusItem);
                            res.ResultType = VerificationResult.VerifyResultType.OK_C;
                        }
                        else
                        {
                            res.MsgCode = "I-0043";
                            res.SetMsgParameter(res.radiusItem);
                            res.ResultType = VerificationResult.VerifyResultType.OK;
                        }
                    }
                    else
                    {
                        res.MsgCode = "W-0036";
                        res.SetMsgParameter(res.radiusItem);
                        res.ResultType = VerificationResult.VerifyResultType.NG;
                    }
                }
                mogVRList.Add(res);
            }
            //}

            TGVerificationResultItem.Instance.UpdateForMOG(selAlignmentName, mogVRList);
        }

        /// <summary>
        /// 路肩折れのチェック
        /// </summary>
        /// <param name="tgip"></param>
        /// <param name="cs"></param>
        /// <param name="vr"></param>
        private void CheckForRoadShoulderGradient(TGInputParameters tgip, CrossSect_OGExtension cs, decimal cwGradient, TG_RSG_VerificationResult rsgVr)
        {
            var chkRes = this.GetRoadShoulderVerifyPoint(tgip, cs, cwGradient, rsgVr);
            var vr = (from T in this.rscrList where T.resultCode == chkRes select T.vResult).FirstOrDefault();

            rsgVr.ResultType = vr.ResultType;
            rsgVr.MsgCode = vr.MsgCode;
        }

        /// <summary>
        /// 自転車道、自転車歩行者道を設けない第3種の道路か
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool HasNotBicycleLanesType3Road(CrossSect_OGExtension cs, TGInputParameters tgip)
        {
            return cs.dcssList.AsEnumerable().Where(row => ((row.name_J == Name_JItems.Sidepath || row.group1Name == Name_JItems.Sidepath || row.group2Name == Name_JItems.Sidepath) ||
                                                           (row.name_J == Name_JItems.Cycletrack || row.group1Name == Name_JItems.Cycletrack || row.group2Name == Name_JItems.Cycletrack)) &&
                                                           row.isTarget &&
                                                           0 < row.cspList.First().roadWidth)
                                             .Select(row => row).Any() == false &&
                   tgip.si.rtT.Item1 == 3;
        }

        /// <summary>
        /// 曲線区間内にSuperevelevavtionがあるか
        /// </summary>
        /// <param name="c"></param>
        /// <param name="sList"></param>
        /// <returns></returns>
        private Superelevation ExistsSuperelevationInCurve(Curve c, List<Superelevation> sList)
        {
            Superelevation retS = null;
            for (int i = 0; i < sList.Count(); i++)
            {
                var p1 = sList[i].FullSuperSta;
                var p2 = sList[i].RunoffSta;

                if (c.BC <= p1 && p1 < p2 && p2 <= c.EC)
                {
                    retS = sList[i];
                }
                else if (p1 < c.BC && c.BC < c.EC && c.EC < p2)
                {
                    retS = sList[i];                    
                }
                else if (c.BC <= p1 && p1 < c.EC && c.EC < p2)
                {
                    retS = sList[i];
                }
                else if (p1 < c.BC && c.BC < p2 && p2 <= c.EC)
                {
                    retS = sList[i];
                }

                if (retS != null)
                {
                    break;
                }
            }

            return retS;
        }

        /// <summary>
        /// 曲線半径は片勾配を打ち切る最小曲線半径以上か
        /// </summary>
        /// <param name="c"></param>
        /// <param name="fSSOG"></param>
        /// <param name="tgip"></param>
        /// <returns></returns>
        private bool IsStoppingOnesidedGradient(Curve c, List<Structs.JSON.Stdstoponesidedgradient> fSSOG, TGInputParameters tgip)
        {
            var g = (from T in fSSOG
                     where T.NormalCrown == tgip.StraightLineTransverseGradient &&
                           T.DesignSpeed == tgip.si.ds
                     select T).First();
            var stdVal = tgip.si.rtT.Item1 == 4 ? g.SPCVal : g.STDVal;

            if (stdVal == 0) return false;

            return stdVal <= c.radius;
        }

        /// <summary>
        /// 指定横断形状の勾配を取得(%)
        /// </summary>
        /// <param name="dcss"></param>
        /// <returns></returns>
        private decimal GetGradient(DesignCrossSectSurf_OGExtension dcss)
        {
            var retVal = 0.0M;

            decimal x1, x2, y1, y2;
            //at point of 3 has diffs just Y point. so should be use a item1 and 2.
            x1 = dcss.cspList[0].roadPositionX;
            x2 = dcss.cspList[1].roadPositionX;
            y1 = dcss.cspList[0].roadHight;
            y2 = dcss.cspList[1].roadHight;

            retVal = ((y2 - y1) / (x2 - x1)) * 100;

            return Math.Round(Math.Abs(retVal), 3, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 幅員構成路肩チェック用のチェックポイント
        /// </summary>
        /// <param name="vArray"></param>
        /// <returns></returns>
        private static int GetRoadShoulderVerifyPointDef(List<bool> vArray)
        {
            int retVal = 0;

            if (vArray.Count != CHANGING_ROADSHOULDER_DEP_NEST) throw new Exception("路肩折れ判定の条件が変更されています。判定できません。");

            for (int i = 0; i < CHANGING_ROADSHOULDER_DEP_NEST; i++)
            {
                retVal += vArray[i] ? (int)Math.Pow(2, i) : 0;
            }

            return retVal;
        }

        /// <summary>
        /// 路肩の構成を順番に判定
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private int GetRoadShoulderVerifyPoint(TGInputParameters tgip, CrossSect_OGExtension cs, decimal cwGradient, TG_RSG_VerificationResult rsgVr)
        {
            int retVal = 0;
            Action<int> action = null;

            action = (cNest) =>
            {
                bool chkRes = false;

                switch (cNest)
                {
                    case (int)ChangingRoadShoulderChkNum.IsWidthUpperStdVal:
                        chkRes = this.IsWidthUpperStdVal(cs, rsgVr);
                        break;
                    case (int)ChangingRoadShoulderChkNum.IsSnowyColdArea:
                        chkRes = this.IsSnowyColdArea(tgip, cs);
                        break;
                    case (int)ChangingRoadShoulderChkNum.IsChangingGradientInRoadShoulder:
                        chkRes = this.IsChangingGradientInRoadShoulder(cs, rsgVr);
                        break;
                    case (int)ChangingRoadShoulderChkNum.IsAppropriateGradient:
                        chkRes = this.IsAppropriateGradient(tgip, cs, rsgVr);
                        break;
                    case (int)ChangingRoadShoulderChkNum.IsAppropriateChangingGradientPoint:
                        chkRes = this.IsAppropriateChangingGradientPoint(cs, rsgVr);
                        break;
                    default:
                        throw new Exception("未実装の路肩チェック項目です。判定できません。");
                }

                retVal += chkRes ? (int)Math.Pow(2, cNest) : 0;

                if (cNest == (int)ChangingRoadShoulderChkNum.IsChangingGradientInRoadShoulder && chkRes == false)
                {
                    //路肩折れ無しの時点でチェックを抜ける
                    return;
                }

                if (cNest != CHANGING_ROADSHOULDER_DEP_NEST - 1)
                {
                    cNest += 1;
                    action(cNest);
                }
            };
            action(0);

            return retVal;
        }

        /// <summary>
        /// 路肩の幅員が基準値以上か
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool IsWidthUpperStdVal(CrossSect_OGExtension cs, TG_RSG_VerificationResult rsgVr)
        {
            var lSideRsWidth = cs.GetRoadShoulderWidth(DCSSSide.Left);
            var lSideReqCg = 1.75 < lSideRsWidth;
            var rSideRsWidth = cs.GetRoadShoulderWidth(DCSSSide.Right);
            var rSideReqCg = 1.75 < rSideRsWidth;

            var reqCg = lSideReqCg || rSideReqCg;

            rsgVr.isRequireChangingGradient = $"{reqCg}";

            return reqCg;
        }

        /// <summary>
        /// 積雪寒冷の度がはなはだしい地域に該当するか
        /// </summary>
        /// <param name="tgip"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool IsSnowyColdArea(TGInputParameters tgip, CrossSect_OGExtension cs)
        {
            return tgip.IsSnowyColdArea;
        }

        /// <summary>
        /// 路肩折れがあるか
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool IsChangingGradientInRoadShoulder(CrossSect_OGExtension cs, TG_RSG_VerificationResult rsgVr)
        {
            var lSideIsCg = cs.IsChangingGradientInRoadShoulder(DCSSSide.Left);
            var rSideIsCg = cs.IsChangingGradientInRoadShoulder(DCSSSide.Right);
            var retVal = lSideIsCg.Item1 || rSideIsCg.Item1;

            rsgVr.isChangingGradient = $"{retVal}";
            return retVal;
        }

        /// <summary>
        /// 車道の片勾配に応じた路肩勾配となっているか
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool IsAppropriateGradient(TGInputParameters tgip, CrossSect_OGExtension cs, TG_RSG_VerificationResult rsgVr)
        {
            var sag = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.TG.StdAppropriateGradient;

            var lSideIsCg = cs.IsChangingGradientInRoadShoulder(DCSSSide.Left);
            var rSideIsCg = cs.IsChangingGradientInRoadShoulder(DCSSSide.Right);

            Func<DCSSSide, Name_JItems, decimal, (bool, decimal, decimal)> func = null;
            func = (side, cgPointName, rsGradient) =>
            {
                decimal cwGradient = cs.GetCarriagewayGradient(side);
                decimal carriagewayGradient = Math.Abs(cwGradient);

                var stdRsGradient = (from T in sag
                                     where T.MinGradient <= carriagewayGradient &&
                                           carriagewayGradient <= T.MaxGradient &&
                                           T.IsSnowyColdArea == tgip.IsSnowyColdArea
                                     select T.STDVal).FirstOrDefault();
                if (stdRsGradient == 0.0M) stdRsGradient = carriagewayGradient;
                return (rsGradient == (side == DCSSSide.Left ? -stdRsGradient : stdRsGradient), rsGradient, cwGradient);
            };
            var lSide = lSideIsCg.Item1 ? func(DCSSSide.Left, lSideIsCg.Item2, lSideIsCg.Item3) : (true, 0.0M, 0.0M);
            var rSide = rSideIsCg.Item1 ? func(DCSSSide.Right, rSideIsCg.Item2, rSideIsCg.Item3) : (true, 0.0M, 0.0M);

            rsgVr.roadshoulderGradient = $"左：{(lSideIsCg.Item1 ? $"{Math.Round(lSide.Item2, 3, MidpointRounding.AwayFromZero)}%" : "-")}{Environment.NewLine}" +
                                         $"右：{(rSideIsCg.Item1 ? $"{Math.Round(rSide.Item2, 3, MidpointRounding.AwayFromZero)}%" : "-")}";
            rsgVr.onesidedRoadGradient = $"左：{(lSideIsCg.Item1 ? $"{Math.Round(lSide.Item3, 3, MidpointRounding.AwayFromZero)}%" : "-")}{Environment.NewLine}" +
                                         $"右：{(rSideIsCg.Item1 ? $"{Math.Round(rSide.Item3, 3, MidpointRounding.AwayFromZero)}%" : "-")}";

            return lSide.Item1 && rSide.Item1;
        }

        /// <summary>
        /// 路肩折れの位置は側帯端となっているか
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool IsAppropriateChangingGradientPoint(CrossSect_OGExtension cs, TG_RSG_VerificationResult rsgVr)
        {
            var lSideIsCg = cs.IsChangingGradientInRoadShoulder(DCSSSide.Left);
            var rSideIsCg = cs.IsChangingGradientInRoadShoulder(DCSSSide.Right);

            var retVal = lSideIsCg.Item2 == Name_JItems.Roadshoulder || rSideIsCg.Item2 == Name_JItems.Roadshoulder;

            var lPosition = lSideIsCg.Item2 == Name_JItems.Roadshoulder ? $"側帯端" : lSideIsCg.Item2 == Name_JItems.None ? "-" : "側帯端以外";
            var rPosition = rSideIsCg.Item2 == Name_JItems.Roadshoulder ? $"側帯端" : rSideIsCg.Item2 == Name_JItems.None ? "-" : "側帯端以外";
            
            rsgVr.changingGradientPosition = $"左：{lPosition}{Environment.NewLine}" +
                                             $"右：{rPosition}";
            //側帯端であるため路肩で変化していればOK
            return retVal;
        }
    }
}
