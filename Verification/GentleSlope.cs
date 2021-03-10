using i_ConVerificationSystem.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static i_ConVerificationSystem.Forms.Gradient.GGInputParameter;

namespace i_ConVerificationSystem.Verification
{
    class GentleSlope
    {
        /// <summary>
        /// Case番号
        /// </summary>
        public enum Case
        {
            Case0 = 0, Case1, Case2, Case3, Case4, Case5, Case6, Case7, Case8
        }

        /// <summary>
        /// 勾配変化点の座標
        /// </summary>
        private struct PVI
        {
            public decimal X;
            public decimal Y;

            public PVI(decimal x, decimal y)
            {
                X = x;
                Y = y;
            }
        }

        /// <summary>
        /// 縦断曲線を含んだ勾配変化点
        /// </summary>
        private struct ParaCurve
        {
            public int Length;
            public PVI PVI;

            public ParaCurve(int len, decimal x, decimal y)
            {
                Length = len;
                PVI = new PVI(x, y);
            }

            public ParaCurve(int len, PVI p)
            {
                Length = len;
                PVI = p;
            }
        }

        /// <summary>
        /// 判定する縦断曲線の測点
        /// </summary>
        private struct PVCL
        {
            public Case Case;
            public decimal I;
            public decimal NI;
            public decimal VCR;
            public decimal Xvcbcl;

            public decimal Xpsl;
            public decimal Xpslgs;

            public PVCL(Case c, decimal i, decimal nI, decimal vcr, decimal xvcbcl, decimal xpsl, decimal xpslgs)
            {
                Case = c;
                I = i;
                NI = nI;
                VCR = vcr;
                Xvcbcl = xvcbcl;
                Xpsl = xpsl;
                Xpslgs = xpslgs;
            }

            public PVCL(decimal i, decimal nI, decimal vcr, decimal xvcbcl, decimal xpsl, decimal xpslgs)
            {
                Case = Case.Case0;
                I = i;
                NI = nI;
                VCR = vcr;
                Xvcbcl = xvcbcl;
                Xpsl = xpsl;
                Xpslgs = xpslgs;
            }

            public void DataInitialize(decimal i, decimal vcr, decimal xvcbcl, decimal xpsl, decimal xpslgs)
            {
                Case = Case.Case0;
                I = i;
                VCR = vcr;
                Xvcbcl = xvcbcl;
                Xpsl = xpsl;
                Xpslgs = xpslgs;
            }
        }

        public decimal GSBB { get; set; }
        public decimal GSBE { get; set; }
        public decimal GSEB { get; set; }
        public decimal GSEE { get; set; }
        public decimal XBPSL { get; set; }
        public decimal XEPSL { get; set; }
        public Case GSBCase { get; set; }
        public Case GSECase { get; set; }

        /// <summary>
        /// 緩勾配区間長の照査
        /// </summary>
        /// <param name="profAlignXe"></param>
        /// <param name="ggip"></param>
        /// <param name="alignmentName"></param>
        public void HasGentleSlopeArea(XElement profAlignXe, GGInputParameters ggip, string alignmentName)
        {
            if (profAlignXe is null || ggip is null || CommonMethod.CanBeVerifyGG(ggip.si.rtT.Item1, ggip.si.rtT.Item2) == false)
            {
                var vr = new VerificationResult();
                vr.ResultType = VerificationResult.VerifyResultType.SKIP;
                GGVerificationResultItem.Instance.Update(alignmentName, true, vr);
                GGVerificationResultItem.Instance.Update(alignmentName, false, vr);

                return;
            }

            var t = this.GetIncline(profAlignXe);
            this.GetAndCheckEnsureSlope(alignmentName, ggip, t.XvclbcList, t.XvclecList, t.iList, t.VCRList);
        }

        /// <summary>
        /// 縦断勾配のリストを取得
        /// </summary>
        /// <param name="profAlignXe"></param>
        /// <returns></returns>
        private (List<decimal> XvclbcList, List<decimal> XvclecList, List<decimal> iList, List<decimal> VCRList) GetIncline(XElement profAlignXe)
        {
            var eSel = profAlignXe.Elements("PVI").ToList();
            PVI PVI1 = new PVI();
            PVI PVI2 = new PVI();

            PVI1.X = decimal.Parse(eSel[0].Value.Split(' ')[0]);
            PVI1.Y = decimal.Parse(eSel[0].Value.Split(' ')[1]);
            PVI2.X = decimal.Parse(eSel[1].Value.Split(' ')[0]);
            PVI2.Y = decimal.Parse(eSel[1].Value.Split(' ')[1]);

            var retPVIBeginPoint = PVI1.X;
            var retPVIEndPoint = PVI2.X;

            List<ParaCurve> pcList = (from T in profAlignXe.Elements("ParaCurve")
                                      select new ParaCurve((int)((double)T.Attribute("length")),
                                                           decimal.Parse(T.Value.Split(' ')[0]),
                                                           decimal.Parse(T.Value.Split(' ')[1]))).ToList();

            //縦断勾配リストを作成
            var iList = new List<decimal>();
            //縦断曲線の測点リストを作成
            var XvclbcList = new List<decimal>();
            var XvclecList = new List<decimal>();
            decimal bX = PVI1.X, bY = PVI1.Y, aX, aY;

            foreach (var item in pcList)
            {
                //in=((Yn+1)-Yn)/((Xn+1)-Xn)
                aX = item.PVI.X; aY = item.PVI.Y;
                iList.Add(Math.Round((aY-bY) / (aX-bX), 3, MidpointRounding.AwayFromZero));

                //Xvclbc n=(Xn+1)-(VCLn/2)
                //Xvclec n=(Xn+1)+(VCLn/2)
                XvclbcList.Add(item.PVI.X - (item.Length / 2.0M));
                XvclecList.Add(item.PVI.X + (item.Length / 2.0M));

                bX = item.PVI.X; bY = item.PVI.Y;
            }
            aX = PVI2.X; aY = PVI2.Y;
            iList.Add(Math.Round((aY - bY) / (aX - bX), 3, MidpointRounding.AwayFromZero));

            //縦断曲線半径リストを作成
            var VCRList = new List<decimal>();
            decimal bI = iList[0], aI;
            int iidx = 1;
            foreach (var item in pcList)
            {
                //VCRn=(VCLn)/|in-(in+1)|
                aI = iList[iidx];
                VCRList.Add(item.Length / Math.Abs((bI - aI)));
                bI = iList[iidx];
                iidx++;
            }

            return (XvclbcList, XvclecList, iList, VCRList);
        }

        /// <summary>
        /// 照査実行
        /// </summary>
        /// <param name="bPointNo"></param>
        /// <param name="bAddLen"></param>
        /// <param name="ePointNo"></param>
        /// <param name="eAddLen"></param>
        /// <param name="rtT"></param>
        /// <param name="XvclbcList"></param>
        /// <param name="XvclecList"></param>
        /// <param name="iList"></param>
        /// <param name="VCRList"></param>
        /// <returns></returns>
        private void GetAndCheckEnsureSlope(string alignmentName, GGInputParameters ggip, List<decimal> XvclbcList, List<decimal> XvclecList, List<decimal> iList, List<decimal> VCRList)
        {
            decimal Xbpsl = ggip.bpn * ggip.si.interval + ggip.bAddLen;
            decimal Xepsl = ggip.epn * ggip.si.interval + ggip.eAddLen;
            int lgs = CommonMethod.GetLgs(ggip.si.rtT.Item1, ggip.si.rtT.Item2);
            decimal Xbpslgs = Xbpsl - lgs;
            decimal Xepslgs = Xepsl + lgs;

            //TEST
            this.XBPSL = Xbpsl;
            this.XEPSL = Xepsl;

            decimal bsBCheckPoint, bsECheckPoint, esBCheckPoint, esECheckPoint;

            var bsRetVal = this.GetCheckPoint(true, Xbpsl, Xbpslgs, XvclbcList, XvclecList, iList);
            var esRetVal = this.GetCheckPoint(false, Xepsl, Xepslgs, XvclbcList, XvclecList, iList);

            bsBCheckPoint = bsRetVal.bCheckPoint;
            bsECheckPoint = bsRetVal.eCheckPoint;
            esBCheckPoint = esRetVal.bCheckPoint;
            esECheckPoint = esRetVal.eCheckPoint;

            var bsRetCase = this.CaseCheck(true, Xbpsl, Xbpslgs, XvclbcList, XvclecList, VCRList);
            var esRetCase = this.CaseCheck(false, Xepsl, Xepslgs, XvclbcList, XvclecList, VCRList);

            this.GSBCase = bsRetCase.c;
            this.GSECase = esRetCase.c;

            PVCL bsPVCL, bsPVCL2, esPVCL, esPVCL2;
            bsPVCL = new PVCL(bsRetCase.c, bsRetVal.bipn, bsRetVal.nBipn, bsRetCase.vcr, bsRetCase.xvclbc, Xbpsl, Xbpslgs);
            bsPVCL2 = new PVCL(bsRetCase.c, bsRetVal.eipn, bsRetVal.nEipn, bsRetCase.vcr2, bsRetCase.xvclbc2, Xbpsl, Xbpslgs);
            esPVCL = new PVCL(esRetCase.c, esRetVal.bipn, esRetVal.nBipn, esRetCase.vcr, esRetCase.xvclbc, Xepsl, Xepslgs);
            esPVCL2 = new PVCL(esRetCase.c, esRetVal.eipn, esRetVal.nEipn, esRetCase.vcr2, esRetCase.xvclbc2, Xepsl, Xepslgs);

            this.GSBB = bsRetVal.bipn;
            this.GSBE = bsRetVal.eipn;
            this.GSEB = esRetVal.bipn;
            this.GSEE = esRetVal.eipn;

            Action<PVCL, PVCL, int, bool> action = null;
            action = (pvcl, pvcl2, cIdx, isBeginSide) =>
            {
                var vr = new VerificationResult();
                decimal bi = 0.0M, ei = 0.0M;
                switch (pvcl.Case)
                {
                    case Case.Case0:
                    case Case.Case1:
                    case Case.Case3:
                        bi = iList[cIdx];
                        break;
                    case Case.Case2:
                    case Case.Case4:
                        ei = iList[cIdx + 1];
                        break;
                    case Case.Case5:
                        //必ず計算する
                        break;
                    case Case.Case6:
                        bi = iList[cIdx];
                        ei = iList[cIdx + 1];
                        break;
                    case Case.Case7:
                        ei = iList[cIdx + 1];
                        break;
                    case Case.Case8:
                        bi = iList[cIdx + 1];
                        ei = iList[cIdx + 2];
                        break;
                    default:
                        break;
                }

                if (Math.Abs(bi) * 100 <= 2.5M && Math.Abs(ei) * 100 <= 2.5M){
                    decimal ibpvcl = 0.0M, iepvcl = 0.0M;

                    switch (pvcl.Case)
                    {
                        case Case.Case0:
                        case Case.Case1:
                        case Case.Case2:
                        case Case.Case6:
                            //緩勾配区間内に縦断曲線がないためOK
                            //Case0はPVIしか無いときに入る
                            vr.MsgCode = "I-0001";
                            vr.ResultType = VerificationResult.VerifyResultType.OK;
                            break;
                        case Case.Case3:
                            iepvcl = this.GetIPVCL(pvcl, false, isBeginSide);

                            this.GSBE = iepvcl;
                            break;
                        case Case.Case4:
                            ibpvcl = this.GetIPVCL(pvcl, true, isBeginSide);

                            this.GSBB = ibpvcl;
                            break;
                        default:
                            ibpvcl = this.GetIPVCL(pvcl, true, isBeginSide);
                            iepvcl = this.GetIPVCL(pvcl2, false, isBeginSide);

                            this.GSBB = ibpvcl;
                            this.GSBE = iepvcl;
                            break;
                    }

                    if (Math.Abs(ibpvcl) * 100 <= 2.5M && Math.Abs(iepvcl) * 100 <= 2.5M)
                    {
                        vr.MsgCode = "I-0001";
                        vr.ResultType = VerificationResult.VerifyResultType.OK;
                    }
                    else
                    {
                        vr.MsgCode = "W-0002";
                        vr.ResultType = VerificationResult.VerifyResultType.NG;
                    }
                }
                else
                {
                    //停止線位置の緩勾配区間の確保ができていないためNG
                    vr.MsgCode = "W-0001";
                    vr.ResultType = VerificationResult.VerifyResultType.NG;
                }

                GGVerificationResultItem.Instance.Update(alignmentName, isBeginSide, vr);
            };

            action(bsPVCL, bsPVCL2, bsRetCase.caseIndex, true);
            action(esPVCL, esPVCL2, esRetCase.caseIndex, false);
        }

        /// <summary>
        /// チェックポイントを取得
        /// </summary>
        /// <param name="isBeginPoint"></param>
        /// <param name="Xpsl"></param>
        /// <param name="Xpslgs"></param>
        /// <param name="XvclbcList"></param>
        /// <param name="XvclecList"></param>
        /// <param name="iList"></param>
        /// <returns></returns>
        private (decimal bCheckPoint, decimal eCheckPoint, decimal bipn, decimal eipn, decimal nBipn, decimal nEipn) GetCheckPoint(bool isBeginPoint, decimal Xpsl, decimal Xpslgs, List<decimal> XvclbcList, List<decimal> XvclecList, List<decimal> iList)
        {
            //返答値
            decimal bCheckPoint = 0.0M, eCheckPoint = 0.0M, bipn = 0.0M, eipn = 0.0M, nBipn = 0.0M, nEipn = 0.0M;

            //直前に判定した縦断曲線情報
            decimal bXvclec = 0.0M, bI = 0.0M;

            if (XvclbcList.Count() == 0 && XvclecList.Count() == 0)
            {
                if (iList.Count() != 0)
                {
                    //PVIしかない場合
                    bipn = iList[0];
                    eipn = iList[0];
                    nBipn = iList[0];
                    nEipn = iList[0];
                }

                return (bCheckPoint, eCheckPoint, bipn, eipn, nBipn, nEipn);
            }

            for (int i = 0; i < XvclbcList.Count; i++)
            {
                if (isBeginPoint)
                {
                    //起点側チェック

                    //緩勾配区間の測点位置チェック
                    if (i == 0 && Xpslgs <= XvclbcList[i])
                    {
                        //測点が最初の縦断曲線より左側であるなら
                        bCheckPoint = Xpslgs;
                        bipn = iList[i];
                        nBipn = iList[i + 1];
                    }
                    else if (bXvclec <= Xpslgs && Xpslgs <= XvclbcList[i])
                    {
                        //測点が直前の縦断曲線終端との間であるなら
                        bCheckPoint = XvclbcList[i];
                        bipn = iList[i];
                        nBipn = iList[i + 1];
                    }
                    else if (XvclbcList[i] < Xpslgs && Xpslgs < XvclecList[i])
                    {
                        //測点が指定縦断曲線の中であるなら
                        bCheckPoint = XvclbcList[i];
                        bipn = iList[i];
                        nBipn = iList[i + 1];
                    }
                    else if (XvclecList[i] <= Xpslgs)
                    {
                        //測点が指定縦断曲線より右側であるなら
                        bCheckPoint = Xpslgs;
                        bipn = iList[i + 1];
                        nBipn = iList[i + 1];
                    }

                    //停止線位置チェック
                    if (i == 0 && Xpsl <= XvclbcList[i])
                    {
                        //停止線が最初の縦断曲線より左側であるなら
                        eCheckPoint = Xpsl;
                        eipn = iList[i];
                        nEipn = iList[i + 1];
                    }
                    else if (bXvclec <= Xpsl && Xpsl <= XvclbcList[i])
                    {
                        //停止線が直前の縦断曲線終端との間であるなら
                        eCheckPoint = XvclbcList[i];
                        eipn = iList[i];
                        nEipn = iList[i + 1];
                    }
                    else if (XvclbcList[i] < Xpsl && Xpsl < XvclecList[i])
                    {
                        //停止線が指定縦断曲線の中であるなら
                        eCheckPoint = XvclecList[i];
                        eipn = iList[i];
                        nEipn = iList[i + 1];
                    }
                    else if (XvclecList[i] <= Xpsl)
                    {
                        //最後の縦断曲線より右側であるなら
                        eCheckPoint = Xpsl;
                        eipn = iList[i + 1];
                        nEipn = iList[i + 1];
                    }
                }
                else
                {
                    //終点側チェック

                    //停止線位置チェック
                    if (i == 0 && Xpsl <= XvclbcList[i])
                    {
                        //停止線が最初の縦断曲線より左側であるなら
                        bCheckPoint = Xpsl;
                        bipn = iList[i];
                        nBipn = iList[i + 1];
                    }
                    else if (bXvclec <= Xpsl && Xpsl <= XvclbcList[i])
                    {
                        //停止線が直前の縦断曲線終端との間であるなら
                        bCheckPoint = XvclbcList[i];
                        //ipn = bI;
                        bipn = iList[i];
                        nBipn = iList[i + 1];
                    }
                    else if (XvclbcList[i] < Xpsl && Xpsl < XvclecList[i])
                    {
                        //停止線が指定縦断曲線の中であるなら
                        bCheckPoint = XvclecList[i];
                        bipn = iList[i];
                        nBipn = iList[i + 1];
                    }
                    else if (XvclecList[i] <= Xpsl)
                    {
                        //最後の縦断曲線より右側であるなら
                        bCheckPoint = Xpsl;
                        bipn = iList[i + 1];
                        nBipn = iList[i + 1];
                    }

                    //緩勾配区間の測点位置チェック
                    if (i == 0 && Xpslgs <- XvclbcList[i])
                    {
                        //測点が最初の縦断曲線より左側であるなら
                        eCheckPoint = Xpslgs;
                        eipn = iList[i];
                        nEipn = iList[i + 1];
                    }
                    else if (bXvclec <= Xpslgs && Xpslgs <= XvclbcList[i])
                    {
                        //測点が直前の縦断曲線終端との間であるなら
                        eCheckPoint = XvclbcList[i];
                        eipn = iList[i];
                        nEipn = iList[i + 1];
                    }
                    else if (XvclbcList[i] < Xpslgs && Xpslgs < XvclecList[i])
                    {
                        //測点が指定縦断曲線の中であるなら
                        eCheckPoint = XvclbcList[i];
                        eipn = iList[i];
                        nEipn = iList[i + 1];
                    }
                    else if (XvclecList[i] <= Xpslgs)
                    {
                        //測点が指定縦断曲線より右側であるなら
                        eCheckPoint = Xpslgs;
                        eipn = iList[i + 1];
                        nEipn = iList[i + 1];
                    }
                }

                bXvclec = XvclecList[i];
                bI = iList[i];
            }

            return (bCheckPoint, eCheckPoint, bipn, eipn, nBipn, nEipn);
        }

        /// <summary>
        /// ケース番号を取得
        /// </summary>
        /// <param name="isBeginPoint"></param>
        /// <param name="Xpsl"></param>
        /// <param name="Xpslgs"></param>
        /// <param name="bCheckPoint"></param>
        /// <param name="eCheckPoint"></param>
        /// <param name="XvclbcList"></param>
        /// <param name="XvclecList"></param>
        /// <returns></returns>
        private (Case c, decimal xvclbc, decimal xvclbc2, decimal vcr, decimal vcr2, int caseIndex) CaseCheck(bool isBeginPoint, decimal Xpsl, decimal Xpslgs, 
            List<decimal> XvclbcList, List<decimal> XvclecList, List<decimal> VCRList)
        {
            Case retCase = Case.Case0;
            decimal retXvclbc = 0.0M, retVcr = 0.0M, retXvclbc2 = 0.0M, retVcr2 = 0.0M;
            int retCaseIndex = 0;

            //PVIのみで構成されていれば判定せず返答
            if (XvclbcList.Count() == 0 && XvclecList.Count() == 0) return (retCase, retXvclbc, retXvclbc2, retVcr, retVcr2, retCaseIndex);

            for (int i = 0; i < XvclbcList.Count; i++)
            {
                //チェックフロー
                //数字はCase番号。1はFalseで継続、4,6はTrueで継続
                //最終的にTrueになったものをCase番号とする
                //いずれでもない場合はCase2となる
                //
                //1 F
                //  |- 3, 4, 5, 6
                //        T     T-3
                //        7, 8
                // Finally 2
                if (retCase == Case.Case0)
                {
                    //Case1チェック
                    if ((isBeginPoint ? Xpslgs : Xpsl) < XvclbcList[i] &&
                        (isBeginPoint ? Xpsl : Xpslgs) <= XvclbcList[i] &&
                        (isBeginPoint ? Xpslgs : Xpsl) < XvclecList[i] &&
                        (isBeginPoint ? Xpsl : Xpslgs) < XvclecList[i])
                    {
                        //勾配変化点の各測点(Xvclbc,Xvclec)よりも左側であれば
                        //順序 slgs sl bc ec
                        //Case1
                        retCase = Case.Case1;
                        retXvclbc = XvclbcList[i];
                        retVcr = VCRList[i];
                        retCaseIndex = i;
                        //チェック終了
                        break;
                    }
                    //3,4,5,6チェック
                    else if ((isBeginPoint ? Xpslgs : Xpsl) <= XvclbcList[i] &&
                              XvclbcList[i] < (isBeginPoint ? Xpsl : Xpslgs) &&
                              (isBeginPoint ? Xpslgs : Xpsl) < XvclecList[i] &&
                              (isBeginPoint ? Xpsl : Xpslgs) < XvclecList[i])
                    {
                        //勾配変化点の各測点の間にあれば（左寄り）
                        //slgs bc sl ec
                        //Case3
                        retCase = Case.Case3;
                        retXvclbc = XvclbcList[i];
                        retVcr = VCRList[i];
                        retCaseIndex = i;
                        //チェック終了
                        break;
                    }
                    else if (XvclbcList[i] < (isBeginPoint ? Xpslgs : Xpsl) &&
                             XvclbcList[i] < (isBeginPoint ? Xpsl : Xpslgs) &&
                             (isBeginPoint ? Xpslgs : Xpsl) < XvclecList[i] &&
                             XvclecList[i] <= (isBeginPoint ? Xpsl : Xpslgs))
                    {
                        //勾配変化点の各測点の間にあれば（右寄り）
                        //bc slgs ec sl
                        //Case4
                        retCase = Case.Case4;
                        retXvclbc = XvclbcList[i];
                        retVcr = VCRList[i];
                        retCaseIndex = i;
                        //Case7,8チェックへ継続
                    }
                    else if (XvclbcList[i] < (isBeginPoint ? Xpslgs : Xpsl) &&
                             XvclbcList[i] < (isBeginPoint ? Xpsl : Xpslgs) &&
                             (isBeginPoint ? Xpslgs : Xpsl) < XvclecList[i] &&
                             (isBeginPoint ? Xpsl : Xpslgs) < XvclecList[i])
                    {
                        //勾配変化点の各測点の間にあれば（中寄り）
                        //bc slgs sl ec
                        //Case5
                        retCase = Case.Case5;
                        retXvclbc = XvclbcList[i];
                        retVcr = VCRList[i];
                        retXvclbc2 = XvclbcList[i];
                        retVcr2 = VCRList[i];
                        retCaseIndex = i;
                        //チェック終了
                        break;
                    }
                    else if ((isBeginPoint ? Xpslgs : Xpsl) <= XvclbcList[i] &&
                             XvclbcList[i] < (isBeginPoint ? Xpsl : Xpslgs) &&
                             (isBeginPoint ? Xpslgs : Xpsl) < XvclecList[i] &&
                             XvclecList[i] <= (isBeginPoint ? Xpsl : Xpslgs))
                    {
                        //勾配変化点の各測点を包むようにあれば
                        //slgs bc ec sl
                        //Case6
                        retCase = Case.Case6;
                        retXvclbc = XvclbcList[i];
                        retVcr = VCRList[i];
                        retCaseIndex = i;
                        //Case3チェックへ継続
                    }
                }

                if (retCase == Case.Case6)
                {
                    if ((isBeginPoint ? Xpslgs : Xpsl) < XvclbcList[i] &&
                         XvclbcList[i] < (isBeginPoint ? Xpslgs : Xpsl) &&
                        (isBeginPoint ? Xpslgs : Xpsl) < XvclecList[i] &&
                        (isBeginPoint ? Xpslgs : Xpsl) < XvclecList[i])
                    {
                        //勾配変化点の各測点の間にあれば（左寄り）
                        //slgs bc sl ec
                        //Case3
                        retCase = Case.Case3;
                        retXvclbc = XvclbcList[i];
                        retVcr = VCRList[i];
                        //チェック終了
                        break;
                    }
                }

                //残り1件に満たない場合はCase7,8の判定は不可
                if (XvclbcList.Count() - 1 <= i) continue;

                if (XvclbcList[i] != (isBeginPoint ? Xpslgs : Xpsl) &&
                    (isBeginPoint ? Xpslgs : Xpsl) < XvclecList[i] &&
                    XvclecList[i] <= XvclbcList[i + 1] &&
                    XvclbcList[i + 1] < (isBeginPoint ? Xpsl : Xpslgs) &&
                    (isBeginPoint ? Xpsl : Xpslgs) != XvclecList[i + 1])
                {
                    //複数の勾配変化点をまたいでいれば（2か所）
                    //bc1 slgs ec1 bc2 sl ec2
                    //CheckPointを採用するとCase7,8の判別ができない
                    //最小区間長の最長が40mに対して測点間隔は20or100mであるため3つを超えて出現しない
                    //->CheckPointは採用しなくてよい
                    //Case7
                    retCase = Case.Case7;
                    retXvclbc = XvclbcList[i] < (isBeginPoint ? Xpslgs : Xpsl) ? XvclbcList[i] : (isBeginPoint ? Xpslgs : Xpsl);
                    retVcr = VCRList[i];
                    retXvclbc2 = (isBeginPoint ? Xpsl : Xpslgs) < XvclecList[i + 1] ? XvclbcList[i + 1] : (isBeginPoint ? Xpsl : Xpslgs);
                    retVcr2 = VCRList[i + 1];
                    //Case8のチェック継続
                }
                if (i < XvclbcList.Count() - 2 &&
                    XvclbcList[i] < (isBeginPoint ? Xpslgs : Xpsl) &&
                    (isBeginPoint ? Xpslgs : Xpsl) < XvclecList[i] &&
                    XvclecList[i] < XvclbcList[i + 2] &&
                    XvclbcList[i + 2] < (isBeginPoint ? Xpsl : Xpslgs) &&
                    (isBeginPoint ? Xpsl : Xpslgs) < XvclecList[i + 2])
                {
                    //残り2件以上ありかつ複数の勾配変化点をまたいでいれば（3か所）
                    //bc1 slgs ec1 bc2 ec2 bc3 sl ec3
                    //Case8
                    retCase = Case.Case8;
                    retXvclbc = XvclbcList[i];
                    retVcr = VCRList[i];
                    retXvclbc2 = XvclbcList[i + 2];
                    retVcr2 = VCRList[i + 2];
                    //チェック終了
                    break;
                }
            }

            if (retCase == Case.Case0)
            {
                //いずれでもない場合はCase2となる
                retCase = Case.Case2;
                retCaseIndex = XvclbcList.Count - 1;
            }

            return (retCase, retXvclbc, retXvclbc2, retVcr, retVcr2, retCaseIndex);
        }

        /// <summary>
        /// 緩勾配区間端部の縦断曲線内の勾配を取得
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private decimal GetIPVCL(PVCL p, bool getBeginPoint, bool isBeginPoint)
        {
            decimal retVal = 0.0M;

            decimal totalDistance = (getBeginPoint ? (isBeginPoint ? p.Xpslgs : p.Xpsl) : (isBeginPoint ? p.Xpsl : p.Xpslgs));
            decimal slopeDiff = Math.Round(((totalDistance - p.Xvcbcl) / p.VCR), 3, MidpointRounding.AwayFromZero);

            if (p.I < p.NI)
            {
                retVal = p.I + slopeDiff;
            }
            else
            {
                retVal = p.I - slopeDiff;
            }

            return retVal;
        }
    }
}
