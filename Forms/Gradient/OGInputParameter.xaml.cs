using i_ConVerificationSystem.Forms.Settings.Base;
using i_ConVerificationSystem.Forms.Settings.StdWidthComposition;
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

namespace i_ConVerificationSystem.Forms.Gradient
{
    /// <summary>
    /// OGInputParameter.xaml の相互作用ロジック
    /// </summary>
    public partial class OGInputParameter : UserControl
    {
        public string alignmentName { get; set; }

        public OGInputParameter()
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

        public ReactiveProperty<bool> FHCenter { get; set; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> FHBoth { get; set; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> FHLeft { get; set; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> FHRight { get; set; } = new ReactiveProperty<bool>();

        /// <summary>
        /// リフレッシュ
        /// </summary>
        public void RefreshConditions()
        {
            var selXAlignment = XMLLoader.Instance.GetAlignmentFromName(alignmentName);
            ogmlv.CreateAlignmentTabs(selXAlignment);
        }

        public class OGInputParameters
        {
            public enum FHPosition
            {
                Center,
                Both,
                Left,
                Right
            }

            public StdInformation si { get; set; }
            public decimal StraightLineTransverseGradient { get; set; }
            public FHPosition FHP { get; set; }
        }

        /// <summary>
        /// 片勾配すりつけの条件値
        /// </summary>
        /// <returns></returns>
        public OGInputParameters GetOGInputParameters()
        {
            var retVal = new OGInputParameters();
            retVal.si = sig.GetStdInformation();

            if (retVal.si is null) return null;

            retVal.StraightLineTransverseGradient = decimal.TryParse(txtStraightLineTransverseGradient.Text, out _) ? 
                                                            decimal.Parse(txtStraightLineTransverseGradient.Text) : 
                                                            retVal.si.sltg;
            retVal.FHP = GetFHPosition();
            return retVal;
        }

        /// <summary>
        /// FH位置ラジオボタンの返答
        /// </summary>
        /// <returns></returns>
        private OGInputParameters.FHPosition GetFHPosition()
        {
            //if (rdbFHCenter.IsChecked is true) return OGInputParameters.FHPosition.Center;
            //if (rdbFHBoth.IsChecked is true) return OGInputParameters.FHPosition.Both;
            //if (rdbFHLeft.IsChecked is true) return OGInputParameters.FHPosition.Left;
            //if (rdbFHRight.IsChecked is true) return OGInputParameters.FHPosition.Right;
            if (FHCenter.Value) return OGInputParameters.FHPosition.Center;
            if (FHBoth.Value) return OGInputParameters.FHPosition.Both;
            if (FHLeft.Value) return OGInputParameters.FHPosition.Left;
            if (FHRight.Value) return OGInputParameters.FHPosition.Right;
            //通常はここまでこないはず
            return OGInputParameters.FHPosition.Center;
        }

        /// <summary>
        /// 片勾配すりつけの条件値ロード
        /// </summary>
        /// <param name="ogip"></param>
        private void SetOGInputParameters(OGInputParameters ogip)
        {
            //共通情報のセット
            sig.SetStdInformation(ogip.si);

            //入力条件のセット
            if (ogip.StraightLineTransverseGradient != decimal.Zero)
            {
                txtStraightLineTransverseGradient.Text = ogip.StraightLineTransverseGradient.ToString();
            }
            ChangeIsReadOnlySltg();
            SetFHPosition(ogip.FHP);
        }

        /// <summary>
        /// FH位置ラジオボタンのロード
        /// </summary>
        /// <param name="fhp"></param>
        private void SetFHPosition(OGInputParameters.FHPosition fhp)
        {
            switch (fhp)
            {
                case OGInputParameters.FHPosition.Center:
                    FHCenter.Value = true;
                    break;
                case OGInputParameters.FHPosition.Both:
                    FHBoth.Value = true;
                    break;
                case OGInputParameters.FHPosition.Left:
                    FHLeft.Value = true;
                    break;
                case OGInputParameters.FHPosition.Right:
                    FHRight.Value = true;
                    break;
                default:
                    FHCenter.Value = true;
                    break;
            }
        }

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

            var ogip = GetOGInputParameters();

            if (!(ogip is null))
            {
                AppSettingsManager.Instance.GenerateAppSettingsForOGSettings(alignmentName, ogip);
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

                var ogip = AppSettingsManager.Instance.DeselializeForOGSettings(alignmentName);
                SetOGInputParameters(ogip);
            }
            catch (Exception)
            {
                //Nothing
            }
        }


        private void SetDefaultSelection()
        {
            FHCenter.Value = true;
        }

        private void txtStraightLineTransverseGradient_LostFocus(object sender, RoutedEventArgs e)
        {
            ChangeIsReadOnlySltg();
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
}
