using i_ConVerificationSystem.Extensions.EnumExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace i_ConVerificationSystem.Structs
{
    public class CrossSects
    {
        public class CrossSect : IEquatable<CrossSect>, IComparable<CrossSect>
        {
            public int ID;
            public string name;
            public decimal sta;
            //public List<DesignCrossSectSurf> dcssList;
            public decimal clOffset;
            public decimal fhOffset;

            public override int GetHashCode()
            {
                return this.ID.GetHashCode();
            }

            bool IEquatable<CrossSect>.Equals(CrossSect other)
            {
                if (this.ID != other.ID || this.name != other.name || this.sta != other.sta) return false;
                return true;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || this.GetType() != obj.GetType()) return false;

                var other = (CrossSect)obj;
                if (this.ID != other.ID || this.name != other.name || this.sta != other.sta) return false;
                return true;
            }

            public override string ToString()
            {
                return name.ToString();
            }

            public int CompareTo(CrossSect other)
            {
                return this.ID - other.ID;
            }
        }

        /// <summary>
        /// 比較用クラス
        /// </summary>
        public class CompareCrossSect : IEqualityComparer<CrossSect>
        {
            public bool Equals(CrossSect x, CrossSect y)
            {
                if (object.ReferenceEquals(x, y)) return true;
                return x.Equals(y);
            }

            public int GetHashCode(CrossSect obj)
            {
                return obj.GetHashCode();
            }
        }

        public class DesignCrossSectSurf : IEquatable<DesignCrossSectSurf>, IComparable<DesignCrossSectSurf>, IComparer<DesignCrossSectSurf>
        {
            /// <summary>
            /// 左右
            /// </summary>
            public enum DCSSSide
            {
                [Description("Left")]
                Left = 0,
                [Description("Right")]
                Right = 1,
                [Description("-")]
                Other = 2
            };
            public class DCSSSideProvider : EnumSourceProvider<DCSSSide> { }

            //LandXMLで設定されているname
            public string name { get; set; }
            //画面で入力された日本語名
            //public string name_J;
            public DCSSSide side { get; set; }
            public List<CrossSectPnt> cspList;

            public override int GetHashCode()
            {
                return this.cspList.GetHashCode();
            }

            bool IEquatable<DesignCrossSectSurf>.Equals(DesignCrossSectSurf other)
            {
                if (this.cspList.Count != other.cspList.Count) return false;
                foreach (var csp in this.cspList)
                {
                    if (!(other.cspList.Contains(csp, new CompareCrossSectPnt()))) return false;
                }
                return true;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || this.GetType() != obj.GetType()) return false;

                var other = (DesignCrossSectSurf)obj;
                if (this.cspList.Count != other.cspList.Count) return false;
                foreach (var csp in this.cspList)
                {
                    if (!(other.cspList.Contains(csp, new CompareCrossSectPnt()))) return false;
                }
                return true;
            }

            public int CompareTo(DesignCrossSectSurf other)
            {
                //if (this.side < other.side) return -1;
                //else if (this.side > other.side) return 1;
                //else return 0;
                return Compare(this, other);
            }

            public int Compare(DesignCrossSectSurf x, DesignCrossSectSurf y)
            {
                var cx = (from T in x.cspList orderby T.roadPositionNo descending select T).FirstOrDefault();
                var cy = (from T in y.cspList orderby T.roadPositionNo descending select T).FirstOrDefault();
                return new CrossSectPnt().Compare(cx, cy);
            }
        }

        public class CrossSectPnt : IEquatable<CrossSectPnt>, IComparable<CrossSectPnt>, IComparer<CrossSectPnt>
        {
            public string code; //code
            public bool isCenter; //センターか
            public int roadWidthCompositionNo; //横断形状番号(Lx)
            public int roadPositionNo; //構成番号(nx)
            public decimal roadWidth; //幅員
            public decimal roadHight; //標高
            public decimal roadPositionX; //X座標

            /// <summary>
            /// 幅員の小数点3桁までを返答する
            /// </summary>
            /// <returns></returns>
            public string GetRoadWidthString()
            {
                return roadWidth == 0.0M ? "0" : Math.Round(roadWidth, 3, MidpointRounding.AwayFromZero).ToString();
            }

            /// <summary>
            /// 指定地点からの幅員を返答する
            /// </summary>
            /// <param name="p1"></param>
            /// <returns></returns>
            public string GetWidthStringBetweenRoad(decimal p)
            {
                var w = Math.Abs(roadPositionX - p);
                return Math.Round(w, 3, MidpointRounding.AwayFromZero).ToString();
            }

            public override int GetHashCode()
            {
                return this.code.GetHashCode() ^ this.roadWidthCompositionNo.GetHashCode() ^ this.roadPositionNo.GetHashCode() ^ this.roadWidth.GetHashCode();
            }

            bool IEquatable<CrossSectPnt>.Equals(CrossSectPnt other)
            {
                return (this.code == other.code && 
                        this.roadWidthCompositionNo == other.roadWidthCompositionNo && 
                        this.roadPositionNo == other.roadPositionNo && 
                        this.roadWidth == other.roadWidth);
            }

            public override bool Equals(object obj)
            {
                if (obj == null || this.GetType() != obj.GetType()) return false;

                var other = (CrossSectPnt)obj;
                return (this.code == other.code &&
                        this.roadWidthCompositionNo == other.roadWidthCompositionNo &&
                        this.roadPositionNo == other.roadPositionNo &&
                        this.roadWidth == other.roadWidth);
            }

            public int CompareTo(CrossSectPnt other)
            {
                return Compare(this, other);
            }

            public int Compare(CrossSectPnt x, CrossSectPnt y)
            {
                if (x.code.StartsWith("L"))
                {
                    //左であるとき
                    //右のほうが大きい
                    if (y.code.StartsWith("R")) return -1;

                    if (x.roadPositionNo > y.roadPositionNo) return -1;
                    else if (x.roadPositionNo < y.roadPositionNo) return 1;
                    else return 0;
                }
                else
                {
                    //右であるとき
                    //左のほうが小さい
                    if (y.code.StartsWith("L")) return 1;

                    if (x.roadPositionNo < y.roadPositionNo) return -1;
                    else if (x.roadPositionNo > y.roadPositionNo) return 1;
                    else return 0;
                }
            }
        }

        /// <summary>
        /// 比較用クラス
        /// </summary>
        public class CompareCrossSectPnt : IEqualityComparer<CrossSectPnt>
        {
            public bool Equals(CrossSectPnt x, CrossSectPnt y)
            {
                if (object.ReferenceEquals(x, y)) return true;
                return x.Equals(y);
            }

            public int GetHashCode(CrossSectPnt obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
