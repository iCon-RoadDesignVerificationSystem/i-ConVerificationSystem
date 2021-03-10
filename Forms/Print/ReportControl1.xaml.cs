using i_ConVerificationSystem;
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
    public partial class ReportControl1 : UserControl
    {
        public ReportControl1()
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

        public string projectName
        {
            get { return XMLLoader.Instance.GetProjectName(); }
            set { projectName = value; }
        }

        public string fileName
        {
            get { return XMLLoader.Instance.XMLFileName; }
            set { fileName = value; }
        }

        public string companyName
        {
            get { return XMLLoader.Instance.GetCompanyName(); }
            set { companyName = value; }
        }

        /// <summary>
        /// LandXMLのエラー数
        /// </summary>
        public string LandXMLErrorCount
        {
            get { return $"{_LandXMLErrorCount}件"; }
            set { }
        }

        public int _LandXMLErrorCount {
            get
            {
                if (XMLLoader.Instance.IsLoaded)
                {
                    if (XMLLoader.Instance.ValidateXML())
                    {
                        return 1;
                    }
                    else if (XMLLoader.Instance.IsRoadOfArchitecture() == false)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            set { } }

        /// <summary>
        /// 幅員構成のエラー数
        /// </summary>
        public string WCErrorCount
        {
            get { return $"{_WCErrorCount}({_WCOKCCount})件"; }
            set { }
        }

        public int _WCErrorCount
        {
            get
            {
                return WCVerificationResultItem.Instance.GetErrorCount();
            }
        }
        public int _WCOKCCount
        {
            get
            {
                return WCVerificationResultItem.Instance.GetOK_CCount();
            }
        }

        /// <summary>
        /// 幅員のエラー数
        /// </summary>
        public string WErrorCount
        {
            get { return $"{_WErrorCount}({_WOKCCount})件"; }
            set { }
        }

        public int _WErrorCount
        {
            get
            {
                return WVerificationResultItem.Instance.GetErrorCount();
            }
        }
        public int _WOKCCount
        {
            get
            {
                return WVerificationResultItem.Instance.GetOK_CCount();
            }
        }

        /// <summary>
        /// 横断勾配のエラー数
        /// </summary>
        public string TGErrorCount
        {
            get { return $"{_TGErrorCount}({_TGOKCCount})件"; }
            set { }
        }

        public int _TGErrorCount
        {
            get
            {
                return TGVerificationResultItem.Instance.GetErrorCount();
            }
        }
        public int _TGOKCCount
        {
            get
            {
                return TGVerificationResultItem.Instance.GetOK_CCount();
            }
        }

        /// <summary>
        /// 片勾配すりつけのエラー数
        /// </summary>
        public string OGErrorCount
        {
            get { return $"{_OGErrorCount}({_OGOKCCount})件"; }
            set { }
        }

        public int _OGErrorCount
        {
            get
            {
                return OGVerificationResultItem.Instance.GetErrorCount();
            }
        }

        public int _OGOKCCount
        {
            get
            {
                return OGVerificationResultItem.Instance.GetOK_CCount();
            }
        }

        /// <summary>
        /// 緩勾配区間長のエラー数
        /// </summary>
        public string GGErrorCount
        {
            get { return $"{_GGErrorCount}({_GGOKCCount})件"; }
            set { }
        }

        public int _GGErrorCount { 
            get
            {
                return GGVerificationResultItem.Instance.GetErrorCount();
            }
        }
        public int _GGOKCCount
        {
            get
            {
                return GGVerificationResultItem.Instance.GetOK_CCount();
            }
        }
    }
}
