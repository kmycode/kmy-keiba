using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.ExNumber;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.AnalysisTable.Script
{
  public class AnalysisTableScriptConfigModel
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public static AnalysisTableScriptConfigModel Default { get; } = new();

    public CheckableCollection<AnalysisTableScriptItem> Configs { get; } = new();

    private AnalysisTableScriptConfigModel()
    {
    }

    public void Initialize()
    {
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Configs.Clear();

        foreach (var config in AnalysisTableScriptUtil.Scripts)
        {
          this.Configs.Add(new AnalysisTableScriptItem(config));
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
        var data = new AnalysisTableScriptData
        {
        };
        await db.AnalysisTableScripts!.AddAsync(data);
        await db.SaveChangesAsync();

        var item = new AnalysisTableScriptItem(data);
        AnalysisTableScriptUtil.Scripts.Add(data);
        this.Configs.Add(item);
      }
      catch (Exception ex)
      {
        logger.Error("スクリプト項目追加でエラー", ex);
      }
    }

    public async Task RemoveConfigAsync(AnalysisTableScriptItem config)
    {
      try
      {
        using var db = new MyContext();
        db.AnalysisTableScripts!.Remove(config.Data);
        await db.SaveChangesAsync();

        AnalysisTableScriptUtil.Scripts.Remove(config.Data);
        this.Configs.Remove(config);

        AnalysisTableConfigModel.Instance.OnExternalNumberConfigChanged();
      }
      catch (Exception ex)
      {
        logger.Error("スクリプト項目削除でエラー", ex);
      }
    }
  }

  public class AnalysisTableScriptItem : ICheckableItem, IDisposable
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);
    private readonly CompositeDisposable _disposables = new();

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public AnalysisTableScriptData Data { get; }

    public ReactiveProperty<string> Name { get; } = new();

    public ReactiveProperty<string> Script { get; } = new();

    public AnalysisTableScriptItem(AnalysisTableScriptData data)
    {
      this.Data = data;
      this.Name.Value = data.Name;
      this.Script.Value = data.Script;

      this.Name
        .CombineLatest(this.Script)
        .Subscribe(async _ =>
        {
          if (this.Name.Value != data.Name || this.Script.Value != data.Script)
          {
            try
            {
              using var db = new MyContext();
              db.AnalysisTableScripts!.Attach(this.Data);
              this.Data.Name = this.Name.Value;
              this.Data.Script = this.Script.Value;
              await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
              logger.Error("スクリプトデータの保存でエラー", ex);
            }
          }
        }).AddTo(this._disposables);
    }

    public void Dispose()
    {
      this._disposables.Dispose();
    }
  }
}
