using i_ConVerificationSystem.Forms.Print;
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

namespace i_ConVerificationSystem.Forms.Print
{
    /// <summary>
    /// PrintForm.xaml の相互作用ロジック
    /// </summary>
    public partial class PrintForm : UserControl
    {
        public PrintForm()
        {
            InitializeComponent();
        }

        private ReportData _model = new ReportData();

        private PageIndex _pageIndex = PageIndex.Page1;
        private enum PageIndex
        {
            Page1 = 0,
            Page2,
            Page3
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_pageIndex == PageIndex.Page1) return;

            reportArea.Children.Clear();

            switch (_pageIndex)
            {
                case PageIndex.Page2:
                    reportArea.Children.Add(new ReportControl1());
                    _pageIndex = PageIndex.Page1;
                    break;
                case PageIndex.Page3:
                    reportArea.Children.Add(new ReportControl2());
                    _pageIndex = PageIndex.Page2;
                    break;
                default:
                    break;
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_pageIndex == PageIndex.Page3) return;

            reportArea.Children.Clear();

            switch (_pageIndex)
            {
                case PageIndex.Page1:
                    reportArea.Children.Add(new ReportControl2());
                    _pageIndex = PageIndex.Page2;
                    break;
                case PageIndex.Page2:
                    reportArea.Children.Add(new ReportControl3());
                    _pageIndex = PageIndex.Page3;
                    break;
                default:
                    break;
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            new PrintClass().Print(_model);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            reportArea.Children.Clear();
            reportArea.Children.Add(new ReportControl1());
            _pageIndex = PageIndex.Page1;

        }
    }

    //印刷データクラス
    public class ReportData : INotifyPropertyChanged
    {
        private string _message = string.Empty;
        public event PropertyChangedEventHandler PropertyChanged;

        //メッセージプロパティ
        public string Message
        {
            get => _message;
            set
            {
                if (_message != value)
                {
                    //値が変更されたら値を更新して変更を通知
                    _message = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
                }
            }
        }
    }

}
