using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace i_ConVerificationSystem
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        //Dispose
        private CompositeDisposable Disposable { get; } = new CompositeDisposable();
        //Model
        public MainWindowModel Model { get; }
        //プロパティ、コマンド群
        public ReactiveCommand LoadJLandXML { get; }
        public ReactiveCommand LoadConditions { get; }
        public ReactiveCommand SaveConditions { get; }
        public ReactiveCommand LoadStdConditions { get; }
        public ReactiveCommand CloseApplication { get; }
        public ReactiveCommand OpenWCConditions { get; }
        public ReactiveCommand OpenTGConditions { get; }
        public ReactiveCommand OpenOGConditions { get; }
        public ReactiveCommand OpenGGConditions { get; }

        //コンストラクタ
        public MainWindowViewModel(MainWindowModel model)
        {
            this.Model = model;
            LoadJLandXML = new ReactiveCommand().AddTo(Disposable);
            LoadConditions = new ReactiveCommand().AddTo(Disposable);
            SaveConditions = new ReactiveCommand().AddTo(Disposable);
            LoadStdConditions = new ReactiveCommand().AddTo(Disposable);
            CloseApplication = new ReactiveCommand().AddTo(Disposable);
            OpenWCConditions = new ReactiveCommand().AddTo(Disposable);
            OpenTGConditions = new ReactiveCommand().AddTo(Disposable);
            OpenOGConditions = new ReactiveCommand().AddTo(Disposable);
            OpenGGConditions = new ReactiveCommand().AddTo(Disposable);
        }

        //Dispose
        public void Dispose()
        {
            Disposable.Dispose();
        }
    }
}
