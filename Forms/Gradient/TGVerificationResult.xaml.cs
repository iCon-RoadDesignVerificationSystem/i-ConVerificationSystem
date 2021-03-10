using i_ConVerificationSystem.Structs;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static i_ConVerificationSystem.Structs.TGVerificationResultItem;

namespace i_ConVerificationSystem.Forms.Gradient
{
    /// <summary>
    /// TGVerificationResult.xaml の相互作用ロジック
    /// </summary>
    public partial class TGVerificationResult : UserControl
    {
        public ObservableCollection<TG_VerificationResult> StListData { get; set; }
        public ObservableCollection<TG_VerificationResult> SiListData { get; set; }
        public ObservableCollection<TG_MOG_VerificationResult> MOGListData { get; set; }
        public ObservableCollection<TG_RSG_VerificationResult> RSGListData { get; set; }

        public TGVerificationResult()
        {
            StListData = new ObservableCollection<TG_VerificationResult>();
            SiListData = new ObservableCollection<TG_VerificationResult>();
            MOGListData = new ObservableCollection<TG_MOG_VerificationResult>();
            RSGListData = new ObservableCollection<TG_RSG_VerificationResult>();

            InitializeComponent();

            DataContext = this;
        }

        public void RefreshDataGrid(TGVerificationResultItems tgvr)
        {
            StListData.Clear();
            SiListData.Clear();
            MOGListData.Clear();
            RSGListData.Clear();

            StListData.Add(tgvr.VR_StraightLineTransverseGradient);
            foreach (var item in tgvr.VR_SidewalkCrownList)
            {
                SiListData.Add(item);
            }
            foreach (var item in tgvr.VR_MaximumOnesidedGradientList)
            {
                MOGListData.Add(item);
            }
            foreach (var item in tgvr.VR_RoadShoulderGradientList)
            {
                RSGListData.Add(item);
            }
        }
    }
}
