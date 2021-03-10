using i_ConVerificationSystem;
using i_ConVerificationSystem.Forms.Gradient;
using i_ConVerificationSystem.Forms.Main;
using i_ConVerificationSystem.Forms.Plot;
using i_ConVerificationSystem.Forms.Settings;
using i_ConVerificationSystem.Forms.Settings.Base;
using i_ConVerificationSystem.Forms.Settings.StdWidthComposition;
using i_ConVerificationSystem.Forms.Settings.VerificationItem;
using i_ConVerificationSystem.Forms.WidthComposition;
using i_ConVerificationSystem.JSON;
using i_ConVerificationSystem.Structs;
using i_ConVerificationSystem.Verification;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
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
using System.Xml.Linq;
using static i_ConVerificationSystem.Structs.OGExtensions;

namespace i_ConVerificationSystem
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; } = new MainWindowViewModel(new MainWindowModel());

        public MainWindow()
        {
            InitializeComponent();

            UpdateConditionsStatus();

            ViewModel.LoadJLandXML.Subscribe(_ =>
            {
                ConfirmSaveConditions();
                JLandXMLOpen();
            });
            ViewModel.LoadConditions.Subscribe(_ =>
            {
                ConfirmSaveConditions();
                if (AppSettingsManager.Instance.LoadJsonFile())
                {
                    JumpToLandXMLTab();
                    UpdateConditionsStatus();
                    ClearVerificationResult();
                    RefreshAllTabControls();
                }
            });
            ViewModel.SaveConditions.Subscribe(_ =>
            {
                if (AppSettingsManager.Instance.SaveJsonFile())
                {
                    UpdateConditionsStatus();
                }
            });
            ViewModel.LoadStdConditions.Subscribe(_ =>
            {
                StdVerificationConditionsManager.Instance.LoadJsonFile();
                JumpToLandXMLTab();
                UpdateConditionsStatus();
                ClearVerificationResult();
            });
            ViewModel.CloseApplication.Subscribe(_ =>
            {
                ConfirmSaveConditions();
                Close();
            });
            ViewModel.OpenWCConditions.Subscribe(_ =>
            {
                var f = new GeneralSettingWindow(GeneralSettingWindow.DispConditions.WC);
                if (f.ShowGeneralScreen(this, "幅員・幅員構成照査の条件値設定"))
                {
                    ScreenUpdateFromOtherScreen();
                }
            });
            ViewModel.OpenTGConditions.Subscribe(_ =>
            {
                var f = new GeneralSettingWindow(GeneralSettingWindow.DispConditions.TG);
                if (f.ShowGeneralScreen(this, "横断勾配照査の条件値設定"))
                {
                    ScreenUpdateFromOtherScreen();
                }
            });
            ViewModel.OpenOGConditions.Subscribe(_ =>
            {
                var f = new GeneralSettingWindow(GeneralSettingWindow.DispConditions.OG);
                if (f.ShowGeneralScreen(this, "片勾配すりつけ照査の条件値設定"))
                {
                    ScreenUpdateFromOtherScreen();
                }
            });
            ViewModel.OpenGGConditions.Subscribe(_ =>
            {
                var f = new GeneralSettingWindow(GeneralSettingWindow.DispConditions.GG);
                if (f.ShowGeneralScreen(this, "緩勾配区間長照査の条件値設定"))
                {
                    ScreenUpdateFromOtherScreen();
                }
            });
            
            DataContext = ViewModel;
        }

        /// <summary>
        /// 条件値変更による照査結果リセット
        /// </summary>
        private void ScreenUpdateFromOtherScreen()
        {
            ClearVerificationResult();
            RefreshAllTabControls();
        }

        /// <summary>
        /// J-LandXMLオープン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileOpenButton_Click(object sender, RoutedEventArgs e)
        {
            JLandXMLOpen();
        }

        /// <summary>
        /// J-LandXMLオープンメイン
        /// </summary>
        private void JLandXMLOpen()
        {
            using (System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = "J-LandXMLファイル (*.xml)|*.xml";
                dialog.FilterIndex = 1;
                dialog.Title = "J-LandXMLファイルを開く";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtXMLFilePath.Text = dialog.FileName;
                    XMLLoader.Instance.LoadXML(dialog.FileName);

                    //各設定ファイルのイニシャライズ
                    AppSettingsManager.Instance.ClearAppSettings();
                    JumpToLandXMLTab();
                    if (ValidationJLandXMLFile())
                    {
                        CreateAlignmentMenuItems();
                    }
                    UpdateConditionsStatus();
                    RefreshAllTabControls();
                    ClearVerificationResult();
                }
            }
        }

        /// <summary>
        /// 全ての照査結果と照査結果タブをクリア
        /// </summary>
        private void ClearVerificationResult()
        {
            //過去の照査結果をクリア
            GGVerificationResultItem.Instance.Clear();
            TGVerificationResultItem.Instance.Clear();
            OGVerificationResultItem.Instance.Clear();
            WCVerificationResultItem.Instance.Clear();
            WVerificationResultItem.Instance.Clear();

            //照査結果タブのリフレッシュ
            GGVR.RefreshDataGrid();
            TGVR.RefreshVerificationResult();
            OGVR.RefreshVerificationResult();
            WCVR.RefreshVerificationResult();
        }

        /// <summary>
        /// J-LandXML検証
        /// </summary>
        private bool ValidationJLandXMLFile()
        {
            string ngXmlMessage = $"【NG】参照ファイルは、LandXMLファイルではありません。";
            string ngDataMessage = $"【NG】参照ファイルは、道路データではありません。";
            string okMessage = $"【OK】参照ファイルは、LandXMLの道路データです。";
            bool retVal = false;

            if (XMLLoader.Instance.ValidateXML())
            {
                //エラーあり
                txtJLandXMLValidateResult.Text = ngXmlMessage;
                retVal = false;
            }
            else
            {
                //エラーなし
                //ユーザが任意に変更できるため、ファイル名では判定しない
                //if (XMLLoader.Instance.XMLFileName.Contains("ROA"))
                //{
                //    txtJLandXMLValidateResult.Text = okMessage;
                //    retVal = true;
                //}
                //else
                //{
                if (XMLLoader.Instance.IsRoadOfArchitecture())
                {
                    txtJLandXMLValidateResult.Text = okMessage;
                    retVal = true;
                }
                else
                {
                    txtJLandXMLValidateResult.Text = ngDataMessage;
                    retVal = false;
                }
                //}
            }
            return retVal;
        }

        /// <summary>
        /// 線形メニューバー作成
        /// </summary>
        private void CreateAlignmentMenuItems()
        {
            var aliList = XMLLoader.Instance.GetAlignmentTupleList();
            M_Std.Items.Clear();

            foreach (var aliName in aliList)
            {
                var mI = new MenuItem();
                mI.Header = aliName.Item1;
                mI.Click += StdItems_Click;
                M_Std.Items.Add(mI);
            }
        }

        /// <summary>
        /// 線形メニューバークリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StdItems_Click(object sender, RoutedEventArgs e)
        {
            var mI = sender as MenuItem;
            if (!(mI is null))
            {
                OpenStdSettings(mI.Header.ToString());
            }
        }

        /// <summary>
        /// 標準幅員設定画面の表示
        /// </summary>
        /// <param name="alignmentName"></param>
        private void OpenStdSettings(string alignmentName)
        {
            var alignmentXe = XMLLoader.Instance.GetAlignmentFromName(alignmentName);
            var f = new StdWidthCompositionWindow();
            f.selAlignmentName = alignmentName;
            f.selXAlignment = alignmentXe;
            if (f.ShowStdWidthCompositionWindow(this))
            {
                ScreenUpdateFromOtherScreen();
            }
        }

        /// <summary>
        /// ステータス更新
        /// </summary>
        private void UpdateConditionsStatus()
        {
            IsStdConditionsJsonLoaded.Text = StdVerificationConditionsManager.Instance.JsonFileName.Value;
            IsStdConditionsJsonLoaded.Foreground = StdVerificationConditionsManager.Instance.IsLoaded.Value ? Brushes.Black : Brushes.Red;
            IsConditionsJsonLoaded.Text = AppSettingsManager.Instance.IsLoaded ? "はい" : "いいえ";
            IsConditionsJsonLoaded.Foreground = AppSettingsManager.Instance.IsLoaded ? Brushes.Black : Brushes.Red;
        }

        /// <summary>
        /// 保存確認
        /// </summary>
        private void ConfirmSaveConditions()
        {
            if (AppSettingsManager.Instance.SaveRequire)
            {
                var msgRes = MessageBox.Show("条件値に変更があります。保存しますか？", "確認", MessageBoxButton.YesNo);

                if (msgRes == MessageBoxResult.Yes)
                {
                    AppSettingsManager.Instance.SaveJsonFile();
                }
            }
        }

        /// <summary>
        /// 緩勾配区間長の照査実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnVerificationGG_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in GGCon.tcAlignments.Items)
            {
                var gs = new GentleSlope();
                var tb = (TabItem_Extensions)item;
                var ggInputParameter = (GGInputParameter)tb.Content;
                var ggip = ggInputParameter.GetGGInputParameters();
                var profAlign = XMLLoader.Instance.GetProfAlignFromAlignmentName(ggInputParameter.alignmentName);
                gs.HasGentleSlopeArea(profAlign, ggip, ggInputParameter.alignmentName);
            }

            GGVR.RefreshDataGrid();
        }

        /// <summary>
        /// 幅員構成照査実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnVerificationWC_Click(object sender, RoutedEventArgs e)
        {
            var wc = new WidthComposition();

            var wcipList = WCCon.GetWCInputParameters();
            foreach (var wcip in wcipList)
            {
                var csList = WCStd.GetCsFromName(wcip.alignmentName);
                foreach (var cs in csList)
                {
                    //照査結果タブに反映
                    var (wcTotalResult, trwc) = wc.IsCorrectWidthComposition(wcip.alignmentName, wcip.GetWCInputParams(), cs);
                }
            }

            WCVR.RefreshVerificationResult();
        }

        /// <summary>
        /// 幅員照査実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnVerificationW_Click(object sender, RoutedEventArgs e)
        {
            var wc = new WidthComposition();

            var wcipList = WCon.GetWCInputParameters();
            foreach (var wcip in wcipList)
            {
                var csList = WStd.GetCsFromName(wcip.alignmentName);
                foreach (var cs in csList)
                {
                    //照査結果タブに反映
                    var wResult= wc.IsCorrectWidth(wcip.alignmentName, wcip.GetWCInputParams(), cs);
                }
                WStd.SetOgcsList(wcip.alignmentName, csList);
            }
        }

        /// <summary>
        /// 横断勾配照査実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnVerificationTG_Click(object sender, RoutedEventArgs e)
        {
            var tg = new TransverseGradient();

            foreach (TabItem_Extensions tgConItem in TGCon.tcAlignments.Items)
            {
                var tginputParameter = (TGInputParameter)tgConItem.Content;
                var tgip = tginputParameter.GetTGInputParameters();
                var selXAlignment = XMLLoader.Instance.GetAlignmentFromName(tgConItem.Header.ToString());
                //照査結果タブに表示
                tg.IsCorrectTransverseGradient(selXAlignment, tgip);
            }

            TGVR.RefreshVerificationResult();
        }

        /// <summary>
        /// 片勾配すりつけ照査実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnVerificationOG_Click(object sender, RoutedEventArgs e)
        {
            var og = new OnesidedGradient();

            foreach (TabItem_Extensions ogConItem in OGCon.tcAlignments.Items)
            {
                var oginputParameter = (OGInputParameter)ogConItem.Content;
                var ogip = oginputParameter.GetOGInputParameters();
                var selXAlignment = XMLLoader.Instance.GetAlignmentFromName(ogConItem.Header.ToString());
                //照査結果タブに表示
                og.IsCorrectOnesidedGradient(selXAlignment, ogip);
            }

            OGVR.RefreshVerificationResult();
        }

        /// <summary>
        /// 概略平面図を表示（幅員構成照査タブから）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShowPlaneDiagram_Click(object sender, RoutedEventArgs e)
        {
            if (WCCon.tcAlignments.Items.Count == 0) return;

            var wcConItem = (TabItem_Extensions)WCCon.tcAlignments.SelectedItem;
            if (wcConItem is null) wcConItem = (TabItem_Extensions)WCCon.tcAlignments.Items[0];
            var wcInputParameter = (WCInputParameter)wcConItem.Content;
            var selAli = wcInputParameter.selXAlignment;
            ShowPlaintGraph(selAli);
        }

        /// <summary>
        /// 概略平面図を表示
        /// </summary>
        /// <param name="selXAlignment"></param>
        private void ShowPlaintGraph(XElement selXAlignment)
        {
            var plTest = new PlainPlot();
            plTest.alignmentXe = selXAlignment;
            plTest.Owner = this;
            plTest.ShowDialog();
        }

        /// <summary>
        /// 概略平面図を表示（幅員照査タブから）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShowPlaneDiagramW_Click(object sender, RoutedEventArgs e)
        {
            if (WCon.tcAlignments.Items.Count == 0) return;

            var wConItem = (TabItem_Extensions)WCon.tcAlignments.SelectedItem;
            if (wConItem is null) wConItem = (TabItem_Extensions)WCon.tcAlignments.Items[0];
            var wInputParameter = (WCInputParameter)wConItem.Content;
            var selAli = wInputParameter.selXAlignment;
            ShowPlaintGraph(selAli);
        }

        /// <summary>
        /// 幅員照査結果ウィンドウを表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWVR_Click(object sender, RoutedEventArgs e)
        {
            if (WStd.tcAlignments.Items.Count == 0) return;

            var s = WStd.GetSelectedOgcsListAndName();

            var wVrWin = new WVRWindow(s.Item1, s.Item2);
            wVrWin.Owner = this;
            wVrWin.RefreshVerificationResult();
            wVrWin.ShowDialog();
        }

        /// <summary>
        /// 初めの画面に戻る
        /// </summary>
        private void JumpToLandXMLTab()
        {
            MainTabControl.SelectedItem = TiMain;
        }

        /// <summary>
        /// 全ての照査タブをリフレッシュ
        /// </summary>
        private void RefreshAllTabControls()
        {
            //幅員構成
            WCCon.RefreshConditions();
            WCStd.RefreshConditions();

            //幅員
            WCon.RefreshConditions();
            WStd.RefreshConditions();

            //横断勾配
            TGCon.RefreshConditions();

            //片勾配すりつけ
            OGCon.RefreshConditions();

            //緩勾配区間長
            GGCon.RefreshConditions();
        }

        /// <summary>
        /// フォームクローズ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfirmSaveConditions();
        }

        /// <summary>
        /// はじめに基準値ファイルを読み込み必須
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("基準値ファイルを選択してください。");
            //基準値ファイルを読み込み
            ViewModel.LoadStdConditions.Execute();
            if (StdVerificationConditionsManager.Instance.IsLoaded.Value == false)
            {
                MessageBox.Show($"基準値ファイルが無いため照査を実行できません。{Environment.NewLine}プログラムを終了します。");
                this.Close();
            }
        }
    }
}
