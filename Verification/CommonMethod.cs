using i_ConVerificationSystem.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static i_ConVerificationSystem.Structs.OGExtensions.DesignCrossSectSurf_OGExtension;

namespace i_ConVerificationSystem.Verification
{
    static class CommonMethod
    {
        /// <summary>
        /// パターン
        /// </summary>
        private const string ROADCLASS_REGEX = @"(?<=[\w'])[\d\.]+(?=[\w'])";

        /// <summary>
        /// 道路規格から種別と級の数値を取得する
        /// </summary>
        /// <param name="rcProperty"></param>
        /// <returns></returns>
        public static Tuple<int, int> GetRoadClass(XElement rcProperty)
        {
            var retVal = new Tuple<int, int>(0, 0);

            if (!(rcProperty is null) && !(rcProperty.Attribute("label") is null))
            {
                if (rcProperty.Attribute("label").Value == "classification")
                {
                    var rcVal = Regex.Matches(rcProperty.Attribute("value").Value, ROADCLASS_REGEX);

                    retVal = new Tuple<int, int>(int.Parse(rcVal[0].Value), int.Parse(rcVal[1].Value));
                }
                else
                {
                    throw new Exception($"{rcProperty}は道路規格ではありません。");
                }
            }

            return retVal;
        }

        /// <summary>
        /// 緩勾配区間長の照査が可能か判定する
        /// </summary>
        /// <param name="s">道路種別</param>
        /// <param name="c">道路区分</param>
        /// <returns></returns>
        public static bool CanBeVerifyGG(int s, int c)
        {
            if (s == 1 || s == 2) return false;

            return true;
        }

        /// <summary>
        /// 緩勾配区間長の最小値を取得する
        /// </summary>
        /// <param name="s">道路種別</param>
        /// <param name="c">道路区分</param>
        /// <returns></returns>
        public static int GetLgs(int s, int c)
        {
            int retVal;

            var ggLgs = StdVerificationConditionsManager.Instance.stdVerificationConditions.STDValues.GG.MinimumLgs;
            retVal = (from T in ggLgs
                      where T.RoadType == s &&
                            T.RoadClass == c
                      select T.Lgs).FirstOrDefault();

            if (retVal == 0)
            {
                throw new Exception($"緩勾配判定が出来ない道路規格です。第{s}種第{c}級");
            }

            return retVal;
        }

        /// <summary>
        /// 必要緩勾配区間端
        /// </summary>
        /// <param name="pointNo"></param>
        /// <param name="addPoint"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static string GetXpslgsPositionString(int pointNo, decimal addPoint, int alpha, int lgs, bool isBeginPoint)
        {
            decimal xpslgs = pointNo * alpha + addPoint + (isBeginPoint ? -lgs : lgs);
            int gsPointNo = (int)(xpslgs / alpha);
            decimal gsAddPoint = xpslgs % alpha;

            while (gsAddPoint < 0)
            {
                gsAddPoint = gsAddPoint + alpha;
                gsPointNo--;
            }

            return $"No.{gsPointNo}+{gsAddPoint}";
        }

        /// <summary>
        /// Name_JItemsを日本語名に変換
        /// </summary>
        /// <param name="nStr"></param>
        /// <returns></returns>
        public static string GetName_JFromGroupNameCode(Name_JItems nStr)
        {
            var njItems = new Dictionary<Name_JItems, string>()
            {
                {Name_JItems.None, "-" },
                {Name_JItems.CenterStrip, "中央帯" },
                {Name_JItems.CenterSprit, "中央分離帯" },
                {Name_JItems.CenterMarginalStrip, "中央帯側帯" },
                {Name_JItems.Carriageway, "車道" },
                {Name_JItems.AdditionalLane, "付加車線" },
                {Name_JItems.Roadshoulder, "路肩" },
                {Name_JItems.MarginalStrip, "路肩側帯" },
                {Name_JItems.RoadshoulderR, "右側路肩" },
                {Name_JItems.PlantingLane, "植樹帯" },
                {Name_JItems.Sidewalk, "歩道" },
                {Name_JItems.Sidepath, "自転車歩行者道" },
                {Name_JItems.Cycletrack, "自転車道" },
                {Name_JItems.Bikelane, "自転車通行帯" },
                {Name_JItems.StoppingArea, "停車帯" },
                {Name_JItems.Other, "その他" }
            };

            return njItems[nStr];
        }


        /// <summary>
        /// 片勾配すりつけ率の計算
        /// </summary>
        /// <param name="b">回転軸～車道縁までの距離</param>
        /// <param name="beginPointGradient">起点側勾配率</param>
        /// <param name="endPointGradient">終点側勾配率</param>
        /// <param name="beginSta">起点側Sta</param>
        /// <param name="endSta">終点側Sta</param>
        /// <returns></returns>
        public static decimal GetOnesidedGradientRate(decimal b, decimal beginPointGradient, decimal endPointGradient, decimal beginSta, decimal endSta)
        {
            //参考：https://dourosekkei.hateblo.jp/entry/2020/04/01/015323
            //上昇率：b * (終点 - 始点)の勾配率の差 / 100
            //片勾配すりつけ率： 上昇率 / (endSta - beginSta)
            if (endSta - beginSta == 0) return 0;
            return Math.Round((b * (endPointGradient - beginPointGradient) / 100) / (endSta - beginSta), 3, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// ΔiとLsが判明している片勾配すりつけ率の計算
        /// </summary>
        /// <param name="b"></param>
        /// <param name="deltaI"></param>
        /// <param name="beginSta"></param>
        /// <param name="endSta"></param>
        /// <returns></returns>
        public static decimal GetOnesidedGradientQ(decimal b, decimal deltaI, decimal ls)
        {
            if (ls == 0) return 0;
            return Math.Round(b * deltaI / 100 / ls, 3, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// ΔiとLsが判明している片勾配すりつけ率の計算（すりつけ率計算の分母を返答する）
        /// </summary>
        /// <param name="b"></param>
        /// <param name="deltaI"></param>
        /// <param name="ls"></param>
        /// <returns></returns>
        public static decimal GetOnesidedGradientRate(decimal b, decimal deltaI, decimal ls)
        {
            if (b == 0 || deltaI == 0) return 0;
            return Math.Round(ls / (b * deltaI / 100), 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 片勾配すりつけ率を返答(1/nという書式)
        /// </summary>
        /// <param name="b"></param>
        /// <param name="deltaI"></param>
        /// <param name="ls"></param>
        /// <returns></returns>
        public static string GetOnesidedGradientFormattedRate(decimal b, decimal deltaI, decimal ls)
        {
            if (b == 0 || deltaI == 0) return "-";
            return $"1 / {GetOnesidedGradientRate(b, deltaI, ls)}";
        }
    }
}
