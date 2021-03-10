using i_ConVerificationSystem.Forms.Gradient;
using i_ConVerificationSystem.Forms.Settings.Base;
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
    ///     <MyNamespace:StdTabItem/>
    ///
    /// </summary>
    public class StdTabItem : TabItem_Extensions
    {
        static StdTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StdTabItem), new FrameworkPropertyMetadata(typeof(StdTabItem)));
        }

        /// <summary>
        /// 閉じるボタンがクリックされた場合に発生させるルーティングイベントの識別子
        /// </summary>
        public static readonly RoutedEvent CloseButtonClickEvent = EventManager.RegisterRoutedEvent("CloseButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StdTabItem));

        /// <summary>
        /// イベント
        /// </summary>
        public event RoutedEventHandler CloseButtonClick
        {
            add { AddHandler(CloseButtonClickEvent, value); }
            remove { RemoveHandler(CloseButtonClickEvent, value); }
        }

        /// <summary>
        /// テンプレートが適用された際に発生するイベントのハンドラ
        /// 閉じるボタンがクリックされた際のイベントハンドラを登録する
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Button closeButton = base.GetTemplateChild("CloseButton") as Button;
            closeButton.Click += new RoutedEventHandler(closeButton_Click);
        }

        /// <summary>
        /// 閉じるボタンがクリックされた際のイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            //ClosePageEventを発生させる
            RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent, this));

            e.Handled = true;
        }

        /// <summary>
        /// OGMapに描画されているCSを取得
        /// </summary>
        /// <returns></returns>
        public CrossSect_OGExtension GetCrossSect_OGExtension()
        {
            //var co = this.Content as WindowsFormsHost;
            var co = this.Content as OGMap;
            if (co is null)
            {
                return null;
            }
            else
            {
                //var c = co.Child as OGMap2;
                //if (c is null) return null;
                //return c.GetCrossSect_Extension();
                return co.GetCrossSect_Extension();
            }
        }
    }
}
