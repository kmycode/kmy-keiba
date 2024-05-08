using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

namespace KmyKeiba.Models.Connection.Connector
{
  public interface IPostProcessing
  {
    ProcessingStep Step { get; }

    Task RunAsync();
  }

  public static class PostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public static async Task RunAsync(ReactiveProperty<ProcessingStep> step, bool isRt, PostProcessingCollection steps)
    {
      var state = DownloadStatus.Instance;

      state.IsCancelProcessing.Value = false;

      var isDownloading = isRt ? state.IsRTDownloading : state.IsDownloading;
      var downloadingLink = isRt ? state.RTDownloadingLink : state.DownloadingLink;
      var isProcessing = isRt ? state.IsRTProcessing : state.IsProcessing;
      var isError = isRt ? state.IsRTError : state.IsError;
      var errorMessage = isRt ? state.RTErrorMessage : state.ErrorMessage;

      isDownloading.Value = true;
      downloadingLink.Value = default;
      isProcessing.Value = true;
      isError.Value = false;

      try
      {
        await steps.RunAsync(step);

        DownloaderModel.Instance.OnRacesUpdated();
      }
      catch (Exception ex)
      {
        logger.Error($"後処理中にエラーが発生しました isRT: {isRt}", ex);
        isError.Value = true;
        errorMessage.Value = "後処理中にエラーが発生しました";
      }
      finally
      {
        isDownloading.Value = false;
        isProcessing.Value = false;
        step.Value = ProcessingStep.Unknown;
        state.IsCancelProcessing.Value = false;
        logger.Info($"後処理完了 isRT: {isRt}");
      }
    }
  }

  public static class PostProcessings
  {
    public static RemoveInvalidDataProcess RemoveInvalidData { get; } = new();
    public static MigrateFrom250Process MigrateFrom250 { get; } = new();
    public static MigrateFrom322Process MigrateFrom322 { get; } = new();
    public static MigrateFrom430Process MigrateFrom430 { get; } = new();
    public static MigrateFrom500Process MigrateFrom500 { get; } = new();
    public static TrainRunningStyleProcess TrainRunningStyle { get; } = new();
    public static RunningStyleProcess RunningStyle { get; } = new();
    public static PreviousRaceDaysProcess PreviousRaceDays { get; } = new();
    public static RiderWinRatesProcess RiderWinRates { get; } = new();
    public static RaceSubjectInfosProcess RaceSubjectInfos { get; } = new();
    public static ResetHorseExtraDataProcess ResetHorseExtraData { get; } = new();
    public static HorseExtraDataProcess HorseExtraData { get; } = new();
    public static StandardTimeProcess StandardTime { get; } = new();

    public static PostProcessingCollection AfterDownload { get; } =
    [
      RemoveInvalidData,
      TrainRunningStyle,
      RunningStyle,
      PreviousRaceDays,
      RiderWinRates,
      RaceSubjectInfos,
      HorseExtraData,
    ];

    public static PostProcessingCollection AfterRTDownload { get; } =
    [
      RemoveInvalidData,
      TrainRunningStyle,
      RunningStyle,
    ];
  }

  public class PostProcessingCollection : List<IPostProcessing>
  {
    public async Task RunAsync(IReactiveProperty<ProcessingStep> step)
    {
      var state = DownloadStatus.Instance;

      state.HasProcessingProgress.Value = true;

      try
      {
        foreach (var item in this)
        {
          step.Value = item.Step;
          await item.RunAsync();
        }
      }
      finally
      {
        state.HasProcessingProgress.Value = false;
      }
    }
  }

  public class RemoveInvalidDataProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.InvalidData;

    public async Task RunAsync()
    {
      logger.Info($"後処理進捗変更: {this.Step}");
      await ShapeDatabaseModel.RemoveInvalidDataAsync();
    }
  }

  public class MigrateFrom250Process : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.MigrationFrom250;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {this.Step}");
      await ShapeDatabaseModel.MigrateFrom250Async(isCanceled: state.IsCancelProcessing);
    }
  }

  public class MigrateFrom322Process : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.MigrationFrom322;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {this.Step}");
      await ShapeDatabaseModel.MigrateFrom322Async(isCanceled: state.IsCancelProcessing);
    }
  }

  public class MigrateFrom430Process : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.MigrationFrom430;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {this.Step}");
      await ShapeDatabaseModel.MigrateFrom430Async(isCanceled: state.IsCancelProcessing);
    }
  }

  public class MigrateFrom500Process : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.MigrationFrom500;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {this.Step}");
      await ShapeDatabaseModel.MigrateFrom500Async(isCanceled: state.IsCancelProcessing);
    }
  }

  public class TrainRunningStyleProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.RunningStyle;

    public Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {this.Step}");
      ShapeDatabaseModel.TrainRunningStyle(isForce: Connectors.Central.IsRunning.Value);

      return Task.CompletedTask;
    }
  }

  public class RunningStyleProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.RunningStyle;

    public Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {this.Step}");
      ShapeDatabaseModel.StartRunningStylePredicting();

      return Task.CompletedTask;
    }
  }

  public class PreviousRaceDaysProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.PreviousRaceDays;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {this.Step}");
      await ShapeDatabaseModel.SetHorseExtraDataAsync(
        isCanceled: state.IsCancelProcessing,
        progress: state.ProcessingProgress,
        progressMax: state.ProcessingProgressMax
      );
    }
  }

  public class RiderWinRatesProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.RiderWinRates;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {this.Step}");
      await ShapeDatabaseModel.SetRiderWinRatesAsync(
        isCanceled: state.IsCancelProcessing,
        progress: state.ProcessingProgress,
        progressMax: state.ProcessingProgressMax
      );
    }
  }

  public class RaceSubjectInfosProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.RaceSubjectInfos;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {this.Step}");
      await ShapeDatabaseModel.SetRaceSubjectDisplayInfosAsync(isCanceled: state.IsCancelProcessing);
    }
  }

  public class ResetHorseExtraDataProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.ResetHorseExtraData;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {this.Step}");
      await ShapeDatabaseModel.ResetHorseExtraTableDataAsync();
    }
  }

  public class HorseExtraDataProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.HorseExtraData;

    public async Task RunAsync()
    {
      if (!DownloadConfig.Instance.IsBuildExtraData.Value) return;

      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {this.Step}");
      await ShapeDatabaseModel.SetHorseExtraTableDataAsync(
        isCanceled: state.IsCancelProcessing,
        progress: state.ProcessingProgress,
        progressMax: state.ProcessingProgressMax
      );
    }
  }

  public class StandardTimeProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.StandardTime;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {this.Step}");
      await ShapeDatabaseModel.MakeStandardTimeMasterDataAsync(
        1990,
        isCanceled: state.IsCancelProcessing,
        progressMax: state.ProcessingProgressMax,
        progress: state.ProcessingProgress
      );
      await ConfigUtil.SetIntValueAsync(SettingKey.LastUpdateStandardTimeYear, DateTime.Today.Year);
    }
  }
}
