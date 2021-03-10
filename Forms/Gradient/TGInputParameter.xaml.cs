using i_ConVerificationSystem.Forms.Base;
using i_ConVerificationSystem.JSON;
using Reactive.Bindings;
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
using static i_ConVerificationSystem.Forms.Base.LandXMLStdInformation;
using static i_ConVerificationSystem.Forms.Gradient.TGInputParameter;

namespace i_ConVerificationSystem.Forms.Gradient
{
    /// <summary>
    /// TGInputParameter.xaml の相互作用ロジック
    /// </summary>
    public partial class TGInputParameter : UserControl
    {
        public string alignmentName { get; set; }

        public TGInputParameter()
        {
            InitializeComponent();
            DataContext = this;
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

        public class TGInputParameters
        {
            public StdInformation si { get; set; }
            public RoadPavingType RoadPavingType { get; set; }
            public SidewalkPavingType SidewalkPavingType { get; set; }
            public decimal StraightLineTransverseGradient { get; set; }
            public bool IsBarrierFree { get; set; }
            public bool IsManyTraffic { get; set; }
            public bool IsSnowyColdArea { get; set; }
            public bool IsSnowyColdOtherArea { get; set; }
        }

        /// <summary>
        /// 条件値が使えるか
        /// </summary>
        /// <returns></returns>
        private bool CanUseTGInputParameters()
        {
            if (cmbRoadPavingType.SelectedIndex == -1) return false;
            if (cmbSidewalkPavingType.SelectedIndex == -1) return false;
            return true;
        }

        /// <summary>
        /// 入力値を取得
        /// </summary>
        /// <returns></returns>
        public TGInputParameters GetTGInputParameters()
        {
            var retVal = new TGInputParameters();
            retVal.si = sig.GetStdInformation();
            if (retVal.si is null || CanUseTGInputParameters() == false) return null;

            retVal.RoadPavingType = (RoadPavingType)cmbRoadPavingType.SelectedValue;
            retVal.SidewalkPavingType = (SidewalkPavingType)cmbSidewalkPavingType.SelectedValue;
            retVal.StraightLineTransverseGradient = decimal.TryParse(txtStraightLineTransverseGradient.Text, out _) ?
                                                            decimal.Parse(txtStraightLineTransverseGradient.Text) :
                                                            retVal.si.sltg;
            retVal.IsBarrierFree = rdbIsBarrierFreeTrue.IsChecked is true;
            retVal.IsManyTraffic = rdbIsManyTrafficTrue.IsChecked is true;
            retVal.IsSnowyColdArea = rdbIsSnowyColdAreaTrue.IsChecked is true;
            retVal.IsSnowyColdOtherArea = rdbIsSnowyColdOtherAreaTrue.IsChecked is true;

            return retVal;
        }

        /// <summary>
        /// 入力値のロード
        /// </summary>
        /// <param name="tgip"></param>
        private void SetTGInputParameters(TGInputParameters tgip)
        {
            //共通情報のセット
            sig.SetStdInformation(tgip.si);

            //入力条件のセット
            cmbRoadPavingType.SelectedValue = tgip.RoadPavingType;
            cmbSidewalkPavingType.SelectedValue = tgip.SidewalkPavingType;
            if (tgip.StraightLineTransverseGradient != decimal.Zero)
            {
                txtStraightLineTransverseGradient.Text = tgip.StraightLineTransverseGradient.ToString();
            }
            ChangeIsReadOnlySltg();

            if (tgip.IsBarrierFree) { rdbIsBarrierFreeTrue.IsChecked = true; } else { rdbIsBarrierFreeFalse.IsChecked = true; }
            if (tgip.IsManyTraffic) { rdbIsManyTrafficTrue.IsChecked = true; } else { rdbIsManyTrafficFalse.IsChecked = true; }
            if (tgip.IsSnowyColdArea) { rdbIsSnowyColdAreaTrue.IsChecked = true; } else { rdbIsSnowyColdAreaFalse.IsChecked = true; }
            if (tgip.IsSnowyColdOtherArea) { rdbIsSnowyColdOtherAreaTrue.IsChecked = true; } else { rdbIsSnowyColdOtherAreaFalse.IsChecked = true; }
        }

        public enum RoadPavingType
        {
            //セメントコンクリートまたはアスファルトコンクリート舗装
            CeCoOrAsCoRoad = 0,
            //その他の路面
            OtherType
        }

        //public List<Tuple<RoadPavingType, string>> RoadPavingTypeList { get { return GetRoadPavingTypeList(); } }

        //private List<Tuple<RoadPavingType, string>> GetRoadPavingTypeList()
        //{
        //    return roadPavingTypeList;
        //}
        //private readonly List<Tuple<RoadPavingType, string>> roadPavingTypeList = new List<Tuple<RoadPavingType, string>>()
        //{
        //    new Tuple<RoadPavingType, string> (RoadPavingType.CeCoOrAsCoRoad, "アスファルトCo舗装またはAsCo舗装等"),
        //    new Tuple<RoadPavingType, string> (RoadPavingType.OtherType, "その他の路面"),
        //};

        public enum SidewalkPavingType
        {
            //透水性舗装
            WaterPermeableType = 0,
            //その他の路面
            OtherType
        }

        //public List<Tuple<SidewalkPavingType, string>> SidewalkPavingTypeList { get { return GetSidewalkPavingTypeList(); } }
        //private List<Tuple<SidewalkPavingType, string>> GetSidewalkPavingTypeList()
        //{
        //    return sidewalkPavingTypeList;
        //}
        //private readonly List<Tuple<SidewalkPavingType, string>> sidewalkPavingTypeList = new List<Tuple<SidewalkPavingType, string>>()
        //{
        //    new Tuple<SidewalkPavingType, string> (SidewalkPavingType.WaterPermeableType, "透水性舗装"),
        //    new Tuple<SidewalkPavingType, string> (SidewalkPavingType.OtherType, "その他の路面"),
        //};

        /// <summary>
        /// 条件値を保存
        /// </summary>
        public void SaveConditions()
        {
            var cp = sig.GetStdInformation();
            if (!(cp is null))
            {
                AppSettingsManager.Instance.GenerateAppSettingsForCommonSettings(alignmentName, cp);
            }

            var tgip = GetTGInputParameters();

            if (!(tgip is null))
            {
                AppSettingsManager.Instance.GenerateAppSettingsForTGSettings(alignmentName, tgip);
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

                var tgip = AppSettingsManager.Instance.DeselializeForTGSettings(alignmentName);
                SetTGInputParameters(tgip);
            }
            catch (Exception)
            {
                //Nothing
            }
        }


        private void SetDefaultSelection()
        {
            cmbRoadPavingType.SelectedIndex = 0;
            cmbSidewalkPavingType.SelectedIndex = 0;
            rdbIsBarrierFreeFalse.IsChecked = true;
            rdbIsManyTrafficFalse.IsChecked = true;
            rdbIsSnowyColdAreaFalse.IsChecked = true;
            rdbIsSnowyColdOtherAreaFalse.IsChecked = true;
        }

        private void ChangeIsReadOnlySltg()
        {
            var sltgVal = decimal.TryParse(sig.sltg.Value, out _) ?
                                decimal.Parse(sig.sltg.Value) :
                                0.0M;
            if (sltgVal == decimal.Zero)
            {
                txtStraightLineTransverseGradient.IsReadOnly = false;
            }
            else
            {
                txtStraightLineTransverseGradient.IsReadOnly = true;
            }
        }

        private void txtStraightLineTransverseGradient_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtBox = sender as TextBox;
            if (txtBox == null) return;

            if (txtBox.IsFocused)
            {
                return;
            }
            else
            {
                ChangeIsReadOnlySltg();
            }
        }

        /// <summary>
        /// ロストフォーカスを検知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtStraightLineTransverseGradient_LostFocus(object sender, RoutedEventArgs e)
        {
            ChangeIsReadOnlySltg();
        }

        private void txtStraightLineTransverseGradient_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                //実数値のみ許可
                e.Handled = !decimal.TryParse(Clipboard.GetText(), out _);
            }
        }

        private void txtStraightLineTransverseGradient_PreviewTextInput(object sender, TextCompositionEventArgs e)
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

    public class RoadPavingTypeToComboBox
    {
        public string _Label { get; set; }
        public RoadPavingType _Value { get; set; }
    }

    public class SidewalkPavingTypeToComboBox
    {
        public string _Label { get; set; }
        public SidewalkPavingType _Value { get; set; }
    }
}
