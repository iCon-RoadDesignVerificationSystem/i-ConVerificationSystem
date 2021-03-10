using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_ConVerificationSystem.Structs.LandXML
{
    class Curve : IEquatable<Curve>, IComparable<Curve>
    {
        public string name { get; set; }
        //直接参照禁止
        public decimal staStart { private get; set; }
        //直接参照禁止
        public decimal length { private get; set; }
        public Clockwise rot { get; set; }
        public decimal radius { get; set; }
        public decimal BC { 
            get
            {
                return Math.Round(staStart, 3, MidpointRounding.AwayFromZero);
            }
        }
        public decimal EC { 
            get
            {
                return Math.Round(staStart + length, 3, MidpointRounding.AwayFromZero);
            }
        }

        public enum Clockwise
        {
            cw = 0,
            ccw
        }

        public override int GetHashCode()
        {
            return this.staStart.GetHashCode();
        }

        public int CompareTo(Curve other)
        {
            return this.GetHashCode() - other.GetHashCode();
        }

        public bool Equals(Curve other)
        {
            return this.staStart == other.staStart;
        }
    }
}
