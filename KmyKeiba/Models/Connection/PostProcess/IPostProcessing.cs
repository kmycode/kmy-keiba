using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection.PostProcess
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
    public static RunningStyleProcess RunningStyle { get; } = new();
    public static PreviousRaceDaysProcess PreviousRaceDays { get; } = new();
    public static RaceSubjectInfosProcess RaceSubjectInfos { get; } = new();
    public static ResetHorseExtraDataProcess ResetHorseExtraData { get; } = new();
    public static HorseExtraDataProcess HorseExtraData { get; } = new();
    public static StandardTimeProcess StandardTime { get; } = new();

    public static PostProcessingCollection AfterDownload { get; } =
    [
      RemoveInvalidData,
      RunningStyle,
      PreviousRaceDays,
      RaceSubjectInfos,
      HorseExtraData,
    ];

    public static PostProcessingCollection AfterRTDownload { get; } =
    [
      RemoveInvalidData,
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
}
