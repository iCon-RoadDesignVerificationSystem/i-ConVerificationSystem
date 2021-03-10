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
using static i_ConVerificationSystem.Structs.WCVerificationResultItems;

namespace i_ConVerificationSystem.Forms.WidthComposition
{
    /// <summary>
    /// WCVerificationResult.xaml の相互作用ロジック
    /// </summary>
    public partial class WCVerificationResult : UserControl
    {
        public ObservableCollection<WCVRIViewItem> ListData { get; set; }
        public WCVerificationResult()
        {
            ListData = new ObservableCollection<WCVRIViewItem>();

            InitializeComponent();

            DataContext = this;
        }

        public void RefreshDataGrid(WCVerificationResultItems wcvri)
        {
            ListData.Clear();

            foreach (var item in wcvri.GetViewItemList())
            {
                ListData.Add(item);
            }
        }
    }
}
