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
using static i_ConVerificationSystem.Forms.Base.LandXMLStdInformation;

namespace i_ConVerificationSystem.Forms.Gradient
{
    /// <summary>
    /// GGInputParameter.xaml の相互作用ロジック
    /// </summary>
    public partial class GGInputParameter : UserControl
    {
        public string alignmentName { get; set; }

        public GGInputParameter()
        {
            InitializeComponent();
            DataContext = this;

            txtBpn.GotFocus += (sender, e) => this.Dispatcher.InvokeAsync(() => { Task.Delay(0); ((TextBox)sender).SelectAll(); });
            txtBal.GotFocus += (sender, e) => this.Dispatcher.InvokeAsync(() => { Task.Delay(0); ((TextBox)sender).SelectAll(); });
            txtEpn.GotFocus += (sender, e) => this.Dispatcher.InvokeAsync(() => { Task.Delay(0); ((TextBox)sender).SelectAll(); });
            txtEal.GotFocus += (sender, e) => this.Dispatcher.InvokeAsync(() => { Task.Delay(0); ((TextBox)sender).SelectAll(); });
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

        public class GGInputParameters
        {
            public StdInformation si { get; set; }
            public int bpn { get; set; }
            public decimal bAddLen { get; set; }
            public int epn { get; set; }
            public decimal eAddLen { get; set; }
        }

        /// <summary>
        /// 緩勾配区間長チェックをしてよいか
        /// </summary>
        /// <returns></returns>
        private bool CanUseGGInputParameters()
        {
            if (sib.txtType.Text == string.Empty || int.TryParse(sib.txtType.Text, out _) == false) return false;
            if (sib.txtClass.Text == string.Empty || int.TryParse(sib.txtClass.Text, out _) == false) return false;
            if (sib.txtDesignSpeed.Text == string.Empty || int.TryParse(sib.txtDesignSpeed.Text, out _) == false) return false;
            if (txtBpn.Text == string.Empty || int.TryParse(txtBpn.Text, out _) == false) return false;
            if (txtBal.Text == string.Empty || decimal.TryParse(txtBal.Text, out _) == false) return false;
            if (txtEpn.Text == string.Empty || int.TryParse(txtEpn.Text, out _) == false) return false;
            if (txtEal.Text == string.Empty || decimal.TryParse(txtEal.Text, out _) == false) return false;

            return true;
        }

        /// <summary>
        /// 緩勾配の条件値
        /// </summary>
        /// <returns></returns>
        public GGInputParameters GetGGInputParameters()
        {
            if (CanUseGGInputParameters() == false) return null;

            var retVal = new GGInputParameters();
            retVal.si = sib.GetStdInformation();
            retVal.bpn = int.Parse(txtBpn.Text);
            retVal.bAddLen = decimal.Parse(txtBal.Text);
            retVal.epn = int.Parse(txtEpn.Text);
            retVal.eAddLen = decimal.Parse(txtEal.Text);
            return retVal;
        }

        /// <summary>
        /// 緩勾配の条件値ロード
        /// </summary>
        /// <param name="ogip"></param>
        private void SetGGInputParameters(GGInputParameters ggip)
        {
            //共通情報のセット
            sib.SetStdInformation(ggip.si);

            //入力条件のセット
            txtBpn.Text = ggip.bpn.ToString();
            txtBal.Text = ggip.bAddLen.ToString();
            txtEpn.Text = ggip.epn.ToString();
            txtEal.Text = ggip.eAddLen.ToString();
        }

        /// <summary>
        /// 基準値取得ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetStdValueButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanUseGGInputParameters() == false) return;
            if (CommonMethod.CanBeVerifyGG(int.Parse(sib.rType.Value), int.Parse(sib.rClass.Value)) == false) return;

            var lgs = CommonMethod.GetLgs(int.Parse(sib.txtType.Text), int.Parse(sib.txtClass.Text));
            txtLgs1.Text = $"L={lgs}m";
            txtXbpsl.Text = $"No.{txtBpn.Text}+{txtBal.Text}";
            txtXbpslgs.Text = CommonMethod.GetXpslgsPositionString(int.Parse(txtBpn.Text), decimal.Parse(txtBal.Text), int.Parse(sib.txtInterval.Text), lgs, true);
            txtLgs2.Text = $"L={lgs}m";
            txtXepsl.Text = $"No.{txtEpn.Text}+{txtEal.Text}";
            txtXepslgs.Text = CommonMethod.GetXpslgsPositionString(int.Parse(txtEpn.Text), decimal.Parse(txtEal.Text), int.Parse(sib.txtInterval.Text), lgs, false);
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

            var ggip = GetGGInputParameters();

            if (!(ggip is null))
            {
                AppSettingsManager.Instance.GenerateAppSettingsForGGSettings(alignmentName, ggip);
            }
        }

        /// <summary>
        /// 条件値を読込
        /// </summary>
        public void LoadConditions()
        {
            try
            {
                var ggip = AppSettingsManager.Instance.DeselializeForGGSettings(alignmentName);
                SetGGInputParameters(ggip);
            }
            catch (Exception)
            {
                //Nothing
            }
        }

        private void txtBpn_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                //整数値のみ許可
                e.Handled = !int.TryParse(Clipboard.GetText(), out _);
            }
        }

        private void txtBpn_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //整数値のみ許可
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void txtBal_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                //実数値のみ許可
                e.Handled = !decimal.TryParse(Clipboard.GetText(), out _);
            }
        }

        private void txtBal_PreviewTextInput(object sender, TextCompositionEventArgs e)
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

        private void txtEpn_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                //整数値のみ許可
                e.Handled = !int.TryParse(Clipboard.GetText(), out _);
            }
        }

        private void txtEpn_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //整数値のみ許可
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void txtEal_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                //実数値のみ許可
                e.Handled = !decimal.TryParse(Clipboard.GetText(), out _);
            }
        }

        private void txtEal_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
}
