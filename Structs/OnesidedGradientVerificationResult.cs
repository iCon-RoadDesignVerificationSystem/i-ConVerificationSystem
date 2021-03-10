using i_ConVerificationSystem.Structs.LandXML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_ConVerificationSystem.Structs
{
    public class OnesidedGradientVerificationResult
    {
        /// <summary>
        /// 番号
        /// </summary>
        public int vrNum { get; set; }
        /// <summary>
        /// （片勾配すりつけ区間）起点側
        /// </summary>
        public OGVerificationResult beginPoint { get; set; }
        /// <summary>
        /// （片勾配すりつけ区間）終点側
        /// </summary>
        public OGVerificationResult endPoint { get; set; }

        /// <summary>
        /// すりつけ区間の照査結果
        /// </summary>
        public class OGVerificationResult
        {
            /// <summary>
            /// 片勾配すりつけ区間の累加距離標（組み合わせがありえる？）
            /// </summary>
            public enum SuperelevationType
            {
                BeginRunoutSta = 0,
                BeginRunoffSta,
                FullSuperSta,
                FullSuperelev,
                RunoffSta,
                StartofRunoutSta,
                EndofRunoutSta
            }

            /// <summary>
            /// 片勾配すりつけ形状
            /// </summary>
            public enum OnesidedGradientShapeEnum
            {
                Standard = 0,
                Egg,
                S
            }

            public OGVerificationResult()
            {
                this.usedS = new Superelevation();
                this.VR_Curves = new VerificationResult();
                this.VR_Gradient4Drainage = new VerificationResult();
                this.VR_MitigationArea = new VerificationResult();
                this.VR_OnesidedRate = new VerificationResult();
                this.VR_Strike2Curve = new VerificationResult();
                this.VR_TransverseGradient = new VerificationResult();
                this.VR_TransverseGradient0Point = new VerificationResult();
            }

            /// <summary>
            /// エラーがあるか
            /// </summary>
            /// <returns></returns>
            public bool HasError()
            {
                if (VR_Curves.ResultType == VerificationResult.VerifyResultType.NG ||
                    VR_Gradient4Drainage.ResultType == VerificationResult.VerifyResultType.NG ||
                    VR_MitigationArea.ResultType == VerificationResult.VerifyResultType.NG ||
                    VR_OnesidedRate.ResultType == VerificationResult.VerifyResultType.NG ||
                    VR_Strike2Curve.ResultType == VerificationResult.VerifyResultType.NG ||
                    VR_TransverseGradient.ResultType == VerificationResult.VerifyResultType.NG ||
                    VR_TransverseGradient0Point.ResultType == VerificationResult.VerifyResultType.NG)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// エラー件数を返答
            /// </summary>
            /// <returns></returns>
            public int GetErrorCount()
            {
                int errCount = 0;
                if (VR_Curves.ResultType == VerificationResult.VerifyResultType.NG) errCount++;
                if (VR_Gradient4Drainage.ResultType == VerificationResult.VerifyResultType.NG) errCount++;
                if (VR_MitigationArea.ResultType == VerificationResult.VerifyResultType.NG) errCount++;
                if (VR_OnesidedRate.ResultType == VerificationResult.VerifyResultType.NG) errCount++;
                if (VR_Strike2Curve.ResultType == VerificationResult.VerifyResultType.NG) errCount++;
                if (VR_TransverseGradient.ResultType == VerificationResult.VerifyResultType.NG) errCount++;
                if (VR_TransverseGradient0Point.ResultType == VerificationResult.VerifyResultType.NG) errCount++;
                return errCount;
            }

            /// <summary>
            /// OK(確認)があるか
            /// </summary>
            /// <returns></returns>
            public bool HasOK_C()
            {
                if (VR_Curves.ResultType == VerificationResult.VerifyResultType.OK_C ||
                    VR_Gradient4Drainage.ResultType == VerificationResult.VerifyResultType.OK_C ||
                    VR_MitigationArea.ResultType == VerificationResult.VerifyResultType.OK_C ||
                    VR_OnesidedRate.ResultType == VerificationResult.VerifyResultType.OK_C ||
                    VR_Strike2Curve.ResultType == VerificationResult.VerifyResultType.OK_C ||
                    VR_TransverseGradient.ResultType == VerificationResult.VerifyResultType.OK_C ||
                    VR_TransverseGradient0Point.ResultType == VerificationResult.VerifyResultType.OK_C)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// OK(確認)があるか
            /// </summary>
            /// <returns></returns>
            public int GetOK_CCount()
            {
                int okcCount = 0;
                if (VR_Curves.ResultType == VerificationResult.VerifyResultType.OK_C) okcCount++;
                if (VR_Gradient4Drainage.ResultType == VerificationResult.VerifyResultType.OK_C) okcCount++;
                if (VR_MitigationArea.ResultType == VerificationResult.VerifyResultType.OK_C) okcCount++;
                if (VR_OnesidedRate.ResultType == VerificationResult.VerifyResultType.OK_C) okcCount++;
                if (VR_Strike2Curve.ResultType == VerificationResult.VerifyResultType.OK_C) okcCount++;
                if (VR_TransverseGradient.ResultType == VerificationResult.VerifyResultType.OK_C) okcCount++;
                if (VR_TransverseGradient0Point.ResultType == VerificationResult.VerifyResultType.OK_C) okcCount++;
                return okcCount;
            }

            /// <summary>
            /// 使用している片勾配すりつけ区間リスト
            /// </summary>
            public Superelevation usedS { get; set; }

            /// <summary>
            /// （片勾配すりつけ区間の累加距離標）起点側
            /// </summary>
            public decimal beginSta { get; set; }
            /// <summary>
            /// （片勾配すりつけ区間の累加距離標）終点側
            /// </summary>
            public decimal endSta { get; set; }
            /// <summary>
            /// 片勾配すりつけ長
            /// </summary>
            public decimal Ls { get; set; }
            /// <summary>
            /// 排水のために必要な最小すりつけの要否
            /// </summary>
            //public bool RequireGradient4Drainage { get; set; }
            /// <summary>
            /// （片勾配すりつけ率）直線部の横断勾配～曲線内最大片勾配
            /// </summary>
            public string Straight2MaximumOnesidedGradientRate { get; set; } = "-";
            /// <summary>
            /// （片勾配すりつけ率）片勾配すりつけ率の変化点～曲線内最大片勾配
            /// </summary>
            public string ChangePoint2MaximumOnesidedGradientRate { get; set; } = "-";
            /// <summary>
            /// （片勾配すりつけ率）曲線内最大片勾配～曲線内最大片勾配
            /// </summary>
            public string MaximumOnesidedGradientRate2MaximumOnesidedGradientRate { get; set; } = "-";
            /// <summary>
            /// 緩和区間の有無
            /// </summary>
            public bool HasMitigationArea { get; set; }
            /// <summary>
            /// 片勾配すりつけ形状
            /// </summary>
            public OnesidedGradientShapeEnum OnesidedGradientShape { get; set; }
            /// <summary>
            /// 片勾配すりつけ率の判定
            /// </summary>
            public VerificationResult VR_OnesidedRate { get; set; }
            /// <summary>
            /// 排水のために必要な最小すりつけの判定
            /// </summary>
            public VerificationResult VR_Gradient4Drainage { get; set; }
            /// <summary>
            /// 片勾配すりつけ区間が緩和区間内に収まっているか
            /// </summary>
            public VerificationResult VR_MitigationArea { get; set; }
            /// <summary>
            /// 横断勾配0の点とKAの差がA/10以下となっているか
            /// </summary>
            public VerificationResult VR_TransverseGradient { get; set; }
            /// <summary>
            /// 複合円の場合に小円1/2、大円1/2の割合ですりつけているか
            /// </summary>
            public VerificationResult VR_Curves { get; set; }
            /// <summary>
            /// 横断勾配0の点がBC点と一致しているか
            /// </summary>
            public VerificationResult VR_TransverseGradient0Point { get; set; }
            /// <summary>
            /// 直線から緩和区間なしに直接、円曲線に接続する場合に一様なすりつけを行う場合に、
            /// 直線部1/2、円曲線部1/2の割合ですりつけを行っているか
            /// </summary>
            public VerificationResult VR_Strike2Curve { get; set; }
        }
    }
}
