using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_ConVerificationSystem.Structs.LandXML
{
    class Line : IEquatable<Line>, IComparable<Line>
    {
        public string name { get; set; }
        public decimal staStart { private get; set; }
        public decimal length { private get; set; }
        public decimal BL
        {
            get
            {
                return Math.Round(staStart, 3, MidpointRounding.AwayFromZero);
            }
        }
        public decimal EL
        {
            get
            {
                return Math.Round(staStart + length, 3, MidpointRounding.AwayFromZero);
            }
        }

        public override int GetHashCode()
        {
            return this.staStart.GetHashCode();
        }

        public int CompareTo(Line other)
        {
            return this.GetHashCode() - other.GetHashCode();
        }

        public bool Equals(Line other)
        {
            return this.staStart == other.staStart;
        }
    }
}
