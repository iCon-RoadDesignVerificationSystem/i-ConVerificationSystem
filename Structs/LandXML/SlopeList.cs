using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_ConVerificationSystem.Structs.LandXML
{
    class SlopeList
    {
        public List<SlopeValue> SlopeValues { get; set; }

        public class SlopeValue : IEquatable<SlopeValue>, IComparable<SlopeValue>
        {
            public decimal sta { get; set; }
            public decimal gradientLeft { get; set; }
            public decimal gradientRight { get; set; }
            /// <summary>
            /// 一車線道路用。左側点を基準にする。おがみは存在しない。
            /// </summary>
            public decimal gradientSingleLane { get; set; }

            public int CompareTo(SlopeValue other)
            {
                return (int)(sta - other.sta);
            }

            public bool Equals(SlopeValue other)
            {
                return other.GetHashCode() == GetHashCode();
            }

            public override int GetHashCode()
            {
                return sta.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null || this.GetType() != obj.GetType()) return false;

                var other = (SlopeValue)obj;
                return obj.GetHashCode() == GetHashCode();
            }
        }

        public class CompareSlopeValue : IEqualityComparer<SlopeValue>
        {
            public bool Equals(SlopeValue x, SlopeValue y)
            {
                return x.GetHashCode() == y.GetHashCode();
            }

            public int GetHashCode(SlopeValue obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
