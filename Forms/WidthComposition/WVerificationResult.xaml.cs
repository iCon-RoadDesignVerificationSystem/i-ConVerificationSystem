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
using static i_ConVerificationSystem.Structs.WVerificationResultItems;

namespace i_ConVerificationSystem.Forms.WidthComposition
{
    /// <summary>
    /// WVerificationResult.xaml の相互作用ロジック
    /// </summary>
    public partial class WVerificationResult : UserControl
    {
        private string name { get; set; }
        public string dispName
        {
            get
            {
                return $"横断形状<{name}>";
            }
        }
        public ObservableCollection<WVRIViewItem> ListData { get; set; }

        public WVerificationResult(string csName)
        {
            ListData = new ObservableCollection<WVRIViewItem>();
            name = csName;

            InitializeComponent();

            DataContext = this;
        }

        public void RefreshDataGrid(WVerificationResultItems wvri)
        {
            ListData.Clear();

            foreach (var item in wvri.GetViewItemList())
            {
                ListData.Add(item);
            }
        }
    }
}
