using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static i_ConVerificationSystem.Structs.LandXML.Curve;

namespace i_ConVerificationSystem.Structs.LandXML
{
    class Spiral : IEquatable<Spiral>, IComparable<Spiral>
    {
        public string name { get; set; }
        public decimal staStart { private get; set; }
        public decimal length { private get; set; }
        public SpiType spiType { get; set; }
        public Clockwise rot { get; set; }
        public decimal clothoidParameter { get; set; }
        public decimal BS
        {
            get
            {
                return Math.Round(staStart, 3, MidpointRounding.AwayFromZero);
            }
        }
        public decimal ES
        {
            get
            {
                return Math.Round(staStart + length, 3, MidpointRounding.AwayFromZero);
            }
        }

        public enum SpiType
        {
            clothoid = 0
        }

        public override int GetHashCode()
        {
            return this.staStart.GetHashCode();
        }

        public int CompareTo(Spiral other)
        {
            return this.GetHashCode() - other.GetHashCode();
        }

        public bool Equals(Spiral other)
        {
            return this.staStart == other.staStart;
        }
    }
}
