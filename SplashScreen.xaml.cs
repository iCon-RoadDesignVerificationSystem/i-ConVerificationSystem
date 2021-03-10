using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace i_ConVerificationSystem
{
    /// <summary>
    /// SplashScreen.xaml の相互作用ロジック
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

            var ver = Assembly.GetEntryAssembly().GetName().Version;
            AssemblyVersion.Value = $"{ver.Major}.{ver.Minor}";
            Title_.Value = $"{Title_.Value} Ver{AssemblyVersion.Value}";

            DataContext = this;
        }

        public ReactiveProperty<string> Title_ { get; private set; } = new ReactiveProperty<string>("道路設計照査システム");
        public ReactiveProperty<string> AssemblyVersion { get; private set; } = new ReactiveProperty<string>("");

        private void FormFadeAnimation_Completed(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
