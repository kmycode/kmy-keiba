using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection.PostProcess
{
  public class RaceSubjectInfosProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.RaceSubjectInfos;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {Step}");
      await SetRaceSubjectDisplayInfosAsync(isCanceled: state.IsCancelProcessing);
    }

    private static async Task SetRaceSubjectDisplayInfosAsync(DateOnly? startMonth = null, bool isForce = false, ReactiveProperty<bool>? isCanceled = null)
    {
      DateTime month;
      if (startMonth != null)
      {
        month = new DateTime(startMonth.Value.Year, startMonth.Value.Month, 1);
      }
      else
      {
        month = new DateTime(1986, 1, 1);
      }

      using var db = new MyContext();
      await db.TryBeginTransactionAsync();

      var races = db.Races!.Where(r => r.StartTime >= month && r.Course >= RaceCourse.LocalMinValue);
      if (!isForce)
      {
        races = races.Where(r => r.SubjectDisplayInfo == string.Empty);
      }
      races = races.OrderBy(r => r.StartTime);

      var count = 0;

      try
      {
        foreach (var race in races)
        {
          var subject = new RaceSubjectInfo(race);
          var cls1 = subject.Subject.DisplayClass.ToString()?.ToLower() ?? string.Empty;
          var cls2 = subject.Subject.SecondaryClass?.ToString()?.ToLower() ?? string.Empty;
          race.SubjectDisplayInfo = $"{cls1}/{cls2}/{subject.Subject.ClassName}";
          race.SubjectInfo1 = cls1;
          race.SubjectInfo2 = cls2;
          count++;

          if (count > 10000)
          {
            await db.SaveChangesAsync();
            await db.CommitAsync();
            count = 0;

            if (isCanceled?.Value == true)
            {
              return;
            }
          }
        }
        await db.SaveChangesAsync();
        await db.CommitAsync();
      }
      catch (Exception ex)
      {
        logger.Error("レースの条件解析中にエラー", ex);
      }
    }
  }
}
