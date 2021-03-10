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

namespace i_ConVerificationSystem.Forms.Gradient
{
    /// <summary>
    /// OGVerificationResult.xaml の相互作用ロジック
    /// </summary>
    public partial class OGVerificationResult : UserControl
    {
        public ObservableCollection<OGVerificationResultItems> ListData { get; set; }

        public OGVerificationResult()
        {
            ListData = new ObservableCollection<OGVerificationResultItems>();

            InitializeComponent();

            DataContext = this;
        }

        public void RefreshDataGrid(List<OnesidedGradientVerificationResult> ogvrList)
        {
            ListData.Clear();

            foreach (var item in ogvrList)
            {
                ListData.Add(new OGVerificationResultItems(item.vrNum, true, item.beginPoint));
                ListData.Add(new OGVerificationResultItems(item.vrNum, false, item.endPoint));
            }
        }
    }
}
