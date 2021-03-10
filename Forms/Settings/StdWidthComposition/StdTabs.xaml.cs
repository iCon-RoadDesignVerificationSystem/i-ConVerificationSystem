using i_ConVerificationSystem.Forms.Gradient;
using i_ConVerificationSystem.Forms.Settings.Base;
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
using static i_ConVerificationSystem.Forms.Gradient.OGInputParameter;
using static i_ConVerificationSystem.Structs.OGExtensions;

namespace i_ConVerificationSystem.Forms.Settings.StdWidthComposition
{
    /// <summary>
    /// StdTabs.xaml の相互作用ロジック
    /// </summary>
    public partial class StdTabs : UserControl
    {
        public StdTabs()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 編集可不可
        /// </summary>
        [Browsable(true)]
        public bool IsEditable { get; set; } = true;

        /// <summary>
        /// リフレッシュ
        /// </summary>
        public void RefreshConditions(List<CrossSect_OGExtension> ogcsList)
        {
            tcAlignments.Items.Clear();

            foreach (var ogcs in ogcsList)
            {
                var tp = new TabItem_Extensions();
                var ogMap = new OGMap();
                ogMap.IsEditable = IsEditable;
                ogMap.DrawWC(ogcs);
                tp.Header = ogcs.ToString();
                tp.Content = ogMap;
                tcAlignments.Items.Add(tp);
            }

            tcAlignments.SelectedIndex = 0;
        }

        public void SaveConditions()
        {
            foreach (var item in tcAlignments.Items)
            {
                var tp = item as TabItem_Extensions;
                if (!(tp is null))
                {
                    var ogMap = tp.Content as OGMap;
                    if (!(ogMap is null))
                    {
                        
                    }
                }
            }
        }

        /// <summary>
        /// 表示されているStdTabItemのCSを取得
        /// </summary>
        /// <returns></returns>
        public List<CrossSect_OGExtension> GetCrossSect_OGExtensionList()
        {
            var retVal = new List<CrossSect_OGExtension>();

            foreach (var item in tcAlignments.Items)
            {
                var tp = item as TabItem_Extensions;
                if (!(tp is null))
                {
                    var ogMap = tp.Content as OGMap;
                    if (!(ogMap is null))
                    {
                        var cs = ogMap.GetCrossSect_Extension();
                        if (!(cs is null))
                        {
                            retVal.Add(cs);
                        }
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        /// 表示している断面の設定を他の断面に適用する
        /// </summary>
        /// <returns></returns>
        public List<CrossSect_OGExtension> ApplyStdSettingsToOtherTabs(OGInputParameters.FHPosition fhp)
        {
            var retList = new List<CrossSect_OGExtension>();

            var currentItem = (TabItem_Extensions)tcAlignments.SelectedItem;
            var currentOGMap = (OGMap)currentItem.Content;
            var currentCs = currentOGMap.GetCrossSect_Extension();
            foreach (var item in tcAlignments.Items)
            {
                var tp = item as TabItem_Extensions;
                if (!(tp is null))
                {
                    var ogMap = tp.Content as OGMap;
                    if (!(ogMap is null) && ogMap != currentOGMap)
                    {
                        var isNotApplied = ogMap.ApplyStdSettingsFromOtherSetting(currentCs, fhp);
                        if (isNotApplied)
                        {
                            retList.Add(ogMap.GetCrossSect_Extension());
                        }
                    }
                }
            }

            return retList;
        }

        /// <summary>
        /// 指定タブを開く
        /// </summary>
        /// <param name="tabHeaderString"></param>
        public void ShowAnyTab(string tabHeaderString)
        {
            foreach (var item in tcAlignments.Items)
            {
                var tp = item as TabItem_Extensions;
                if (!(tp is null) && tp.Header.ToString() == tabHeaderString)
                {
                    tcAlignments.SelectedItem = tp;
                    break;
                }
            }
        }
    }
}
