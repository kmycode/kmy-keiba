using KmyKeiba.JVLink.Wrappers;
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
    private readonly JVLinkLoader model = new();
    private readonly CompositeDisposable disposables = new();

    public ReactiveProperty<DateTime> StartTime => this.model.StartTime;

    public ReactiveProperty<DateTime> EndTime => this.model.EndTime;

    public ReactiveProperty<bool> IsSetEndTime => this.model.IsSetEndTime;

    public ReactiveProperty<bool> IsLoading => this.model.IsLoading;

    public ReactiveProperty<int> Downloaded => this.model.Downloaded;

    public ReactiveProperty<int> DownloadSize => this.model.DownloadSize;

    public ReactiveProperty<int> Saved => this.model.Saved;

    public ReactiveProperty<int> SaveSize => this.model.SaveSize;

    public ReactiveProperty<int> Loaded => this.model.Loaded;

    public ReactiveProperty<int> LoadSize => this.model.LoadSize;

    public ReactiveProperty<JVLinkLoadResult> LoadErrorCode => this.model.LoadErrorCode;

    public ReactiveProperty<JVLinkReadResult> ReadErrorCode => this.model.ReadErrorCode;

    public ReactiveProperty<bool> IsDatabaseError => this.model.IsDatabaseError;

    public ReadOnlyReactiveProperty<bool> IsError => this.model.IsError;

    public ReactiveProperty<bool> IsCentralError => this.model.IsCentralError;

    public ReactiveProperty<bool> IsLocalError => this.model.IsLocalError;

    public ReactiveProperty<int> ProcessSize => this.model.ProcessSize;

    public ReactiveProperty<int> Processed => this.model.Processed;

    public string Title => "JV-Linkデータ読み込み";

    public event Action<IDialogResult>? RequestClose;

    public LoadJVLinkDialogViewModel()
    {
      this.LoadLocalCommand = this.model
        .IsLoading
        .Select((v) => !v)
        .ToReactiveCommand();
      this.LoadLocalCommand.Subscribe(() => _ = this.model.LoadLocalAsync());

      this.LoadCentralCommand = this.model
        .IsLoading
        .Select((v) => !v)
        .ToReactiveCommand();
      this.LoadCentralCommand.Subscribe(() => _ = this.model.LoadCentralAsync());
    }

    public bool CanCloseDialog() => !this.model.IsLoading.Value;

    public void OnDialogClosed()
    {
      this.model.Dispose();
      this.disposables.Dispose();
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
    }

    public ReactiveCommand LoadCentralCommand { get; } = new();

    public ReactiveCommand LoadLocalCommand { get; } = new();
  }
}
