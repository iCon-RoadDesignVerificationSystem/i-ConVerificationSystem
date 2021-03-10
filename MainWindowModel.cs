using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reflection;
using System.Windows.Media;
using i_ConVerificationSystem.JSON;
using Reactive.Bindings;

namespace i_ConVerificationSystem
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ReactiveProperty<string> JLandXMLValidateResult { get; } = new ReactiveProperty<string>("-");
        public ReactiveProperty<string> ConditionsJsonFilePath { get; } = AppSettingsManager.Instance.JsonFilePath;
        public ReactiveProperty<DateTime?> ConditionsUpdatedTime { get; } = AppSettingsManager.Instance.UpdatedTime;
        public ReactiveProperty<DateTime?> ConditionsSavedTime { get; } = AppSettingsManager.Instance.SavedTime;
        public ReactiveProperty<string> StdConditionsJsonFilePath { get; } = StdVerificationConditionsManager.Instance.JsonFilePath;
        public ReactiveProperty<string> AssemblyVersion { get; private set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<string> Title { get; private set; } = new ReactiveProperty<string>("道路設計照査システム");

        public MainWindowModel()
        {
            var ver = Assembly.GetEntryAssembly().GetName().Version;
            AssemblyVersion.Value = $"{ver.Major}.{ver.Minor}";
            Title.Value = $"{Title.Value} Ver{AssemblyVersion.Value}";
        }
    }
}
