﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public abstract class MyContextBase : DbContext
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public DbSet<SystemData>? SystemData { get; set; }

    #region Data

    public DbSet<RaceData>? Races { get; set; }

    public DbSet<RaceHorseData>? RaceHorses { get; set; }

    public DbSet<HorseData>? Horses { get; set; }

    public DbSet<HorseBloodData>? HorseBloods { get; set; }

    public DbSet<HorseBloodInfoData>? HorseBloodInfos { get; set; }

    public DbSet<BornHorseData>? BornHorses { get; set; }

    public DbSet<HorseSaleData>? HorseSales { get; set; }

    public DbSet<SingleOddsTimeline>? SingleOddsTimelines { get; set; }

    public DbSet<PlaceOddsData>? PlaceOdds { get; set; }

    public DbSet<FrameNumberOddsData>? FrameNumberOdds { get; set; }

    public DbSet<QuinellaOddsData>? QuinellaOdds { get; set; }

    public DbSet<QuinellaPlaceOddsData>? QuinellaPlaceOdds { get; set; }

    public DbSet<ExactaOddsData>? ExactaOdds { get; set; }

    public DbSet<TrioOddsData>? TrioOdds { get; set; }

    public DbSet<TrifectaOddsData>? TrifectaOdds { get; set; }

    public DbSet<RefundData>? Refunds { get; set; }

    public DbSet<TrainingData>? Trainings { get; set; }

    public DbSet<WoodtipTrainingData>? WoodtipTrainings { get; set; }

    public DbSet<RiderData>? Riders { get; set; }

    public DbSet<TrainerData>? Trainers { get; set; }

    #endregion

    #region Jrdb

    public DbSet<JrdbRaceHorseData>? JrdbRaceHorses { get; set; }

    #endregion

    #region App

    public DbSet<RaceHorseExtraData>? RaceHorseExtras { get; set; }

    public DbSet<RaceStandardTimeMasterData>? RaceStandardTimes { get; set; }

    public DbSet<TicketData>? Tickets { get; set; }

    public DbSet<RaceChangeData>? RaceChanges { get; set; }

    public DbSet<MemoData>? Memos { get; set; }

    public DbSet<ExpansionMemoConfig>? MemoConfigs { get; set; }

    public DbSet<PointLabelData>? PointLabels { get; set; }

    public DbSet<ExternalNumberData>? ExternalNumbers { get; set; }

    public DbSet<ExternalNumberConfig>? ExternalNumberConfigs { get; set; }

    public DbSet<FinderConfigData>? FinderConfigs { get; set; }

    public DbSet<FinderColumnData>? FinderColumns { get; set; }

    public DbSet<CheckHorseData>? CheckHorses { get; set; }

    public DbSet<AnalysisTableData>? AnalysisTables { get; set; }

    public DbSet<AnalysisTableRowData>? AnalysisTableRows { get; set; }

    public DbSet<AnalysisTableWeightData>? AnalysisTableWeights { get; set; }

    public DbSet<AnalysisTableWeightRowData>? AnalysisTableWeightRows { get; set; }

    public DbSet<AnalysisTableScriptData>? AnalysisTableScripts { get; set; }

    public DbSet<DelimiterData>? Delimiters { get; set; }

    public DbSet<DelimiterRowData>? DelimiterRows { get; set; }

    public DbSet<HorseMarkConfigData>? HorseMarkConfigs { get; set; }

    public DbSet<HorseMarkData>? HorseMarks { get; set; }

    #endregion

    private IDbContextTransaction? _transaction;

    protected string ConnectionString { get; set; } = string.Empty;

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // => optionsBuilder.UseMySql($@"server=localhost;database=kmykeiba;uid=root;pwd=takaki;", ServerVersion.AutoDetect(@"server=localhost;database=kmykeiba;uid=root;pwd=takaki;"));
    // => optionsBuilder.UseMySql(this.ConnectionString, ServerVersion.AutoDetect(this.ConnectionString));

    public async Task CommitAsync()
    {
      if (this._transaction != null)
      {
        await this._transaction.CommitAsync();
        await this._transaction.DisposeAsync();
        this._transaction = await this.Database.BeginTransactionAsync();
      }
    }

    public async Task BeginTransactionAsync()
    {
      this._transaction = await this.GenerateTransactionAsync();
    }

    protected virtual async Task<IDbContextTransaction> GenerateTransactionAsync()
    {
      return await this.Database.BeginTransactionAsync();
    }

    public async Task TryBeginTransactionAsync()
    {
      var tryCount = 0;
      var isSucceed = false;
      while (!isSucceed)
      {
        try
        {
          await this.BeginTransactionAsync();
          isSucceed = true;
        }
        catch (Exception ex) when (ex.Message.Contains('5') && ex.Message.ToLower().Contains("sqlite") && ex.Message.Contains("lock"))
        {
          logger.Warn("トランザクション試行失敗", ex);

          tryCount++;
          if (tryCount >= 30 * 60)
          {
            throw new Exception(ex.Message, ex);
          }
          await Task.Delay(1000);
        }
      }
    }

    public override void Dispose()
    {
      this._transaction?.Commit();

      base.Dispose();
      this._transaction?.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      this._transaction?.DisposeAsync();
    }

    public void Rollback()
    {
      this._transaction?.Rollback();
      this._transaction = null;
    }

    public async Task RollbackAsync()
    {
      if (this._transaction != null)
      {
        await this._transaction.RollbackAsync();
        this._transaction = null;
      }
    }
  }
}
