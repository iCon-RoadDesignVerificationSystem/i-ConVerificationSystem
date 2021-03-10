using i_ConVerificationSystem.Forms.Settings.StdWidthComposition;
using i_ConVerificationSystem.JSON;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml.Linq;
using static i_ConVerificationSystem.Forms.Gradient.OGInputParameter;
using static i_ConVerificationSystem.Structs.OGExtensions;

namespace i_ConVerificationSystem.Forms.Settings
{
    /// <summary>
    /// SandBox.xaml の相互作用ロジック
    /// </summary>
    public partial class StdWidthCompositionWindow : Window
    {
        public XElement selXAlignment { get; set; }
        public string selAlignmentName { get; set; }
        private List<CrossSect_OGExtension> ogcsList { get; set; }
        private List<CrossSect_OGExtension> org_ogcsList { get; set; }
        public List<CrossSect_OGExtension> _ogcsList 
        {
            get { return ogcsList; } 
        }
        private bool _RequireScreenUpdate = false;

        public StdWidthCompositionWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ogmlv.CreateAlignmentTabs(selXAlignment);
            var ogip = AppSettingsManager.Instance.DeselializeForOGSettings(selAlignmentName);
            SetFHPosition(ogip.FHP);
        }

        private void CreateAlignmentTabs(bool isRetry)
        {
            ////タブを生成
            //if (ogcsList is null || ogcsList.Count == 0)
            //{
            //    //初期表示用
            //    ogcsList = XMLLoader.Instance.GetCrossSectsOG(selXAlignment, false);
            //}
            //ogcsList = AppSettingsManager.Instance.GetCrossSect_OGList(selAlignmentName, ogcsList, isRetry);
            //org_ogcsList = ogcsList;
            ////var p = new StdTabPages();
            ////p.ShowAlignmentTabPages(stdCs);
            //var p = new StdTabControl();
            //p.ShowAlignmentTabPages(ogcsList);
            //this.STP.Children.Add(p);

            //CreateTreeViewItems();
        }

        private void ClearAllTabs()
        {
            //Alignmentを再選択
            //一度なかったことにするが、条件値は記録されているため大丈夫なはず
            //ogcsList = null;
            //this.STP.Children.Clear();
            //CreateAlignmentTabs(true);
        }

        //private List<CrossSect_OGExtension> GetStdWidthComposition(List<CrossSect_OGExtension> ogcsList)
        //{
        //    var retList = new List<CrossSect_OGExtension>();
        //    org_ogcsList = ogcsList;

        //    foreach (var ogcs in ogcsList)
        //    {
        //        if (!(retList.Contains(ogcs, new CompareCrossSect_OGExtension()))) retList.Add(ogcs);
        //    }

        //    return retList;
        //}

        public bool ShowStdWidthCompositionWindow(Window owner)
        {
            Owner = owner;
            ShowDialog();
            return _RequireScreenUpdate;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ogmlv.SaveOGMapItems(GetFHPosition()))
            {
                _RequireScreenUpdate = true;
                this.Close();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Cancelボタン
            ogcsList = org_ogcsList;

            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //もとに戻すボタン
            if (MessageBox.Show("標準幅員を再抽選します。よろしいですか？\r\n※条件値は保持されます", "",MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            ClearAllTabs();
        }

        /// <summary>
        /// FH位置ラジオボタンの返答
        /// </summary>
        /// <returns></returns>
        private OGInputParameters.FHPosition GetFHPosition()
        {
            if (rdbFHCenter.IsChecked is true) return OGInputParameters.FHPosition.Center;
            if (rdbFHBoth.IsChecked is true) return OGInputParameters.FHPosition.Both;
            if (rdbFHLeft.IsChecked is true) return OGInputParameters.FHPosition.Left;
            if (rdbFHRight.IsChecked is true) return OGInputParameters.FHPosition.Right;
            //通常はここまでこないはず
            return OGInputParameters.FHPosition.Center;
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
                    rdbFHCenter.IsChecked = true;
                    break;
                case OGInputParameters.FHPosition.Both:
                    rdbFHBoth.IsChecked = true;
                    break;
                case OGInputParameters.FHPosition.Left:
                    rdbFHLeft.IsChecked = true;
                    break;
                case OGInputParameters.FHPosition.Right:
                    rdbFHRight.IsChecked = true;
                    break;
                default:
                    rdbFHCenter.IsChecked = true;
                    break;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("現在表示している断面の設定を他の断面に適用します。よろしいですか？", "", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            var fhp = GetFHPosition();
            ogmlv.ApplyStdSettingsToOtherTabs(fhp);
        }
    }
}
