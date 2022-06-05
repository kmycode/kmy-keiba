using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Race;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.ViewModels
{
  internal class RaceViewModelBase : INotifyPropertyChanged, IDisposable
  {
    protected readonly CompositeDisposable _disposables = new();
    protected readonly RaceModel model = new();
    protected readonly DownloaderModel downloader = DownloaderModel.Instance;

    public DownloaderModel Downloader => this.downloader;

    public ReactiveProperty<RaceInfo?> Race => this.model.Info;

    public ReactiveProperty<bool> IsLoaded => this.model.IsLoaded;

    public ReactiveProperty<bool> IsFirstRaceLoadStarted => this.model.IsFirstLoadStarted;

    public ReactiveProperty<bool> IsViewExpection => this.model.IsViewExpection;

    public ReactiveProperty<bool> IsViewResult => this.model.IsViewResult;

    public ReactiveProperty<bool> IsSelectedAllHorses => this.model.IsSelectedAllHorses;

    public ReactiveProperty<bool> CanSave => this.downloader.CanSaveOthers;

    public ReactiveProperty<bool> IsModelError => this.model.IsError;

    public ReactiveProperty<string> ModelErrorMessage => this.model.ErrorMessage;

    public void Dispose()
    {
      this._disposables.Dispose();
    }

#pragma warning disable CS0067
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
  }
}
