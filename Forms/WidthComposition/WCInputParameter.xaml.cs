using i_ConVerificationSystem.JSON;
using i_ConVerificationSystem.Verification;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Xml.Linq;
using static i_ConVerificationSystem.Forms.Base.LandXMLStdInformation;
using static i_ConVerificationSystem.Forms.WidthComposition.WCInputParameter;

namespace i_ConVerificationSystem.Forms.WidthComposition
{
    /// <summary>
    /// WCInputParameter.xaml の相互作用ロジック
    /// </summary>
    public partial class WCInputParameter : UserControl
    {
        public XElement selXAlignment { get; set; }
        public string alignmentName
        {
            get { return XMLLoader.Instance.GetAlignmentName(selXAlignment); }
        }

        /// <summary>
        /// 編集可不可
        /// </summary>
        [Browsable(true)]
        public bool IsEditable
        {
            get { return c.IsEnabled; }
            set { c.IsEnabled = value; }
        }

        public class WCInputParams
        {
            ////道路規格・等級
            //public Tuple<int, int> RtT;
            ////設計速度
            //public int Ds;
            //基本情報
            public StdInformation std;
            //計画交通量
            public long Ptv;
            //大型車混入率
            public decimal Lvmr;
            //自転車ネットワーク計画の有無
            public bool IsBnp;
            //第3種第1級道路に接続する第4種第1級道路
            public bool IsConnect41to31;
            //主要幹線に該当する第3種第2級または第4種第1級の道路
            public bool IsConnect41to32;
            //普通道路／小型道路
            public RoadSideStandard Rss;
            //暫定計画の有無
            public bool Ispp;
            //自転車交通量(500台/日以上か)
            public bool Qcycle;
            //歩行者交通量(500人/日以上か)
            public bool Qpede;
            //路上施設の種類
            public StreetSideFacilitiesType Ssft;
            //道路の存する地域の地形
            public Topography Tg;
            //交差点の多い第4種道路
            public bool Islcp4;

            public WCInputParams(StdInformation s, long ptv, decimal lvmr, bool isBnp, bool isConnect41to31, bool isConnect41to32,
                RoadSideStandard rss, bool ispp, bool qcycle, bool qpede, StreetSideFacilitiesType ssft, Topography tg, bool islcp4)
            {
                std = s;
                Ptv = ptv;
                Lvmr = lvmr;
                IsBnp = isBnp;
                IsConnect41to31 = isConnect41to31;
                IsConnect41to32 = isConnect41to32;
                Rss = rss;
                Ispp = ispp;
                Qcycle = qcycle;
                Qpede = qpede;
                Ssft = ssft;
                Tg = tg;
                Islcp4 = islcp4;
            }
        }

        public enum RoadSideStandard
        {
            None = 0,
            NormalRoad,
            SmallRoad
        };

        public enum StreetSideFacilitiesType
        {
            None = 0,
            Footbridge,
            BenchShed,
            Trees,
            Bench,
            ETC
        };

        public enum Topography
        {
            None = 0,
            Plain,
            Mountainous,
        };

        public WCInputParameter()
        {
            InitializeComponent();
            DataContext = this;
        }

        public WCInputParams GetWCInputParams()
        {
            if (this.CanBeUsingThisComponent())
            {
                var retVal = new WCInputParams(sib.GetStdInformation(),
                               long.Parse(txtPtv.Text),
                               decimal.Parse(txtLvmr.Text),
                               rdbIsBnpTrue.IsChecked is true,
                               rdbIsConnect41to31True.IsChecked is true,
                               rdbIsConnect41to32True.IsChecked is true,
                               (RoadSideStandard)cmbRss.SelectedValue,
                               rdbIsppTrue.IsChecked is true,
                               rdbQcycleStdOverTrue.IsChecked is true,
                               rdbQpedeStdOverTrue.IsChecked is true,
                               (StreetSideFacilitiesType)cmbSsft.SelectedValue,
                               (Topography)cmbTg.SelectedValue,
                               rdbIsLcp4True.IsChecked is true);

                return retVal;
            }
            else
            {
                return null;
            }
        }

        public void SetWCInputParams(WCInputParams wcip)
        {
            sib.SetStdInformation(wcip.std);
            txtPtv.Text = wcip.Ptv.ToString();
            txtLvmr.Text = wcip.Lvmr.ToString();
            if (wcip.IsBnp) rdbIsBnpTrue.IsChecked = true;
            else rdbIsBnpFalse.IsChecked = true;
            if (wcip.IsConnect41to31) rdbIsConnect41to31True.IsChecked = true;
            else rdbIsConnect41to31False.IsChecked = true;
            if (wcip.IsConnect41to32) rdbIsConnect41to32True.IsChecked = true;
            else rdbIsConnect41to32False.IsChecked = true;
            cmbRss.SelectedValue = wcip.Rss;
            if (wcip.Ispp) rdbIsppTrue.IsChecked = true;
            else rdbIsppFalse.IsChecked = true;
            if (wcip.Qcycle) rdbQcycleStdOverTrue.IsChecked = true;
            else rdbQcycleStdOverFalse.IsChecked = true;
            if (wcip.Qpede) rdbQpedeStdOverTrue.IsChecked = true;
            else rdbQpedeStdOverFalse.IsChecked = true;
            cmbSsft.SelectedValue = wcip.Ssft;
            cmbTg.SelectedValue = wcip.Tg;
            if (wcip.Islcp4) rdbIsLcp4True.IsChecked = true;
            else rdbIsLcp4False.IsChecked = true;
        }

        private bool CanBeUsingThisComponent()
        {
            if (sib.txtType.Text == string.Empty || int.TryParse(sib.txtType.Text, out _) == false) return false;
            if (sib.txtClass.Text == string.Empty || int.TryParse(sib.txtClass.Text, out _) == false) return false;
            if (sib.txtDesignSpeed.Text == string.Empty || int.TryParse(sib.txtDesignSpeed.Text, out _) == false) return false;
            if (txtPtv.Text == string.Empty) return false;
            if (txtLvmr.Text == string.Empty) return false;
            if (cmbRss.SelectedIndex == -1) return false;
            if (cmbSsft.SelectedIndex == -1) return false;
            if (cmbTg.SelectedIndex == -1) return false;

            return true;
        }

        /// <summary>
        /// 条件値を保存
        /// </summary>
        public void SaveConditions()
        {
            var cp = sib.GetStdInformation();
            if (!(cp is null))
            {
                AppSettingsManager.Instance.GenerateAppSettingsForCommonSettings(alignmentName, cp);
            }

            var wcip = GetWCInputParams();

            if (!(wcip is null))
            {
                AppSettingsManager.Instance.GenerateAppSettingsForWCSettings(alignmentName, wcip);
            }
        }

        /// <summary>
        /// 条件値を読込
        /// </summary>
        public void LoadConditions()
        {
            try
            {
                SetDefaultSelection();

                var cp = AppSettingsManager.Instance.DeselializeForCommonSettings(alignmentName);
                sib.SetStdInformation(cp);

                var wcip = AppSettingsManager.Instance.DeselializeForWCSettings(alignmentName);
                SetWCInputParams(wcip);
            }
            catch (Exception)
            {
                //Nothing
            }
        }
        
        /// <summary>
        /// 初期値をセット（最終タブページ以外のラジオボタンがなぜか全てIsChecked=falseになるため）
        /// </summary>
        private void SetDefaultSelection()
        {
            rdbIsBnpFalse.IsChecked = true;
            rdbIsConnect41to31False.IsChecked = true;
            rdbIsConnect41to32False.IsChecked = true;
            cmbRss.SelectedIndex = 0;
            rdbIsppFalse.IsChecked = true;
            cmbSsft.SelectedIndex = 0;
            cmbTg.SelectedIndex = 0;
            rdbIsLcp4False.IsChecked = true;
            rdbQcycleStdOverFalse.IsChecked = true;
            rdbQpedeStdOverFalse.IsChecked = true;
        }

        private void txtPtv_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //整数値のみ許可
            e.Handled = !long.TryParse(e.Text, out _);
        }

        private void txtPtv_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                //整数値のみ許可
                e.Handled = !long.TryParse(Clipboard.GetText(), out _);
            }
        }

        private void txtLvmr_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                //実数値のみ許可
                e.Handled = !decimal.TryParse(Clipboard.GetText(), out _);
            }
        }

        private void txtLvmr_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //実数値のみ許可
            var regex = new Regex("[^0-9\\.]+");
            var text = e.Text;
            bool result = regex.IsMatch(text);

            var t = sender as TextBox;
            if (t != null && text == ".")
            {
                //既に小数点が入っているなら入れない
                result = t.Text.Contains(".");
            }

            e.Handled = result;
        }
    }

    public class RoadSideStandardToComboBox
    {
        public string _Label { get; set; }
        public RoadSideStandard _Value { get; set; }

    }

    public class StreetSideFacilitiesTypeToComboBox
    {
        public string _Label { get; set; }
        public StreetSideFacilitiesType _Value { get; set; }

    }

    public class TopographyToComboBox
    {
        public string _Label { get; set; }
        public Topography _Value { get; set; }

    }
}
