using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_ConVerificationSystem.Structs.LandXML
{
    public class Superelevation : IEquatable<Superelevation>, IComparable<Superelevation>
    {

        public enum EAdverseSE
        {
            adverse = 0,
            nonAdverse
        }

        public enum SuperelevationType
        {
            BeginRunoutSta = 0,
            BeginRunoffSta,
            FullSuperSta,
            RunoffSta,
            StartofRunoutSta,
            EndofRunoutSta,
            ReverseCrown,
            FlatSta
        }

        public decimal staStart { get; set; }
        public decimal staEnd { get; set; }
        public decimal BeginRunoutSta { get; set; }
        public decimal BeginRunoffSta { get; set; }
        public decimal FullSuperSta { get; set; }
        public decimal FullSuperelev { get; set; }
        public decimal RunoffSta { get; set; }
        public decimal StartofRunoutSta { get; set; }
        public decimal EndofRunoutSta { get; set; }
        public EAdverseSE AdverseSE { get; set; }
        public List<decimal> ReverseCrown { private get; set; } = new List<decimal>();
        public decimal FlatSta { get; set; } = 0;
        public decimal LeftDeltaI { get; set; }
        public decimal RightDeltaI { get; set; }

        public List<Tuple<SuperelevationType, decimal>> oneSidedItemList { get; set; }

        /// <summary>
        /// 起点側ReverseCrownをセット
        /// </summary>
        /// <param name="r"></param>
        public void SetForBeginSideReverseCrown(decimal r)
        {
            ReverseCrown.Add(r);
        }

        /// <summary>
        /// 終点側ReverseCrownをセット
        /// </summary>
        public void SetForEndSideReverseCrown(decimal r)
        {
            ReverseCrown.Add(r);
        }

        /// <summary>
        /// 起点側ReverseCrownを所持しているか
        /// </summary>
        public bool HasBeginSideReverseCrown
        {
            get
            {
                if (ReverseCrown.Any())
                {
                    //ReverseCrownは最大2件
                    if (ReverseCrown.Count() == 2)
                    {
                        return true;
                    }
                    else
                    {
                        var r = ReverseCrown[0];
                        if (r <= FullSuperSta)
                        {
                            //最大片勾配の測点以下なら起点側あり
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 起点側ReverseCrownを取得
        /// </summary>
        public decimal BeginSideReverseCrown {
            get
            {
                if (ReverseCrown.Any())
                {
                    //ReverseCrownは最大2件
                    if (ReverseCrown.Count() == 2)
                    {
                        return ReverseCrown[0];
                    }
                    else
                    {
                        if (HasBeginSideReverseCrown)
                        {
                            return ReverseCrown[0];
                        }
                        else
                        {
                            return 0.0M;
                        }
                    }
                }
                else
                {
                    return 0.0M;
                }
            }
        }

        /// <summary>
        /// 終点側ReverseCrownを所持しているか
        /// </summary>
        public bool HasEndSideReverseCrown
        {
            get
            {
                if (ReverseCrown.Any())
                {
                    //ReverseCrownは最大2件
                    if (ReverseCrown.Count() == 2)
                    {
                        return true;
                    }
                    else
                    {
                        var r = ReverseCrown[0];
                        if (FullSuperSta < r)
                        {
                            //最大片勾配の測点を超えていれば終点側あり
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 終点側ReverseCrownを取得
        /// </summary>
        public decimal EndSideReverseCrown
        {
            get
            {
                if (ReverseCrown.Any())
                {
                    //ReverseCrownは最大2件
                    if (ReverseCrown.Count() == 2)
                    {
                        return ReverseCrown[1];
                    }
                    else
                    {
                        if (HasEndSideReverseCrown)
                        {
                            return ReverseCrown[0];
                        }
                        else
                        {
                            return 0.0M;
                        }
                    }
                }
                else
                {
                    return 0.0M;
                }
            }
        }

        public override int GetHashCode()
        {
            //staStartとFullSuperStaは同じ位置が入るが、FullSuperStaが必須項目であるためこちらで判定
            return this.staEnd.GetHashCode() ^ this.FullSuperSta.GetHashCode();
        }

        public int CompareTo(Superelevation other)
        {
            return this.GetHashCode() - other.GetHashCode();
        }

        public bool Equals(Superelevation other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }
    }
}
