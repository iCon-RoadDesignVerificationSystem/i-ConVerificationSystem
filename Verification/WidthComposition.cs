using i_ConVerificationSystem.JSON;
using i_ConVerificationSystem.LinqExtention;
using i_ConVerificationSystem.Structs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using static i_ConVerificationSystem.Forms.WidthComposition.WCInputParameter;
using static i_ConVerificationSystem.Structs.CrossSects.DesignCrossSectSurf;
using static i_ConVerificationSystem.Structs.OGExtensions;
using static i_ConVerificationSystem.Structs.OGExtensions.DesignCrossSectSurf_OGExtension;

namespace i_ConVerificationSystem.Verification
{
    /// <summary>
    /// 幅員・幅員構成のチェック
    /// </summary>
    class WidthComposition
    {
        private const int ROADSHOULDER_DEP_NEST = 4;
        private enum RoadShoulderChkNum
        {
            HasStoppingLane = 0,
            IsSeparatedRoadAndHasCenterStrip,
            HasRoadShoulder,
            HasRightRoadShoulder
        }

        private struct RoadShoulderChkResult
        {
            public VerificationResult vResult;
            public int resultCode;
        }
        private readonly List<RoadShoulderChkResult> rscrList = new List<RoadShoulderChkResult>()
        {
            //1.停車帯または自転車通行帯があるか
            //2.中央帯を設けない分離道路か
            //3.路肩があるか
            //4.右側路肩があるか
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0015", ResultType = VerificationResult.VerifyResultType.NG },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true, true, true, true })},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0014", ResultType = VerificationResult.VerifyResultType.NG },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true, true, true, false})},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "I-0009", ResultType = VerificationResult.VerifyResultType.OK },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true, true, false, true})},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0013", ResultType = VerificationResult.VerifyResultType.NG },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true, true, false, false})},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0017", ResultType = VerificationResult.VerifyResultType.NG },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true, false, true, true})},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0018", ResultType = VerificationResult.VerifyResultType.NG },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true, false, true, false})},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0016", ResultType = VerificationResult.VerifyResultType.NG },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true, false, false, true})},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "I-0010", ResultType = VerificationResult.VerifyResultType.OK },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{true, false, false, false})},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "I-0007", ResultType = VerificationResult.VerifyResultType.OK },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false, true, true, true})},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0009", ResultType = VerificationResult.VerifyResultType.NG },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false, true, true, false})},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0008", ResultType = VerificationResult.VerifyResultType.NG },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false, true, false, true})},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0007", ResultType = VerificationResult.VerifyResultType.NG },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false, true, false, false})},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0012", ResultType = VerificationResult.VerifyResultType.NG },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false, false, true, true})},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "I-0008", ResultType = VerificationResult.VerifyResultType.OK },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false, false, true, false})},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0011", ResultType = VerificationResult.VerifyResultType.NG },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false, false, false, true})},
            new RoadShoulderChkResult {vResult = new VerificationResult{ MsgCode = "W-0010", ResultType = VerificationResult.VerifyResultType.NG },
                                       resultCode = GetRoadShoulderVerifyPointDef(new List<bool>{false, false, false, false})}
        };

        private enum SidewalkBicycleRequire
        {
            None = 0, //歩道なし
            Sidewalk, //歩道
            Sidepath, //自転車歩行者道
            BikelaneAndSidewalk, //自転車通行帯＋歩道
            CycletrackAndSidewalk //自転車道＋歩道
        }

        private enum CarriagewayWidthResult
        {
            NG = 0,
            Standard1,
            Special1,
            Standard2,
            Standard3,
            Special2
        }

        private enum CenterLineResult
        {
            NG = 0,
            Standard,
            Special
        }

        private enum CenterLineCheckItem
        {
            Strip = 0,
            Separator
        }

        private enum RoadShoulderWidthResult
        {
            NG = 0,
            StandardL,
            StandardR,
            SpecialL,
            DesireL,
            DesireR,
            Tunnel
        }

        private enum RoadShoulderStripWidthResult
        {
            NG = 0,
            Standard,
            Special
        }

        private enum WidthVerifyCheckPoint
        {
            Name_J = 0,
            Group1,
            Group2
        }

        /// <summary>
        /// 幅員構成要素の適切性を判定
        /// </summary>
        /// <returns></returns>
        public (bool, OGExtensions.TotalResult_WidthComposition) IsCorrectWidthComposition(string alignmentName, WCInputParams wcip, CrossSect_OGExtension cs)
        {
            bool retVal = false;
            var wcResult = new OGExtensions.TotalResult_WidthComposition();

            if (wcip is null)
            {
                var vr = new VerificationResult();
                vr.ResultType = VerificationResult.VerifyResultType.SKIP;
                wcResult.RoadShoulder = vr;
                wcResult.CarriagewayLaneCount = vr;
                wcResult.CenterLane = vr;
                wcResult.PlantingLane = vr;
                wcResult.StoppingLane = vr;
                wcResult.MarginalStrip = vr;
                wcResult.SideWalkAndBicycleLane = vr;
                retVal = true;
            }
            else
            {
                //路肩の判定
                wcResult.RoadShoulder = this.CheckForRoadShoulder(wcip, cs);
                //車線数の判定
                wcResult.CarriagewayLaneCount = this.CheckForRoadCount(wcip, cs);
                //中央帯および中央帯側帯の判定
                wcResult.CenterLane = this.CheckForCenterStrip(wcip, cs);
                //植樹帯の判定
                wcResult.PlantingLane = this.CheckForPlantingLane(wcip, cs);
                //停車帯の判定
                wcResult.StoppingLane = this.CheckForStoppingLane(wcip, cs);
                //路肩側帯の判定
                wcResult.MarginalStrip = this.CheckForRoadShoulderMarginalStrip(wcip, cs);
                //歩行者・自転車通行空間の判定
                wcResult.SideWalkAndBicycleLane = this.CheckForSideWalkAndBicycleLane(wcip, cs);

                if (wcResult.GetTotalResult() == VerificationResult.VerifyResultType.OK)
                {
                    retVal = true;
                }
            }

            //照査結果の保持
            WCVerificationResultItem.Instance.Update(alignmentName, cs.sta.ToString(), wcResult, cs);

            return (retVal, wcResult);
        }

        /// <summary>
        /// 路肩の判定
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        private VerificationResult CheckForRoadShoulder(WCInputParams wcip, CrossSect_OGExtension cs)
        {
            var chkRes = this.GetRoadShoulderVerifyPoint(cs);
            var retVal = (from T in this.rscrList where T.resultCode == chkRes select T.vResult).FirstOrDefault();

            return retVal;
        }

        /// <summary>
        /// 車線数の判定
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        private VerificationResult CheckForRoadCount(WCInputParams wcip, CrossSect_OGExtension cs)
        {
            var retVal = new VerificationResult();
            int chkRoadCount = 0;
            if (wcip.std.rtT.Item1 == 3 && wcip.std.rtT.Item2 == 5 || wcip.std.rtT.Item1 == 4 && wcip.std.rtT.Item2 == 4)
            {
                chkRoadCount = 1;
            }
            else
            {
                if (wcip.Ptv <= this.GetQcr(wcip, false))
                {
                    chkRoadCount = 2;
                }
                else
                {
                    int chkCount = 4;
                    long qcr = this.GetQcr(wcip, true);

                    while ((qcr * chkCount) < wcip.Ptv)
                    {
                        chkCount += 2;
                    }
                    chkRoadCount = chkCount;
                }
            }

            if (chkRoadCount == cs.GetRoadCount())
            {
                retVal.MsgCode = "I-0006";
                retVal.ResultType = VerificationResult.VerifyResultType.OK;
            }
            else
            {
                retVal.MsgCode = "W-0006";
                retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
            }

            return retVal;
        }

        /// <summary>
        /// 中央帯および中央帯側帯の判定
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        private VerificationResult CheckForCenterStrip(WCInputParams wcip, CrossSect_OGExtension cs)
        {
            int roadCount = cs.GetRoadCount();
            bool hasCSMS = this.HasCenterStripAndMarginalStrip(cs);
            var retVal = new VerificationResult();

            if ((wcip.std.rtT.Item1 == 2 || (wcip.std.rtT.Item1 == 3 && wcip.std.rtT.Item2 == 1)) || 4 <= roadCount)
            {
                if (hasCSMS)
                {
                    retVal.MsgCode = "I-0011";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK;
                }
                else
                {
                    retVal.MsgCode = "W-0019";
                    retVal.ResultType = VerificationResult.VerifyResultType.NG;
                }
            }
            else
            {
                if (wcip.std.rtT.Item1 == 1 && roadCount <= 3)
                {
                    if (hasCSMS)
                    {
                        retVal.MsgCode = "I-0012";
                        retVal.ResultType = VerificationResult.VerifyResultType.OK;
                    }
                    else
                    {
                        retVal.MsgCode = "I-0013";
                        retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                    }
                }
                else if (hasCSMS)
                {
                    retVal.MsgCode = "I-0015";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                }
                else
                {
                    retVal.MsgCode = "I-0014";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK;
                }
            }

            return retVal;
        }

        /// <summary>
        /// 植樹帯の判定
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        private VerificationResult CheckForPlantingLane(WCInputParams wcip, CrossSect_OGExtension cs)
        {
            var retVal = new VerificationResult();
            bool hasPZone = this.HasPlantingZone(cs);

            if (wcip.std.rtT.Item1 == 4 && (wcip.std.rtT.Item2 == 1 || wcip.std.rtT.Item2 == 2))
            {
                if (hasPZone)
                {
                    retVal.MsgCode = "I-0016";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK;
                }
                else
                {
                    retVal.MsgCode = "I-0017";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                }
            }
            else
            {
                if (hasPZone)
                {
                    retVal.MsgCode = "I-0019";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                }
                else
                {
                    retVal.MsgCode = "I-0018";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK;
                }
            }

            return retVal;
        }

        /// <summary>
        /// 停車帯の判定
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        private VerificationResult CheckForStoppingLane(WCInputParams wcip, CrossSect_OGExtension cs)
        {
            var retVal = new VerificationResult();
            bool hasSL = this.HasStoppingLane(cs);

            if (wcip.std.rtT.Item1 == 4)
            {
                if (hasSL)
                {
                    retVal.MsgCode = "I-0020";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK;
                }
                else
                {
                    retVal.MsgCode = "I-0021";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                }
            }
            else
            {
                if (hasSL)
                {
                    retVal.MsgCode = "W-0020";
                    retVal.ResultType = VerificationResult.VerifyResultType.NG;
                }
                else
                {
                    retVal.MsgCode = "I-0022";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK;
                }
            }

            return retVal;
        }

        /// <summary>
        /// 路肩側帯の判定
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        private VerificationResult CheckForRoadShoulderMarginalStrip(WCInputParams wcip, CrossSect_OGExtension cs)
        {
            bool hasRSMS = this.HasRoadShoulderMarginalStrip(cs);
            var retVal = new VerificationResult();

            if (wcip.std.rtT.Item1 == 1 || wcip.std.rtT.Item1 == 2)
            {
                if (hasRSMS)
                {
                    retVal.MsgCode = "I-0091";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK;
                }
                else
                {
                    retVal.MsgCode = "W-0062";
                    retVal.ResultType = VerificationResult.VerifyResultType.NG;
                }
            }
            else
            {
                if (hasRSMS)
                {
                    retVal.MsgCode = "W-0061";
                    retVal.ResultType = VerificationResult.VerifyResultType.NG;
                }
                else
                {
                    retVal.MsgCode = "I-0090";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK;
                }
            }

            return retVal;
        }

        /// <summary>
        /// 歩行者・自転車通行空間の判定
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        private VerificationResult CheckForSideWalkAndBicycleLane(WCInputParams wcip, CrossSect_OGExtension cs)
        {
            var retVal = new VerificationResult();
            if (wcip.std.rtT.Item1 == 3 || wcip.std.rtT.Item1 == 4)
            {
                //TODO:ネストが深すぎるので要見直し
                SidewalkBicycleRequire reqLanes;
                if (wcip.IsBnp)
                {
                    if (50 < wcip.std.ds)
                    {
                        //自転車道＋歩道
                        reqLanes = SidewalkBicycleRequire.CycletrackAndSidewalk;
                    }
                    else
                    {
                        if (40 < wcip.std.ds && 4000 < wcip.Ptv)
                        {
                            //自転車通行帯＋歩道
                            reqLanes = SidewalkBicycleRequire.BikelaneAndSidewalk;
                        }
                        else
                        {
                            if (wcip.std.rtT.Item1 == 4)
                            {
                                //歩道
                                reqLanes = SidewalkBicycleRequire.Sidewalk;
                            }
                            else
                            {
                                if (wcip.std.rtT.Item1 == 3 && wcip.std.rtT.Item2 != 5 && wcip.Qpede)
                                {
                                    //歩道
                                    reqLanes = SidewalkBicycleRequire.Sidewalk;
                                }
                                else
                                {
                                    //OK
                                    reqLanes = SidewalkBicycleRequire.None;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (wcip.Qcycle)
                    {
                        if (4000 <= wcip.Ptv)
                        {
                            if (60 <= wcip.std.ds)
                            {
                                //自転車道＋歩道
                                reqLanes = SidewalkBicycleRequire.CycletrackAndSidewalk;
                            }
                            else
                            {
                                //自転車通行帯＋歩道
                                reqLanes = SidewalkBicycleRequire.BikelaneAndSidewalk;
                            }
                        }
                        else
                        {
                            if (60 <= wcip.std.ds)
                            {
                                //自転車道＋歩道
                                reqLanes = SidewalkBicycleRequire.CycletrackAndSidewalk;
                            }
                            else
                            {
                                if (40 < wcip.std.ds)
                                {
                                    //自転車通行帯＋歩道
                                    reqLanes = SidewalkBicycleRequire.BikelaneAndSidewalk;
                                }
                                else
                                {
                                    if (wcip.std.rtT.Item1 == 4)
                                    {
                                        //歩道
                                        reqLanes = SidewalkBicycleRequire.Sidewalk;
                                    }
                                    else
                                    {
                                        if (wcip.std.rtT.Item1 == 3 && wcip.std.rtT.Item2 != 5 && wcip.Qpede)
                                        {
                                            //歩道
                                            reqLanes = SidewalkBicycleRequire.Sidewalk;
                                        }
                                        else
                                        {
                                            //OK
                                            reqLanes = SidewalkBicycleRequire.None;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (4000 <= wcip.Ptv)
                        {
                            if (wcip.Qpede)
                            {
                                if (60 <= wcip.std.ds)
                                {
                                    //自転車道＋歩道
                                    reqLanes = SidewalkBicycleRequire.CycletrackAndSidewalk;
                                }
                                else
                                {
                                    //自転車通行帯＋歩道
                                    reqLanes = SidewalkBicycleRequire.BikelaneAndSidewalk;
                                }
                            }
                            else
                            {
                                //自転車歩行者道
                                reqLanes = SidewalkBicycleRequire.Sidepath;
                            }
                        }
                        else
                        {
                            if (wcip.std.rtT.Item1 == 4)
                            {
                                //歩道
                                reqLanes = SidewalkBicycleRequire.Sidewalk;
                            }
                            else
                            {
                                if (wcip.std.rtT.Item1 == 3 && wcip.std.rtT.Item2 != 5 && wcip.Qpede)
                                {
                                    //歩道
                                    reqLanes = SidewalkBicycleRequire.Sidewalk;
                                }
                                else
                                {
                                    //OK
                                    reqLanes = SidewalkBicycleRequire.None;
                                }
                            }
                        }
                    }
                }

                if (this.HasRequireLane4SidewalkAndBicycleLane(cs, reqLanes))
                {
                    retVal.MsgCode = "I-0023";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK;
                }
                else
                {
                    retVal.MsgCode = "I-0024";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                }
            }
            else
            {
                retVal.MsgCode = "I-0023";
                retVal.ResultType = VerificationResult.VerifyResultType.OK;
            }

            return retVal;
        }

        /// <summary>
        /// 必須通行帯があるか
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="reqLanes"></param>
        /// <returns></returns>
        private bool HasRequireLane4SidewalkAndBicycleLane(CrossSect_OGExtension cs, SidewalkBicycleRequire reqLanes)
        {
            switch (reqLanes)
            {
                case SidewalkBicycleRequire.None:
                    return true;
                case SidewalkBicycleRequire.Sidewalk:
                    return (from T in cs.dcssList
                            where T.name_J == Name_JItems.Sidewalk &&
                                  T.isTarget
                            select T).Any();
                case SidewalkBicycleRequire.Sidepath:
                    return (from T in cs.dcssList
                            where T.name_J == Name_JItems.Sidepath &&
                                  T.isTarget
                            select T).Any();
                case SidewalkBicycleRequire.BikelaneAndSidewalk:
                    var hasBikelane = (from T in cs.dcssList
                                       where T.name_J == Name_JItems.Bikelane &&
                                             T.isTarget
                                       select T).Any();
                    var hasSidewalk = (from T in cs.dcssList
                                       where T.name_J == Name_JItems.Sidewalk &&
                                             T.isTarget
                                       select T).Any();
                    return hasBikelane && hasSidewalk;
                case SidewalkBicycleRequire.CycletrackAndSidewalk:
                    var hasCycletrack = (from T in cs.dcssList
                                         where T.name_J == Name_JItems.Cycletrack &&
                                               T.isTarget
                                         select T).Any();
                    hasSidewalk = (from T in cs.dcssList
                                   where T.name_J == Name_JItems.Sidewalk &&
                                         T.isTarget
                                   select T).Any();
                    return hasCycletrack && hasSidewalk;
                default:
                    return true;
            }
        }

        /// <summary>
        /// 路肩側帯があるか
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool HasRoadShoulderMarginalStrip(CrossSect_OGExtension cs)
        {
            return (from T in cs.dcssList 
                    where (T.name_J == Name_JItems.MarginalStrip || 
                           (T.group1Name == Name_JItems.MarginalStrip && T.group1 != GroupCode.None) || 
                           (T.group2Name == Name_JItems.MarginalStrip && T.group2 != GroupCode.None)) &&
                           T.isTarget
                    select T).Any();
        }

        /// <summary>
        /// 停車帯があるか
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool HasStoppingLane(CrossSect_OGExtension cs)
        {
            return (from T in cs.dcssList 
                    where (T.name_J == Name_JItems.StoppingArea || 
                           (T.group1Name == Name_JItems.StoppingArea && T.group1 != GroupCode.None) || 
                           (T.group2Name == Name_JItems.StoppingArea && T.group2 != GroupCode.None)) &&
                          T.isTarget
                    select T).Any();
        }

        /// <summary>
        /// 植樹帯があるか
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool HasPlantingZone(CrossSect_OGExtension cs)
        {
            return (from T in cs.dcssList 
                    where (T.name_J == Name_JItems.PlantingLane || 
                           (T.group1Name == Name_JItems.PlantingLane && T.group1 != GroupCode.None) || 
                           (T.group2Name == Name_JItems.PlantingLane && T.group2 != GroupCode.None)) &&
                          T.isTarget 
                    select T).Any();
        }

        /// <summary>
        /// 中央帯および中央帯側帯があるか
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool HasCenterStripAndMarginalStrip(CrossSect_OGExtension cs)
        {
            return HasCenterStrip(cs) && HasCenterMarginalStrip(cs);
        }

        /// <summary>
        /// 中央帯があるか
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool HasCenterStrip(CrossSect_OGExtension cs)
        {
            return (from T in cs.dcssList
                    where (T.name_J == Name_JItems.CenterStrip  ||
                           (T.group1Name == Name_JItems.CenterStrip && T.group1 != GroupCode.None) ||
                           (T.group2Name == Name_JItems.CenterStrip && T.group2 != GroupCode.None)) &&
                          T.isTarget
                    select T).Any();
        }

        /// <summary>
        /// 中央帯側帯があるか
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool HasCenterMarginalStrip(CrossSect_OGExtension cs)
        {
            return (from T in cs.dcssList
                    where (T.name_J == Name_JItems.CenterMarginalStrip ||
                           (T.group1Name == Name_JItems.CenterMarginalStrip && T.group1 != GroupCode.None) ||
                           (T.group2Name == Name_JItems.CenterMarginalStrip && T.group2 != GroupCode.None)) &&
                          T.isTarget
                    select T).Any();
        }

        /// <summary>
        /// QCRの倍率を取得
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="isReGet"></param>
        /// <returns></returns>
        public decimal GetPRatio(WCInputParams wcip, bool isReGet)
        {
            decimal retVal = 1M;

            if (wcip.Islcp4)
            {
                retVal = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.WC.PlannedTvRatio;
                if (isReGet)
                {
                    retVal = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.WC.Planned4TvRatio;
                }
            }

            return retVal;
        }

        /// <summary>
        /// 設計基準交通量QCRを取得
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="isReGet"></param>
        /// <returns></returns>
        private long GetQcr(WCInputParams wcip, bool isReGet)
        {
            decimal pRatio = GetPRatio(wcip, isReGet);
            long qcr = 0;
            if (isReGet)
            {
                var p4t = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.WC.Planned4Tv;
                qcr = (from T in p4t
                       where T.RoadType == wcip.std.rtT.Item1 &&
                             T.RoadClass == wcip.std.rtT.Item2 &&
                             T.Topography == wcip.Tg
                       select T.PlannedTv).FirstOrDefault();

                if (qcr == 0)
                {
                    qcr = (from T in p4t
                           where T.RoadType == wcip.std.rtT.Item1 &&
                                 T.RoadClass == wcip.std.rtT.Item2 &&
                                 T.Topography == Topography.None
                           select T.PlannedTv).FirstOrDefault();
                }
            }
            else
            {
                var pt = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.WC.PlannedTv;
                qcr = (from T in pt
                       where T.RoadType == wcip.std.rtT.Item1 &&
                             T.RoadClass == wcip.std.rtT.Item2 &&
                             T.Topography == wcip.Tg
                       select T.PlannedTv).FirstOrDefault();

                if (qcr == 0)
                {
                    qcr = (from T in pt
                           where T.RoadType == wcip.std.rtT.Item1 &&
                                 T.RoadClass == wcip.std.rtT.Item2 &&
                                 T.Topography == Topography.None
                           select T.PlannedTv).FirstOrDefault();
                }
            }

            qcr = (long)(qcr * pRatio);

            return qcr;
        }

        /// <summary>
        /// 停車帯または自転車通行帯があるか
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool HasStoppingLaneOrBicycleLane(CrossSect_OGExtension cs)
        {
            return (from T in cs.dcssList
                    where (T.name_J == Name_JItems.StoppingArea || T.name_J == Name_JItems.Bikelane ||
                           ((T.group1Name == Name_JItems.StoppingArea || T.group1Name == Name_JItems.Bikelane) && T.group1 != GroupCode.None) ||
                           ((T.group2Name == Name_JItems.StoppingArea || T.group2Name == Name_JItems.Bikelane) && T.group2 != GroupCode.None)) &&
                          T.isTarget
                    select T).Any();
        }

        /// <summary>
        /// 中央帯を設けない分離道路であるか
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool IsSeparatedRoadAndHasCenterStrip(CrossSect_OGExtension cs)
        {
            var hasLeftSide = (from T in cs.dcssList
                               where T.side == DCSSSide.Left &&
                                     T.isTarget
                               select T).Any();
            var hasRightSide = (from T in cs.dcssList
                                where T.side == DCSSSide.Right &&
                                      T.isTarget
                                select T).Any();
            var isSeparatedRoad = hasLeftSide ^ hasRightSide;

            if (isSeparatedRoad == false)
            {
                //1つの線形で分離させているか判定
                //構成番号(nx)が0の座標がclOffsetを考慮して0以外であれば分離道路
                var f1Position = cs.dcssList.Select(row => row.cspList.Where(cRow => cRow.roadPositionNo == 0).Select(cRow => Math.Abs(cRow.roadPositionX) - Math.Abs(cs.clOffset)).First()).First();
                if (f1Position != 0.0M) isSeparatedRoad = true;
            }

            var hasCenterStrip = (from T in cs.dcssList 
                                  where (T.name_J == Name_JItems.CenterStrip || 
                                         (T.group1Name == Name_JItems.CenterStrip && T.group1 != GroupCode.None) || 
                                         (T.group2Name == Name_JItems.CenterStrip && T.group2 != GroupCode.None)) &&
                                        T.isTarget
                                  select T).Any();

            if (isSeparatedRoad)
            {
                return !hasCenterStrip;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 路肩があるか
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool HasRoadShoulder(CrossSect_OGExtension cs)
        {
            return (from T in cs.dcssList 
                    where (T.name_J == Name_JItems.Roadshoulder || 
                           (T.group1Name == Name_JItems.Roadshoulder && T.group1 != GroupCode.None) || 
                           (T.group2Name == Name_JItems.Roadshoulder && T.group2 != GroupCode.None)) &&
                          T.isTarget
                    select T).Any();
        }

        /// <summary>
        /// 右側路肩があるか
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool HasRightRoadShoulder(CrossSect_OGExtension cs)
        {
            return (from T in cs.dcssList 
                    where (T.name_J == Name_JItems.RoadshoulderR || 
                           (T.group1Name == Name_JItems.RoadshoulderR && T.group1 != GroupCode.None) || 
                           (T.group2Name == Name_JItems.RoadshoulderR && T.group2 != GroupCode.None)) &&
                          T.isTarget
                    select T).Any();
        }

        /// <summary>
        /// 道路区分に応じた幅員となっているか判定
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        public bool IsCorrectWidth(string alignmentName, WCInputParams wcip, CrossSect_OGExtension cs)
        {
            bool retVal = false;
            if (wcip is null) return retVal;

            Action<WidthVerifyCheckPoint, DesignCrossSectSurf_OGExtension> action = null;
            action = (cPoint, cDcss) =>
            {
                var cGroup = Name_JItems.None;

                IEnumerable<DesignCrossSectSurf_OGExtension> sameGroup = null;
                switch (cPoint)
                {
                    case WidthVerifyCheckPoint.Name_J:
                        //単線のチェック
                        sameGroup = (from T in cs.dcssList where T.Equals(cDcss) select T);
                        cGroup = cDcss.name_J;
                        break;
                    case WidthVerifyCheckPoint.Group1:
                        //同じグループコード1の集約
                        if (cDcss.group1 == GroupCode.None)
                        {
                            //グループコード無しは単線
                            sameGroup = (from T in cs.dcssList where T.Equals(cDcss) select T);
                        }
                        else
                        {
                            sameGroup = (from T in cs.dcssList where T.group1 == cDcss.group1 && T.isTarget select T);
                        }
                        cGroup = cDcss.group1Name;
                        break;
                    case WidthVerifyCheckPoint.Group2:
                        //同じグループコード2の集約
                        if (cDcss.group2 == GroupCode.None)
                        {
                            //グループコード無しは単線
                            sameGroup = (from T in cs.dcssList where T.Equals(cDcss) select T);
                        }
                        else
                        {
                            sameGroup = (from T in cs.dcssList where T.group2 == cDcss.group2 && T.isTarget select T);
                        }
                        cGroup = cDcss.group2Name;
                        break;
                    default:
                        break;
                }

                var res = new VerificationResult();
                //同一グループで集計し、それぞれのResultをセットする
                switch (cGroup)
                {
                    case Name_JItems.None:
                    case Name_JItems.Other:
                        //その他
                        //照査なし
                        break;
                    case Name_JItems.CenterStrip:
                        //中央帯
                        res = CheckForCenterStripWidth(wcip, cs).Item1;
                        foreach (var item in sameGroup)
                        {
                            switch (cPoint)
                            {
                                case WidthVerifyCheckPoint.Name_J:
                                    item.result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group1:
                                    item.group1Result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group2:
                                    item.group2Result = res;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case Name_JItems.CenterSprit:
                        //中央分離帯
                        res = CheckForCenterStripWidth(wcip, cs).Item2;
                        foreach (var item in sameGroup)
                        {
                            switch (cPoint)
                            {
                                case WidthVerifyCheckPoint.Name_J:
                                    item.result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group1:
                                    item.group1Result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group2:
                                    item.group2Result = res;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case Name_JItems.CenterMarginalStrip:
                        //中央帯側帯
                        res = CheckForCenterMarginalStripWidth(wcip, sameGroup);
                        foreach (var item in sameGroup)
                        {
                            switch (cPoint)
                            {
                                case WidthVerifyCheckPoint.Name_J:
                                    item.result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group1:
                                    item.group1Result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group2:
                                    item.group2Result = res;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case Name_JItems.AdditionalLane:
                    case Name_JItems.Carriageway:
                        //付加車線
                        //車道
                        res = CheckForCarriagewayWidth(wcip, sameGroup);
                        foreach (var item in sameGroup)
                        {
                            switch (cPoint)
                            {
                                case WidthVerifyCheckPoint.Name_J:
                                    item.result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group1:
                                    item.group1Result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group2:
                                    item.group2Result = res;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case Name_JItems.Roadshoulder:
                    case Name_JItems.RoadshoulderR:
                        //路肩
                        //右側路肩
                        CheckForRoadShoulderWidth(wcip, cs, sameGroup.ToList(), cPoint, cGroup);
                        break;
                    case Name_JItems.MarginalStrip:
                        //路肩側帯
                        res = CheckForRoadShoulderStripWidth(wcip, sameGroup);
                        foreach (var item in sameGroup)
                        {
                            switch (cPoint)
                            {
                                case WidthVerifyCheckPoint.Name_J:
                                    item.result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group1:
                                    item.group1Result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group2:
                                    item.group2Result = res;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case Name_JItems.PlantingLane:
                        //植樹帯
                        res = CheckForPlantingLaneWidth(wcip, sameGroup);
                        foreach (var item in sameGroup)
                        {
                            switch (cPoint)
                            {
                                case WidthVerifyCheckPoint.Name_J:
                                    item.result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group1:
                                    item.group1Result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group2:
                                    item.group2Result = res;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case Name_JItems.Sidewalk:
                        //歩道
                        res = CheckForSideWalkWidth(wcip, sameGroup, cs);
                        foreach (var item in sameGroup)
                        {
                            switch (cPoint)
                            {
                                case WidthVerifyCheckPoint.Name_J:
                                    item.result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group1:
                                    item.group1Result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group2:
                                    item.group2Result = res;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case Name_JItems.Sidepath:
                        //自転車歩行者道
                        res = CheckForSidepathWidth(wcip, sameGroup, cs);
                        foreach (var item in sameGroup)
                        {
                            switch (cPoint)
                            {
                                case WidthVerifyCheckPoint.Name_J:
                                    item.result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group1:
                                    item.group1Result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group2:
                                    item.group2Result = res;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case Name_JItems.Cycletrack:
                        //自転車道
                        res = CheckForCycletrackWidth(wcip, sameGroup);
                        foreach (var item in sameGroup)
                        {
                            switch (cPoint)
                            {
                                case WidthVerifyCheckPoint.Name_J:
                                    item.result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group1:
                                    item.group1Result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group2:
                                    item.group2Result = res;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case Name_JItems.Bikelane:
                        //自転車通行帯
                        res = CheckForBikelaneWidth(wcip, sameGroup);
                        foreach (var item in sameGroup)
                        {
                            switch (cPoint)
                            {
                                case WidthVerifyCheckPoint.Name_J:
                                    item.result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group1:
                                    item.group1Result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group2:
                                    item.group2Result = res;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case Name_JItems.StoppingArea:
                        //停車帯
                        res = CheckForStoppingLaneWidth(wcip, sameGroup);
                        foreach (var item in sameGroup)
                        {
                            switch (cPoint)
                            {
                                case WidthVerifyCheckPoint.Name_J:
                                    item.result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group1:
                                    item.group1Result = res;
                                    break;
                                case WidthVerifyCheckPoint.Group2:
                                    item.group2Result = res;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            };

            foreach (var dcss in cs.dcssList)
            {
                if (dcss.isTarget == false || dcss.name_J == Name_JItems.None) continue;

                if (dcss.group2 == GroupCode.None || dcss.name_J != dcss.group2Name)
                {
                    if (dcss.group1 == GroupCode.None)
                    {
                        action(WidthVerifyCheckPoint.Name_J, dcss);
                    }
                    else
                    {
                        action(WidthVerifyCheckPoint.Group1, dcss);
                    }
                }

                if (dcss.group2 != GroupCode.None)
                {
                    action(WidthVerifyCheckPoint.Group2, dcss);
                }
            }

            //グループごとに照査結果を持つ必要がある。幅員のTotalResultはそれら全てがOKであればOKとなる
            //TotalResultを呼び出し
            WVerificationResultItem.Instance.Update(alignmentName, cs.sta.ToString(), new TotalResult_Width(), cs);

            return retVal;
        }

        /// <summary>
        /// 車線幅員のチェック
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="dcss"></param>
        /// <returns></returns>
        private VerificationResult CheckForCarriagewayWidth(WCInputParams wcip, IEnumerable<DesignCrossSectSurf_OGExtension> ieDcss)
        {
            var retVal = new VerificationResult();
            var cwResult = this.GetCarriagewayWidthResult(wcip, ieDcss);
            
            if (cwResult == CarriagewayWidthResult.NG)
            {
                retVal.MsgCode = "W-0047";
                retVal.ResultType = VerificationResult.VerifyResultType.NG;

                return retVal;
            }

            switch (cwResult)
            {
                case CarriagewayWidthResult.Special1:
                    if (wcip.Rss == RoadSideStandard.NormalRoad)
                    {
                        if (wcip.std.rtT.Item1 == 1 && (wcip.std.rtT.Item2 == 1 || wcip.std.rtT.Item2 == 2)){
                            retVal.MsgCode = "I-0053";
                            retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                        }
                        else
                        {
                            if (wcip.std.rtT.Item1 == 2 && wcip.std.rtT.Item2 == 1)
                            {
                                if (wcip.std.ds == 60)
                                {
                                    retVal.MsgCode = "I-0054";
                                    retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                                }
                                else
                                {
                                    retVal.MsgCode = "I-0055";
                                    retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                                }
                            }
                            else
                            {
                                if ((wcip.std.rtT.Item1 == 3 && wcip.std.rtT.Item2 == 2) ||
                                    (wcip.std.rtT.Item1 == 4 && (wcip.std.rtT.Item2 == 1 || wcip.std.rtT.Item2 == 2))){
                                    if (wcip.IsConnect41to31)
                                    {
                                        retVal.MsgCode = "I-0056";
                                        retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                                    }
                                    else if (wcip.IsConnect41to32)
                                    {
                                        retVal.MsgCode = "I-0057";
                                        retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                                    }
                                    else if (30 < wcip.Lvmr)
                                    {
                                        retVal.MsgCode = "I-0058";
                                        retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                                    }
                                    else
                                    {
                                        retVal.MsgCode = "I-0059";
                                        retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                                    }
                                }
                                else
                                {
                                    retVal.MsgCode = "I-0059";
                                    retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (((wcip.std.rtT.Item1 == 1 && wcip.std.rtT.Item2 == 2) && wcip.std.ds == 80) ||
                            ((wcip.std.rtT.Item1 == 1 && wcip.std.rtT.Item2 == 3) && wcip.std.ds == 60) ||
                            ((wcip.std.rtT.Item1 == 2 && wcip.std.rtT.Item2 == 1) && wcip.std.ds == 60))
                        {
                            retVal.MsgCode = "I-0063";
                            retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                        }
                        else
                        {
                            retVal.MsgCode = "I-0064";
                            retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                        }
                    }
                    break;
                case CarriagewayWidthResult.Standard1:
                    if (wcip.Rss == RoadSideStandard.NormalRoad)
                    {
                        retVal.MsgCode = "I-0052";
                        retVal.ResultType = VerificationResult.VerifyResultType.OK;
                    }
                    else
                    {
                        retVal.MsgCode = "I-0062";
                        retVal.ResultType = VerificationResult.VerifyResultType.OK;
                    }
                    break;
                case CarriagewayWidthResult.Standard2:
                case CarriagewayWidthResult.Standard3:
                    if (wcip.Rss == RoadSideStandard.NormalRoad)
                    {
                        retVal.MsgCode = "I-0060";
                        retVal.ResultType = VerificationResult.VerifyResultType.OK;
                    }
                    else
                    {
                        retVal.MsgCode = "I-0065";
                        retVal.ResultType = VerificationResult.VerifyResultType.OK;
                    }
                    break;
                case CarriagewayWidthResult.Special2:
                    if (wcip.Rss == RoadSideStandard.NormalRoad)
                    {
                        retVal.MsgCode = "I-0061";
                        retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                    }
                    else
                    {
                        retVal.MsgCode = "I-0066";
                        retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                    }
                    break;
                default:
                    break;
            }

            return retVal;
        }
        
        /// <summary>
        /// どの車線幅員基準値を使用しているか取得
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="dcss"></param>
        /// <returns></returns>
        private CarriagewayWidthResult GetCarriagewayWidthResult(WCInputParams wcip, IEnumerable<DesignCrossSectSurf_OGExtension> ieDcss)
        {
            var cw = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.W.CarriagewayWidth;
            var v = (from T in cw
                     where T.RoadType == wcip.std.rtT.Item1 &&
                           T.RoadClass == wcip.std.rtT.Item2 &&
                           T.RoadSideStandard == wcip.Rss
                     select T).FirstOrDefault();
            if (v is null)
            {
                v = (from T in cw
                     where T.RoadType == wcip.std.rtT.Item1 &&
                           T.RoadClass == wcip.std.rtT.Item2 &&
                           T.RoadSideStandard == RoadSideStandard.None
                     select T).FirstOrDefault();
                if (v is null)
                {
                    return CarriagewayWidthResult.NG;
                }
            }

            var roadWidth = ieDcss.Select(row => row.cspList.First()).Sum(row => row.roadWidth);

            if (v.STDVal1 == roadWidth) return CarriagewayWidthResult.Standard1;
            if (v.SPCVal1 == roadWidth) return CarriagewayWidthResult.Special1;
            if (v.STDVal2 == roadWidth) return CarriagewayWidthResult.Standard2;
            if (v.STDVal3.Contains(roadWidth)) return CarriagewayWidthResult.Standard3;
            if (v.SPCVal2 == roadWidth) return CarriagewayWidthResult.Special2;
            return CarriagewayWidthResult.NG;
        }

        /// <summary>
        /// 中央帯、中央分離帯の幅員のチェック
        /// 中央帯側帯は別ロジック
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        private (VerificationResult, VerificationResult) CheckForCenterStripWidth(WCInputParams wcip, CrossSect_OGExtension cs)
        {
            var cRetSt = this.GetCenterLineResult(wcip, cs, CenterLineCheckItem.Strip);
            var cRetSe = this.GetCenterLineResult(wcip, cs, CenterLineCheckItem.Separator);

            var retCenterStrip = ConvertCenterLineResult2VerificationResult(cRetSt, "中央帯");
            var retCenterSeparator = ConvertCenterLineResult2VerificationResult(cRetSe, "中央分離帯");

            return (retCenterStrip, retCenterSeparator);
        }

        /// <summary>
        /// 中央帯照査結果を照査結果に変換
        /// </summary>
        /// <param name="cRet"></param>
        /// <param name="msgPrm"></param>
        /// <returns></returns>
        private VerificationResult ConvertCenterLineResult2VerificationResult(CenterLineResult cRet, string msgPrm)
        {
            var retVal = new VerificationResult();
            switch (cRet)
            {
                case CenterLineResult.NG:
                    retVal.MsgCode = "W-0053";
                    retVal.SetMsgParameter(msgPrm);
                    retVal.ResultType = VerificationResult.VerifyResultType.NG;
                    break;
                case CenterLineResult.Standard:
                    retVal.MsgCode = "I-0073";
                    retVal.SetMsgParameter(msgPrm);
                    retVal.ResultType = VerificationResult.VerifyResultType.OK;
                    break;
                case CenterLineResult.Special:
                    retVal.MsgCode = "I-0074";
                    retVal.SetMsgParameter(msgPrm);
                    retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                    break;
                default:
                    break;
            }
            return retVal;
        }

        /// <summary>
        /// 中央帯の幅員チェック結果を取得
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="dcss"></param>
        /// <returns></returns>
        private CenterLineResult GetCenterLineResult(WCInputParams wcip, CrossSect_OGExtension cs, CenterLineCheckItem c)
        {
            //中央帯、中央側帯は両サイドの情報が必要
            decimal cWidth = 0M;
            decimal stdVal = 0M, spcVal = 0M;

            var cSprit = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.W.CenterSpritWidth;
            var cMarginal = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.W.CenterMarginalWidth;

            var csStd = (from T in cSprit
                         where T.RoadType == wcip.std.rtT.Item1 &&
                               T.RoadClass == wcip.std.rtT.Item2
                         select T).FirstOrDefault();
            var cmStd = (from T in cMarginal
                         where T.RoadType == wcip.std.rtT.Item1 &&
                               T.RoadClass == wcip.std.rtT.Item2
                         select T).FirstOrDefault();
            if (csStd is null || cmStd is null) return CenterLineResult.NG;

            switch (c)
            {
                case CenterLineCheckItem.Strip:
                    stdVal = csStd.STDVal + cmStd.STDVal * 2;
                    spcVal = (csStd.SPCVal == 0 ? csStd.STDVal : csStd.SPCVal) + (cmStd.SPCVal == 0 ? cmStd.STDVal : cmStd.SPCVal) * 2;

                    cWidth = (from T in cs.dcssList
                              where (T.name_J == Name_JItems.CenterStrip || T.group1Name == Name_JItems.CenterStrip || T.group2Name == Name_JItems.CenterStrip) && T.isTarget
                              select T.cspList.First().roadWidth).Sum();

                    break;
                case CenterLineCheckItem.Separator:
                    stdVal = csStd.STDVal;
                    spcVal = csStd.SPCVal == 0 ? csStd.STDVal : csStd.SPCVal;

                    cWidth = (from T in cs.dcssList
                              where (T.name_J == Name_JItems.CenterSprit || T.group1Name == Name_JItems.CenterSprit || T.group2Name == Name_JItems.CenterSprit) && T.isTarget
                              select T.cspList.First().roadWidth).Sum();

                    break;
                default:
                    break;
            }

            if (stdVal <= cWidth) return CenterLineResult.Standard;
            if (spcVal <= cWidth) return CenterLineResult.Special;
            return CenterLineResult.NG;
        }

        /// <summary>
        /// 中央帯側帯用の幅員チェック
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="dcss"></param>
        /// <returns></returns>
        private VerificationResult CheckForCenterMarginalStripWidth(WCInputParams wcip, IEnumerable<DesignCrossSectSurf_OGExtension> ieDcss)
        {
            var cm = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.W.CenterMarginalWidth;

            var cWidth = ieDcss.Select(row => row.cspList.First()).Sum(row => row.roadWidth);

            Func<CenterLineResult> func = null;
            func = () =>
            {
                var v = (from T in cm
                         where T.RoadType == wcip.std.rtT.Item1 &&
                               T.RoadClass == wcip.std.rtT.Item2
                         select T).FirstOrDefault();
                if (v is null) return CenterLineResult.NG;

                if (v.STDVal == cWidth) return CenterLineResult.Standard;
                if (v.SPCVal == cWidth) return CenterLineResult.Special;
                return CenterLineResult.NG;
            };

            return ConvertCenterLineResult2VerificationResult(func(), "中央帯側帯");
        }

        /// <summary>
        /// 路肩幅員のチェック
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="cs"></param>
        /// <param name="dDcss"></param>
        private void CheckForRoadShoulderWidth(WCInputParams wcip, CrossSect_OGExtension cs, 
                                               List<DesignCrossSectSurf_OGExtension> dcssList, WidthVerifyCheckPoint cPoint, Name_JItems cGroup)
        {
            var retVal = new VerificationResult();

            var lRoadShoulderGroup = new List<DesignCrossSectSurf_OGExtension>();
            var rRoadShoulderGroup = new List<DesignCrossSectSurf_OGExtension>();

            if (cGroup == Name_JItems.Roadshoulder)
            {
                //路肩用判定
                lRoadShoulderGroup = (from T in dcssList
                                      where T.name_J == Name_JItems.Roadshoulder ||
                                            (T.group1Name == Name_JItems.Roadshoulder && T.group1 != GroupCode.None) ||
                                            (T.group2Name == Name_JItems.Roadshoulder && T.group2 != GroupCode.None)
                                      select T).ToList();
                //逆サイドの右側路肩を取る
                rRoadShoulderGroup = (from T in cs.dcssList
                                      where T.name_J == Name_JItems.RoadshoulderR ||
                                            (T.group1Name == Name_JItems.RoadshoulderR && T.group1 != GroupCode.None) ||
                                            (T.group2Name == Name_JItems.RoadshoulderR && T.group2 != GroupCode.None) &&
                                            T.isTarget &&
                                            T.side != dcssList.First().side
                                      select T).ToList();
            }
            else 
            {
                //右側路肩用判定
                lRoadShoulderGroup = (from T in cs.dcssList
                                      where T.name_J == Name_JItems.Roadshoulder ||
                                            (T.group1Name == Name_JItems.Roadshoulder && T.group1 != GroupCode.None) ||
                                            (T.group2Name == Name_JItems.Roadshoulder && T.group2 != GroupCode.None) &&
                                            T.isTarget &&
                                            T.side != dcssList.First().side
                                      select T).ToList();
                //逆サイドの右側路肩を取る
                rRoadShoulderGroup = (from T in dcssList
                                      where T.name_J == Name_JItems.RoadshoulderR ||
                                            (T.group1Name == Name_JItems.RoadshoulderR && T.group1 != GroupCode.None) ||
                                            (T.group2Name == Name_JItems.RoadshoulderR && T.group2 != GroupCode.None)
                                      select T).ToList();
            }
            var lResult = GetRoadShoulderWidthResult(wcip, cs, lRoadShoulderGroup, false);
            var rResult = GetRoadShoulderWidthResult(wcip, cs, rRoadShoulderGroup, true, lResult);

            foreach (var item in lRoadShoulderGroup)
            {
                var vResult = new VerificationResult();
                switch (lResult)
                {
                    case RoadShoulderWidthResult.NG:
                        vResult.MsgCode = "W-0058";
                        vResult.ResultType = VerificationResult.VerifyResultType.NG;
                        break;
                    case RoadShoulderWidthResult.StandardL:
                        if (wcip.Rss == RoadSideStandard.NormalRoad)
                        {
                            vResult.MsgCode = "I-0085";
                        }
                        else
                        {
                            vResult.MsgCode = "I-0003";
                        }
                        vResult.ResultType = VerificationResult.VerifyResultType.OK;
                        break;
                    case RoadShoulderWidthResult.SpecialL:
                        vResult.MsgCode = "I-0086";
                        vResult.ResultType = VerificationResult.VerifyResultType.OK_C;
                        break;
                    case RoadShoulderWidthResult.DesireL:
                        vResult.MsgCode = "I-0087";
                        vResult.ResultType = VerificationResult.VerifyResultType.OK;
                        break;
                    case RoadShoulderWidthResult.Tunnel:
                        if (wcip.Rss == RoadSideStandard.NormalRoad)
                        {
                            vResult.MsgCode = "I-0084";
                        }
                        else
                        {
                            vResult.MsgCode = "I-0002";
                        }
                        vResult.ResultType = VerificationResult.VerifyResultType.OK_C;
                        break;
                    case RoadShoulderWidthResult.StandardR:
                    case RoadShoulderWidthResult.DesireR:
                    default:
                        break;
                }

                switch (cPoint)
                {
                    case WidthVerifyCheckPoint.Name_J:
                        item.result = vResult;
                        break;
                    case WidthVerifyCheckPoint.Group1:
                        item.group1Result = vResult;
                        break;
                    case WidthVerifyCheckPoint.Group2:
                        item.group2Result = vResult;
                        break;
                    default:
                        break;
                }
            }

            foreach (var item in rRoadShoulderGroup)
            {
                var vResult = new VerificationResult();
                switch (rResult)
                {
                    case RoadShoulderWidthResult.NG:
                        vResult.MsgCode = "W-0060";
                        vResult.ResultType = VerificationResult.VerifyResultType.NG;
                        break;
                    case RoadShoulderWidthResult.StandardR:
                        if (lResult == RoadShoulderWidthResult.Tunnel)
                        {
                            if (wcip.Rss == RoadSideStandard.NormalRoad)
                            {
                                vResult.MsgCode = "W-0059";
                            }
                            else
                            {
                                vResult.MsgCode = "W-0004";
                            }
                            vResult.ResultType = VerificationResult.VerifyResultType.NG;
                        }
                        else
                        {
                            if (wcip.Rss == RoadSideStandard.NormalRoad)
                            {
                                vResult.MsgCode = "I-0088";
                            }
                            else
                            {
                                vResult.MsgCode = "I-0004";
                            }
                            vResult.ResultType = VerificationResult.VerifyResultType.OK;
                        }
                        break;
                    case RoadShoulderWidthResult.DesireR:
                        if (lResult == RoadShoulderWidthResult.Tunnel)
                        {
                            if (wcip.Rss == RoadSideStandard.NormalRoad)
                            {
                                vResult.MsgCode = "W-0059";
                            }
                            else
                            {
                                vResult.MsgCode = "W-0004";
                            }
                            vResult.ResultType = VerificationResult.VerifyResultType.NG;
                        }
                        else
                        {
                            if (wcip.Rss == RoadSideStandard.NormalRoad)
                            {
                                vResult.MsgCode = "I-0088";
                            }
                            else
                            {
                                vResult.MsgCode = "I-0004";
                            }
                            vResult.ResultType = VerificationResult.VerifyResultType.OK;
                        }
                        break;
                    case RoadShoulderWidthResult.Tunnel:
                        if (wcip.Rss == RoadSideStandard.NormalRoad)
                        {
                            vResult.MsgCode = "I-0089";
                        }
                        else
                        {
                            vResult.MsgCode = "I-0005";
                        }
                        vResult.ResultType = VerificationResult.VerifyResultType.OK_C;
                        break;
                    case RoadShoulderWidthResult.StandardL:
                    case RoadShoulderWidthResult.SpecialL:
                    case RoadShoulderWidthResult.DesireL:
                    default:
                        break;
                }

                switch (cPoint)
                {
                    case WidthVerifyCheckPoint.Name_J:
                        item.result = vResult;
                        break;
                    case WidthVerifyCheckPoint.Group1:
                        item.group1Result = vResult;
                        break;
                    case WidthVerifyCheckPoint.Group2:
                        item.group2Result = vResult;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 路肩にどの幅員基準値を使用しているか取得
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="cs"></param>
        /// <param name="dcssList"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        private RoadShoulderWidthResult GetRoadShoulderWidthResult(WCInputParams wcip, CrossSect_OGExtension cs, 
                                                                   List<DesignCrossSectSurf_OGExtension> dcssList,
                                                                   bool isRightSide, RoadShoulderWidthResult lResult = RoadShoulderWidthResult.NG)
        {
            var rsWidth = dcssList.Select(row => row.cspList.First()).Sum(row => row.roadWidth);

            if (this.IsSeparated1LineType1Road(wcip, cs)) {
                var rss1 = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.W.RoadShoulderS1Width;
                var v = (from T in rss1
                         where T.RoadType == wcip.std.rtT.Item1 &&
                               T.RoadClass == wcip.std.rtT.Item2 &&
                               T.RoadSideStandard == wcip.Rss
                         select T).FirstOrDefault();
                if (v is null) return RoadShoulderWidthResult.NG;

                if (isRightSide)
                {
                    if (v.STDValR <= rsWidth) return RoadShoulderWidthResult.StandardR;
                }
                else
                {
                    if (v.STDValL <= rsWidth) return RoadShoulderWidthResult.StandardL;
                    if (v.SPCValL <= rsWidth) return RoadShoulderWidthResult.SpecialL;
                }
            }
            else
            {
                var rs = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.W.RoadShoulderWidth;
                var v = (from T in rs
                         where T.RoadType == wcip.std.rtT.Item1 &&
                               T.RoadClass == wcip.std.rtT.Item2 &&
                               T.RoadSideStandard == wcip.Rss
                         select T).FirstOrDefault();

                if (v is null) return RoadShoulderWidthResult.NG;

                if (isRightSide)
                {
                    if (lResult == RoadShoulderWidthResult.Tunnel && v.STDValR == v.TUNVal)
                    {
                        //路肩照査結果がトンネル基準値であり右側基準値とトンネル基準値が同じであるとき、先にトンネル基準値であるか判定する
                        if (v.DESValR <= rsWidth) return RoadShoulderWidthResult.DesireR;
                        if (v.TUNVal <= rsWidth) return RoadShoulderWidthResult.Tunnel;
                        if (v.STDValR <= rsWidth) return RoadShoulderWidthResult.StandardR;
                    }
                    else
                    {
                        if (v.DESValR <= rsWidth) return RoadShoulderWidthResult.DesireR;
                        if (v.STDValR <= rsWidth) return RoadShoulderWidthResult.StandardR;
                        if (v.TUNVal <= rsWidth) return RoadShoulderWidthResult.Tunnel;
                    }
                }
                else
                {
                    if (v.DESValL <= rsWidth) return RoadShoulderWidthResult.DesireL;
                    if (v.STDValL <= rsWidth) return RoadShoulderWidthResult.StandardL;
                    if (v.SPCValL <= rsWidth) return RoadShoulderWidthResult.SpecialL;
                    if (v.TUNVal <= rsWidth) return RoadShoulderWidthResult.Tunnel;
                }
            }

            return RoadShoulderWidthResult.NG;
        }

        /// <summary>
        /// 分離片側1車線の第1種道路か
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        private bool IsSeparated1LineType1Road(WCInputParams wcip, CrossSect_OGExtension cs)
        {
            var roadCount = (int)Math.Floor((double)cs.GetRoadCount() / 2);
            //単線であった場合は1車線あるものとする
            if (roadCount == 0) roadCount = 1;
            var hasCenterStrip = HasCenterStrip(cs);

            if (roadCount == 1 && hasCenterStrip && wcip.std.rtT.Item1 == 1)
            {
                //第1種道路で片側1車線、中央帯がある道路であるか
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 停車帯幅員のチェック
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="dcss"></param>
        /// <returns></returns>
        private VerificationResult CheckForStoppingLaneWidth(WCInputParams wcip, IEnumerable<DesignCrossSectSurf_OGExtension> ieDcss)
        {
            var retVal = new VerificationResult();
            var slWidth = (double)ieDcss.Select(row => row.cspList.First()).Sum(row => row.roadWidth);

            if (slWidth == 2.5)
            {
                retVal.MsgCode = "I-0079";
                retVal.ResultType = VerificationResult.VerifyResultType.OK;
            }
            else if (slWidth == 1.5)
            {
                retVal.MsgCode = "I-0080";
                retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
            }
            else
            {
                retVal.MsgCode = "W-0056";
                retVal.ResultType = VerificationResult.VerifyResultType.NG;
            }

            return retVal;
        }

        /// <summary>
        /// 植樹帯幅員のチェック
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="dcss"></param>
        /// <returns></returns>
        private VerificationResult CheckForPlantingLaneWidth(WCInputParams wcip, IEnumerable<DesignCrossSectSurf_OGExtension> ieDcss)
        {
            var retVal = new VerificationResult();
            var plWidth = (double)ieDcss.Select(row => row.cspList.First()).Sum(row => row.roadWidth);

            if (1.0 <= plWidth && plWidth <= 2.0)
            {
                if (plWidth == 1.5)
                {
                    retVal.MsgCode = "I-0081";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK;
                }
                else
                {
                    retVal.MsgCode = "I-0082";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                }
            }
            else if (2.0 < plWidth)
            {
                retVal.MsgCode = "I-0083";
                retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
            }
            else
            {
                retVal.MsgCode = "W-0057";
                retVal.ResultType = VerificationResult.VerifyResultType.NG;
            }

            return retVal;
        }

        /// <summary>
        /// 歩道幅員のチェック
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="dcss"></param>
        /// <returns></returns>
        private VerificationResult CheckForSideWalkWidth(WCInputParams wcip, IEnumerable<DesignCrossSectSurf_OGExtension> ieDcss, CrossSect_OGExtension cs)
        {
            var retVal = new VerificationResult();
            var sWidth = (double)ieDcss.Select(row => row.cspList.First()).Sum(row => row.roadWidth);
            //植樹帯があるなら路上施設幅員を加算しない
            var ssftWidth = HasPlantingZone(cs) ? 0.0 : this.GetWidthSSFT(wcip);
            double stdVal = wcip.Qpede ? 3.5 : 2.0;

            if ((stdVal + ssftWidth) <= sWidth)
            {
                retVal.MsgCode = "I-0072";
                retVal.ResultType = VerificationResult.VerifyResultType.OK;
            }
            else
            {
                retVal.MsgCode = "W-0052";
                retVal.ResultType = VerificationResult.VerifyResultType.NG;
            }

            return retVal;
        }

        /// <summary>
        /// 自転車歩行者道幅員のチェック
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="dcss"></param>
        /// <returns></returns>
        private VerificationResult CheckForSidepathWidth(WCInputParams wcip, IEnumerable<DesignCrossSectSurf_OGExtension> ieDcss, CrossSect_OGExtension cs)
        {
            var retVal = new VerificationResult();
            var sWidth = (double)ieDcss.Select(row => row.cspList.First()).Sum(row => row.roadWidth);
            var ssftWidth = HasPlantingZone(cs) ? 0.0 : this.GetWidthSSFT(wcip);
            double stdVal = wcip.Qpede ? 4.0 : 3.0;

            if ((stdVal + ssftWidth) <= sWidth)
            {
                retVal.MsgCode = "I-0071";
                retVal.ResultType = VerificationResult.VerifyResultType.OK;
            }
            else
            {
                retVal.MsgCode = "W-0051";
                retVal.ResultType = VerificationResult.VerifyResultType.NG;
            }

            return retVal;
        }

        /// <summary>
        /// 自転車通行帯幅員のチェック
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="dcss"></param>
        /// <returns></returns>
        private VerificationResult CheckForBikelaneWidth(WCInputParams wcip, IEnumerable<DesignCrossSectSurf_OGExtension> ieDcss)
        {
            var retVal = new VerificationResult();
            var bWidth = (double)ieDcss.Select(row => row.cspList.First()).Sum(row => row.roadWidth);

            if (1.5 <= bWidth)
            {
                retVal.MsgCode = "I-0069";
                retVal.ResultType = VerificationResult.VerifyResultType.OK;
            }
            else if (1.0 <= bWidth)
            {
                retVal.MsgCode = "I-0070";
                retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
            }
            else
            {
                retVal.MsgCode = "W-0050";
                retVal.ResultType = VerificationResult.VerifyResultType.NG;
            }

            return retVal;
        }

        /// <summary>
        /// 自転車道幅員のチェック
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="dcss"></param>
        /// <returns></returns>
        private VerificationResult CheckForCycletrackWidth(WCInputParams wcip, IEnumerable<DesignCrossSectSurf_OGExtension> ieDcss)
        {
            var retVal = new VerificationResult();
            var cWidth = (double)ieDcss.Select(row => row.cspList.First()).Sum(row => row.roadWidth);

            if (2.0 <= cWidth)
            {
                retVal.MsgCode = "I-0067";
                retVal.ResultType = VerificationResult.VerifyResultType.OK;
            }
            else if (1.5 <= cWidth)
            {
                retVal.MsgCode = "I-0068";
                retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
            }
            else
            {
                retVal.MsgCode = "W-0049";
                retVal.ResultType = VerificationResult.VerifyResultType.NG;
            }

            return retVal;
        }

        /// <summary>
        /// 路上施設に応じた追加幅員を取得
        /// </summary>
        /// <param name="wcip"></param>
        /// <returns></returns>
        private double GetWidthSSFT(WCInputParams wcip)
        {
            double retVal;
            switch (wcip.Ssft)
            {
                case StreetSideFacilitiesType.None:
                    retVal = 0.0;
                    break;
                case StreetSideFacilitiesType.Footbridge:
                    retVal = 3.0;
                    break;
                case StreetSideFacilitiesType.BenchShed:
                    retVal = 2.0;
                    break;
                case StreetSideFacilitiesType.Trees:
                    retVal = 1.5;
                    break;
                case StreetSideFacilitiesType.Bench:
                    retVal = 1.0;
                    break;
                case StreetSideFacilitiesType.ETC:
                    retVal = 0.5;
                    break;
                default:
                    retVal = 0.0;
                    break;
            }

            return retVal;
        }

        /// <summary>
        /// 路肩側帯幅員のチェック
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="dcss"></param>
        /// <returns></returns>
        private VerificationResult CheckForRoadShoulderStripWidth(WCInputParams wcip, IEnumerable<DesignCrossSectSurf_OGExtension> ieDcss)
        {
            var retVal = new VerificationResult();
            var chkRes = this.GetRoadShoulderStripWidthResult(wcip, ieDcss);

            switch (chkRes)
            {
                case RoadShoulderStripWidthResult.NG:
                    retVal.MsgCode = (wcip.Rss == RoadSideStandard.NormalRoad) ? "W-0063" : "W-0065";
                    retVal.ResultType = VerificationResult.VerifyResultType.NG;
                    break;
                case RoadShoulderStripWidthResult.Standard:
                    retVal.MsgCode = (wcip.Rss == RoadSideStandard.NormalRoad) ? "I-0092" : "I-0096";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK;
                    break;
                case RoadShoulderStripWidthResult.Special:
                    retVal.MsgCode = (wcip.Rss == RoadSideStandard.NormalRoad) ? "I-0093" : "W-XXXX";
                    retVal.ResultType = VerificationResult.VerifyResultType.OK_C;
                    break;
                default:
                    break;
            }

            return retVal;
        }

        /// <summary>
        /// 路肩側帯がどの幅員基準値を使っているか取得
        /// </summary>
        /// <param name="wcip"></param>
        /// <param name="dcss"></param>
        /// <returns></returns>
        private RoadShoulderStripWidthResult GetRoadShoulderStripWidthResult(WCInputParams wcip, IEnumerable<DesignCrossSectSurf_OGExtension> ieDcss)
        {
            var rssWidth = ieDcss.Select(row => row.cspList.First()).Sum(row => row.roadWidth);

            var rssw = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.W.RoadShoulderStripWidth;
            var v = (from T in rssw
                     where T.RoadType == wcip.std.rtT.Item1 &&
                           T.RoadClass == wcip.std.rtT.Item2 &&
                           T.RoadSideStandard == wcip.Rss
                     select T).FirstOrDefault();

            if (v is null) return RoadShoulderStripWidthResult.NG;

            if (v.STDVal == rssWidth) return RoadShoulderStripWidthResult.Standard;
            if (v.SPCVal == rssWidth) return RoadShoulderStripWidthResult.Special;
            return RoadShoulderStripWidthResult.NG;

        }

        /// <summary>
        /// 幅員構成路肩チェック用のチェックポイント
        /// </summary>
        /// <param name="vArray"></param>
        /// <returns></returns>
        private static int GetRoadShoulderVerifyPointDef(List<bool> vArray)
        {
            int retVal = 0;
           
            if (vArray.Count != ROADSHOULDER_DEP_NEST) throw new Exception("路肩判定の条件が変更されています。判定できません。");
            
            for (int i = 0; i < ROADSHOULDER_DEP_NEST; i++)
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
        private int GetRoadShoulderVerifyPoint(CrossSect_OGExtension cs)
        {
            int retVal = 0;
            Action<int> action = null;

            action = (cNest) =>
            {
                bool chkRes = false;

                switch (cNest)
                {
                    case (int)RoadShoulderChkNum.HasStoppingLane:
                        chkRes = this.HasStoppingLaneOrBicycleLane(cs);
                        break;
                    case (int)RoadShoulderChkNum.IsSeparatedRoadAndHasCenterStrip:
                        chkRes = this.IsSeparatedRoadAndHasCenterStrip(cs);
                        break;
                    case (int)RoadShoulderChkNum.HasRoadShoulder:
                        chkRes = this.HasRoadShoulder(cs);
                        break;
                    case (int)RoadShoulderChkNum.HasRightRoadShoulder:
                        chkRes = this.HasRightRoadShoulder(cs);
                        break;
                    default:
                        throw new Exception("未実装の路肩チェック項目です。判定できません。");
                }

                retVal += chkRes ? (int)Math.Pow(2, cNest) : 0;

                if (cNest != ROADSHOULDER_DEP_NEST - 1)
                {
                    cNest += 1;
                    action(cNest);
                }
            };
            action(0);

            return retVal;
        }
    }
}
