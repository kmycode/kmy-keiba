using KmyKeiba.Models.Logics;
using KmyKeiba.ViewEvents;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.ViewModels
{
  class PredictRunningStyleDialogViewModel : BindableBase, IDialogAware
  {
    private readonly CompositeDisposable disposables = new();
    private readonly PredictRunningStyleModel model = new();

    public ReactiveProperty<bool> IsProcessing => this.model.IsProcessing;

    public ReactiveProperty<bool> IsError => this.model.IsError;

    public ReactiveProperty<bool> CanPredict => this.model.CanPredict;

    public ReactiveProperty<int> ProcessCount => this.model.ProcessCount;

    public ReactiveProperty<int> Processed => this.model.Processed;

    public FileDialogCaller Caller { get; } = new();

    public string Title => "脚質を予測";

    public event Action<IDialogResult>? RequestClose;

    public bool CanCloseDialog() => !this.model.IsProcessing.Value;

    public void OnDialogClosed()
    {
      this.disposables.Dispose();
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
      this.model.FileDialogCalled += (sender, e) =>
      {
        this.Caller.Call(e);
      };
    }

    public PredictRunningStyleDialogViewModel()
    {
      this.OpenFileCommand = this.IsProcessing.Select((p) => !p).ToReactiveCommand();
      this.OpenFileCommand
        .Subscribe(() => this.model.OpenFile())
        .AddTo(this.disposables);

      this.LoadDatabaseCommand = this.IsProcessing.Select((p) => !p).ToReactiveCommand();
      this.LoadDatabaseCommand
        .Subscribe(() => this.model.Training())
        .AddTo(this.disposables);

      this.PredictCommand = this.IsProcessing
        .Select((p) => !p)
        .CombineLatest(this.CanPredict, (a, b) => a && b)
        .ToReactiveCommand();
      this.PredictCommand
        .Subscribe(() => this.model.Predict())
        .AddTo(this.disposables);

      this.SaveFileCommand = this.IsProcessing
        .Select((p) => !p)
        .CombineLatest(this.CanPredict, (a, b) => a && b)
        .ToReactiveCommand();
      this.SaveFileCommand
        .Subscribe(() => this.model.SaveFile())
        .AddTo(this.disposables);

      this.ResetCommand = this.IsProcessing.Select((p) => !p).ToReactiveCommand();
      this.ResetCommand
        .Subscribe(() => this.model.Reset())
        .AddTo(this.disposables);
    }

    public ReactiveCommand OpenFileCommand { get; }

    public ReactiveCommand LoadDatabaseCommand { get; }

    public ReactiveCommand SaveFileCommand { get; }

    public ReactiveCommand PredictCommand { get; }

    public ReactiveCommand ResetCommand { get; }
  }
}
