using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KmyKeiba.Models.Race.HorseMark
{
  public class HorseMarkModel : IDisposable
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private readonly CompositeDisposable _disposables = new();
    private readonly IReadOnlyList<RaceHorseAnalyzer> _horses;
    private readonly string _raceKey;

    public HorseMarkConfigModel Config => HorseMarkConfigModel.Instance;

    public ReactiveCollection<HorseMarkRow> Rows { get; } = new();

    private HorseMarkModel(string raceKey, IReadOnlyList<RaceHorseAnalyzer> horses)
    {
      this._raceKey = raceKey;
      this._horses = horses;

      this.Config.Configs.CollectionChangedAsObservable().Subscribe(_ => this.OnMarkConfigChanged(null)).AddTo(this._disposables);
    }

    private void OnMarkConfigChanged(IEnumerable<HorseMarkData>? marks)
    {
      if (marks == null)
      {
        marks = this.Rows.Where(r => r.IsCustomMark).SelectMany(r => r.Cells).Select(c => c.Data).ToArray();
      }

      var targetRows = this.Rows.Where(r => r.IsCustomMark).ToArray();
      foreach (var cell in targetRows.SelectMany(r => r.Cells))
      {
        cell.Dispose();
        this._disposables.Remove(cell);
      }
      foreach (var row in targetRows)
      {
        this.Rows.Remove(row);
      }

      foreach (var config in HorseMarkConfigModel.Instance.Configs
        .GroupJoin(marks, c => c.Data.Id, m => m.ConfigId, (c, ms) => new { Config = c, Marks = ms, }))
      {
        var row = new HorseMarkRow
        {
          CustomMarkConfig = config.Config,
        };
        foreach (var horse in this._horses)
        {
          var cellData = config.Marks.FirstOrDefault(m => m.Key == horse.Data.Key);
          if (cellData == null)
          {
            cellData = new HorseMarkData
            {
              RaceKey = this._raceKey,
              Key = horse.Data.Key,
              ConfigId = config.Config.Data.Id,
            };
          }

          row.Cells.Add(new HorseMarkCell(cellData, config.Config).AddTo(this._disposables));
        }

        this.Rows.Add(row);
      }
    }

    public static async Task<HorseMarkModel> CreateAsync(MyContext db, string raceKey, IReadOnlyList<JrdbRaceHorseData> jrdbs, IReadOnlyList<RaceHorseAnalyzer> horses)
    {
      await HorseMarkUtil.InitializeAsync(db);
      HorseMarkConfigModel.Instance.Initialize();

      var model = new HorseMarkModel(raceKey, horses);

      // JRDBなどの印
      var jrdbMarks = horses.GroupJoin(jrdbs, h => h.Data.Key, j => j.Key, (h, js) => new { Horse = h, Jrdb = js.FirstOrDefault(), });
      if (jrdbMarks.Any(j => j.Jrdb != null))
      {
        var row1 = new HorseMarkRow { DefaultName = "JRDB-IDM印", };
        var row2 = new HorseMarkRow { DefaultName = "JRDB-騎手印", };
        var row3 = new HorseMarkRow { DefaultName = "JRDB-情報印", };
        var row4 = new HorseMarkRow { DefaultName = "JRDB-激走印", };
        var row5 = new HorseMarkRow { DefaultName = "JRDB-厩舎印", };
        var row6 = new HorseMarkRow { DefaultName = "JRDB-調教印", };
        var row7 = new HorseMarkRow { DefaultName = "JRDB-芝印", };
        var row8 = new HorseMarkRow { DefaultName = "JRDB-ダート印", };
        var row9 = new HorseMarkRow { DefaultName = "JRDB-総合印", };
        foreach (var horse in jrdbMarks)
        {
          if (horse.Jrdb == null)
          {
            row1.Cells.Add(new HorseMarkCell(RaceHorseMark.Default));
          }
          else
          {
            row1.Cells.Add(new HorseMarkCell(horse.Jrdb.IdmMark.ToAppMark()));
            row2.Cells.Add(new HorseMarkCell(horse.Jrdb.RiderMark.ToAppMark()));
            row3.Cells.Add(new HorseMarkCell(horse.Jrdb.InfoMark.ToAppMark()));
            row4.Cells.Add(new HorseMarkCell(horse.Jrdb.SpeedMark.ToAppMark()));
            row5.Cells.Add(new HorseMarkCell(horse.Jrdb.StableMark.ToAppMark()));
            row6.Cells.Add(new HorseMarkCell(horse.Jrdb.TrainingMark.ToAppMark()));
            row7.Cells.Add(new HorseMarkCell(horse.Jrdb.TurfMark.ToAppMark()));
            row8.Cells.Add(new HorseMarkCell(horse.Jrdb.DirtMark.ToAppMark()));
            row9.Cells.Add(new HorseMarkCell(horse.Jrdb.TotalMark.ToAppMark()));
          }
        }
        model.Rows.Add(row1);
        model.Rows.Add(row2);
        model.Rows.Add(row3);
        model.Rows.Add(row4);
        model.Rows.Add(row5);
        model.Rows.Add(row6);
        model.Rows.Add(row7);
        model.Rows.Add(row8);
        model.Rows.Add(row9);
      }

      // カスタム印
      var marks = await db.HorseMarks!.Where(m => m.RaceKey == raceKey).ToArrayAsync();
      model.OnMarkConfigChanged(marks);

      return model;
    }

    public void Dispose()
    {
      this._disposables.Dispose();

      foreach (var cell in this.Rows.SelectMany(r => r.Cells))
      {
        cell.Dispose();
      }
    }
  }

  public class HorseMarkRow
  {
    public string DefaultName { get; set; } = string.Empty;

    public ReactiveProperty<string> Name => this.CustomMarkConfig?.Name ?? this.DefaultNameAsReactiveProperty;

    private ReactiveProperty<string> DefaultNameAsReactiveProperty => this._defaultNameAsReactiveProperty ??= new(this.DefaultName);
    private ReactiveProperty<string>? _defaultNameAsReactiveProperty;

    public HorseMarkConfig? CustomMarkConfig { get; set; }

    public bool IsCustomMark => this.CustomMarkConfig != null;

    public ReactiveCollection<HorseMarkCell> Cells { get; } = new();
  }

  public class HorseMarkCell : IDisposable, IHorseMarkSetter
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private readonly CompositeDisposable _disposables = new();

    public HorseMarkData Data { get; }

    public HorseMarkConfig? Config { get; }

    public ReactiveProperty<RaceHorseMark> Mark { get; } = new();

    public bool CanEdit { get; }

    public HorseMarkCell(RaceHorseMark mark)
    {
      this.Data = new();
      this.Mark.Value = mark;
    }

    public HorseMarkCell(HorseMarkData data, HorseMarkConfig? config)
    {
      this.Data = data;
      this.Mark.Value = data.Mark;
      this.Config = config;
      this.CanEdit = true;

      this.Mark.Skip(1).Subscribe(async mark =>
      {
        try
        {
          using var db = new MyContext();

          if (this.Data.Id == default)
          {
            await db.HorseMarks!.AddAsync(this.Data);
          }
          else
          {
            db.HorseMarks!.Attach(this.Data);
          }
          this.Data.Mark = mark;
          await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
          logger.Error("印の保存でエラー", ex);
        }
      }).AddTo(this._disposables);

      DownloaderModel.Instance.CanSaveOthers.Subscribe(canSave =>
      {
        ((CommandBase<string>)this.SetMarkCommand).OnCanExecuteChanged();
      }).AddTo(this._disposables);
    }

    #region Command

    public ICommand SetMarkCommand =>
      this._setDoubleCircleMarkCommand ??=
        new CommandBase<string>(mark => this.Mark.Value = EnumUtil.ToHorseMark(mark), () => DownloaderModel.Instance.CanSaveOthers.Value);
    private ICommand? _setDoubleCircleMarkCommand;

    #endregion

    public void Dispose()
    {
      this._disposables.Dispose();
    }
  }
}
