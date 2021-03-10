using i_ConVerificationSystem.Forms.Gradient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static i_ConVerificationSystem.Structs.OGExtensions;

namespace i_ConVerificationSystem.Forms.Settings.StdWidthComposition
{
    /// <summary>
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:i_ConVerificationSystem.Forms.Settings.StdWidthComposition"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:i_ConVerificationSystem.Forms.Settings.StdWidthComposition;assembly=i_ConVerificationSystem.Forms.Settings.StdWidthComposition"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:StdTabControl/>
    ///
    /// </summary>
    
    /// <summary>
    /// StdTabItemが閉じられた際に発生するイベントを処理するハンドラのデリゲート
    /// </summary>
    public delegate void PageCLoseEventHandler(object sender, PageCloseEventArgs e);

    /// <summary>
    /// StdTabItemを閉じることができるTabControl
    /// </summary>
    public class StdTabControl : TabControl
    {
        static StdTabControl()
        {
            //外観はTabControlを適用
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StdTabControl), new FrameworkPropertyMetadata(typeof(TabControl)));
        }

        /// <summary>
        /// コンストラクタ
        /// イベントハンドラの登録
        /// </summary>
        public StdTabControl()
        {
            AddHandler(StdTabItem.CloseButtonClickEvent, new RoutedEventHandler(PageCloseButtonClick));
        }

        /// <summary>
        /// ページが閉じられた際に発生するルーティングイベントを識別するための識別子
        /// </summary>
        public static readonly RoutedEvent PageCloseEvent = EventManager.RegisterRoutedEvent("PageClose", RoutingStrategy.Bubble, typeof(PageCLoseEventHandler), typeof(StdTabControl));

        /// <summary>
        /// StdTabItemの閉じるボタンのクリックイベントを処理してPageCloseEventを発生させる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageCloseButtonClick(object sender, RoutedEventArgs e)
        {
            StdTabItem tp = e.OriginalSource as StdTabItem;

            //自身からページを削除
            Items.Remove(tp);

            //ルーティングイベントを発生
            RaiseEvent(new PageCloseEventArgs(PageCloseEvent, this, tp));
        }

        public void ShowAlignmentTabPages(List<CrossSect_OGExtension> ogcsList)
        {
            foreach (var ogcs in ogcsList)
            {
                var tp = new StdTabItem();
                //var wHost = new WindowsFormsHost();
                var ogMap = new OGMap();
                //wHost.Child = ogMap;
                ogMap.DrawWC(ogcs);
                tp.Header = ogcs.ToString();
                //tp.Content = wHost;
                tp.Content = ogMap;
                this.Items.Add(tp);
            }

            if (this.Items.Count == 1)
            {
                //タブページが1枚しかないときにうまく出ない問題の回避
                var tp = new StdTabItem();
                tp.Header = "ダミー";
                tp.Content = "ダミー";
                this.Items.Add(tp);
                this.Items.Remove(tp);
            }
        }

        /// <summary>
        /// 表示されているStdTabItemのCSを取得
        /// </summary>
        /// <returns></returns>
        public List<CrossSect_OGExtension> GetCrossSect_OGExtensionList()
        {
            var retVal = new List<CrossSect_OGExtension>();

            foreach (var item in this.Items)
            {
                var tp = item as StdTabItem;

                if (!(tp is null))
                {
                    var cs = tp.GetCrossSect_OGExtension();
                    if (!(cs is null))
                    {
                        retVal.Add(cs);
                    }
                }
            }

            return retVal;
        }
    }
}
