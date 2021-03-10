using i_ConVerificationSystem.JSON;
using i_ConVerificationSystem.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace i_ConVerificationSystem.Forms.Print
{
    /// <summary>
    /// ReportControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ReportControl2 : UserControl
    {
        public ReportControl2()
        {
            DataContext = this;
            InitializeComponent();
            //DataContext = this;
        }

        public string checkDate
        {
            get { return $"チェック日：{DateTime.Today.Year}年{DateTime.Today.Month}月{DateTime.Today.Day}日"; }
            set { checkDate = value; }
        }

        /// <summary>
        /// 基本条件 No.4 設計基本条件 3) 道路構造（道路区分、計画交通量、設計速度、横断面等）を確認したか。
        /// </summary>
        public string messageRoadDesign4_3
        {
            get
            {
                string msg = "";
                if (WCVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "幅員構成")}{Environment.NewLine}";
                }
                else if (WVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "幅員")}{Environment.NewLine}";
                }

                if (WCVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0001")}";
                }
                else if (WVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0002")}";
                }
                else
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0006")}";
                }

                return msg;
            }
            set { messageRoadDesign4_3 = value; }
        }

        /// <summary>
        /// 基本条件 No.5 幾何構造、線形条件 2) 幾何構造の使用値（歩道の有無、車線幅員、片勾配、視距等）は適正か。
        /// </summary>
        public string messageRoadDesign5_2
        {
            get
            {
                string msg = "";
                if (WCVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "幅員構成")}{Environment.NewLine}";
                }
                else if (WVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "幅員")}{Environment.NewLine}";
                }
                else if (TGVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "横断勾配")}{Environment.NewLine}";
                }
                else if (OGVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "片勾配すりつけ")}{Environment.NewLine}";
                }
                else if (GGVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "緩勾配区間長")}{Environment.NewLine}";
                }

                if (WCVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0001")}";
                }
                else if (WVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0002")}";
                }
                else if (TGVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0003")}";
                }
                else if (OGVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0004")}";
                }
                else if (GGVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0005")}";
                }
                else
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0007")}";
                }

                return msg;
            }
            set { messageRoadDesign5_2 = value; }
        }

        /// <summary>
        /// 基本条件 No.5 幾何構造、線形条件 3) 積雪寒冷地等の場合、積雪寒冷地等の地域特性を踏まえた幾何構造の使用値となっているか。
        /// </summary>
        public string messageRoadDesign5_3
        {
            get
            {
                string msg = "";
                if (TGVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "横断勾配")}{Environment.NewLine}";
                }

                if (TGVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0003")}";
                }
                else
                {
                    //積雪寒冷とそうでないでメッセージが変わる
                    if (AppSettingsManager.Instance.HasSnowyColdArea())
                    {
                        msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0008")}";
                    }
                    else
                    {
                        msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0009")}";
                    }
                }

                return msg;
            }
            set { messageRoadDesign5_3 = value; }
        }

        /// <summary>
        /// 基本条件 No.5 幾何構造、線形条件 5) 幅員の決定根拠は明確で適正か。（道路規格との適合、積雪寒冷地の適用及び堆雪幅、道路付属施設に配慮した有効幅員の確保など）
        /// </summary>
        public string messageRoadDesign5_5
        {
            get
            {
                string msg = "";
                if (WCVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "幅員構成")}{Environment.NewLine}";
                }
                else if (WVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "幅員")}{Environment.NewLine}";
                }

                if (WCVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0001")}";
                }
                else if (WVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0002")}";
                }
                else
                {
                    //積雪寒冷とそうでないでメッセージが変わる
                    if (AppSettingsManager.Instance.HasSnowyColdArea())
                    {
                        msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0010")}";
                    }
                    else
                    {
                        msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0011")}";
                    }
                }

                return msg;
            }
            set { messageRoadDesign5_5 = value; }
        }

        /// <summary>
        /// 基本条件 No.16 関連道路(側道、副道、取付交通) 1) 幅員、延長、断面、道路幾何構造は適正か。
        /// </summary>
        public string messageRoadDesign16_1
        {
            get
            {
                string msg = "";
                if (WCVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "幅員構成")}{Environment.NewLine}";
                }
                else if (WVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "幅員")}{Environment.NewLine}";
                }

                if (WCVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0001")}";
                }
                else if (WVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0002")}";
                }
                else
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0012")}";
                }

                return msg;
            }
            set { messageRoadDesign16_1 = value; }
        }

        /// <summary>
        /// 細部条件 No.3 一般図 1) 平面図、縦断図、横断図は設計基本条件と整合が図られているか。
        /// </summary>
        public string messageRoadDesign3_1
        {
            get
            {
                string msg = "";
                if (WCVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "幅員構成")}{Environment.NewLine}";
                }
                else if (WVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "幅員")}{Environment.NewLine}";
                }
                else if (TGVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "横断勾配")}{Environment.NewLine}";
                }

                if (WCVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0001")}";
                }
                else if (WVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0002")}";
                }
                else if (TGVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0003")}";
                }
                else
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0006")}";
                }

                return msg;
            }
            set { messageRoadDesign3_1 = value; }
        }
    }
}
