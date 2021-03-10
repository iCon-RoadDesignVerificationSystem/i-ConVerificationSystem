using i_ConVerificationSystem.Verification;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Xml.Linq;

namespace i_ConVerificationSystem.Forms.Base
{
    /// <summary>
    /// LandXMLStdInformation.xaml の相互作用ロジック
    /// </summary>
    public partial class LandXMLStdInformation : UserControl
    {
        public XElement selXAlignment { get; set; }

        public LandXMLStdInformation()
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

        public class StdInformation
        {
            public Tuple<int, int> rtT { get; set; }
            public int ds { get; set; }
            public decimal sltg { get; set; }
            public int interval { get; set; }
        }

        public ReactiveProperty<string> rType { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> rClass { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> ds { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> sltg { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> interval { get; set; } = new ReactiveProperty<string>();

        /// <summary>
        /// LandXMLから基本情報を取得
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetFromLandXMLButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(selXAlignment is null))
            {
                ClearStdInformation();

                var xRoadClass = XMLLoader.Instance.GetRoadClass(selXAlignment);
                var rtT = CommonMethod.GetRoadClass(xRoadClass);
                //txtType.Text = $"{rtT.Item1}";
                //txtClass.Text = $"{rtT.Item2}";
                rType.Value = $"{rtT.Item1}";
                rClass.Value = $"{rtT.Item2}";

                var xDesignSpeed = XMLLoader.Instance.GetDesignSpeed(selXAlignment);
                //txtDesignSpeed.Text = xDesignSpeed[0];
                ds.Value = xDesignSpeed[0];

                string nc = XMLLoader.Instance.GetNormalCrown(selXAlignment);
                sltg.Value = $"{nc}";

                int i = XMLLoader.Instance.GetInterval(selXAlignment);
                //txtInterval.Text = $"{i}";
                interval.Value = $"{i}";
            }
        }

        /// <summary>
        /// クリア(OneWayで通知するため)
        /// </summary>
        private void ClearStdInformation()
        {
            //txtType.Text = string.Empty;
            //txtClass.Text = string.Empty;
            //txtDesignSpeed.Text = string.Empty;
            rType.Value = string.Empty;
            rClass.Value = string.Empty;
            ds.Value = string.Empty;
            sltg.Value = string.Empty;
            interval.Value = string.Empty;
            //txtInterval.Text = string.Empty;
        }

        /// <summary>
        /// 基本情報が使えるか（取得済みか）
        /// </summary>
        /// <returns></returns>
        private bool CanUseStdInformation()
        {
            //if (txtType.Text == string.Empty || int.TryParse(txtType.Text, out _) == false) return false;
            //if (txtClass.Text == string.Empty || int.TryParse(txtClass.Text, out _) == false) return false;
            //if (txtDesignSpeed.Text == string.Empty || int.TryParse(txtDesignSpeed.Text, out _) == false) return false;
            if (rType.Value == string.Empty || int.TryParse(rType.Value, out _) == false) return false;
            if (rClass.Value == string.Empty || int.TryParse(rClass.Value, out _) == false) return false;
            if (ds.Value == string.Empty || int.TryParse(ds.Value, out _) == false) return false;
            return true;
        }

        /// <summary>
        /// 基本情報取得
        /// </summary>
        /// <returns></returns>
        public StdInformation GetStdInformation()
        {
            if (CanUseStdInformation() == false) return null;

            var retVal = new StdInformation();
            //retVal.rtT = new Tuple<int, int>(int.Parse(txtType.Text), int.Parse(txtClass.Text));
            //retVal.ds = int.Parse(txtDesignSpeed.Text);
            retVal.rtT = new Tuple<int, int>(int.Parse(rType.Value), int.Parse(rClass.Value));
            retVal.ds = int.Parse(ds.Value);
            retVal.sltg = decimal.TryParse(sltg.Value, out _) ?
                                  decimal.Parse(sltg.Value) :
                                  0.0M;
            //retVal.interval = int.Parse(txtInterval.Text);
            retVal.interval = int.Parse(interval.Value);

            return retVal;
        }

        /// <summary>
        /// 基本情報セット
        /// </summary>
        /// <param name="std"></param>
        public void SetStdInformation(StdInformation std)
        {
            //道路情報が取れていなければ無視
            if (std is null || std.rtT is null) return;
            rType.Value = $"{std.rtT.Item1}";
            rClass.Value = $"{std.rtT.Item2}";
            ds.Value = $"{std.ds}";
            sltg.Value = $"{std.sltg}";
            interval.Value = $"{std.interval}";
        }
    }
}
