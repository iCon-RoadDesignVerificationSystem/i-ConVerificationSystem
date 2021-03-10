using i_ConVerificationSystem.JSON;
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
using static i_ConVerificationSystem.Forms.Gradient.OGInputParameter;
using static i_ConVerificationSystem.Structs.OGExtensions;

namespace i_ConVerificationSystem.Forms.Settings.StdWidthComposition
{
    /// <summary>
    /// OGMapListView.xaml の相互作用ロジック
    /// </summary>
    public partial class OGMapListView : UserControl
    {
        public OGMapListView()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 編集可不可
        /// </summary>
        [Browsable(true)]
        public bool IsEditable {
            get
            {
                return p.IsEditable;
            }
            set
            {
                p.IsEditable = value;
            }
        }

        private XElement selXAlignment { get; set; }
        private string selAlignmentName {
            get
            {
                return XMLLoader.Instance.GetAlignmentName(selXAlignment);
            }
        }
        public ReactiveProperty<List<ListViewItemOGCS>> ogcsList { get; private set; } = new ReactiveProperty<List<ListViewItemOGCS>>();

        public sealed class ListViewItemOGCS {
            public CrossSect_OGExtension ogcs { get; set; } = new CrossSect_OGExtension();
            public ReactiveProperty<bool> IsNotApplied { get; set; } = new ReactiveProperty<bool>(false);
            public ListViewItemOGCS(CrossSect_OGExtension o)
            {
                ogcs = o;
                IsNotApplied.Value = false;
            }
            public override string ToString()
            {
                return ogcs.ToString();
            }
        };

        private void CreateListViewItem(List<CrossSect_OGExtension> oList)
        {
            ogcsList.Value = (from T in oList
                              select new ListViewItemOGCS(T)).ToList();
        }

        public List<CrossSect_OGExtension> GetListViewItem()
        {
            return (from T in ogcsList.Value
                    select T.ogcs).ToList();
        }

        private void UpdateListViewItem(List<CrossSect_OGExtension> oList)
        {
            foreach (var item in ogcsList.Value)
            {
                var t = (from T in oList
                         where T == item.ogcs
                         select T).Any();
                item.IsNotApplied.Value = t;
            }
        }

        public void CreateAlignmentTabs(XElement selXAlignment)
        {
            this.selXAlignment = selXAlignment;

            //タブを生成
            if (ogcsList.Value is null || ogcsList.Value.Count == 0)
            {
                //初期表示用
                //ogcsList.Value = XMLLoader.Instance.GetCrossSectsOG(selXAlignment, false);
                CreateListViewItem(XMLLoader.Instance.GetCrossSectsOG(selXAlignment, false));
            }
            //ogcsList.Value = AppSettingsManager.Instance.GetCrossSect_OGList(selAlignmentName, ogcsList.Value, false);
            CreateListViewItem(AppSettingsManager.Instance.GetCrossSect_OGList(selAlignmentName, GetListViewItem(), false));
            //p.RefreshConditions(ogcsList.Value);
            p.RefreshConditions(GetListViewItem());
        }

        public bool SaveOGMapItems(OGInputParameters.FHPosition fhp)
        {
            //確定ボタン
            //ogcsList.Value = p.GetCrossSect_OGExtensionList();
            var ogcsList = p.GetCrossSect_OGExtensionList();
            var validateNGList = new List<string>();
            foreach (var ogcs in ogcsList)
            {
                if (ogcs.ValidateForSave(fhp) == false)
                {
                    validateNGList.Add(ogcs.ToString());
                }
            }
            if (validateNGList.Any())
            {
                var ret = MessageBox.Show($"不適切なFH位置または車道縁位置が指定されています。{Environment.NewLine}以下の横断面は照査結果が得られない可能性がありますが、保存しますか？{Environment.NewLine}※10件まで表示しています。{Environment.NewLine}{string.Join(Environment.NewLine, validateNGList.Take(10))}",
                    "",
                    MessageBoxButton.YesNo);
                if (ret != MessageBoxResult.Yes)
                {
                    return false;
                }
            }

            CreateListViewItem(ogcsList);
            //AppSettingsManager.Instance.GenerateAppSettingsForStd(selAlignmentName, ogcsList.Value, fhp);
            AppSettingsManager.Instance.GenerateAppSettingsForStd(selAlignmentName, GetListViewItem(), fhp);
            return true;
        }

        public void RefreshOGMap(List<CrossSect_OGExtension> ogcsList)
        {
            //this.ogcsList.Value = ogcsList;
            //p.RefreshConditions(this.ogcsList.Value);
            CreateListViewItem(ogcsList);
            p.RefreshConditions(GetListViewItem());
        }

        private void lstStdCs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstStdCs.SelectedItem == null) return;

            //p.ShowAnyTab(((CrossSect_OGExtension)lstStdCs.SelectedItem).ToString());
            var selVal = (ListViewItemOGCS)lstStdCs.SelectedItem;
            p.ShowAnyTab(selVal.ogcs.ToString());
        }

        public void ApplyStdSettingsToOtherTabs(OGInputParameters.FHPosition fhp)
        {
            UpdateListViewItem(p.ApplyStdSettingsToOtherTabs(fhp));
            //ここで全部に適用できなかった横断面のIsNotAppliedをTrueに更新。
        }
    }
}
