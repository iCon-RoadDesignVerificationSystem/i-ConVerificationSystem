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
using i_ConVerificationSystem.Forms.Base;
using i_ConVerificationSystem.Structs;
using static i_ConVerificationSystem.Structs.GGVerificationResultItem;

namespace i_ConVerificationSystem.Forms.Gradient
{
    /// <summary>
    /// GGVerificationResult.xaml の相互作用ロジック
    /// </summary>
    public partial class GGVerificationResult : UserControl
    {
        public ObservableCollection<GGVerificationResultItems> ListData { get; set; }

        public GGVerificationResult()
        {
            ListData = new ObservableCollection<GGVerificationResultItems>();

            InitializeComponent();

            DataContext = this;
        }

        public void RefreshDataGrid()
        {
            ListData.Clear();

            foreach (var ggvr in GGVerificationResultItem.Instance.ggvrPairs)
            {
                ListData.Add(ggvr.Value);
            }
        }

    }
}
