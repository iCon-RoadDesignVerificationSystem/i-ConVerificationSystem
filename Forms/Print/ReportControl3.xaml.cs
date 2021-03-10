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
    public partial class ReportControl3 : UserControl
    {
        public ReportControl3()
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
        /// 基本条件 No.4 設計基本条件 3) 道路（本線、従道路、取付道路等）の構造、規格を確認したか。
        /// </summary>
        public string messageRoadIntersection4_3
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
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0013")}";
                }

                return msg;
            }
            set { messageRoadIntersection4_3 = value; }
        }

        /// <summary>
        /// 基本条件 No.4 設計基本条件 3) 自転車通行空間を考慮したか。また、自転車・歩行者の分離を確認したか。
        /// </summary>
        public string messageRoadIntersection4_13
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
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0014")}";
                }

                return msg;
            }
            set { messageRoadIntersection4_13 = value; }
        }

        /// <summary>
        /// 基本条件 No.5 幾何構造、線形条件 1) 平面・縦断線形（本線、従道路）の採用値や緩勾配区間の確保は妥当か。また、組み合わせは適正か。
        /// </summary>
        public string messageRoadIntersection5_1
        {
            get
            {
                string msg = "";
                if (GGVerificationResultItem.Instance.HasVerificationResult() == false)
                {
                    msg = $"{MessageMasterManager.Instance.GetMessage("P-0018", "緩勾配区間長")}{Environment.NewLine}";
                }

                if (GGVerificationResultItem.Instance.HasError())
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0005")}";
                }
                else
                {
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0015")}";
                }

                return msg;
            }
            set { messageRoadIntersection5_1 = value; }
        }

        /// <summary>
        /// 基本条件 No.5 幾何構造、線形条件 2) 幅員構成は適正か。また、幅員構成上、自転車専用帯等自転車走行空間を考慮したか。
        /// </summary>
        public string messageRoadIntersection5_2
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
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0016")}";
                }

                return msg;
            }
            set { messageRoadIntersection5_2 = value; }
        }

        /// <summary>
        /// 細部条件 No.5 詳細検討 9) 道路詳細設計と整合はとれているか。
        /// </summary>
        public string messageRoadIntersection5_9
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
                    msg = $"{msg}{MessageMasterManager.Instance.GetMessage("P-0017")}";
                }

                return msg;
            }
            set { messageRoadIntersection5_9 = value; }
        }
    }
}
