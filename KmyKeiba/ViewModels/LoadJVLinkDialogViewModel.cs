using KmyKeiba.Models.Logics;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.ViewModels
{
  class LoadJVLinkDialogViewModel : BindableBase, IDialogAware
  {
    private readonly LoadJVLinkModel model = new();
    private readonly CompositeDisposable disposables = new();

    public ReactiveProperty<DateTime> StartTime => this.model!.StartTime;

    public ReactiveProperty<DateTime> EndTime => this.model!.EndTime;

    public ReactiveProperty<bool> IsSetEndTime => this.model!.IsSetEndTime;

    public ReactiveProperty<bool> IsLoading => this.model!.IsLoading;

    public ReactiveProperty<int> Downloaded => this.model!.Downloaded;

    public ReactiveProperty<int> DownloadSize => this.model!.DownloadSize;

    public ReactiveProperty<int> Saved => this.model!.Saved;

    public ReactiveProperty<int> SaveSize => this.model!.SaveSize;

    public ReadOnlyReactiveProperty<double> DownloadProgress => this.model!.DownloadProgress;

    public ReadOnlyReactiveProperty<double> SaveProgress => this.model!.SaveProgress;

    public ReactiveProperty<bool> CanClose { get; } = new(true);

    public string Title => "JV-Linkデータ読み込み";

    public event Action<IDialogResult>? RequestClose;

    public LoadJVLinkDialogViewModel()
    {
      this.LoadLocalCommand = this.model
        .IsLoading
        .Select((v) => !v)
        .ToReactiveCommand();
      this.LoadLocalCommand.Subscribe(() => this.model.LoadLocalAsync());
    }

    public bool CanCloseDialog() => this.CanClose.Value;

    public void OnDialogClosed()
    {
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
    }

    public ReactiveCommand LoadLocalCommand { get; } = new();
  }
}
