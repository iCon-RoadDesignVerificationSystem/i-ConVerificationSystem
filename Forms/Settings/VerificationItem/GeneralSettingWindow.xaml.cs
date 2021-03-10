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
using System.Windows.Shapes;

namespace i_ConVerificationSystem.Forms.Settings.VerificationItem
{
    /// <summary>
    /// GeneralSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class GeneralSettingWindow : Window
    {
        private bool _RequireScreenUpdate = false;

        public GeneralSettingWindow(DispConditions disp)
        {
            AddHandler(GGConditions.CancelButtonClickEvent, new RoutedEventHandler(CancelButtonClick));
            AddHandler(TGConditions.CancelButtonClickEvent, new RoutedEventHandler(CancelButtonClick));
            AddHandler(OGConditions.CancelButtonClickEvent, new RoutedEventHandler(CancelButtonClick));
            AddHandler(WCConditions.CancelButtonClickEvent, new RoutedEventHandler(CancelButtonClick));
            AddHandler(GGConditions.DecisionButtonClickEvent, new RoutedEventHandler(DecisionButtonClick));
            AddHandler(TGConditions.DecisionButtonClickEvent, new RoutedEventHandler(DecisionButtonClick));
            AddHandler(OGConditions.DecisionButtonClickEvent, new RoutedEventHandler(DecisionButtonClick));
            AddHandler(WCConditions.DecisionButtonClickEvent, new RoutedEventHandler(DecisionButtonClick));

            InitializeComponent();

            switch (disp)
            {
                case DispConditions.GG:
                    var g = new GGConditions();
                    g.RefreshConditions();
                    this.Content = g;
                    break;
                case DispConditions.TG:
                    var t = new TGConditions();
                    t.RefreshConditions();
                    this.Content = t;
                    break;
                case DispConditions.OG:
                    var o = new OGConditions();
                    o.RefreshConditions();
                    this.Content = o;
                    break;
                case DispConditions.WC:
                    var w = new WCConditions();
                    w.RefreshConditions();
                    this.Content = w;
                    break;
                default:
                    this.Close();
                    break;
            }
        }

        public enum DispConditions
        {
            GG = 0,
            TG,
            OG,
            WC
        }

        /// <summary>
        /// 閉じるボタンのクリックイベントを処理してPageCloseEventを発生させる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecisionButtonClick(object sender, RoutedEventArgs e)
        {
            _RequireScreenUpdate = true;
            Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            _RequireScreenUpdate = false;
            Close();
        }

        /// <summary>
        /// 確定ボタンを押したか判定する画面表示メソッド
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public bool ShowGeneralScreen(Window owner, string title)
        {
            Owner = owner;
            Title = title;
            ShowDialog();
            return _RequireScreenUpdate;
        }

    }
}
