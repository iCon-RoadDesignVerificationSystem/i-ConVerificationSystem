using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_ConVerificationSystem.Structs.JSON
{
    [JsonObject]
    class AppSettings
    {
        public AppSettings()
        {
            alignments = new Alignments();
            alignments.alignment = new List<Alignment>();

        }

        [JsonProperty("alignments")]
        public Alignments alignments { get; set; }
     
        [JsonObject]
        public class Alignments
        {
            [JsonProperty("alignment")]
            public List<Alignment> alignment { get; set; }
        }

        [JsonObject]
        public class Alignment
        {
            public Alignment()
            {
                name = "";
                commonsettings = new Commonsettings();
                wcsettings = new Wcsettings();
                tgsettings = new Tgsettings();
                ogsettings = new Ogsettings();
                ggsettings = new Ggsettings();
                stdwc = new List<Stdwc>();
            }

            [JsonProperty("name")]
            public string name { get; set; }
            [JsonProperty("commonsettings")]
            public Commonsettings commonsettings { get; set; }
            [JsonProperty("wcsettings")]
            public Wcsettings wcsettings { get; set; }
            [JsonProperty("tgsettings")]
            public Tgsettings tgsettings { get; set; }
            [JsonProperty("ogsettings")]
            public Ogsettings ogsettings { get; set; }
            [JsonProperty("gssettings")]
            public Ggsettings ggsettings { get; set; }
            [JsonProperty("stdwc")]
            public List<Stdwc> stdwc { get; set; }
        }

        [JsonObject]
        public class Commonsettings
        {
            [JsonProperty("type")]
            public string type { get; set; }
            [JsonProperty("class")]
            public string _class { get; set; }
            [JsonProperty("ds")]
            public string ds { get; set; }
            [JsonProperty("sltg")]
            public string sltg { get; set; }
            [JsonProperty("interval")]
            public string interval { get; set; }
        }

        [JsonObject]
        public class Wcsettings
        {
            [JsonProperty("ptv")]
            public string ptv { get; set; }
            [JsonProperty("lvmr")]
            public string lvmr { get; set; }
            [JsonProperty("isbnp")]
            public string isbnp { get; set; }
            [JsonProperty("isconnect41to31")]
            public string isconnect41to31 { get; set; }
            [JsonProperty("isconnect41to32")]
            public string isconnect41to32 { get; set; }
            [JsonProperty("rss")]
            public string rss { get; set; }
            [JsonProperty("ispp")]
            public string ispp { get; set; }
            [JsonProperty("qcycle")]
            public string qcycle { get; set; }
            [JsonProperty("qpede")]
            public string qpede { get; set; }
            [JsonProperty("ssft")]
            public string ssft { get; set; }
            [JsonProperty("tg")]
            public string tg { get; set; }
            [JsonProperty("islcp4")]
            public string islcp4 { get; set; }
        }

        [JsonObject]
        public class Tgsettings
        {
            [JsonProperty("rpt")]
            public string rpt { get; set; }
            [JsonProperty("spt")]
            public string spt { get; set; }
            [JsonProperty("sltg")]
            public string sltg { get; set; }
            [JsonProperty("isbf")]
            public string isbf { get; set; }
            [JsonProperty("ismt")]
            public string ismt { get; set; }
            [JsonProperty("issca")]
            public string issca { get; set; }
            [JsonProperty("isscoa")]
            public string isscoa { get; set; }
        }

        [JsonObject]
        public class Ogsettings
        {
            [JsonProperty("fhp")]
            public string fhp { get; set; }
        }

        [JsonObject]
        public class Ggsettings
        {
            [JsonProperty("bpn")]
            public string bpn { get; set; }
            [JsonProperty("bal")]
            public string bal { get; set; }
            [JsonProperty("epn")]
            public string epn { get; set; }
            [JsonProperty("eal")]
            public string eal { get; set; }
        }

        [JsonObject]
        public class Stdwc
        {
            public Stdwc()
            {
                dcss = new List<Dcss>();
            }

            [JsonProperty("name")]
            public string name { get; set; }
            [JsonProperty("sta")]
            public string sta { get; set; }
            [JsonProperty("isstd")]
            public string isstd { get; set; }
            [JsonProperty("dcss")]
            public List<Dcss> dcss { get; set; }
        }

        [JsonObject]
        public class Dcss
        {
            [JsonProperty("code")]
            public string code { get; set; }
            [JsonProperty("istarget")]
            public string istarget { get; set; }
            [JsonProperty("name_j")]
            public string name_j { get; set; }
            [JsonProperty("g1name")]
            public string g1name { get; set; }
            [JsonProperty("g2name")]
            public string g2name { get; set; }
            [JsonProperty("g1")]
            public string g1 { get; set; }
            [JsonProperty("g2")]
            public string g2 { get; set; }
            [JsonProperty("isfh")]
            public string isfh { get; set; }
            [JsonProperty("isepr")]
            public string isepr { get; set; }
        }
    }
}
