using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static i_ConVerificationSystem.Forms.Gradient.TGInputParameter;
using static i_ConVerificationSystem.Forms.WidthComposition.WCInputParameter;

namespace i_ConVerificationSystem.Structs.JSON
{
    [JsonObject]
    public class StdVerificationConditions
    {
        [JsonProperty("STDValues")]
        public Stdvalues STDValues { get; set; }
    }

    public class Stdvalues
    {
        [JsonProperty("WC")]
        public WC WC { get; set; }
        [JsonProperty("W")]
        public W W { get; set; }
        [JsonProperty("TG")]
        public TG TG { get; set; }
        [JsonProperty("OG")]
        public OG OG { get; set; }
        [JsonProperty("GG")]
        public GG GG { get; set; }
    }

    public class WC
    {
        [JsonProperty("PlannedTvRatio")]
        public decimal PlannedTvRatio { get; set; }
        [JsonProperty("Planned4TvRatio")]
        public decimal Planned4TvRatio { get; set; }
        [JsonProperty("PlannedTv")]
        public List<Plannedtv> PlannedTv { get; set; }
        [JsonProperty("Planned4Tv")]
        public List<Planned4tv> Planned4Tv { get; set; }
    }

    public class Plannedtv
    {
        [JsonProperty("-RoadType")]
        public int RoadType { get; set; }
        [JsonProperty("-RoadClass")]
        public int RoadClass { get; set; }
        [JsonProperty("-Topography")]
        public Topography Topography { get; set; }
        [JsonProperty("-PlannedTv")]
        public int PlannedTv { get; set; }
    }

    public class Planned4tv
    {
        [JsonProperty("-RoadType")]
        public int RoadType { get; set; }
        [JsonProperty("-RoadClass")]
        public int RoadClass { get; set; }
        [JsonProperty("-Topography")]
        public Topography Topography { get; set; }
        [JsonProperty("-PlannedTv")]
        public int PlannedTv { get; set; }
    }

    public class W
    {
        [JsonProperty("CarriagewayWidth")]
        public List<Carriagewaywidth> CarriagewayWidth { get; set; }
        [JsonProperty("CenterMarginalWidth")]
        public List<Centermarginalwidth> CenterMarginalWidth { get; set; }
        [JsonProperty("CenterSpritWidth")]
        public List<Centerspritwidth> CenterSpritWidth { get; set; }
        [JsonProperty("RoadShoulderWidth")]
        public List<Roadshoulderwidth> RoadShoulderWidth { get; set; }
        [JsonProperty("RoadShoulderS1Width")]
        public List<Roadshoulders1width> RoadShoulderS1Width { get; set; }
        [JsonProperty("RoadShoulderStripWidth")]
        public List<Roadshoulderstripwidth> RoadShoulderStripWidth { get; set; }
    }

    public class Carriagewaywidth
    {
        [JsonProperty("-RoadType")]
        public int RoadType { get; set; }
        [JsonProperty("-RoadClass")]
        public int RoadClass { get; set; }
        [JsonProperty("-RoadSideStandard")]
        public RoadSideStandard RoadSideStandard { get; set; }
        [JsonProperty("-STDVal1")]
        public decimal STDVal1 { get; set; }
        [JsonProperty("-SPCVal1")]
        public decimal SPCVal1 { get; set; }
        [JsonProperty("-STDVal2")]
        public decimal STDVal2 { get; set; }
        [JsonProperty("-STDVal3")]
        public decimal[] STDVal3 { get; set; }
        [JsonProperty("-SPCVal2")]
        public decimal SPCVal2 { get; set; }
    }

    public class Centermarginalwidth
    {
        [JsonProperty("-RoadType")]
        public int RoadType { get; set; }
        [JsonProperty("-RoadClass")]
        public int RoadClass { get; set; }
        [JsonProperty("-STDVal")]
        public decimal STDVal { get; set; }
        [JsonProperty("-SPCVal")]
        public decimal SPCVal { get; set; }
    }

    public class Centerspritwidth
    {
        [JsonProperty("-RoadType")]
        public int RoadType { get; set; }
        [JsonProperty("-RoadClass")]
        public int RoadClass { get; set; }
        [JsonProperty("-STDVal")]
        public decimal STDVal { get; set; }
        [JsonProperty("-SPCVal")]
        public decimal SPCVal { get; set; }
    }

    public class Roadshoulderwidth
    {
        [JsonProperty("-RoadType")]
        public int RoadType { get; set; }
        [JsonProperty("-RoadClass")]
        public int RoadClass { get; set; }
        [JsonProperty("-RoadSideStandard")]
        public RoadSideStandard RoadSideStandard { get; set; }
        [JsonProperty("-STDValL")]
        public decimal STDValL { get; set; }
        [JsonProperty("-SPCValL")]
        public decimal SPCValL { get; set; }
        [JsonProperty("-DESValL")]
        public decimal DESValL { get; set; }
        [JsonProperty("-STDValR")]
        public decimal STDValR { get; set; }
        [JsonProperty("-DESValR")]
        public decimal DESValR { get; set; }
        [JsonProperty("-TUNVal")]
        public decimal TUNVal { get; set; }
    }

    public class Roadshoulders1width
    {
        [JsonProperty("-RoadType")]
        public int RoadType { get; set; }
        [JsonProperty("-RoadClass")]
        public int RoadClass { get; set; }
        [JsonProperty("-RoadSideStandard")]
        public RoadSideStandard RoadSideStandard { get; set; }
        [JsonProperty("-STDValL")]
        public decimal STDValL { get; set; }
        [JsonProperty("-SPCValL")]
        public decimal SPCValL { get; set; }
        [JsonProperty("-STDValR")]
        public decimal STDValR { get; set; }
    }

    public class Roadshoulderstripwidth
    {
        [JsonProperty("-RoadType")]
        public int RoadType { get; set; }
        [JsonProperty("-RoadClass")]
        public int RoadClass { get; set; }
        [JsonProperty("-RoadSideStandard")]
        public RoadSideStandard RoadSideStandard { get; set; }
        [JsonProperty("-STDVal")]
        public decimal STDVal { get; set; }
        [JsonProperty("-SPCVal")]
        public decimal SPCVal { get; set; }
    }

    public class TG
    {
        [JsonProperty("StdNormalCrown")]
        public List<Stdnormalcrown> StdNormalCrown { get; set; }
        [JsonProperty("StdOnesidedGradient")]
        public List<Stdonesidedgradient> StdOnesidedGradient { get; set; }
        [JsonProperty("StdStopOnesidedGradient")]
        public List<Stdstoponesidedgradient> StdStopOnesidedGradient { get; set; }
        [JsonProperty("StdOnesidedGradientForType4")]
        public List<Stdonesidedgradientfortype4> StdOnesidedGradientForType4 { get; set; }
        [JsonProperty("StdAppropriateGradient")]
        public List<Stdappropriategradient> StdAppropriateGradient { get; set; }
    }

    public class Stdnormalcrown
    {
        [JsonProperty("-IsOnesideOneLine")]
        public bool IsOnesideOneLine { get; set; }
        [JsonProperty("-RoadPavingType")]
        public RoadPavingType RoadPavingType { get; set; }
        [JsonProperty("-STDCrown")]
        public decimal[] STDCrown { get; set; }
    }

    public class Stdonesidedgradient
    {
        [JsonProperty("-NormalCrown")]
        public decimal NormalCrown { get; set; }
        [JsonProperty("-DesignSpeed")]
        public int DesignSpeed { get; set; }
        [JsonProperty("-MinRadius")]
        public int MinRadius { get; set; }
        [JsonProperty("-MaxRadius")]
        public int MaxRadius { get; set; }
        [JsonProperty("-CONVal1")]
        public decimal CONVal1 { get; set; }
        [JsonProperty("-CONVal2")]
        public decimal CONVal2 { get; set; }
        [JsonProperty("-CONVal3")]
        public decimal CONVal3 { get; set; }
    }

    public class Stdstoponesidedgradient
    {
        [JsonProperty("-NormalCrown")]
        public decimal NormalCrown { get; set; }
        [JsonProperty("-DesignSpeed")]
        public int DesignSpeed { get; set; }
        [JsonProperty("-STDVal")]
        public int STDVal { get; set; }
        [JsonProperty("-SPCVal")]
        public int SPCVal { get; set; }
    }

    public class Stdonesidedgradientfortype4
    {
        [JsonProperty("-NormalCrown")]
        public decimal NormalCrown { get; set; }
        [JsonProperty("-DesignSpeed")]
        public int DesignSpeed { get; set; }
        [JsonProperty("-MinRadius")]
        public int MinRadius { get; set; }
        [JsonProperty("-MaxRadius")]
        public int MaxRadius { get; set; }
        [JsonProperty("-ConVal4")]
        public decimal ConVal4 { get; set; }
    }

    public class Stdappropriategradient
    {
        [JsonProperty("-MinGradient")]
        public decimal MinGradient { get; set; }
        [JsonProperty("-MaxGradient")]
        public decimal MaxGradient { get; set; }
        [JsonProperty("-IsSnowyColdArea")]
        public bool IsSnowyColdArea { get; set; }
        [JsonProperty("-STDVal")]
        public decimal STDVal { get; set; }
    }

    public class OG
    {
        [JsonProperty("OnesidedGradientRate")]
        public List<Onesidedgradientrate> OnesidedGradientRate { get; set; }
        [JsonProperty("DrainageLw")]
        public List<Drainagelw> DrainageLw { get; set; }
    }

    public class Onesidedgradientrate
    {
        [JsonProperty("-DesignSpeed")]
        public int DesignSpeed { get; set; }
        [JsonProperty("-Rate")]
        public int Rate { get; set; }
    }

    public class Drainagelw
    {
        [JsonProperty("-NormalCrown")]
        public decimal NormalCrown { get; set; }
        [JsonProperty("-AxisToEndOfCarriagewayLength")]
        public decimal AxisToEndOfCarriagewayLength { get; set; }
        [JsonProperty("-Lw")]
        public int Lw { get; set; }
    }

    public class GG
    {
        [JsonProperty("MinimumLgs")]
        public List<Minimumlg> MinimumLgs { get; set; }
    }

    public class Minimumlg
    {
        [JsonProperty("-RoadType")]
        public int RoadType { get; set; }
        [JsonProperty("-RoadClass")]
        public int RoadClass { get; set; }
        [JsonProperty("-Lgs")]
        public int Lgs { get; set; }
    }
}
