using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.AnalysisTable;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.ExNumber
{
  public class ExternalNumberConfigModel
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public static ExternalNumberConfigModel Default { get; } = new();

    public CheckableCollection<ExternalNumberConfigItem> Configs { get; } = new();

    private ExternalNumberConfigModel()
    {
    }

    public void Initialize()
    {
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Configs.Clear();

        foreach (var config in ExternalNumberUtil.Configs)
        {
          this.Configs.Add(new ExternalNumberConfigItem(config));
        }

        if (this.Configs.Any())
        {
          this.Configs.First().IsChecked.Value = true;
        }
      });
    }

    public async Task AddConfigAsync()
    {
      try
      {
        using var db = new MyContext();
        var data = new ExternalNumberConfig
        {
          FileFormat = ExternalNumberFileFormat.RaceCsv,
          ValuesFormat = ExternalNumberValuesFormat.NumberOnly,
          SortRule = ExternalNumberSortRule.Larger,
        };
        await db.ExternalNumberConfigs!.AddAsync(data);
        await db.SaveChangesAsync();

        data.Order = (short)data.Id;
        await db.SaveChangesAsync();

        var item = new ExternalNumberConfigItem(data);
        ExternalNumberUtil.Configs.Add(data);
        this.Configs.Add(item);

        AnalysisTableConfigModel.Instance.OnExternalNumberConfigChanged();
      }
      catch (Exception ex)
      {
        logger.Error("外部指数追加でエラー", ex);
      }
    }

    public async Task RemoveConfigAsync(ExternalNumberConfigItem config)
    {
      try
      {
        using var db = new MyContext();
        db.ExternalNumberConfigs!.Remove(config.Data);
        await db.SaveChangesAsync();

        ExternalNumberUtil.Configs.Remove(config.Data);
        this.Configs.Remove(config);

        AnalysisTableConfigModel.Instance.OnExternalNumberConfigChanged();
      }
      catch (Exception ex)
      {
        logger.Error("外部指数削除でエラー", ex);
      }
    }
  }

  public class ExternalNumberConfigItem : IDisposable, ICheckableItem
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private readonly CompositeDisposable _disposables = new();
    private bool _isInitializing = true;

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public ExternalNumberConfig Data { get; }

    public ReactiveProperty<string> Name { get; } = new();

    public ReactiveProperty<string> FileNamePattern { get; } = new();

    public ReactiveProperty<bool> IsFormatRaceCsv { get; } = new();

    public ReactiveProperty<bool> IsFormatRaceFixedLength { get; } = new();

    public ReactiveProperty<bool> IsFormatHorseCsv { get; } = new();

    public ReactiveProperty<bool> IsFormatHorseFixedLength { get; } = new();

    public ReactiveProperty<bool> IsValuesNumberOnly { get; } = new();

    public ReactiveProperty<bool> IsValuesNumberAndOrder { get; } = new();

    public ReactiveProperty<bool> IsSortLarger { get; } = new();

    public ReactiveProperty<bool> IsSortSmaller { get; } = new();

    public ReactiveProperty<bool> IsSortSmallerWithoutZero { get; } = new();

    public ReactiveProperty<bool> IsLoading { get; } = new();

    public ReactiveProperty<DateTime> StartDate { get; } = new(DateTime.Today.AddDays(-30));

    public ReactiveProperty<DateTime> EndDate { get; } = new(DateTime.Today.AddDays(7));

    public ReactiveProperty<int> LoadProgress { get; } = new();

    public ReactiveProperty<int> LoadProgressMax { get; } = new();

    public ExternalNumberConfigItem(ExternalNumberConfig data)
    {
      this.Data = data;

      this.CheckInvalidData();
      this.LoadFromData();

      this.Name
        .CombineLatest(this.FileNamePattern)
        .CombineLatest(this.IsFormatRaceCsv)
        .CombineLatest(this.IsFormatRaceFixedLength)
        .CombineLatest(this.IsFormatHorseCsv)
        .CombineLatest(this.IsFormatHorseFixedLength)
        .CombineLatest(this.IsValuesNumberOnly)
        .CombineLatest(this.IsValuesNumberAndOrder)
        .CombineLatest(this.IsSortLarger)
        .CombineLatest(this.IsSortSmaller)
        .CombineLatest(this.IsSortSmallerWithoutZero)
        .Subscribe(async _ =>
      {
        if (this._isInitializing)
        {
          return;
        }

        if ((this.IsFormatHorseCsv.Value == false && this.IsFormatRaceFixedLength.Value == false && this.IsFormatRaceCsv.Value == false && this.IsFormatHorseFixedLength.Value == false) ||
          (this.IsValuesNumberOnly.Value == false && this.IsValuesNumberAndOrder.Value == false) ||
          (this.IsSortLarger.Value == false && this.IsSortSmaller.Value == false && this.IsSortSmallerWithoutZero.Value == false))
        {
          // UIのせいでなぜか全部falseになってイベント通知してくることがある
          this.LoadFromData();
          return;
        }

        try
        {
          using var db = new MyContext();
          db.ExternalNumberConfigs!.Attach(this.Data);

          this.Data.Name = this.Name.Value;
          this.Data.FileNamePattern = this.FileNamePattern.Value;
          this.Data.FileFormat = this.IsFormatRaceCsv.Value ? ExternalNumberFileFormat.RaceCsv :
            this.IsFormatRaceFixedLength.Value ? ExternalNumberFileFormat.RaceFixedLength :
            this.IsFormatHorseCsv.Value ? ExternalNumberFileFormat.RaceHorseCsv :
            this.IsFormatHorseFixedLength.Value ? ExternalNumberFileFormat.RaceHorseFixedLength : ExternalNumberFileFormat.Unknown;
          this.Data.ValuesFormat = this.IsValuesNumberAndOrder.Value ? ExternalNumberValuesFormat.NumberAndOrder : ExternalNumberValuesFormat.NumberOnly;
          this.Data.SortRule = this.IsSortSmaller.Value ? ExternalNumberSortRule.Smaller :
            this.IsSortSmallerWithoutZero.Value ? ExternalNumberSortRule.SmallerWithoutZero : ExternalNumberSortRule.Larger;

          await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
          logger.Error("外部指数データの保存でエラー", ex);
        }
      }).AddTo(this._disposables);

      this._isInitializing = false;
    }

    private void CheckInvalidData()
    {
      var data = this.Data;

      if (data.FileFormat == ExternalNumberFileFormat.Unknown)
      {
        data.FileFormat = ExternalNumberFileFormat.RaceCsv;
      }
      if (data.ValuesFormat == ExternalNumberValuesFormat.Unknown)
      {
        data.ValuesFormat = ExternalNumberValuesFormat.NumberOnly;
      }
      if (data.SortRule == ExternalNumberSortRule.Unknown)
      {
        data.SortRule = ExternalNumberSortRule.Larger;
      }
    }

    private void LoadFromData()
    {
      var data = this.Data;

      this.Name.Value = data.Name;
      this.FileNamePattern.Value = data.FileNamePattern;
      switch (data.FileFormat)
      {
        case ExternalNumberFileFormat.RaceCsv:
          this.IsFormatRaceCsv.Value = true;
          break;
        case ExternalNumberFileFormat.RaceFixedLength:
          this.IsFormatRaceFixedLength.Value = true;
          break;
        case ExternalNumberFileFormat.RaceHorseCsv:
          this.IsFormatHorseCsv.Value = true;
          break;
        case ExternalNumberFileFormat.RaceHorseFixedLength:
          this.IsFormatHorseFixedLength.Value = true;
          break;
      }
      switch (data.ValuesFormat)
      {
        case ExternalNumberValuesFormat.NumberAndOrder:
          this.IsValuesNumberAndOrder.Value = true;
          break;
        case ExternalNumberValuesFormat.NumberOnly:
          this.IsValuesNumberOnly.Value = true;
          break;
      }
      switch (data.SortRule)
      {
        case ExternalNumberSortRule.Larger:
          this.IsSortLarger.Value = true;
          break;
        case ExternalNumberSortRule.Smaller:
          this.IsSortSmaller.Value = true;
          break;
        case ExternalNumberSortRule.SmallerWithoutZero:
          this.IsSortSmallerWithoutZero.Value = true;
          break;
      }
    }

    public void BeginLoadDb()
    {
      this.IsLoading.Value = true;

      // TODO: error
      _ = Task.Run(async () =>
      {
        using var db = new MyContext();
        await db.TryBeginTransactionAsync();
        await ExternalNumberUtil.SaveRangeAsync(db, this.Data, this.StartDate.Value, this.EndDate.Value, this.LoadProgress, this.LoadProgressMax);

        this.IsLoading.Value = false;
      });

    }

    public void Dispose()
    {
      this._disposables.Dispose();
    }
  }
}
