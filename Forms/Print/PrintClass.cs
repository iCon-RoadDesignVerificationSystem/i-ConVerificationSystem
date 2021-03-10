using i_ConVerificationSystem.Forms.Print;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace i_ConVerificationSystem.Forms.Print
{
    internal class PrintClass
    {
        // 規定の用紙サイズ定義 = A4(8.27inch * 96dpi, 11.69inch * 96dpi)
        private const double DEFAULT_PAPER_SIZE_A4_S = 8.27 * 72;
        private const double DEFAULT_PAPER_SIZE_A4_L = 11.69 * 72;

        // 印刷処理
        public void Print(ReportData data)
        {
            // 印刷ダイアログを表示してプリンタや用紙を選択
            var printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                //向きは横固定
                var pticket = printDialog.PrintTicket;
                //pticket.PageMediaSize = new System.Printing.PageMediaSize(System.Printing.PageMediaSizeName.ISOA4);
                pticket.PageOrientation = System.Printing.PageOrientation.Landscape;
                // 選択されたプリンタの能力を取得
                var pcaps = printDialog.PrintQueue.GetPrintCapabilities();

                var fixedDoc = new FixedDocument();
                var page = new FixedPage();
                var page2 = new FixedPage();
                var page3 = new FixedPage();

                // ReportControlを作成して用紙サイズに合わせる
                var vb = new Viewbox
                {
                    Width = pcaps.PageImageableArea.ExtentHeight,
                    Height = pcaps.PageImageableArea.ExtentWidth,
                    Margin = new Thickness(
                        pcaps.PageImageableArea.OriginWidth,
                        pcaps.PageImageableArea.OriginHeight,
                        0,
                        0),
                    Child = new ReportControl1()
                };
                var vb2 = new Viewbox
                {
                    Width = pcaps.PageImageableArea.ExtentHeight,
                    Height = pcaps.PageImageableArea.ExtentWidth,
                    Margin = new Thickness(
                        pcaps.PageImageableArea.OriginWidth,
                        pcaps.PageImageableArea.OriginHeight,
                        0,
                        0),
                    Child = new ReportControl2()
                };
                var vb3 = new Viewbox
                {
                    Width = pcaps.PageImageableArea.ExtentHeight,
                    Height = pcaps.PageImageableArea.ExtentWidth,
                    Margin = new Thickness(
                        pcaps.PageImageableArea.OriginWidth,
                        pcaps.PageImageableArea.OriginHeight,
                        0,
                        0),
                    Child = new ReportControl3()
                };

                // 固定ページドキュメントを作成して固定ページを追加
                page.Children.Add(vb);
                page2.Children.Add(vb2);
                page3.Children.Add(vb3);
                var content = new PageContent();
                var content2 = new PageContent();
                var content3 = new PageContent();
                ((IAddChild)content).AddChild(page);
                ((IAddChild)content2).AddChild(page2);
                ((IAddChild)content3).AddChild(page3);
                fixedDoc.Pages.Add(content);
                fixedDoc.Pages.Add(content2);
                fixedDoc.Pages.Add(content3);

                // 選択したプリンタで印字
                var printFileName = XMLLoader.Instance.XMLFileNameWithoutExtension;
                printDialog.PrintDocument(fixedDoc.DocumentPaginator, printFileName);
            }
        }
    }
}
