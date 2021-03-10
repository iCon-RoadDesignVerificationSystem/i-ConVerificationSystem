using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Linq;
using i_ConVerificationSystem.LinqExtention;
using static i_ConVerificationSystem.Structs.CrossSects;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using i_ConVerificationSystem.Structs;
using i_ConVerificationSystem.Structs.LandXML;
using static i_ConVerificationSystem.Structs.CrossSects.DesignCrossSectSurf;
using static i_ConVerificationSystem.Structs.LandXML.SlopeList;
using static i_ConVerificationSystem.Structs.OGExtensions;
using System.IO;
using System.Globalization;

namespace i_ConVerificationSystem
{
    internal sealed class XMLLoader
    {
        private const char V_SPACE = ' ';

        private static XMLLoader instance = new XMLLoader();

        public static XMLLoader Instance
        {
            get
            {
                return instance;
            }
        }

        private XMLLoader()
        {
            //Nothing
        }

        private XDocument XmlDoc { get; set; }
        public bool IsLoaded { 
            get
            {
                return (!(XmlDoc is null));
            } 
        }

        private string _xmlFilePath { get; set; }
        public string XMLFileName
        {
            get { return Path.GetFileName(_xmlFilePath); }
        }
        public string XMLFileNameWithoutExtension
        {
            get { return Path.GetFileNameWithoutExtension(_xmlFilePath); }
        }

        /// <summary>
        /// 指定XElementのXPathをIndex付きで取得する
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string GetPath(XElement element)
        {
            return string.Join("/", element.AncestorsAndSelf().Reverse()
                .Select(e =>
                {
                    var index = GetIndex(e);
                    if (index == 1)
                    {
                        return e.Name.LocalName;
                    }
                    return string.Format("{0}[{1}]", e.Name.LocalName, GetIndex(e));
                }));
        }

        /// <summary>
        /// 指定XElementのXPathのIndexを返答する
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static int GetIndex(XElement element)
        {
            var i = 1;
            if (element.Parent == null)
            {
                return 1;
            }
            foreach (var e in element.Parent.Elements(element.Name.LocalName))
            {
                if (e == element)
                {
                    break;
                }
                i++;
            }
            return i;
        }

        /// <summary>
        /// XDocumentの名前空間を削除する
        /// </summary>
        private static void RemoveNamespaces(XDocument document)
        {
            var elements = document.Descendants();
            elements.Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
            foreach (var element in elements)
            {
                element.Name = element.Name.LocalName;

                var strippedAttributes =
                    from originalAttribute in element.Attributes().ToArray()
                    select (object)new XAttribute(originalAttribute.Name.LocalName, originalAttribute.Value);

                //Note that this also strips the attributes' line number information
                element.ReplaceAttributes(strippedAttributes.ToArray());
            }
        }

        /// <summary>
        /// XMLのロード
        /// </summary>
        /// <param name="xmlFilePath"></param>
        public void LoadXML(string xmlFilePath)
        {
            //XMLをロードし名前空間を削除する
            _xmlFilePath = xmlFilePath;
            XmlDoc = XDocument.Load(xmlFilePath);
            RemoveNamespaces(XmlDoc);

            //XSDを使ったXMLファイルの検証
            this.ValidateXML();
        }

        /// <summary>
        /// XMLの検証
        /// </summary>
        /// <returns></returns>
        public bool ValidateXML()
        {
            //Read Schema
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add(@"http://www.landxml.org/schema/LandXML-1.2", @"J-LandXML-Schema.xsd");

            bool errors = false;

            XmlDoc.Validate(schemas, (o, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"{e.Message}");
                errors = true;
            });
            System.Diagnostics.Debug.WriteLine($"Doc1 {0}", errors ? "did not validate" : "validated");

            return errors;
        }

        /// <summary>
        /// 読み込んだXMLの全XElementを返答する
        /// </summary>
        /// <returns></returns>
        public string GetAllElementsString()
        {
            StringBuilder strb = new StringBuilder();

            if (!(XmlDoc is null))
            {
                foreach (XElement n in XmlDoc.Elements())
                {
                    strb.Append(n);
                }
            }

            return strb.ToString();
        }

        /// <summary>
        /// 読み込んだXMLの全XPathを返答する
        /// </summary>
        /// <returns></returns>
        public string[] GetAllElementXPath()
        {
            StringBuilder strb = new StringBuilder();

            if (!(XmlDoc is null))
            {
                foreach (XElement e in XmlDoc.Descendants())
                {
                    strb.AppendLine(GetPath(e));
                }
            }

            return strb.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        }

        private Tuple<string, object, Type, LinqExpression> CreateWhere(string columnName, object value, Type type)
        {
            return new Tuple<string, object, Type, LinqExpression>(columnName, value, type, LinqExpression.And);
        }

        /// <summary>
        /// 指定XPathのValueを返答する
        /// </summary>
        /// <param name="XPath"></param>
        /// <returns></returns>
        private string GetXElementValueByXPath(string XPath)
        {
            string retString = string.Empty;

            if (!(XmlDoc is null || XPath == string.Empty))
            {
                XElement eSelect = XmlDoc.XPathSelectElement(XPath);
                if (!(eSelect is null))
                {
                    retString = eSelect.Value;
                }
            }

            return retString;
        }

        /// <summary>
        /// 指定XPathのXElementを返答する
        /// </summary>
        /// <param name="XPath"></param>
        /// <returns></returns>
        private XElement GetXElementByXPath(string XPath)
        {
            XElement retElement = null;

            if (!(XmlDoc is null || XPath == string.Empty))
            {
                retElement = XmlDoc.XPathSelectElement(XPath);
            }

            return retElement;
        }


        private IEnumerable<XElement> GetIEnumerableByXPath(string XPath)
        {
            IEnumerable<XElement> eSelect = null;

            if (!(XmlDoc is null || XPath == string.Empty))
            {
                eSelect = from T in XmlDoc.XPathSelectElements(XPath) select T;
            }

            return eSelect;
        }

        private string GetTargetElementValue(string tarPath, Dictionary<string, string> attPairs)
        {
            if (!(XmlDoc is null))
            {
                //IEnumerable<XElement> eSelect = from T in XmlDoc.XPathSelectElements(tarPath) where T.Attribute("").Value == "" select T;
                IEnumerable<XElement> eSelect1 = from T in XmlDoc.XPathSelectElements(tarPath) select T;

                var expressions = new List<Tuple<string, object, Type, LinqExpression>>();

                foreach (KeyValuePair<string,string> kvp in attPairs)
                {
                    expressions.Add(CreateWhere(nameof(kvp.Key), kvp.Value, typeof(string)));
                }

                var ttList = EnumerableExtensions.DynamicWhere(eSelect1, expressions).ToList();
            }

            return null;
        }

        /// <summary>
        /// プロジェクト名(Project)のnameを取得する
        /// </summary>
        /// <param name="AlignmentsName"></param>
        /// <returns></returns>
        public string GetProjectName()
        {
            try
            {
                string retName = string.Empty;
                string xPath = "LandXML/Project";
                XElement xe = this.GetXElementByXPath(xPath);

                if (!(xe is null))
                {
                    retName = xe.Attribute("name").Value;
                }

                return retName;
            }
            catch (Exception)
            {
                //Projectが無かった場合
                return string.Empty;
            }
        }

        /// <summary>
        /// ファイル作成会社を取得する
        /// </summary>
        /// <returns></returns>
        public string GetCompanyName()
        {
            try
            {
                string retName = string.Empty;
                string xPath = "LandXML/Application/Author";
                XElement xe = this.GetXElementByXPath(xPath);
                if (!(xe is null))
                {
                    retName = xe.Attribute("company").Value;
                }

                return retName;
            }
            catch (Exception)
            {
                //Companyが無かった場合
                return string.Empty;
            }
        }

        /// <summary>
        /// 中心線形セット(Alignments)のnameリストを取得する
        /// </summary>
        /// <returns></returns>
        public List<string> GetAlignmentsName()
        {
            try
            {
                List<string> retStringList = new List<string>();
                string xPath = "LandXML/Alignments";
                var xe = this.GetIEnumerableByXPath(xPath);

                if (!(xe is null))
                {
                    retStringList = (from T in xe select T.Attribute("name").Value).ToList();
                }

                return retStringList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 指定中心線形セット(Alignments)の中心線形(Alignment)のnameリストを取得する
        /// </summary>
        /// <param name="AlignmentsName"></param>
        /// <returns></returns>
        public List<string> GetAlignmentNameList(string AlignmentsName)
        {
            try
            {
                List<string> retNameList = new List<string>();
                string xPath = $"LandXML/Alignments[@name='{AlignmentsName}']/Alignment";
                var xe = this.GetIEnumerableByXPath(xPath);

                if (!(xe is null))
                {
                    retNameList = (from T in xe select T.Attribute("name").Value).ToList();
                }

                return retNameList;
            }
            catch (Exception)
            {
                //中心線形が無かった場合
                return null;
            }
        }
        
        /// <summary>
        /// 中心線形タプルを返答（テスト用）
        /// </summary>
        /// <param name="AlignmentsName"></param>
        /// <returns></returns>
        public List<Tuple<string,XElement>> GetAlignmentTupleList(string AlignmentsName)
        {
            try
            {
                var retTupList = new List<Tuple<string, XElement>>();
                string xPath = $"LandXML/Alignments[@name='{AlignmentsName}']/Alignment";
                var xe = this.GetIEnumerableByXPath(xPath);

                if (!(xe is null))
                {
                    retTupList = (from T in xe select new Tuple<string, XElement>(T.Attribute("name").Value, T)).ToList();
                }

                return retTupList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 中心線形タプルを返答
        /// </summary>
        /// <returns></returns>
        public List<Tuple<string, XElement>> GetAlignmentTupleList()
        {
            try
            {
                var retTupList = new List<Tuple<string, XElement>>();
                string xPath = $"LandXML/Alignments/Alignment";
                string gmXPath = $"../Feature/Property[@label='designGmType' and @value='道路']";
                var xe = this.GetIEnumerableByXPath(xPath);

                if (!(xe is null))
                {
                    retTupList = (from T in xe
                                  where T.XPathSelectElement(gmXPath) != null
                                  select new Tuple<string, XElement>(T.Attribute("name").Value, T)).ToList();
                }

                return retTupList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 中心線形名から中心線形セットを取得
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <returns></returns>
        public XElement GetAlignmentFromName(string alignmentName)
        {
            try
            {
                string xPath = $"LandXML/Alignments/Alignment[@name='{alignmentName}']";
                var xe = this.GetIEnumerableByXPath(xPath);
                if (!(xe is null))
                {
                    return xe.FirstOrDefault();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 中心線形のnameを返答
        /// </summary>
        /// <param name="alignmentXe"></param>
        /// <returns></returns>
        public string GetAlignmentName(XElement alignmentXe)
        {
            try
            {
                return alignmentXe.Attribute("name").Value;
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// 指定中心線形の縦断形状(Profile)を返答する
        /// </summary>
        /// <param name="AlignmentsName"></param>
        /// <param name="AlignmentName"></param>
        /// <returns></returns>
        public List<XElement> GetProfileList(string AlignmentsName, string AlignmentName)
        {
            try
            {
                var retList = new List<XElement>();
                string xPath = $"LandXML/Alignments[@name='{AlignmentsName}']/Alignment[@name='{AlignmentName}']";
                var xe = this.GetIEnumerableByXPath(xPath);

                if (!(xe is null))
                {
                    retList = xe.Elements("Profile").ToList();
                }

                return retList;
            }
            catch (Exception)
            {
                //Profileが無かった場合
                return null;
            }
        }

        /// <summary>
        /// 縦断形状の縦断線形(ProfAlign)を返答する
        /// </summary>
        /// <param name="profileXe"></param>
        /// <returns></returns>
        public List<XElement> GetProfAlign(XElement profileXe)
        {
            try
            {
                var retList = new List<XElement>();

                if (!(profileXe is null))
                {
                    retList = profileXe.Elements("ProfAlign").ToList();
                }

                return retList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 縦断線形を返答
        /// </summary>
        /// <param name="alignmentName"></param>
        /// <returns></returns>
        public XElement GetProfAlignFromAlignmentName(string alignmentName)
        {
            try
            {
                string xPath = $"LandXML/Alignments/Alignment[@name='{alignmentName}']/Profile/ProfAlign";
                var xe = this.GetIEnumerableByXPath(xPath);

                if (!(xe is null))
                {
                    return xe.First();
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 道路規格を返答
        /// </summary>
        /// <param name="alignmentXe"></param>
        /// <returns></returns>
        public XElement GetRoadClass(XElement alignmentXe)
        {
            try
            {
                XElement retXe = null;
                string xPath = "../Feature/Property[@label='classification']";

                if (!(alignmentXe is null))
                {
                    retXe = alignmentXe.XPathSelectElement(xPath);
                }

                return retXe;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 計画交通量を返答
        /// </summary>
        /// <param name="alignmentXe"></param>
        /// <returns></returns>
        public XElement GetTrafficVolume(XElement alignmentXe)
        {
            try
            {
                XElement retXe = null;
                string xPath = "../Feature/Property[@label='trafficVolume']";

                if (!(alignmentXe is null))
                {
                    retXe = alignmentXe.XPathSelectElement(xPath);
                }

                return retXe;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 設計速度を取得
        /// </summary>
        /// <param name="alignmentXe"></param>
        /// <returns></returns>
        public List<string> GetDesignSpeed(XElement alignmentXe)
        {
            try
            {
                List<string> retList = new List<string>();

                if (!(alignmentXe is null))
                {
                    string xPath = $"LandXML/Roadways/Roadway[@alignmentRefs='{alignmentXe.Attribute("name").Value}']/Speeds/DesignSpeed";

                    retList = (from T in XmlDoc.XPathSelectElements(xPath) select T.Attribute("speed").Value).ToList();
                }

                return retList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 横断形状リストを取得
        /// </summary>
        /// <param name="alignmentXe"></param>
        /// <returns></returns>
        public List<CrossSect_OGExtension> GetCrossSectsOG(XElement alignmentXe, bool isGetAllItems)
        {
            try
            {
                List<CrossSect_OGExtension> retList = new List<CrossSect_OGExtension>();

                string CSPCODE_REGEX = @"(?<=\w)\d+";

                if (!(alignmentXe is null))
                {
                    string xPath = $"./CrossSects/CrossSect";
                    var xCss = (from T in alignmentXe.XPathSelectElements(xPath) select T);
                    Func<IEnumerable<XElement>, decimal> func = null;
                    func = (csp) =>
                    {
                        var minVal = (from T in csp select decimal.Parse(T.Value.Split(V_SPACE)[0])).Min();
                        var maxVal = (from T in csp select decimal.Parse(T.Value.Split(V_SPACE)[0])).Max();
                        return Math.Round(Math.Abs(maxVal - minVal), 3, MidpointRounding.AwayFromZero);
                    };

                    retList = (from T in xCss
                               select new CrossSect_OGExtension
                               {
                                   name = (string)T.Attribute("name"),
                                   sta = (decimal)T.Attribute("sta"),
                                   //dcssList = (from T1 in T.XPathSelectElements("./DesignCrossSectSurf[@desc='道路面']")
                                   dcssList = (from T1 in T.XPathSelectElements("./DesignCrossSectSurf")
                                               where (string)T1.Attribute("name") != "SlopeCut" &&
                                               (string)T1.Attribute("name") != "SlopeFill" &&
                                               (string)T1.Attribute("name") != "SubBase" &&
                                               (string)T1.Attribute("name") != "SubGrade" &&
                                               (string)T1.Attribute("name") != "Excavation" &&
                                               (string)T1.Attribute("name") != "Pavement" &&
                                               (string)T1.Attribute("name") != "BermCut" &&
                                               (string)T1.Attribute("name") != "BermFill"
                                               select new DesignCrossSectSurf_OGExtension
                                               {
                                                   name = (string)T1.Attribute("name"),
                                                   side = (string)T1.Attribute("side") == "left" ? DCSSSide.Left : DCSSSide.Right,
                                                   cspList = (from T2 in T1.XPathSelectElements("./CrossSectPnt")
                                                              select new CrossSectPnt
                                                              {
                                                                  code = (string)T2.Attribute("code"),
                                                                  roadWidth = func(T1.XPathSelectElements("./CrossSectPnt")),
                                                                  roadPositionX = Math.Round(decimal.Parse(T2.Value.Split(V_SPACE)[0]), 3, MidpointRounding.AwayFromZero),
                                                                  roadHight = decimal.Parse(T2.Value.Split(V_SPACE)[1]),
                                                                  roadWidthCompositionNo = int.Parse(Regex.Matches(T2.Attribute("code").Value, CSPCODE_REGEX)[0].Value),
                                                                  roadPositionNo = int.Parse(Regex.Matches(T2.Attribute("code").Value, CSPCODE_REGEX)[1].Value),
                                                                  isCenter = T2.Attribute("code").Value.StartsWith("F")
                                                              }).ToList()
                                               }).ToList(),
                                   clOffset = T.XPathSelectElement("./Feature[@name='Formation']/Property[@label='clOffset']") == null ? 0.0M : Math.Round(decimal.Parse(T.XPathSelectElement("./Feature[@name='Formation']/Property[@label='clOffset']").Attribute("value").Value, NumberStyles.AllowExponent | NumberStyles.Float, CultureInfo.InvariantCulture), 3, MidpointRounding.AwayFromZero),
                                   fhOffset = T.XPathSelectElement("./Feature[@name='Formation']/Property[@label='fhOffset']") == null ? 0.0M : Math.Round(decimal.Parse(T.XPathSelectElement("./Feature[@name='Formation']/Property[@label='fhOffset']").Attribute("value").Value, NumberStyles.AllowExponent | NumberStyles.Float, CultureInfo.InvariantCulture), 3, MidpointRounding.AwayFromZero)
                               }).ToList();
                }

                if (isGetAllItems == false)
                {
                    retList = retList.Distinct(new CrossSectCompareOG()).ToList();
                }

                //DesignCrossSectSurfが1件以上あるものを取得
                retList = (from T in retList
                           where 0 < T.dcssList.Count
                           select T).ToList();

                return retList;
            }
            catch (Exception)
            {
                return new List<CrossSect_OGExtension>();
            }
        }

        public class CrossSectCompareOG : IEqualityComparer<CrossSect_OGExtension>
        {
            public bool Equals(CrossSect_OGExtension x, CrossSect_OGExtension y)
            {
                //DCSSの数が一致していること
                //CSPの数が一致していること
                //CSPのGetHashCodeが一致していること
                bool duringOK = false;

                if (x.dcssList.Count == y.dcssList.Count)
                {
                    for (int dcssIdx = 0; dcssIdx < x.dcssList.Count; dcssIdx++)
                    {
                        var xDcss = x.dcssList[dcssIdx]; var yDcss = y.dcssList[dcssIdx];
                        if (xDcss.cspList.Count == yDcss.cspList.Count)
                        {
                            for (int cspIdx = 0; cspIdx < xDcss.cspList.Count; cspIdx++)
                            {
                                var xCsp = xDcss.cspList[cspIdx]; var yCsp = yDcss.cspList[cspIdx];
                                if (xCsp.Equals(yCsp))
                                {
                                    //OK
                                    duringOK = true;
                                    continue;
                                }
                                else
                                {
                                    //次のDCSSをチェック
                                    duringOK = false;
                                    break;
                                }
                            }

                            if (duringOK) return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (duringOK) return true;
                }
                else
                {
                    return false;   
                }

                return false;
            }

            public int GetHashCode(CrossSect_OGExtension obj)
            {
                return obj.GetHashCode();
            }
        }

        /// <summary>
        /// 直線部の横断勾配%を取得
        /// </summary>
        /// <param name="alignmentXe"></param>
        /// <returns></returns>
        public string GetNormalCrown(XElement alignmentXe)
        {
            try
            {
                XElement xe = null;
                string xPath = "./Feature/Property[@label='normalCrown']";

                if (!(alignmentXe is null))
                {
                    xe = alignmentXe.XPathSelectElement(xPath);
                }

                return xe.Attribute("value").Value;
            }
            catch (Exception)
            {
                return "0";
            }
        }

        /// <summary>
        /// SlopeListを使うか
        /// </summary>
        /// <param name="alignmentXe"></param>
        /// <returns></returns>
        public bool IsUseSlopeList(XElement alignmentXe)
        {
            try
            {
                XElement xe = null;
                string xPath = "./Feature[@name='SuperelevationConfig']/Property[@label='useSlopeList']";

                if (!(alignmentXe is null))
                {
                    xe = alignmentXe.XPathSelectElement(xPath);

                    if (xe is null)
                    {
                        return false;
                    }
                }

                return (bool)xe.Attribute("value");
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 一車線道路か
        /// </summary>
        /// <param name="alignmentXe"></param>
        /// <returns></returns>
        public bool IsSingleLaneRoad(XElement alignmentXe)
        {
            try
            {
                XElement xe = null;
                string xPath = "./Feature[@name='SuperelevationConfig']/Property[@label='singleLaneRoad']";

                if (!(alignmentXe is null))
                {
                    xe = alignmentXe.XPathSelectElement(xPath);

                    if (xe is null)
                    {
                        return false;
                    }
                }

                return (bool)xe.Attribute("value");
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 円曲線リスト
        /// </summary>
        /// <param name="alignmentXe"></param>
        /// <returns></returns>
        public List<Curve> GetCurves(XElement alignmentXe)
        {
            try
            {
                var retList = new List<Curve>();

                if (!(alignmentXe is null))
                {
                    string xPath = $"./CoordGeom/Curve";
                    var xCss = (from T in alignmentXe.XPathSelectElements(xPath) select T);

                    retList = (from T in xCss
                               select new Curve
                               {
                                   name = T.Attribute("name") == null ? string.Empty : (string)T.Attribute("name"),
                                   staStart = T.Attribute("staStart") == null ? 0.0M : (decimal)T.Attribute("staStart"),
                                   length = T.Attribute("length") == null ? 0.0M : (decimal)T.Attribute("length"),
                                   rot = (string)T.Attribute("rot") == "cw" ? Curve.Clockwise.cw : Curve.Clockwise.ccw,
                                   radius = T.Attribute("radius") == null ? 0.0M : Math.Round((decimal)(T.Attribute("radius")), 6, MidpointRounding.AwayFromZero),
                               }
                               ).ToList();
                }

                return retList;
            }
            catch (Exception)
            {
                return new List<Curve>();
            }
        }

        /// <summary>
        /// 緩和曲線
        /// </summary>
        /// <param name="alignmentXe"></param>
        /// <returns></returns>
        public List<Spiral> GetSpirals(XElement alignmentXe)
        {
            try
            {
                var retList = new List<Spiral>();

                if (!(alignmentXe is null))
                {
                    string xPath = $"./CoordGeom/Spiral";
                    var xCss = (from T in alignmentXe.XPathSelectElements(xPath) select T);

                    retList = (from T in xCss
                               select new Spiral
                               {
                                   name = T.Attribute("name") == null ? string.Empty : (string)T.Attribute("name"),
                                   staStart = T.Attribute("staStart") == null ? 0.0M : (decimal)T.Attribute("staStart"),
                                   length = T.Attribute("length") == null ? 0.0M : (decimal)T.Attribute("length"),
                                   rot = (string)T.Attribute("rot") == "cw" ? Curve.Clockwise.cw : Curve.Clockwise.ccw,
                                   spiType = Spiral.SpiType.clothoid,
                                   clothoidParameter = T.XPathSelectElements("./Feature/Property").FirstOrDefault() == null ? 0.0M :  Math.Round((decimal)(from T2 in T.XPathSelectElements("./Feature/Property") select T2).FirstOrDefault().Attribute("value"), 6, MidpointRounding.AwayFromZero)
                               }
                               ).ToList();
                }

                return retList;
            }
            catch (Exception)
            {
                return new List<Spiral>();
            }
        }

        /// <summary>
        /// 直線
        /// </summary>
        /// <param name="alignmentXe"></param>
        /// <returns></returns>
        public List<Line> GetLines(XElement alignmentXe)
        {
            try
            {
                var retList = new List<Line>();

                if (!(alignmentXe is null))
                {
                    string xPath = $"./CoordGeom/Line";
                    var xCss = (from T in alignmentXe.XPathSelectElements(xPath) select T);

                    retList = (from T in xCss
                               select new Line
                               {
                                   name = T.Attribute("name") == null ? string.Empty : (string)T.Attribute("name"),
                                   staStart = T.Attribute("staStart") == null ? 0.0M : (decimal)T.Attribute("staStart"),
                                   length = T.Attribute("length") == null ? 0.0M : (decimal)T.Attribute("length"),
                               }
                               ).ToList();
                }

                return retList;
            }
            catch (Exception)
            {
                return new List<Line>();
            }
        }

        /// <summary>
        /// 片勾配すりつけ
        /// </summary>
        /// <param name="alignmentXe"></param>
        /// <returns></returns>
        public List<Superelevation> GetSuperelevationList(XElement alignmentXe)
        {
            try
            {
                var retList = new List<Superelevation>();

                if (!(alignmentXe is null))
                {
                    string xPath = $"./Superelevation";
                    var xCss = (from T in alignmentXe.XPathSelectElements(xPath) select T);
                    var emptyElement = XElement.Parse("<x>0</x>");
                    var emptyAttribute = XElement.Parse("<x value=\"0\">0</x>");

                    retList = (from T in xCss
                               select new Superelevation
                               {
                                   staStart = Math.Round((decimal)T.Attribute("staStart"), 3, MidpointRounding.AwayFromZero),
                                   staEnd = Math.Round((decimal)T.Attribute("staEnd"), 3, MidpointRounding.AwayFromZero),
                                   BeginRunoutSta = Math.Round((decimal)(T.Element("BeginRunoutSta") ?? emptyElement), 3, MidpointRounding.AwayFromZero),
                                   BeginRunoffSta = Math.Round((decimal)(T.Element("BeginRunoffSta") ?? emptyElement), 3, MidpointRounding.AwayFromZero),
                                   FullSuperSta = Math.Round((decimal)(T.Element("FullSuperSta") ?? emptyElement), 3, MidpointRounding.AwayFromZero),
                                   FullSuperelev = Math.Round((decimal)(T.Element("FullSuperelev") ?? emptyElement), 3, MidpointRounding.AwayFromZero),
                                   RunoffSta = Math.Round((decimal)(T.Element("RunoffSta") ?? emptyElement), 3, MidpointRounding.AwayFromZero),
                                   StartofRunoutSta = Math.Round((decimal)(T.Element("StartofRunoutSta") ?? emptyElement), 3, MidpointRounding.AwayFromZero),
                                   EndofRunoutSta = Math.Round((decimal)(T.Element("EndofRunoutSta") ?? emptyElement), 3, MidpointRounding.AwayFromZero),
                                   AdverseSE = (string)T.Element("AdverseSE") == "adverse" ? Superelevation.EAdverseSE.adverse : Superelevation.EAdverseSE.nonAdverse,
                                   ReverseCrown = (from T1 in T.XPathSelectElements("./Feature[@name = 'ReverseCrown']/Property") select Math.Round((decimal)(T1.Attribute("value") ?? emptyAttribute.Attribute("value")), 3, MidpointRounding.AwayFromZero)).ToList(),
                                   FlatSta = (from T1 in T.XPathSelectElements("./Feature[@name = 'FlatSta']/Property") select Math.Round((decimal)(T1.Attribute("value") ?? emptyAttribute.Attribute("value")), 3, MidpointRounding.AwayFromZero)).FirstOrDefault(),
                               }
                               ).ToList();
                }

                return retList;
            }
            catch (Exception)
            {
                return new List<Superelevation>();
            }
        }

        /// <summary>
        /// 任意横断勾配リストを取得
        /// </summary>
        /// <param name="alignmentXe"></param>
        /// <returns></returns>
        public SlopeList GetSlopeList(XElement alignmentXe)
        {
            try
            {
                SlopeList retVal = new SlopeList();

                if (!(alignmentXe is null))
                {
                    string xPath = $"./Feature[@name='slopeList']";
                    var xCss = (from T in alignmentXe.XPathSelectElements(xPath) select T);

                    retVal = (from T in xCss
                              select new SlopeList
                              {
                                  SlopeValues = (from T1 in T.XPathSelectElements("./Property[@label='slopeValue']")
                                                 select new SlopeValue
                                                 {
                                                     sta = Math.Round(decimal.Parse(T1.Attribute("value").Value.Split(' ')[0]), 3, MidpointRounding.AwayFromZero),
                                                     gradientSingleLane = T1.Attribute("value").Value.Split(' ').Count() == 2 ? Math.Round(decimal.Parse(T1.Attribute("value").Value.Split(' ')[1]), 3, MidpointRounding.AwayFromZero) : 0,
                                                     gradientLeft = T1.Attribute("value").Value.Split(' ').Count() == 2 ? 0 : Math.Round(decimal.Parse(T1.Attribute("value").Value.Split(' ')[1]), 3, MidpointRounding.AwayFromZero),
                                                     gradientRight = T1.Attribute("value").Value.Split(' ').Count() == 2 ? 0 : Math.Round(decimal.Parse(T1.Attribute("value").Value.Split(' ')[2]), 3, MidpointRounding.AwayFromZero)
                                                 }).Distinct(new CompareSlopeValue()).ToList()
                              }).FirstOrDefault();
                }
                　
                return retVal;
            }
            catch (Exception)
            {
                return new SlopeList();
            }
        }

        /// <summary>
        /// 測点間隔を取得
        /// </summary>
        /// <param name="alignmentXe"></param>
        /// <returns></returns>
        public int GetInterval(XElement alignmentXe)
        {
            try
            {
                var retVal = 0;

                if (!(alignmentXe is null))
                {
                    string xPath = $"./Feature[@name='Interval']/Property[@label='main']";
                    var xCss = alignmentXe.XPathSelectElement(xPath);

                    retVal = (int)(double)xCss.Attribute("value");
                }

                return retVal;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 道路設計データがあるか
        /// </summary>
        /// <returns></returns>
        public bool IsRoadOfArchitecture()
        {
            try
            {
                bool retVal = false;
                string xPath = "LandXML/Alignments/Feature/Property[@label='designGmType']";
                var xe = this.GetIEnumerableByXPath(xPath);

                if (xe.Any())
                {
                    var roadXe = from T in xe
                                 where T.Attribute("value").Value == "道路"
                                 select T;
                    retVal = roadXe.Any();
                }

                return retVal;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
