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
using static i_ConVerificationSystem.Forms.Gradient.OGInputParameter;
using static i_ConVerificationSystem.Structs.CrossSects.DesignCrossSectSurf;
using static i_ConVerificationSystem.Structs.OGExtensions;
using static i_ConVerificationSystem.Structs.OnesidedGradientVerificationResult;

namespace i_ConVerificationSystem.Verification
{
    class OnesidedGradient
    {
        private XElement selXAlignment { get; set; }
        private string selAlignmentName { get { return XMLLoader.Instance.GetAlignmentName(selXAlignment); } }
        private List<OnesidedGradientVerificationResult> vrList { get; set; }

        private enum OnesidedGradientRatePosition
        {
            /// <summary>
            /// 直線部の横断勾配～曲線内最大片勾配
            /// </summary>
            Straight2MaximumOnesidedGradientRate = 0,
            /// <summary>
            /// 片勾配すりつけ率の変化点～曲線内最大片勾配
            /// </summary>
            ChangePoint2MaximumOnesidedGradientRate,
            /// <summary>
            /// 曲線内最大片勾配～曲線内最大片勾配
            /// </summary>
            MaximumOnesidedGradientRate2MaximumOnesidedGradientRate
        }

        /// <summary>
        /// SlopeListを使うか
        /// </summary>
        private bool IsUseSlopeList { get; set; }
        private void SetIsUseSlopeList()
        {
            IsUseSlopeList = XMLLoader.Instance.IsUseSlopeList(selXAlignment);
        }

        /// <summary>
        /// 片勾配すりつけチェック
        /// </summary>
        /// <param name="selXAlignment"></param>
        /// <param name="ogip"></param>
        /// <param name="cs"></param>
        public void IsCorrectOnesidedGradient(XElement selXAlignment, OGInputParameters ogip)
        {
            if (ogip is null) return;

            this.selXAlignment = selXAlignment;
            var allCsList = XMLLoader.Instance.GetCrossSectsOG(selXAlignment, true);
            allCsList = AppSettingsManager.Instance.GetCrossSect_OGList_ApplyAllCS(selAlignmentName, allCsList);

            //初期処理
            SetIsUseSlopeList();
            this.SetOnesidedGradientStyle(allCsList, ogip.StraightLineTransverseGradient);

            //片勾配すりつけ率のチェック
            this.CheckForOnesidedGradientRate(ogip, allCsList);
            //排水のために必要な最小すりつけのチェック
            this.CheckForDrainage(ogip, allCsList);
            //片勾配すりつけ区間が緩和区間内に収まっているかのチェック
            this.CheckForMitigationArea();
            //直線から緩和区間なしに直接、円曲線に接続する場合に一様なすりつけを行う場合に、直線部1/2、円曲線部1/2の割合ですりつけを行っているか
            this.CheckForStrike2Curve(ogip, allCsList);
            //横断勾配0の点とKAの差がA/10以下となっているか
            this.CheckForTransverseGradient(ogip, allCsList);
            //横断勾配0の点がBC点と一致しているか
            //複合円の場合に小円1/2、大円1/2の割合ですりつけているか
            this.CheckForCurves(ogip, allCsList);

            OGVerificationResultItem.Instance.Update(selAlignmentName, vrList);
        }

        /// <summary>
        /// beginSType,endSType,OnesidedGradientShapeを決める
        /// </summary>
        private void SetOnesidedGradientStyle(List<CrossSect_OGExtension> allCsList, decimal sltg)
        {
            var sList = new List<Superelevation>();
            if (IsUseSlopeList)
            {
                var slopeProvider = new SlopeListClass();
                sList = slopeProvider.GetConvertedSlopeValueList(allCsList, sltg, selXAlignment);
            }
            else
            {
                sList = XMLLoader.Instance.GetSuperelevationList(selXAlignment);
            }

            vrList = new List<OnesidedGradientVerificationResult>();

            for (int vNum = 0; vNum < sList.Count(); vNum++)
            {
                var vr = new OnesidedGradientVerificationResult();
                var bOGVerificationResult = new OGVerificationResult();
                var eOGVerificationResult = new OGVerificationResult();

                vr.vrNum = vNum + 1;

                //起点側
                if (sList[vNum].BeginRunoutSta != 0)
                {
                    //基本型
                    bOGVerificationResult.OnesidedGradientShape = OGVerificationResult.OnesidedGradientShapeEnum.Standard;
                    bOGVerificationResult.usedS = sList[vNum];
                }
                else
                {
                    //それ以外。間違いなく直前に1件以上のvrListがあるはず
                    if (Math.Sign(sList[vNum - 1].FullSuperelev) != Math.Sign(sList[vNum].FullSuperelev))
                    {
                        //S型
                        bOGVerificationResult.OnesidedGradientShape = OGVerificationResult.OnesidedGradientShapeEnum.S;
                        bOGVerificationResult.usedS = sList[vNum];
                    }
                    else
                    {
                        //卵形
                        bOGVerificationResult.OnesidedGradientShape = OGVerificationResult.OnesidedGradientShapeEnum.Egg;
                        bOGVerificationResult.usedS = sList[vNum];
                    }
                }

                //終点側
                if (sList[vNum].EndofRunoutSta != 0)
                {
                    //基本型
                    eOGVerificationResult.OnesidedGradientShape = OGVerificationResult.OnesidedGradientShapeEnum.Standard;
                    eOGVerificationResult.usedS = sList[vNum];
                }
                else
                {
                    //それ以外。次のsListを見る必要がある
                    //最終アイテムに必ずEndofRunoutStaがあることが保証されていれば特に考える必要はない
                    if (vNum + 1 == sList.Count())
                    {
                        //最終アイテムなら、強制的に基本型
                        eOGVerificationResult.OnesidedGradientShape = OGVerificationResult.OnesidedGradientShapeEnum.Standard;
                        eOGVerificationResult.usedS = sList[vNum];
                    }
                    else
                    {
                        if (Math.Sign(sList[vNum].FullSuperelev) != Math.Sign(sList[vNum + 1].FullSuperelev))
                        {
                            //S型
                            eOGVerificationResult.OnesidedGradientShape = OGVerificationResult.OnesidedGradientShapeEnum.S;
                            eOGVerificationResult.usedS = sList[vNum];
                        }
                        else
                        {
                            //卵形
                            eOGVerificationResult.OnesidedGradientShape = OGVerificationResult.OnesidedGradientShapeEnum.Egg;
                            eOGVerificationResult.usedS = sList[vNum];
                        }
                    }
                }

                vr.beginPoint = bOGVerificationResult;
                vr.endPoint = eOGVerificationResult;
                vrList.Add(vr);
            }

            //累加距離標とすりつけ長をセットする
            SetTransverseGradient0PointSta(allCsList, sltg);
        }

        /// <summary>
        /// 累加距離標とすりつけ長（画面表示用）をセットする
        /// </summary>
        /// <param name="csList"></param>
        /// <param name="ogip"></param>
        private void SetTransverseGradient0PointSta(List<CrossSect_OGExtension> csList, decimal sltg)
        {
            var csProvider = new CrossSect_OGExtension();

            for (int i = 0; i < vrList.Count(); i++)
            {
                //起点側
                switch (vrList[i].beginPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        vrList[i].beginPoint.beginSta = vrList[i].beginPoint.usedS.BeginRunoutSta;
                        vrList[i].beginPoint.endSta = vrList[i].beginPoint.usedS.FullSuperSta;
                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                        vrList[i].beginPoint.beginSta = vrList[i - 1].endPoint.usedS.RunoffSta;
                        vrList[i].beginPoint.endSta = vrList[i].beginPoint.usedS.FullSuperSta;
                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        var bSta = vrList[i - 1].endPoint.usedS.RunoffSta;
                        var eSta = vrList[i].beginPoint.usedS.FullSuperSta;

                        var eCs = GetCrossSectMostFarFromEndPointToFHPosition(csList, bSta, eSta);
                        var (elb, _) = eCs.GetRoadwaysWidthFromFHPosition();

                        decimal q = 0.0M;
                        if (vrList[i].beginPoint.usedS.FlatSta != 0)
                        {
                            //FlatStaを持つとき
                            vrList[i].beginPoint.beginSta = vrList[i].beginPoint.usedS.FlatSta;
                            vrList[i].beginPoint.endSta = eSta;
                        }
                        else if (vrList[i - 1].endPoint.usedS.HasEndSideReverseCrown &&
                                 vrList[i].beginPoint.usedS.HasBeginSideReverseCrown)
                        {
                            //両区間にReverseCrownがあるとき
                            var stab = vrList[i].beginPoint.usedS.BeginSideReverseCrown - vrList[i - 1].endPoint.usedS.EndSideReverseCrown;
                            q = 2;
                            stab = Math.Round(stab / q, 3, MidpointRounding.AwayFromZero);

                            //起点側の終了点はFullSupereSta
                            vrList[i].beginPoint.beginSta = vrList[i - 1].endPoint.usedS.EndSideReverseCrown + stab;
                            vrList[i].beginPoint.endSta = eSta;
                        }
                        else if (vrList[i].beginPoint.usedS.HasBeginSideReverseCrown)
                        {
                            //自区間にReverseCrownがあるとき
                            //vrList[i-1].Runoff - vrList[i].BeginSideReverseCrown間
                            var (deltaI, _) = GetDeltaI(i - 1, sltg, false);
                            var (ls, _, _) = GetLs(i - 1, false);
                            q = CommonMethod.GetOnesidedGradientQ(elb, deltaI, ls);
                            var stad = Math.Round((elb * Math.Abs(vrList[i - 1].endPoint.usedS.FullSuperelev) / 100) / q, 3, MidpointRounding.AwayFromZero);

                            //起点側の終了点はFullSupereSta
                            vrList[i].beginPoint.beginSta = vrList[i - 1].endPoint.usedS.RunoffSta + stad;
                            vrList[i].beginPoint.endSta = eSta;
                        }
                        else if (vrList[i - 1].endPoint.usedS.HasEndSideReverseCrown)
                        {
                            //前区間にReverseCrownがあるとき
                            //vrList[i-1].EndSideReverseCrown - vrList[i].FullSuper間
                            var (deltaI, _) = GetDeltaI(i, sltg, true);
                            var (ls, _, _) = GetLs(i, true);
                            q = CommonMethod.GetOnesidedGradientQ(elb, deltaI, ls);
                            var stac = Math.Round((elb * sltg / 100) / q, 3, MidpointRounding.AwayFromZero);

                            //起点側の終了点はFullSupereSta
                            vrList[i].beginPoint.beginSta = vrList[i - 1].endPoint.usedS.EndSideReverseCrown + stac;
                            vrList[i].beginPoint.endSta = eSta;
                        }
                        else
                        {
                            //どちらにもReverseCrownが無いとき
                            //vrList[i-1].Runoff - vrList[i].FullSuper間
                            var (deltaI, _) = GetDeltaI(i, sltg, true);
                            var (ls, _, _) = GetLs(i, true);
                            q = CommonMethod.GetOnesidedGradientQ(elb, deltaI, ls);
                            var staa = Math.Round((elb * Math.Abs(vrList[i - 1].endPoint.usedS.FullSuperelev) / 100) / q, 3, MidpointRounding.AwayFromZero);

                            //起点側の終了点はFullSupereSta
                            vrList[i].beginPoint.beginSta = bSta + staa;
                            vrList[i].beginPoint.endSta = eSta;
                        }
                        break;
                    default:
                        break;
                }

                //終点側
                switch (vrList[i].endPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        vrList[i].endPoint.beginSta = vrList[i].endPoint.usedS.RunoffSta;
                        vrList[i].endPoint.endSta = vrList[i].endPoint.usedS.EndofRunoutSta;
                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                        vrList[i].endPoint.beginSta = vrList[i].endPoint.usedS.RunoffSta;
                        vrList[i].endPoint.endSta = vrList[i + 1].beginPoint.usedS.FullSuperSta;
                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        var bSta = vrList[i].endPoint.usedS.RunoffSta;
                        var eSta = vrList[i + 1].beginPoint.usedS.FullSuperSta;

                        var eCs = GetCrossSectMostFarFromEndPointToFHPosition(csList, bSta, eSta);
                        var (elb, _) = eCs.GetRoadwaysWidthFromFHPosition();

                        decimal q = 0.0M;
                        if (vrList[i].endPoint.usedS.FlatSta != 0)
                        {
                            //FlatStaを持つとき
                            vrList[i].endPoint.beginSta = bSta;
                            vrList[i].endPoint.endSta = vrList[i].endPoint.usedS.FlatSta;
                        }
                        else if (vrList[i].endPoint.usedS.HasEndSideReverseCrown &&
                                 vrList[i + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                        {
                            //両区間にReverseCrownがあるとき
                            var stab = vrList[i + 1].beginPoint.usedS.BeginSideReverseCrown - vrList[i].endPoint.usedS.EndSideReverseCrown;
                            q = 2;
                            stab = Math.Round(stab / q, 3, MidpointRounding.AwayFromZero);

                            //終点側の開始点はRunoffSta
                            vrList[i].endPoint.beginSta = bSta;
                            vrList[i].endPoint.endSta = vrList[i].endPoint.usedS.EndSideReverseCrown + stab;
                        }
                        else if (vrList[i].endPoint.usedS.HasEndSideReverseCrown)
                        {
                            //自区間にReverseCrownがあるとき
                            //vrList[i].EndSideReverseCrown - vrList[i+1].FullSuper間
                            var (deltaI, _) = GetDeltaI(i + 1, sltg, true);
                            var (ls, _, _) = GetLs(i + 1, true);
                            q = CommonMethod.GetOnesidedGradientQ(elb, deltaI, ls);
                            var stad = Math.Round((elb * sltg / 100) / q, 3, MidpointRounding.AwayFromZero);

                            //終点側の開始点はRunoffSta
                            vrList[i].endPoint.beginSta = bSta;
                            vrList[i].endPoint.endSta = vrList[i].endPoint.usedS.EndSideReverseCrown + stad;
                        }
                        else if (vrList[i + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                        {
                            //次区間にReverseCrownがあるとき
                            //vrList[i].Runoff - vrList[i+1].BeginSideReverseCrown間
                            var (deltaI, _) = GetDeltaI(i, sltg, false);
                            var (ls, _, _) = GetLs(i, false);
                            q = CommonMethod.GetOnesidedGradientQ(elb, deltaI, ls);
                            var stac = Math.Round((elb * Math.Abs(vrList[i].endPoint.usedS.FullSuperelev) / 100) / q, 3, MidpointRounding.AwayFromZero);

                            //終点側の開始点はRunoffSta
                            vrList[i].endPoint.beginSta = bSta;
                            vrList[i].endPoint.endSta = vrList[i].endPoint.usedS.RunoffSta + stac;
                        }
                        else
                        {
                            //どちらにもReverseCrownが無いとき
                            //vrList[i].Runoff - vrList[i+1].FullSuper間
                            var (deltaI, _) = GetDeltaI(i + 1, sltg, true);
                            var (ls, _, _) = GetLs(i + 1, true);
                            q = CommonMethod.GetOnesidedGradientQ(elb, deltaI, ls);
                            var staa = Math.Round((elb * Math.Abs(vrList[i].endPoint.usedS.FullSuperelev) / 100) / q, 3, MidpointRounding.AwayFromZero);

                            //終点側の開始点はRunoffSta
                            vrList[i].endPoint.beginSta = bSta;
                            vrList[i].endPoint.endSta = vrList[i].endPoint.usedS.RunoffSta + staa;
                        }
                        break;
                    default:
                        break;
                }

                //すりつけ長を計算
                vrList[i].beginPoint.Ls = vrList[i].beginPoint.endSta - vrList[i].beginPoint.beginSta;
                vrList[i].endPoint.Ls = vrList[i].endPoint.endSta - vrList[i].endPoint.beginSta;
            }
        }

        /// <summary>
        /// 片勾配すりつけ率のチェック
        /// </summary>
        /// <param name="ogip"></param>
        /// <param name="cs"></param>
        private void CheckForOnesidedGradientRate(OGInputParameters ogip, List<CrossSect_OGExtension> csList)
        {
            var ogr = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.OG.OnesidedGradientRate;
            var r = (from T in ogr
                     where T.DesignSpeed == ogip.si.ds
                     select T.Rate).FirstOrDefault();
            if (r == 0) return;
            decimal q = r;
            var csProvider = new CrossSect_OGExtension();

            for (int i = 0; i < vrList.Count(); i++)
            {
                var (bBSta, bESta) = GetChangePointSta(i, true);
                var (eBSta, eESta) = GetChangePointSta(i, false);

                var bCs = GetCrossSectMostFarFromEndPointToFHPosition(csList, bBSta, bESta);
                var eCs = GetCrossSectMostFarFromEndPointToFHPosition(csList, eBSta, eESta);

                var (blb, brb) = bCs.GetRoadwaysWidthFromFHPosition();
                var (elb, erb) = eCs.GetRoadwaysWidthFromFHPosition();

                vrList[i].beginPoint.VR_OnesidedRate = new VerificationResult();
                vrList[i].endPoint.VR_OnesidedRate = new VerificationResult();

                var (bDeltaI, bNonAdverseDeltaI) = GetDeltaI(i, ogip.StraightLineTransverseGradient, true);
                var (eDeltaI, eNonAdverseDeltaI) = GetDeltaI(i, ogip.StraightLineTransverseGradient, false);
                var (bLs, bNonAdverseLs, bResPos) = GetLs(i, true);
                var (eLs, eNonAdverseLs, eResPos) = GetLs(i, false);

                //起点側チェック
                switch (vrList[i].beginPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        var qma = CommonMethod.GetOnesidedGradientRate(blb,
                                                                       bDeltaI,
                                                                       bLs);
                        decimal qmad = q;
                        string fRated = "";
                        if (vrList[i].beginPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                        {
                            qmad = CommonMethod.GetOnesidedGradientRate(blb,
                                                                        bNonAdverseDeltaI,
                                                                        bNonAdverseLs);
                            fRated = CommonMethod.GetOnesidedGradientFormattedRate(blb,
                                                                                   bNonAdverseDeltaI,
                                                                                   bNonAdverseLs);

                            if (bDeltaI == 0)
                            {
                                qma = q;
                            }
                            if (bNonAdverseDeltaI == 0)
                            {
                                qmad = q;
                            }
                        }

                        var fRate = CommonMethod.GetOnesidedGradientFormattedRate(blb, bDeltaI, bLs);
                        string gfRate = "";
                        if (vrList[i].beginPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                        {
                            gfRate = $"左：{fRate}{Environment.NewLine}右：{fRated}";
                        }
                        else
                        {
                            gfRate = $"{fRate}";
                        }

                        switch (bResPos)
                        {
                            case OnesidedGradientRatePosition.Straight2MaximumOnesidedGradientRate:
                                vrList[i].beginPoint.Straight2MaximumOnesidedGradientRate = gfRate;
                                break;
                            case OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate:
                                vrList[i].beginPoint.ChangePoint2MaximumOnesidedGradientRate = gfRate;
                                break;
                            case OnesidedGradientRatePosition.MaximumOnesidedGradientRate2MaximumOnesidedGradientRate:
                                vrList[i].beginPoint.MaximumOnesidedGradientRate2MaximumOnesidedGradientRate = gfRate;
                                break;
                            default:
                                break;
                        }

                        if (q <= qma && q <= qmad)
                        {
                            vrList[i].beginPoint.VR_OnesidedRate.ResultType = VerificationResult.VerifyResultType.OK;
                            vrList[i].beginPoint.VR_OnesidedRate.MsgCode = "I-0025";
                        }
                        else
                        {
                            vrList[i].beginPoint.VR_OnesidedRate.ResultType = VerificationResult.VerifyResultType.NG;
                            vrList[i].beginPoint.VR_OnesidedRate.MsgCode = "W-0021";
                        }

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        //起点側は前回終点側の情報をもとに判定(前回終点側と同じ結果になるはず？)
                        qma = CommonMethod.GetOnesidedGradientRate(blb,
                                                                   bDeltaI,
                                                                   bLs);

                        fRate = CommonMethod.GetOnesidedGradientFormattedRate(blb, bDeltaI, bLs);
                        switch (bResPos)
                        {
                            case OnesidedGradientRatePosition.Straight2MaximumOnesidedGradientRate:
                                vrList[i].beginPoint.Straight2MaximumOnesidedGradientRate = fRate;
                                break;
                            case OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate:
                                vrList[i].beginPoint.ChangePoint2MaximumOnesidedGradientRate = fRate;
                                break;
                            case OnesidedGradientRatePosition.MaximumOnesidedGradientRate2MaximumOnesidedGradientRate:
                                vrList[i].beginPoint.MaximumOnesidedGradientRate2MaximumOnesidedGradientRate = fRate;
                                break;
                            default:
                                break;
                        }

                        if (q <= qma)
                        {
                            vrList[i].beginPoint.VR_OnesidedRate.ResultType = VerificationResult.VerifyResultType.OK;
                            vrList[i].beginPoint.VR_OnesidedRate.MsgCode = "I-0025";
                        }
                        else
                        {
                            vrList[i].beginPoint.VR_OnesidedRate.ResultType = VerificationResult.VerifyResultType.NG;
                            vrList[i].beginPoint.VR_OnesidedRate.MsgCode = "W-0021";
                        }
                        break;
                    default:
                        break;
                }

                //終点側チェック
                switch (vrList[i].endPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        var qma = CommonMethod.GetOnesidedGradientRate(elb,
                                                                       eDeltaI,
                                                                       eLs);
                        decimal qmad = q;
                        string fRated = "";
                        if (vrList[i].endPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                        {
                            qmad = CommonMethod.GetOnesidedGradientRate(elb,
                                                                        eNonAdverseDeltaI,
                                                                        eNonAdverseLs);
                            fRated = CommonMethod.GetOnesidedGradientFormattedRate(elb,
                                                                        eNonAdverseDeltaI,
                                                                        eNonAdverseLs);

                            if (eDeltaI == 0)
                            {
                                qma = q;
                            }
                            if (eNonAdverseDeltaI == 0)
                            {
                                qmad = q;
                            }
                        }

                        var fRate = CommonMethod.GetOnesidedGradientFormattedRate(elb, eDeltaI, eLs);
                        string gfRate = "";
                        if (vrList[i].endPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                        {
                            gfRate = $"左：{fRate}{Environment.NewLine}右：{fRated}";
                        }
                        else
                        {
                            gfRate = $"{fRate}";
                        }

                        switch (eResPos)
                        {
                            case OnesidedGradientRatePosition.Straight2MaximumOnesidedGradientRate:
                                vrList[i].endPoint.Straight2MaximumOnesidedGradientRate = gfRate;
                                break;
                            case OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate:
                                vrList[i].endPoint.ChangePoint2MaximumOnesidedGradientRate = gfRate;
                                break;
                            case OnesidedGradientRatePosition.MaximumOnesidedGradientRate2MaximumOnesidedGradientRate:
                                vrList[i].endPoint.MaximumOnesidedGradientRate2MaximumOnesidedGradientRate = gfRate;
                                break;
                            default:
                                break;
                        }

                        if (q <= qma && q <= qmad)
                        {
                            vrList[i].endPoint.VR_OnesidedRate.ResultType = VerificationResult.VerifyResultType.OK;
                            vrList[i].endPoint.VR_OnesidedRate.MsgCode = "I-0025";
                        }
                        else
                        {
                            vrList[i].endPoint.VR_OnesidedRate.ResultType = VerificationResult.VerifyResultType.NG;
                            vrList[i].endPoint.VR_OnesidedRate.MsgCode = "W-0021";
                        }

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        qma = CommonMethod.GetOnesidedGradientRate(elb,
                                                                   eDeltaI,
                                                                   eLs);

                        fRate = CommonMethod.GetOnesidedGradientFormattedRate(elb, eDeltaI, eLs);
                        switch (eResPos)
                        {
                            case OnesidedGradientRatePosition.Straight2MaximumOnesidedGradientRate:
                                vrList[i].endPoint.Straight2MaximumOnesidedGradientRate = fRate;
                                break;
                            case OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate:
                                vrList[i].endPoint.ChangePoint2MaximumOnesidedGradientRate = fRate;
                                break;
                            case OnesidedGradientRatePosition.MaximumOnesidedGradientRate2MaximumOnesidedGradientRate:
                                vrList[i].endPoint.MaximumOnesidedGradientRate2MaximumOnesidedGradientRate = fRate;
                                break;
                            default:
                                break;
                        }

                        if (q <= qma)
                        {
                            vrList[i].endPoint.VR_OnesidedRate.ResultType = VerificationResult.VerifyResultType.OK;
                            vrList[i].endPoint.VR_OnesidedRate.MsgCode = "I-0025";
                        }
                        else
                        {
                            vrList[i].endPoint.VR_OnesidedRate.ResultType = VerificationResult.VerifyResultType.NG;
                            vrList[i].endPoint.VR_OnesidedRate.MsgCode = "W-0021";
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Δiを算出
        /// </summary>
        /// <param name="currentIndex"></param>
        /// <param name="normalCrown"></param>
        /// <param name="isBeginPoint"></param>
        /// <returns></returns>
        private (decimal deltaI, decimal nonAdverseDeltaI) GetDeltaI(int currentIndex, decimal normalCrown, bool isBeginPoint)
        {
            decimal retDeltaI = 0.0M, retNonAdverseDeltaI = 0.0M;

            if (isBeginPoint)
            {
                //起点側チェック
                switch (vrList[currentIndex].beginPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        var fs = vrList[currentIndex].beginPoint.usedS.FullSuperelev;
                        var nc = normalCrown;

                        if (vrList[currentIndex].beginPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                retDeltaI = Math.Abs(fs) + nc;
                                retNonAdverseDeltaI = Math.Abs(fs) + nc;
                            }
                            else
                            {
                                if (IsUseSlopeList)
                                {
                                    retDeltaI = vrList[currentIndex].beginPoint.usedS.LeftDeltaI;
                                    retNonAdverseDeltaI = vrList[currentIndex].beginPoint.usedS.RightDeltaI;
                                }
                                else
                                {
                                    retDeltaI = Math.Abs(fs - (-nc));
                                    retNonAdverseDeltaI = Math.Abs(fs - nc);
                                }
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                retDeltaI = Math.Abs(fs) - nc;
                            }
                            else
                            {
                                retDeltaI = Math.Abs(fs) + nc;
                            }
                        }

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        fs = vrList[currentIndex].beginPoint.usedS.FullSuperelev;
                        nc = normalCrown;

                        if (vrList[currentIndex].beginPoint.usedS.FlatSta != 0)
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                retDeltaI = Math.Abs(fs) - nc;
                            }
                            else
                            {
                                if (vrList[currentIndex - 1].endPoint.usedS.HasEndSideReverseCrown)
                                {
                                    //1つ前の区間にReverseCrownがあるか
                                    retDeltaI = Math.Abs(fs) + nc;
                                }
                                else
                                {
                                    retDeltaI = Math.Abs(fs);
                                }
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                retDeltaI = Math.Abs(fs) - nc;
                            }
                            else
                            {
                                if (vrList[currentIndex - 1].endPoint.usedS.HasEndSideReverseCrown)
                                {
                                    retDeltaI = Math.Abs(fs) + nc;
                                }
                                else
                                {
                                    //前区間の最大片勾配を減算
                                    retDeltaI = Math.Abs(fs - vrList[currentIndex - 1].endPoint.usedS.FullSuperelev);
                                }
                            }
                        }

                        break;
                    default:
                        break;
                }
            }
            else
            {
                //終点側チェック
                switch (vrList[currentIndex].endPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        var fs = vrList[currentIndex].endPoint.usedS.FullSuperelev;
                        var nc = normalCrown;

                        if (vrList[currentIndex].endPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                retDeltaI = Math.Abs(fs) - nc;
                                retNonAdverseDeltaI = Math.Abs(fs) - nc;
                            }
                            else
                            {
                                if (IsUseSlopeList)
                                {
                                    retDeltaI = vrList[currentIndex].endPoint.usedS.LeftDeltaI;
                                    retNonAdverseDeltaI = vrList[currentIndex].endPoint.usedS.RightDeltaI;
                                }
                                else
                                {
                                    retDeltaI = Math.Abs(fs - (-nc));
                                    retNonAdverseDeltaI = Math.Abs(fs - nc);
                                }
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                retDeltaI = Math.Abs(fs) - nc;
                            }
                            else
                            {
                                retDeltaI = Math.Abs(fs) + nc;
                            }
                        }

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        fs = vrList[currentIndex].endPoint.usedS.FullSuperelev;
                        nc = normalCrown;

                        if (vrList[currentIndex].endPoint.usedS.FlatSta != 0)
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                retDeltaI = Math.Abs(fs) - nc;
                            }
                            else
                            {
                                if (vrList[currentIndex + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                                {
                                    retDeltaI = Math.Abs(fs) + nc;
                                }
                                else
                                {
                                    retDeltaI = Math.Abs(fs);
                                }
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                retDeltaI = Math.Abs(fs) - nc;
                            }
                            else
                            {
                                if (vrList[currentIndex + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                                {
                                    retDeltaI = Math.Abs(fs) + nc;
                                }
                                else
                                {
                                    //次区間の最大片勾配から減算
                                    retDeltaI = Math.Abs(vrList[currentIndex + 1].beginPoint.usedS.FullSuperelev - fs);
                                }
                            }
                        }

                        break;
                    default:
                        break;
                }
            }

            return (retDeltaI, retNonAdverseDeltaI);
        }

        /// <summary>
        /// Lsを算出
        /// </summary>
        /// <param name="currentIndex"></param>
        /// <param name="isBeginPoint"></param>
        /// <returns></returns>
        private (decimal ls, decimal nonAdverseLs, OnesidedGradientRatePosition resPos) GetLs(int currentIndex, bool isBeginPoint)
        {
            decimal retLs = 0.0M, retNonAdverseLs = 0.0M;
            OnesidedGradientRatePosition retResPos = OnesidedGradientRatePosition.Straight2MaximumOnesidedGradientRate;

            if (isBeginPoint)
            {
                //起点側チェック
                switch (vrList[currentIndex].beginPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        var bSta = vrList[currentIndex].beginPoint.usedS.BeginRunoutSta;
                        var eSta = vrList[currentIndex].beginPoint.usedS.FullSuperSta;

                        if (vrList[currentIndex].beginPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retLs = rcSta - bSta;
                                retNonAdverseLs = eSta - bSta;
                                retResPos = OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate;
                            }
                            else
                            {
                                retLs = eSta - bSta;
                                retNonAdverseLs = eSta - bSta;
                                retResPos = OnesidedGradientRatePosition.Straight2MaximumOnesidedGradientRate;
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retLs = eSta - rcSta;
                                retResPos = OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate;
                            }
                            else
                            {
                                retLs = eSta - bSta;
                                retResPos = OnesidedGradientRatePosition.Straight2MaximumOnesidedGradientRate;
                            }
                        }

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        bSta = vrList[currentIndex - 1].endPoint.usedS.RunoffSta;
                        eSta = vrList[currentIndex].beginPoint.usedS.FullSuperSta;

                        if (vrList[currentIndex - 1].endPoint.usedS.FlatSta != 0)
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retLs = eSta - rcSta;
                                retResPos = OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate;
                            }
                            else
                            {
                                if (vrList[currentIndex - 1].endPoint.usedS.HasEndSideReverseCrown)
                                {
                                    //1つ前の区間にReverseCrownがあるか
                                    var rcSta = vrList[currentIndex - 1].endPoint.usedS.EndSideReverseCrown;
                                    retLs = eSta - rcSta;
                                    retResPos = OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate;
                                }
                                else
                                {
                                    var flatSta = vrList[currentIndex - 1].endPoint.usedS.FlatSta;
                                    retLs = eSta - flatSta;
                                    retResPos = OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate;
                                }
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retLs = eSta - rcSta;
                                retResPos = OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate;
                            }
                            else
                            {
                                if (vrList[currentIndex - 1].endPoint.usedS.HasEndSideReverseCrown)
                                {
                                    //1つ前の区間にReverseCrownがあるか
                                    var rcSta = vrList[currentIndex - 1].endPoint.usedS.EndSideReverseCrown;
                                    retLs = eSta - rcSta;
                                    retResPos = OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate;
                                }
                                else
                                {
                                    retLs = eSta - bSta;
                                    retResPos = OnesidedGradientRatePosition.MaximumOnesidedGradientRate2MaximumOnesidedGradientRate;
                                }
                            }
                        }

                        break;
                    default:
                        break;
                }
            }
            else
            {
                //終点側チェック
                switch (vrList[currentIndex].endPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        var bSta = vrList[currentIndex].endPoint.usedS.RunoffSta;
                        var eSta = vrList[currentIndex].endPoint.usedS.EndofRunoutSta;

                        if (vrList[currentIndex].endPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                retLs = rcSta - bSta;
                                retNonAdverseLs = eSta - bSta;
                                retResPos = OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate;
                            }
                            else
                            {
                                retLs = eSta - bSta;
                                retNonAdverseLs = eSta - bSta;
                                retResPos = OnesidedGradientRatePosition.Straight2MaximumOnesidedGradientRate;
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                retLs = rcSta - bSta;
                                retResPos = OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate;
                            }
                            else
                            {
                                retLs = eSta - bSta;
                                retResPos = OnesidedGradientRatePosition.Straight2MaximumOnesidedGradientRate;
                            }
                        }

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        bSta = vrList[currentIndex].endPoint.usedS.RunoffSta;
                        eSta = vrList[currentIndex + 1].beginPoint.usedS.FullSuperSta;

                        if (vrList[currentIndex].endPoint.usedS.FlatSta != 0)
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                retLs = rcSta - bSta;
                                retResPos = OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate;
                            }
                            else
                            {
                                if (vrList[currentIndex + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                                {
                                    //次区間にReverseCrownがあるか
                                    var rcSta = vrList[currentIndex + 1].beginPoint.usedS.BeginSideReverseCrown;
                                    retLs = rcSta - bSta;
                                    retResPos = OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate;
                                }
                                else
                                {
                                    var flatSta = vrList[currentIndex].endPoint.usedS.FlatSta;
                                    retLs = flatSta - bSta;
                                    retResPos = OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate;
                                }
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                retLs = rcSta - bSta;
                                retResPos = OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate;
                            }
                            else
                            {
                                if (vrList[currentIndex + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                                {
                                    //次区間にReverseCrownがあるか
                                    var rcSta = vrList[currentIndex + 1].beginPoint.usedS.BeginSideReverseCrown;
                                    retLs = rcSta - bSta;
                                    retResPos = OnesidedGradientRatePosition.ChangePoint2MaximumOnesidedGradientRate;
                                }
                                else
                                {
                                    retLs = eSta - bSta;
                                    retResPos = OnesidedGradientRatePosition.MaximumOnesidedGradientRate2MaximumOnesidedGradientRate;
                                }
                            }
                        }

                        break;
                    default:
                        break;
                }
            }

            return (retLs, retNonAdverseLs, retResPos);
        }

        /// <summary>
        /// 排水のために必要なすりつけ照査のためのLsを算出
        /// </summary>
        /// <param name="currentIndex"></param>
        /// <param name="isBeginPoint"></param>
        /// <returns></returns>
        private (decimal ls, decimal nonAdverseLs) GetLsForDrainage(int currentIndex, bool isBeginPoint)
        {
            decimal retLs = 0.0M, retNonAdverseLs = 0.0M;

            if (isBeginPoint)
            {
                //起点側チェック
                switch (vrList[currentIndex].beginPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        var bSta = vrList[currentIndex].beginPoint.usedS.BeginRunoutSta;
                        var eSta = vrList[currentIndex].beginPoint.usedS.FullSuperSta;

                        if (vrList[currentIndex].beginPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retLs = rcSta - bSta;
                                retNonAdverseLs = eSta - bSta;
                            }
                            else
                            {
                                retLs = eSta - bSta;
                                retNonAdverseLs = eSta - bSta;
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retLs = rcSta - bSta;
                            }
                            else
                            {
                                retLs = eSta - bSta;
                            }
                        }

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        bSta = vrList[currentIndex - 1].endPoint.usedS.RunoffSta;
                        eSta = vrList[currentIndex].beginPoint.usedS.FullSuperSta;

                        if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                        {
                            if (vrList[currentIndex - 1].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                //ReverseCrown - ReverseCrown
                                var bRcSta = vrList[currentIndex - 1].endPoint.usedS.EndSideReverseCrown;
                                var eRcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retLs = eRcSta - bRcSta;
                            }
                            else
                            {
                                //RunoffSta - ReverseCrown
                                var rcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retLs = rcSta - bSta;
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex - 1].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                //1つ前の区間にReverseCrownがあるか
                                //ReverseCrown - FullSuperSta
                                var rcSta = vrList[currentIndex - 1].endPoint.usedS.EndSideReverseCrown;
                                retLs = eSta - rcSta;
                            }
                            else
                            {
                                //RunoffSta - FullSuperSta
                                retLs = eSta - bSta;
                            }
                        }

                        break;
                    default:
                        break;
                }
            }
            else
            {
                //終点側チェック
                switch (vrList[currentIndex].endPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        var bSta = vrList[currentIndex].endPoint.usedS.RunoffSta;
                        var eSta = vrList[currentIndex].endPoint.usedS.EndofRunoutSta;

                        if (vrList[currentIndex].endPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                retLs = eSta - rcSta;
                                retNonAdverseLs = eSta - bSta;
                            }
                            else
                            {
                                retLs = eSta - bSta;
                                retNonAdverseLs = eSta - bSta;
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                retLs = eSta - rcSta;
                            }
                            else
                            {
                                retLs = eSta - bSta;
                            }
                        }

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        bSta = vrList[currentIndex].endPoint.usedS.RunoffSta;
                        eSta = vrList[currentIndex + 1].beginPoint.usedS.FullSuperSta;

                        if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                        {
                            if (vrList[currentIndex + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                //ReverseCrown - ReverseCrown
                                var bRcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                var eRcSta = vrList[currentIndex + 1].beginPoint.usedS.BeginSideReverseCrown;
                                retLs = eRcSta - bRcSta;
                            }
                            else
                            {
                                //ReverseCrown - FullSuperSta
                                var rcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                retLs = eSta - rcSta;
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                //RunoffSta - ReverseCrown
                                //次区間にReverseCrownがあるか
                                var rcSta = vrList[currentIndex + 1].beginPoint.usedS.BeginSideReverseCrown;
                                retLs = rcSta - bSta;
                            }
                            else
                            {
                                //RunoffSta - FullSuperSta
                                retLs = eSta - bSta;
                            }
                        }
                                                
                        break;
                    default:
                        break;
                }
            }

            return (retLs, retNonAdverseLs);
        }

        /// <summary>
        /// 指定インデックスですりつけ率計算に使うStaを返答する
        /// </summary>
        /// <param name="currentIndex"></param>
        /// <param name="isBeginPoint"></param>
        /// <returns></returns>
        private (decimal bSta, decimal eSta) GetChangePointSta(int currentIndex, bool isBeginPoint)
        {
            decimal retBSta = 0.0M, retESta = 0.0M;

            if (isBeginPoint)
            {
                //起点側チェック
                switch (vrList[currentIndex].beginPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        var bSta = vrList[currentIndex].beginPoint.usedS.BeginRunoutSta;
                        var eSta = vrList[currentIndex].beginPoint.usedS.FullSuperSta;

                        if (vrList[currentIndex].beginPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retBSta = bSta;
                                retESta = rcSta;
                            }
                            else
                            {
                                retBSta = bSta;
                                retESta = eSta;
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retBSta = rcSta;
                                retESta = eSta;
                            }
                            else
                            {
                                retBSta = bSta;
                                retESta = eSta;
                            }
                        }

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        bSta = vrList[currentIndex - 1].endPoint.usedS.RunoffSta;
                        eSta = vrList[currentIndex].beginPoint.usedS.FullSuperSta;

                        if (vrList[currentIndex - 1].endPoint.usedS.FlatSta != 0)
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retBSta = rcSta;
                                retESta = eSta;
                            }
                            else
                            {
                                if (vrList[currentIndex - 1].endPoint.usedS.HasEndSideReverseCrown)
                                {
                                    //1つ前の区間にReverseCrownがあるか
                                    var rcSta = vrList[currentIndex - 1].endPoint.usedS.EndSideReverseCrown;
                                    retBSta = rcSta;
                                    retESta = eSta;
                                }
                                else
                                {
                                    var flatSta = vrList[currentIndex - 1].endPoint.usedS.FlatSta;
                                    retBSta = flatSta;
                                    retESta = eSta;
                                }
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retBSta = rcSta;
                                retESta = eSta;
                            }
                            else
                            {
                                if (vrList[currentIndex - 1].endPoint.usedS.HasEndSideReverseCrown)
                                {
                                    //1つ前の区間にReverseCrownがあるか
                                    var rcSta = vrList[currentIndex - 1].endPoint.usedS.EndSideReverseCrown;
                                    retBSta = rcSta;
                                    retESta = eSta;
                                }
                                else
                                {
                                    retBSta = bSta;
                                    retESta = eSta;
                                }
                            }
                        }

                        break;
                    default:
                        break;
                }
            }
            else
            {
                //終点側チェック
                switch (vrList[currentIndex].endPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        var bSta = vrList[currentIndex].endPoint.usedS.RunoffSta;
                        var eSta = vrList[currentIndex].endPoint.usedS.EndofRunoutSta;

                        if (vrList[currentIndex].endPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                retBSta = bSta;
                                retESta = rcSta;
                            }
                            else
                            {
                                retBSta = bSta;
                                retESta = eSta;
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                retBSta = bSta;
                                retESta = rcSta;
                            }
                            else
                            {
                                retBSta = bSta;
                                retESta = eSta;
                            }
                        }

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        bSta = vrList[currentIndex].endPoint.usedS.RunoffSta;
                        eSta = vrList[currentIndex + 1].beginPoint.usedS.FullSuperSta;

                        if (vrList[currentIndex].endPoint.usedS.FlatSta != 0)
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                retBSta = bSta;
                                retESta = rcSta;
                            }
                            else
                            {
                                if (vrList[currentIndex + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                                {
                                    //次区間にReverseCrownがあるか
                                    var rcSta = vrList[currentIndex + 1].beginPoint.usedS.BeginSideReverseCrown;
                                    retBSta = bSta;
                                    retESta = rcSta;
                                }
                                else
                                {
                                    var flatSta = vrList[currentIndex].endPoint.usedS.FlatSta;
                                    retBSta = bSta;
                                    retESta = flatSta;
                                }
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                retBSta = bSta;
                                retESta = rcSta;
                            }
                            else
                            {
                                if (vrList[currentIndex + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                                {
                                    //次区間にReverseCrownがあるか
                                    var rcSta = vrList[currentIndex + 1].beginPoint.usedS.BeginSideReverseCrown;
                                    retBSta = bSta;
                                    retESta = rcSta;
                                }
                                else
                                {
                                    retBSta = bSta;
                                    retESta = eSta;
                                }
                            }
                        }

                        break;
                    default:
                        break;
                }
            }

            return (retBSta, retESta);
        }

        /// <summary>
        /// 指定インデックスの直線部の横断勾配～勾配変化点のStaを返答する
        /// </summary>
        /// <param name="currentIndex"></param>
        /// <param name="isBeginPoint"></param>
        /// <returns></returns>
        private (decimal bSta, decimal eSta) GetChangePointStaForDrainage(int currentIndex, bool isBeginPoint)
        {
            decimal retBSta = 0.0M, retESta = 0.0M;

            if (isBeginPoint)
            {
                //起点側チェック
                switch (vrList[currentIndex].beginPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        var bSta = vrList[currentIndex].beginPoint.usedS.BeginRunoutSta;
                        var eSta = vrList[currentIndex].beginPoint.usedS.FullSuperSta;

                        if (vrList[currentIndex].beginPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retBSta = bSta;
                                retESta = rcSta;
                            }
                            else
                            {
                                retBSta = bSta;
                                retESta = eSta;
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retBSta = bSta;
                                retESta = rcSta;
                            }
                            else
                            {
                                retBSta = bSta;
                                retESta = eSta;
                            }
                        }

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        bSta = vrList[currentIndex - 1].endPoint.usedS.RunoffSta;
                        eSta = vrList[currentIndex].beginPoint.usedS.FullSuperSta;

                        if (vrList[currentIndex].beginPoint.usedS.HasBeginSideReverseCrown)
                        {
                            if (vrList[currentIndex - 1].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                //ReverseCrown - ReverseCrown
                                var bRcSta = vrList[currentIndex - 1].endPoint.usedS.EndSideReverseCrown;
                                var eRcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retBSta = bRcSta;
                                retESta = eRcSta;
                            }
                            else
                            {
                                //RunoffSta - ReverseCrown
                                var rcSta = vrList[currentIndex].beginPoint.usedS.BeginSideReverseCrown;
                                retBSta = bSta;
                                retESta = rcSta;
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex - 1].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                //1つ前の区間にReverseCrownがあるか
                                //ReverseCrown - FullSuperSta
                                var rcSta = vrList[currentIndex - 1].endPoint.usedS.EndSideReverseCrown;
                                retBSta = rcSta;
                                retESta = eSta;
                            }
                            else
                            {
                                //RunoffSta - FullSuperSta
                                retBSta = bSta;
                                retESta = eSta;
                            }
                        }

                        break;
                    default:
                        break;
                }
            }
            else
            {
                //終点側チェック
                switch (vrList[currentIndex].endPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        var bSta = vrList[currentIndex].endPoint.usedS.RunoffSta;
                        var eSta = vrList[currentIndex].endPoint.usedS.EndofRunoutSta;

                        if (vrList[currentIndex].endPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                retBSta = rcSta;
                                retESta = eSta;
                            }
                            else
                            {
                                retBSta = bSta;
                                retESta = eSta;
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                            {
                                var rcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                retBSta = rcSta;
                                retESta = eSta;
                            }
                            else
                            {
                                retBSta = bSta;
                                retESta = eSta;
                            }
                        }

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        bSta = vrList[currentIndex].endPoint.usedS.RunoffSta;
                        eSta = vrList[currentIndex + 1].beginPoint.usedS.FullSuperSta;

                        if (vrList[currentIndex].endPoint.usedS.HasEndSideReverseCrown)
                        {
                            if (vrList[currentIndex + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                //ReverseCrown - ReverseCrown
                                var bRcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                var eRcSta = vrList[currentIndex + 1].beginPoint.usedS.BeginSideReverseCrown;
                                retBSta = bRcSta;
                                retESta = eRcSta;
                            }
                            else
                            {
                                //ReverseCrown - FullSuperSta
                                var rcSta = vrList[currentIndex].endPoint.usedS.EndSideReverseCrown;
                                retBSta = rcSta;
                                retESta = eSta;
                            }
                        }
                        else
                        {
                            if (vrList[currentIndex + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                            {
                                //RunoffSta - ReverseCrown
                                //次区間にReverseCrownがあるか
                                var rcSta = vrList[currentIndex + 1].beginPoint.usedS.BeginSideReverseCrown;
                                retBSta = bSta;
                                retESta = rcSta;
                            }
                            else
                            {
                                //RunoffSta - FullSuperSta
                                retBSta = bSta;
                                retESta = eSta;
                            }
                        }

                        break;
                    default:
                        break;
                }
            }

            return (retBSta, retESta);
        }

        /// <summary>
        /// 排水のために必要な最小すりつけの要否基準長Lwを取得
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="normalCrown"></param>
        /// <returns></returns>
        private int GetLw(CrossSect_OGExtension cs, decimal normalCrown)
        {
            //基本型かつReverseCrownがある場所のみ判定
            int stdVal = 0;
            if (cs is null) return 0;
           
            var st = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.W.CarriagewayWidth;
            var wcip = AppSettingsManager.Instance.DeselializeForWCSettings(selAlignmentName);
            var roadWidth = (from T in st
                             where T.RoadType == wcip.std.rtT.Item1 &&
                             T.RoadClass == wcip.std.rtT.Item2 &&
                             T.RoadSideStandard == wcip.Rss
                             select T.STDVal1).FirstOrDefault();
            if (roadWidth == 0)
            {
                roadWidth = (from T in st
                             where T.RoadType == wcip.std.rtT.Item1 &&
                             T.RoadClass == wcip.std.rtT.Item2
                             select T.STDVal1).FirstOrDefault();
            }

            var (r2fWidth, _) = cs.GetRoadwaysWidthFromFHPosition();
            decimal r2fWidthCarriagewayCount = Math.Round(r2fWidth / roadWidth, 1);

            var dl = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.OG.DrainageLw;
            var targetDl = (from T in dl
                            where T.NormalCrown == normalCrown
                            orderby T.AxisToEndOfCarriagewayLength ascending
                            select T).ToList();
            for (int i = 0; i < targetDl.Count() - 1; i++)
            {
                var threshold = targetDl[i].AxisToEndOfCarriagewayLength + (targetDl[i + 1].AxisToEndOfCarriagewayLength - targetDl[i].AxisToEndOfCarriagewayLength) / 2;
                if (targetDl[i].AxisToEndOfCarriagewayLength <= r2fWidthCarriagewayCount && r2fWidthCarriagewayCount < threshold)
                {
                    stdVal = targetDl[i].Lw;
                    break;
                }
                else if (threshold <= r2fWidthCarriagewayCount && r2fWidthCarriagewayCount <= targetDl[i + 1].AxisToEndOfCarriagewayLength)
                {
                    stdVal = targetDl[i + 1].Lw;
                    break;
                }
            }
            if (stdVal == 0)
            {
                if (targetDl.Any() && r2fWidthCarriagewayCount < targetDl[0].AxisToEndOfCarriagewayLength)
                {
                    stdVal = targetDl[0].Lw;
                }
                else
                {
                    stdVal = targetDl.Last().Lw;
                }
            }

            stdVal = (from T in dl
                      where T.NormalCrown == normalCrown &&
                            T.AxisToEndOfCarriagewayLength <= r2fWidthCarriagewayCount
                      orderby T.AxisToEndOfCarriagewayLength descending
                      select T.Lw).FirstOrDefault();

            return stdVal;
        }

        /// <summary>
        /// 指定Sta2点でFHから車道端までの距離が遠い方の横断面を返答する
        /// </summary>
        /// <param name="csList"></param>
        /// <param name="bSta"></param>
        /// <param name="eSta"></param>
        /// <returns></returns>
        private CrossSect_OGExtension GetCrossSectMostFarFromEndPointToFHPosition(List<CrossSect_OGExtension> csList, decimal bSta, decimal eSta)
        {
            var csProvider = new CrossSect_OGExtension();
            var bCs = csProvider.GetCSFromSta(csList, bSta);
            var eCs = csProvider.GetCSFromSta(csList, eSta);

            var (blb, _) = bCs == null ? (0,0) : bCs.GetRoadwaysWidthFromFHPosition();
            var (elb, _) = eCs == null ? (0,0) : eCs.GetRoadwaysWidthFromFHPosition();

            if (blb < elb)
            {
                return eCs;
            }
            else
            {
                return bCs;
            }
        }


        /// <summary>
        /// 排水のために必要な最小すりつけのチェック
        /// </summary>
        /// <param name="ogip"></param>
        /// <param name="cs"></param>
        private void CheckForDrainage(OGInputParameters ogip, List<CrossSect_OGExtension> csList)
        {
            var csProvider = new CrossSect_OGExtension();
            var ogr = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.OG.OnesidedGradientRate;
            var r = (from T in ogr
                     where T.DesignSpeed == ogip.si.ds
                     select T.Rate).FirstOrDefault();
            if (r == 0) return;
            decimal stdQ = r;

            for (int i = 0; i < vrList.Count(); i++)
            {
                var (bBSta, bESta) = GetChangePointStaForDrainage(i, true);
                var (eBSta, eESta) = GetChangePointStaForDrainage(i, false);
                var bCs = GetCrossSectMostFarFromEndPointToFHPosition(csList, bBSta, bESta);
                var eCs = GetCrossSectMostFarFromEndPointToFHPosition(csList, eBSta, eESta);
                var blw = GetLw(bCs, ogip.StraightLineTransverseGradient);
                var elw = GetLw(eCs, ogip.StraightLineTransverseGradient);
                var (blb, _) = bCs.GetRoadwaysWidthFromFHPosition();
                var (elb, _) = eCs.GetRoadwaysWidthFromFHPosition();

                //Δiは必ず直線部の横断勾配*2であるはず
                var deltaI = ogip.StraightLineTransverseGradient * 2;
                var (bLs, bNonAdverseLs) = GetLsForDrainage(i, true);
                var (eLs, eNonAdverseLs) = GetLsForDrainage(i, false);

                //起点側チェック
                switch (vrList[i].beginPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        if (bLs == blw)
                        {
                            //OK
                            vrList[i].beginPoint.VR_Gradient4Drainage.ResultType = VerificationResult.VerifyResultType.OK;
                            vrList[i].beginPoint.VR_Gradient4Drainage.MsgCode = "I-0026";
                        }
                        else if (bLs < blw)
                        {
                            //片勾配すりつけ率を判定
                            var q = CommonMethod.GetOnesidedGradientRate(blb, deltaI, bLs);
                            decimal qmad = stdQ;
                            if (vrList[i].beginPoint.OnesidedGradientShape == OGVerificationResult.OnesidedGradientShapeEnum.Standard &&
                                vrList[i].beginPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                            {
                                var nonAdverseDeltaI = ogip.StraightLineTransverseGradient * 2;
                                (deltaI, nonAdverseDeltaI) = GetDeltaI(i, ogip.StraightLineTransverseGradient, true);
                                q = CommonMethod.GetOnesidedGradientRate(blb, deltaI, bLs);
                                qmad = CommonMethod.GetOnesidedGradientRate(blb, nonAdverseDeltaI, bNonAdverseLs);
                                if (deltaI == 0)
                                {
                                    q = stdQ;
                                }
                                if (nonAdverseDeltaI == 0)
                                {
                                    qmad = stdQ;
                                }
                            }
                            if (stdQ <= q && stdQ <= qmad)
                            {
                                //OK
                                vrList[i].beginPoint.VR_Gradient4Drainage.ResultType = VerificationResult.VerifyResultType.OK;
                                vrList[i].beginPoint.VR_Gradient4Drainage.MsgCode = "I-0026";
                            }
                            else
                            {
                                //NG
                                vrList[i].beginPoint.VR_Gradient4Drainage.ResultType = VerificationResult.VerifyResultType.NG;
                                vrList[i].beginPoint.VR_Gradient4Drainage.MsgCode = "W-0022";
                            }
                        }
                        else
                        {
                            //NG
                            vrList[i].beginPoint.VR_Gradient4Drainage.ResultType = VerificationResult.VerifyResultType.NG;
                            vrList[i].beginPoint.VR_Gradient4Drainage.MsgCode = "W-0022";
                        }

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                        //卵形は排水のすりつけ不要
                        vrList[i].beginPoint.VR_Gradient4Drainage.ResultType = VerificationResult.VerifyResultType.SKIP;
                        vrList[i].beginPoint.VR_Gradient4Drainage.MsgCode = "-";
                        break;
                    default:
                        break;
                }

                //終点側チェック
                switch (vrList[i].endPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        if (eLs == elw)
                        {
                            //OK
                            vrList[i].endPoint.VR_Gradient4Drainage.ResultType = VerificationResult.VerifyResultType.OK;
                            vrList[i].endPoint.VR_Gradient4Drainage.MsgCode = "I-0026";
                        }
                        else if (eLs < elw)
                        {
                            //片勾配すりつけ率を判定
                            var q = CommonMethod.GetOnesidedGradientRate(elb, deltaI, eLs);
                            decimal qmad = stdQ;
                            if (vrList[i].endPoint.OnesidedGradientShape == OGVerificationResult.OnesidedGradientShapeEnum.Standard &&
                                vrList[i].endPoint.usedS.AdverseSE == Superelevation.EAdverseSE.nonAdverse)
                            {
                                var nonAdverseDeltaI = ogip.StraightLineTransverseGradient * 2;
                                (deltaI, nonAdverseDeltaI) = GetDeltaI(i, ogip.StraightLineTransverseGradient, false);
                                q = CommonMethod.GetOnesidedGradientRate(elb, deltaI, eLs);
                                qmad = CommonMethod.GetOnesidedGradientRate(elb, nonAdverseDeltaI, eNonAdverseLs);
                                if (deltaI == 0)
                                {
                                    q = stdQ;
                                }
                                if (nonAdverseDeltaI == 0)
                                {
                                    qmad = stdQ;
                                }
                            }
                            if (stdQ <= q && stdQ <= qmad)
                            {
                                //OK
                                vrList[i].endPoint.VR_Gradient4Drainage.ResultType = VerificationResult.VerifyResultType.OK;
                                vrList[i].endPoint.VR_Gradient4Drainage.MsgCode = "I-0026";
                            }
                            else
                            {
                                //NG
                                vrList[i].endPoint.VR_Gradient4Drainage.ResultType = VerificationResult.VerifyResultType.NG;
                                vrList[i].endPoint.VR_Gradient4Drainage.MsgCode = "W-0022";
                            }
                        }
                        else
                        {
                            //NG
                            vrList[i].endPoint.VR_Gradient4Drainage.ResultType = VerificationResult.VerifyResultType.NG;
                            vrList[i].endPoint.VR_Gradient4Drainage.MsgCode = "W-0022";
                        }

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                        //卵形は排水のすりつけ不要
                        vrList[i].endPoint.VR_Gradient4Drainage.ResultType = VerificationResult.VerifyResultType.SKIP;
                        vrList[i].endPoint.VR_Gradient4Drainage.MsgCode = "-";
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 2点間で連続しているCurveを取得する
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private List<Curve> GetCurveBetween2Point(decimal p1, decimal p2)
        {
            var cList = XMLLoader.Instance.GetCurves(selXAlignment);
            var retCList = new List<Curve>();
            Curve currentC = null;
            bool hasCurve = false;

            for (int i = 0; i < cList.Count(); i++)
            {
                var cBPoint = cList[i].BC;
                var cEPoint = cList[i].EC;

                if (cBPoint <= p1 && p1 < p2 && p2 <= cEPoint)
                {
                    hasCurve = true;
                }
                else if (p1 < cBPoint && cBPoint < cEPoint && cEPoint < p2)
                {
                    hasCurve = true;
                }
                else if (cBPoint <= p1 && p1 < cEPoint && cEPoint < p2)
                {
                    hasCurve = true;
                }
                else if (p1 < cBPoint && cBPoint < p2 && p2 <= cEPoint)
                {
                    hasCurve = true;
                }

                if (hasCurve)
                {
                    currentC = cList[i];
                    break;
                }
            }

            if (hasCurve)
            {
                //前後にCurveがあるか？
                var previousC = (from T in cList
                                 where T.EC == currentC.BC
                                 select T).FirstOrDefault();
                if (previousC is null)
                {
                    var nextC = (from T in cList
                                 where T.BC == currentC.EC
                                 select T).FirstOrDefault();
                    if (nextC is null)
                    {
                        return null;
                    }
                    else
                    {
                        retCList.Add(currentC);
                        retCList.Add(nextC);
                        return retCList;
                    }
                }
                else
                {
                    retCList.Add(previousC);
                    retCList.Add(currentC);
                    return retCList;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 2点間で連続しているSpiralを取得する
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private List<Spiral> GetSpiralBetween2Point(decimal p1, decimal p2, bool forCheckRequire)
        {
            var sList = XMLLoader.Instance.GetSpirals(selXAlignment);
            var retSList = new List<Spiral>();
            Spiral currentS = null;
            bool hasSpiral = false;

            for (int i = 0; i < sList.Count(); i++)
            {
                var sBPoint = sList[i].BS;
                var sEPoint = sList[i].ES;

                if (sBPoint <= p1 && p1 < p2 && p2 <= sEPoint)
                {
                    hasSpiral = true;
                }
                else if (p1 < sBPoint && sBPoint < sEPoint && sEPoint < p2)
                {
                    hasSpiral = true;
                }
                else if (sBPoint <= p1 && p1 < sEPoint && sEPoint < p2)
                {
                    hasSpiral = true;
                }
                else if (p1 < sBPoint && sBPoint < p2 && p2 <= sEPoint)
                {
                    hasSpiral = true;
                }

                if (hasSpiral)
                {
                    currentS = sList[i];
                    break;
                }
            }

            if (hasSpiral)
            {
                //前後にCurveがあるか？
                var previousS = (from T in sList
                                 where T.ES == currentS.BS
                                 select T).FirstOrDefault();
                if (previousS is null)
                {
                    var nextS = (from T in sList
                                 where T.BS == currentS.ES
                                 select T).FirstOrDefault();
                    if (nextS is null)
                    {
                        if (forCheckRequire)
                        {
                            retSList.Add(currentS);
                            return retSList;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        retSList.Add(currentS);
                        retSList.Add(nextS);
                        return retSList;
                    }
                }
                else
                {
                    retSList.Add(previousS);
                    retSList.Add(currentS);
                    return retSList;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 片勾配すりつけ区間が緩和区間内に収まっているかのチェック
        /// </summary>
        private void CheckForMitigationArea()
        {
            Action<decimal, decimal, OGVerificationResult> action = null;
            action = (b, e, p) =>
            {
                var sList = GetSpiralBetween2Point(b, e, true);
                if (sList != null && sList.Any())
                {
                    p.HasMitigationArea = true;
                    var bSPoint = sList.First().BS;
                    var eSPoint = sList.Last().ES;
                    if (bSPoint <= b && e <= eSPoint)
                    {
                        //緩和曲線がすりつけ区間を包括していればOK
                        p.VR_MitigationArea.ResultType = VerificationResult.VerifyResultType.OK;
                        p.VR_MitigationArea.MsgCode = "I-0027";
                    }
                    else
                    {
                        //緩和曲線がすりつけ区間を包括していなければNG
                        p.VR_MitigationArea.ResultType = VerificationResult.VerifyResultType.NG;
                        p.VR_MitigationArea.MsgCode = "W-0023";
                    }
                }
                else
                {
                    p.HasMitigationArea = false;
                    p.VR_MitigationArea.ResultType = VerificationResult.VerifyResultType.SKIP;
                    p.VR_MitigationArea.MsgCode = "-";
                }
            };

            for (int i = 0; i < vrList.Count(); i++)
            {
                //起点側チェック
                switch (vrList[i].beginPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        var bPoint = vrList[i].beginPoint.usedS.BeginRunoutSta;
                        var ePoint = vrList[i].beginPoint.usedS.FullSuperSta;
                        action(bPoint, ePoint, vrList[i].beginPoint);

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                        bPoint = vrList[i - 1].endPoint.usedS.RunoffSta;
                        ePoint = vrList[i].beginPoint.usedS.FullSuperSta;
                        action(bPoint, ePoint, vrList[i].beginPoint);

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        bPoint = vrList[i - 1].endPoint.usedS.RunoffSta;
                        ePoint = vrList[i].beginPoint.usedS.FullSuperSta;
                        action(bPoint, ePoint, vrList[i].beginPoint);

                        break;
                    default:
                        break;
                }

                //終点側チェック
                switch (vrList[i].endPoint.OnesidedGradientShape)
                {
                    case OGVerificationResult.OnesidedGradientShapeEnum.Standard:
                        var bPoint = vrList[i].endPoint.usedS.RunoffSta;
                        var ePoint = vrList[i].endPoint.usedS.EndofRunoutSta;
                        action(bPoint, ePoint, vrList[i].endPoint);

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.Egg:
                        bPoint = vrList[i].endPoint.usedS.RunoffSta;
                        ePoint = vrList[i + 1].beginPoint.usedS.FullSuperSta;
                        action(bPoint, ePoint, vrList[i].endPoint);

                        break;
                    case OGVerificationResult.OnesidedGradientShapeEnum.S:
                        bPoint = vrList[i].endPoint.usedS.RunoffSta;
                        ePoint = vrList[i + 1].beginPoint.usedS.FullSuperSta;
                        action(bPoint, ePoint, vrList[i].endPoint);

                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 直線から緩和区間なしに直接、円曲線に接続する場合に一様なすりつけを行う場合に、
        /// 直線部1/2、円曲線部1/2の割合ですりつけを行っているか
        /// </summary>
        /// <param name="ogip"></param>
        /// <param name="cs"></param>
        private void CheckForStrike2Curve(OGInputParameters ogip, List<CrossSect_OGExtension> csList)
        {
            var csProvider = new CrossSect_OGExtension();
            var sList = XMLLoader.Instance.GetSpirals(this.selXAlignment);
            var cList = XMLLoader.Instance.GetCurves(this.selXAlignment);
            var lList = XMLLoader.Instance.GetLines(this.selXAlignment);

            for (int i = 0; i < vrList.Count(); i++)
            {
                //var bCs = csProvider.GetCSFromSta(csList, vrList[i].beginPoint.usedS.FullSuperSta);
                //var eCs = csProvider.GetCSFromSta(csList, vrList[i].endPoint.usedS.RunoffSta);

                //基本型かつ緩和区間が無い区間のみ判定
                //起点側
                if (vrList[i].beginPoint.OnesidedGradientShape == OGVerificationResult.OnesidedGradientShapeEnum.Standard &&
                    vrList[i].beginPoint.HasMitigationArea == false)
                {
                    var bSta = vrList[i].beginPoint.usedS.BeginRunoutSta;
                    var eSta = vrList[i].beginPoint.usedS.FullSuperSta;
                    var l = eSta - bSta;
                    var bCs = GetCrossSectMostFarFromEndPointToFHPosition(csList, bSta, eSta);
                    var blw = GetLw(bCs, ogip.StraightLineTransverseGradient);

                    decimal lLen;
                    Line beginLine;
                    if (l <= blw)
                    {

                        lLen = Math.Round(bSta + l / 2, 3, MidpointRounding.AwayFromZero);
                        beginLine = (from T in lList
                                     where T.EL == lLen
                                     select T).FirstOrDefault();
                    }
                    else
                    {
                        lLen = bSta;
                        beginLine = (from T in lList
                                     where (T.EL - blw) == lLen
                                     select T).FirstOrDefault();
                    }

                    if (beginLine == null)
                    {
                        vrList[i].beginPoint.VR_Strike2Curve.ResultType = VerificationResult.VerifyResultType.NG;
                        vrList[i].beginPoint.VR_Strike2Curve.MsgCode = "W-0027";
                    }
                    else
                    {
                        vrList[i].beginPoint.VR_Strike2Curve.ResultType = VerificationResult.VerifyResultType.OK;
                        vrList[i].beginPoint.VR_Strike2Curve.MsgCode = "I-0031";
                    }
                }
                else
                {
                    vrList[i].beginPoint.VR_Strike2Curve.ResultType = VerificationResult.VerifyResultType.SKIP;
                    vrList[i].beginPoint.VR_Strike2Curve.MsgCode = "-";
                }
                //終点側
                if (vrList[i].endPoint.OnesidedGradientShape == OGVerificationResult.OnesidedGradientShapeEnum.Standard &&
                    vrList[i].endPoint.HasMitigationArea == false)
                {
                    var bSta = vrList[i].endPoint.usedS.RunoffSta;
                    var eSta = vrList[i].endPoint.usedS.EndofRunoutSta;
                    var l = eSta - bSta;
                    var eCs = GetCrossSectMostFarFromEndPointToFHPosition(csList, bSta, eSta);
                    var elw = GetLw(eCs, ogip.StraightLineTransverseGradient);

                    decimal lLen;
                    Curve endCurve;
                    if (l <= elw)
                    {

                        lLen = Math.Round(bSta + l / 2, 3, MidpointRounding.AwayFromZero);
                        endCurve = (from T in cList
                                    where (T.EC) == lLen
                                    select T).FirstOrDefault();
                    }
                    else
                    {
                        lLen = eSta;
                        endCurve = (from T in cList
                                    where (T.EC + elw) == lLen
                                    select T).FirstOrDefault();
                    }

                    if (endCurve == null)
                    {
                        vrList[i].endPoint.VR_Strike2Curve.ResultType = VerificationResult.VerifyResultType.NG;
                        vrList[i].endPoint.VR_Strike2Curve.MsgCode = "W-0027";
                    }
                    else
                    {
                        vrList[i].endPoint.VR_Strike2Curve.ResultType = VerificationResult.VerifyResultType.OK;
                        vrList[i].endPoint.VR_Strike2Curve.MsgCode = "I-0031";
                    }
                }
                else
                {
                    vrList[i].endPoint.VR_Strike2Curve.ResultType = VerificationResult.VerifyResultType.SKIP;
                    vrList[i].endPoint.VR_Strike2Curve.MsgCode = "-";
                }
            }
        }

        /// <summary>
        /// 指定幾何要素の範囲内に他の幾何要素が含まれているか（Spiral用）
        /// </summary>
        /// <param name="sList"></param>
        /// <returns></returns>
        private bool HasOtherItemsForSpiral(List<Spiral> sList)
        {
            var cList = XMLLoader.Instance.GetCurves(selXAlignment);
            var lList = XMLLoader.Instance.GetLines(selXAlignment);

            var bPoint = sList.First().BS;
            var ePoint = sList.Last().BS;

            var hasAnyCurve = (from T in cList
                               where bPoint <= T.BC &&
                                     T.BC <= ePoint
                               select T).Any();
            var hasAnyLine = (from T in lList
                              where bPoint <= T.BL &&
                                    T.BL <= ePoint
                              select T).Any();

            return hasAnyCurve || hasAnyLine;
        }

        /// <summary>
        /// 指定幾何要素の範囲内に他の幾何要素が含まれているか（Curve用）
        /// </summary>
        /// <param name="cList"></param>
        /// <returns></returns>
        private bool HasOtherItemsForCurve(List<Curve> cList)
        {
            var sList = XMLLoader.Instance.GetSpirals(selXAlignment);
            var lList = XMLLoader.Instance.GetLines(selXAlignment);

            var bPoint = cList.First().BC;
            var ePoint = cList.Last().BC;

            var hasAnySpiral = (from T in sList
                                where bPoint <= T.BS &&
                                      T.BS <= ePoint
                                select T).Any();
            var hasAnyLine = (from T in lList
                              where bPoint <= T.BL &&
                                    T.BL <= ePoint
                              select T).Any();

            return hasAnySpiral || hasAnyLine;
        }

        /// <summary>
        /// 横断勾配0の点とKAの差がA/10以下となっているか
        /// </summary>
        /// <param name="ogip"></param>
        /// <param name="cs"></param>
        private void CheckForTransverseGradient(OGInputParameters ogip, List<CrossSect_OGExtension> csList)
        {
            var csProvider = new CrossSect_OGExtension();
            var sList = XMLLoader.Instance.GetSpirals(this.selXAlignment);

            for (int i = 0; i < vrList.Count(); i++)
            {
                var (bBSta, bESta) = GetChangePointSta(i, true);
                var (eBSta, eESta) = GetChangePointSta(i, false);
                //var bCs = csProvider.GetCSFromSta(csList, vrList[i].beginPoint.usedS.FullSuperSta);
                var bCs = GetCrossSectMostFarFromEndPointToFHPosition(csList, bBSta, bESta);
                var (blb, _) = bCs.GetRoadwaysWidthFromFHPosition();
                var eCs = GetCrossSectMostFarFromEndPointToFHPosition(csList, eBSta, eESta);
                //var eCs = csProvider.GetCSFromSta(csList, vrList[i].endPoint.usedS.RunoffSta);
                var (elb, _) = eCs.GetRoadwaysWidthFromFHPosition();

                //起点側は必ずSkip
                vrList[i].beginPoint.VR_TransverseGradient.ResultType = VerificationResult.VerifyResultType.SKIP;
                vrList[i].beginPoint.VR_TransverseGradient.MsgCode = "-";

                //緩和区間を持つS型（終点側）であること
                if (vrList[i].endPoint.OnesidedGradientShape == OGVerificationResult.OnesidedGradientShapeEnum.S &&
                    vrList[i].endPoint.HasMitigationArea == true)
                {
                    var bSta = vrList[i].endPoint.usedS.RunoffSta;
                    var eSta = vrList[i + 1].beginPoint.usedS.FullSuperSta;
                    var betweenSpiral = GetSpiralBetween2Point(bSta, eSta, false);

                    if (betweenSpiral is null || betweenSpiral.Count < 2 || HasOtherItemsForSpiral(betweenSpiral))
                    {
                        //取れないか2箇所未満の場合は連続しないためスキップ
                        vrList[i].endPoint.VR_TransverseGradient.ResultType = VerificationResult.VerifyResultType.SKIP;
                        vrList[i].endPoint.VR_TransverseGradient.MsgCode = "-";
                    }
                    else
                    {
                        var KAPoint = betweenSpiral[1].BS;
                        var APoint = betweenSpiral[1].clothoidParameter / 10;

                        decimal q = 0.0M;
                        if (vrList[i].endPoint.usedS.FlatSta != 0)
                        {
                            //FlatStaを持つとき
                            if (Math.Abs(vrList[i].endPoint.usedS.FlatSta - KAPoint) <= APoint)
                            {
                                vrList[i].endPoint.VR_TransverseGradient.ResultType = VerificationResult.VerifyResultType.OK;
                                vrList[i].endPoint.VR_TransverseGradient.MsgCode = "I-0028";
                            }
                            else
                            {
                                vrList[i].endPoint.VR_TransverseGradient.ResultType = VerificationResult.VerifyResultType.NG;
                                vrList[i].endPoint.VR_TransverseGradient.MsgCode = "W-0024";
                            }
                        }
                        else if (vrList[i].endPoint.usedS.HasEndSideReverseCrown &&
                                 vrList[i + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                        {
                            //両区間にReverseCrownがあるとき
                            var stab = vrList[i + 1].beginPoint.usedS.BeginSideReverseCrown - vrList[i].endPoint.usedS.EndSideReverseCrown;
                            q = 2;
                            stab = Math.Round(stab / q, 3, MidpointRounding.AwayFromZero);

                            if (Math.Abs(vrList[i].endPoint.usedS.EndSideReverseCrown + stab - KAPoint) <= APoint)
                            {
                                vrList[i].endPoint.VR_TransverseGradient.ResultType = VerificationResult.VerifyResultType.OK;
                                vrList[i].endPoint.VR_TransverseGradient.MsgCode = "I-0028";
                            }
                            else
                            {
                                vrList[i].endPoint.VR_TransverseGradient.ResultType = VerificationResult.VerifyResultType.NG;
                                vrList[i].endPoint.VR_TransverseGradient.MsgCode = "W-0024";
                            }
                        }
                        else if (vrList[i].endPoint.usedS.HasEndSideReverseCrown)
                        {
                            //自区間にReverseCrownがあるとき
                            var (deltaI, _) = GetDeltaI(i + 1, ogip.StraightLineTransverseGradient, true);
                            var (ls, _, _) = GetLs(i + 1, true);
                            q = CommonMethod.GetOnesidedGradientQ(elb, deltaI, ls);
                            var stad = Math.Round((elb * ogip.StraightLineTransverseGradient / 100) / q, 3, MidpointRounding.AwayFromZero);

                            if (Math.Abs(vrList[i].endPoint.usedS.EndSideReverseCrown + stad - KAPoint) <= APoint)
                            {
                                vrList[i].endPoint.VR_TransverseGradient.ResultType = VerificationResult.VerifyResultType.OK;
                                vrList[i].endPoint.VR_TransverseGradient.MsgCode = "I-0028";
                            }
                            else
                            {
                                vrList[i].endPoint.VR_TransverseGradient.ResultType = VerificationResult.VerifyResultType.NG;
                                vrList[i].endPoint.VR_TransverseGradient.MsgCode = "W-0024";
                            }
                        }
                        else if (vrList[i + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                        {
                            //次区間にReverseCrownがあるとき
                            var (deltaI, _) = GetDeltaI(i, ogip.StraightLineTransverseGradient, false);
                            var (ls, _, _) = GetLs(i, false);
                            q = CommonMethod.GetOnesidedGradientQ(elb, deltaI, ls);
                            var stac = Math.Round((elb * Math.Abs(vrList[i].endPoint.usedS.FullSuperelev) / 100) / q, 3, MidpointRounding.AwayFromZero);

                            if (Math.Abs(vrList[i].endPoint.usedS.RunoffSta + stac - KAPoint) <= APoint)
                            {
                                vrList[i].endPoint.VR_TransverseGradient.ResultType = VerificationResult.VerifyResultType.OK;
                                vrList[i].endPoint.VR_TransverseGradient.MsgCode = "I-0028";
                            }
                            else
                            {
                                vrList[i].endPoint.VR_TransverseGradient.ResultType = VerificationResult.VerifyResultType.NG;
                                vrList[i].endPoint.VR_TransverseGradient.MsgCode = "W-0024";
                            }
                        }
                        else
                        {
                            //どちらにもReverseCrownが無いとき
                            var (deltaI, _) = GetDeltaI(i + 1, ogip.StraightLineTransverseGradient, true);
                            var (ls, _, _) = GetLs(i + 1, true);
                            q = CommonMethod.GetOnesidedGradientQ(elb, deltaI, ls);
                            var staa = Math.Round((elb * Math.Abs(vrList[i].endPoint.usedS.FullSuperelev) / 100) / q, 3, MidpointRounding.AwayFromZero);

                            if (Math.Abs(vrList[i].endPoint.usedS.RunoffSta + staa - KAPoint) <= APoint)
                            {
                                vrList[i].endPoint.VR_TransverseGradient.ResultType = VerificationResult.VerifyResultType.OK;
                                vrList[i].endPoint.VR_TransverseGradient.MsgCode = "I-0028";
                            }
                            else
                            {
                                vrList[i].endPoint.VR_TransverseGradient.ResultType = VerificationResult.VerifyResultType.NG;
                                vrList[i].endPoint.VR_TransverseGradient.MsgCode = "W-0024";
                            }
                        }
                    }
                }
                else 
                {
                    vrList[i].endPoint.VR_TransverseGradient.ResultType = VerificationResult.VerifyResultType.SKIP;
                    vrList[i].endPoint.VR_TransverseGradient.MsgCode = "-";
                }
            }
        }

        /// <summary>
        /// 横断勾配0の点がBC点と一致しているか
        /// 複合円の場合に小円1/2、大円1/2の割合ですりつけているか
        /// </summary>
        /// <param name="ogip"></param>
        /// <param name="cs"></param>
        private void CheckForCurves(OGInputParameters ogip, List<CrossSect_OGExtension> csList)
        {
            var csProvider = new CrossSect_OGExtension();
            var cList = XMLLoader.Instance.GetCurves(this.selXAlignment);

            //区間内のCurveを取ってきて、2件以上かつcw=>cw or ccw=>ccwなら判定？
            //それ以外はスキップ扱い

            for (int i = 0; i < vrList.Count(); i++)
            {
                var (bSta, eSta) = GetChangePointSta(i, false);
                var eCs = GetCrossSectMostFarFromEndPointToFHPosition(csList, bSta, eSta);
                var (elb, _) = eCs.GetRoadwaysWidthFromFHPosition();

                //判定外はスキップ(VR_Curvesを判定)
                vrList[i].beginPoint.VR_Curves.ResultType = VerificationResult.VerifyResultType.SKIP;
                vrList[i].beginPoint.VR_Curves.MsgCode = "-";
                vrList[i].beginPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.SKIP;
                vrList[i].beginPoint.VR_TransverseGradient0Point.MsgCode = "-";

                //緩和区間を持たないS型か卵形（終点側）のみ照査する
                if ((vrList[i].endPoint.OnesidedGradientShape == OGVerificationResult.OnesidedGradientShapeEnum.S ||
                    vrList[i].endPoint.OnesidedGradientShape == OGVerificationResult.OnesidedGradientShapeEnum.Egg) &&
                    vrList[i].endPoint.HasMitigationArea == false)
                {
                    var fCList = GetCurveBetween2Point(vrList[i].endPoint.usedS.RunoffSta, vrList[i + 1].beginPoint.usedS.FullSuperSta);

                    if (fCList is null)
                    {
                        //区間内にCurveが無いためスキップ
                        vrList[i].endPoint.VR_Curves.ResultType = VerificationResult.VerifyResultType.SKIP;
                        vrList[i].endPoint.VR_Curves.MsgCode = "-";
                        vrList[i].endPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.SKIP;
                        vrList[i].endPoint.VR_TransverseGradient0Point.MsgCode = "-";
                    }
                    else
                    {
                            //連続しているCurve
                            if (HasOtherItemsForCurve(fCList))
                            {
                                //(無いとは思うが)Curve区間内に他の要素があればスキップ
                                vrList[i].endPoint.VR_Curves.ResultType = VerificationResult.VerifyResultType.SKIP;
                                vrList[i].endPoint.VR_Curves.MsgCode = "-";
                                vrList[i].endPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.SKIP;
                                vrList[i].endPoint.VR_TransverseGradient0Point.MsgCode = "-";
                            }
                            else
                            {
                            if (fCList[0].rot == fCList[1].rot)
                            {
                                vrList[i].endPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.SKIP;
                                vrList[i].endPoint.VR_TransverseGradient0Point.MsgCode = "-";

                                //複合円である場合
                                var stae = Math.Round((vrList[i + 1].beginPoint.usedS.FullSuperSta - vrList[i].endPoint.usedS.RunoffSta) / 2, 3, MidpointRounding.AwayFromZero);
                                if (vrList[i].endPoint.usedS.RunoffSta + stae == fCList[1].BC)
                                {
                                    vrList[i].endPoint.VR_Curves.ResultType = VerificationResult.VerifyResultType.OK;
                                    vrList[i].endPoint.VR_Curves.MsgCode = "I-0030";
                                }
                                else
                                {
                                    vrList[i].endPoint.VR_Curves.ResultType = VerificationResult.VerifyResultType.NG;
                                    vrList[i].endPoint.VR_Curves.MsgCode = "W-0026";
                                }
                            }
                            else
                            {
                                decimal q = 0.0M;
                                vrList[i].endPoint.VR_Curves.ResultType = VerificationResult.VerifyResultType.SKIP;
                                vrList[i].endPoint.VR_Curves.MsgCode = "-";

                                //緩和区間を持たないS字曲線の場合(VR_TransverseGradient0Pointを判定)
                                if (vrList[i].endPoint.usedS.FlatSta == 0)
                                {
                                    if (vrList[i].endPoint.usedS.HasEndSideReverseCrown && vrList[i + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                                    {
                                        //両方の片勾配すりつけ区間にReverseCrownがある場合
                                        var stab = vrList[i + 1].beginPoint.usedS.BeginSideReverseCrown - vrList[i].endPoint.usedS.EndSideReverseCrown;
                                        q = 2;
                                        stab = Math.Round(stab / q, 3, MidpointRounding.AwayFromZero);
                                        if (vrList[i].endPoint.usedS.EndSideReverseCrown + stab == fCList[1].BC)
                                        {
                                            vrList[i].endPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.OK;
                                            vrList[i].endPoint.VR_TransverseGradient0Point.MsgCode = "I-0029";
                                        }
                                        else
                                        {
                                            vrList[i].endPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.NG;
                                            vrList[i].endPoint.VR_TransverseGradient0Point.MsgCode = "W-0025";
                                        }
                                    }
                                    else if (vrList[i].endPoint.usedS.HasEndSideReverseCrown)
                                    {
                                        //自区間にReverseCrownがあるとき
                                        var (deltaI, _) = GetDeltaI(i + 1, ogip.StraightLineTransverseGradient, true);
                                        var (ls, _, _) = GetLs(i + 1, true);
                                        q = CommonMethod.GetOnesidedGradientQ(elb, deltaI, ls);
                                        var stad = Math.Round((elb * ogip.StraightLineTransverseGradient / 100) / q, 3, MidpointRounding.AwayFromZero);
                                        if (vrList[i].endPoint.usedS.EndSideReverseCrown + stad == cList[1].BC)
                                        {
                                            vrList[i].endPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.OK;
                                            vrList[i].endPoint.VR_TransverseGradient0Point.MsgCode = "I-0029";
                                        }
                                        else
                                        {
                                            vrList[i].endPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.NG;
                                            vrList[i].endPoint.VR_TransverseGradient0Point.MsgCode = "W-0025";
                                        }
                                    }
                                    else if (vrList[i + 1].beginPoint.usedS.HasBeginSideReverseCrown)
                                    {
                                        //次区間にReverseCrownがあるとき
                                        var (deltaI, _) = GetDeltaI(i, ogip.StraightLineTransverseGradient, false);
                                        var (ls, _, _) = GetLs(i, false);
                                        q = CommonMethod.GetOnesidedGradientQ(elb, deltaI, ls);
                                        var stac = Math.Round((elb * Math.Abs(vrList[i].endPoint.usedS.FullSuperelev) / 100) / q, 3, MidpointRounding.AwayFromZero);
                                        if (vrList[i].endPoint.usedS.RunoffSta + stac == fCList[1].BC)
                                        {
                                            vrList[i].endPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.OK;
                                            vrList[i].endPoint.VR_TransverseGradient0Point.MsgCode = "I-0029";
                                        }
                                        else
                                        {
                                            vrList[i].endPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.NG;
                                            vrList[i].endPoint.VR_TransverseGradient0Point.MsgCode = "W-0025";
                                        }
                                    }
                                    else
                                    {
                                        //どちらにもReverseCrownが無いとき
                                        var (deltaI, _) = GetDeltaI(i + 1, ogip.StraightLineTransverseGradient, true);
                                        var (ls, _, _) = GetLs(i + 1, true);
                                        q = CommonMethod.GetOnesidedGradientQ(elb, deltaI, ls);
                                        var staa = Math.Round((elb * Math.Abs(vrList[i].endPoint.usedS.FullSuperelev) / 100) / q, 3, MidpointRounding.AwayFromZero);
                                        if (vrList[i].endPoint.usedS.RunoffSta + staa == fCList[1].BC)
                                        {
                                            vrList[i].endPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.OK;
                                            vrList[i].endPoint.VR_TransverseGradient0Point.MsgCode = "I-0029";
                                        }
                                        else
                                        {
                                            vrList[i].endPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.NG;
                                            vrList[i].endPoint.VR_TransverseGradient0Point.MsgCode = "W-0025";
                                        }
                                    }
                                }
                                else
                                {
                                    //FlatStaがある場合
                                    if (vrList[i].endPoint.usedS.FlatSta == fCList[1].BC)
                                    {
                                        vrList[i].endPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.OK;
                                        vrList[i].endPoint.VR_TransverseGradient0Point.MsgCode = "I-0029";
                                    }
                                    else
                                    {
                                        vrList[i].endPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.NG;
                                        vrList[i].endPoint.VR_TransverseGradient0Point.MsgCode = "W-0025";
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //対象外はスキップ
                    vrList[i].endPoint.VR_Curves.ResultType = VerificationResult.VerifyResultType.SKIP;
                    vrList[i].endPoint.VR_Curves.MsgCode = "-";
                    vrList[i].endPoint.VR_TransverseGradient0Point.ResultType = VerificationResult.VerifyResultType.SKIP;
                    vrList[i].endPoint.VR_TransverseGradient0Point.MsgCode = "-";
                }
            }
        }
    }
}
