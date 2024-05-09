using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Data;
using KmyKeiba.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Injection.Private
{
  internal class TimeDeviationValueCalculator : ITimeDeviationValueCalculator
  {
    private async Task<double> GetCourseCorrectionAsync(RaceData race, RaceStandardTimeMasterData standardTime, string average)
    {
      var sampleRace = new RaceData
      {
        StartTime = race.StartTime,
        TrackType = race.TrackType,
        TrackGround = race.TrackGround,
        TrackOption = race.TrackOption,
        TrackCondition = race.TrackCondition,
        Distance = race.Distance,
        Course = RaceCourse.Nakayama,
      };

      using var db = new MyContext();
      var baseTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, sampleRace);

      // UmaConnしか使ってない人用
      if (baseTime.SampleCount == 0)
      {
        sampleRace.TrackType = TrackType.Unknown;
        baseTime = await this.LoadBaseStandardTimeFromFileAsync(sampleRace);
      }

      if (baseTime.SampleCount > 0)
      {
        if (average == nameof(RaceStandardTimeMasterData.Average))
        {
          return standardTime.Average / baseTime.Average;
        }
        if (average == nameof(RaceStandardTimeMasterData.A3FAverage))
        {
          return standardTime.A3FAverage / baseTime.A3FAverage;
        }
        if (average == nameof(RaceStandardTimeMasterData.UntilA3FAverage))
        {
          return standardTime.UntilA3FAverage / baseTime.UntilA3FAverage;
        }
      }

      return race.Course <= RaceCourse.CentralMaxValue ? 1.0 : 0.9;
    }

    public async Task<double> GetA3HTimeDeviationValueAsync(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData standardTime)
    {
      if (standardTime.SampleCount == 0 || horse.AfterThirdHalongTime == default)
      {
        return default;
      }

      var value = StatisticSingleArray.CalcDeviationValue(horse.AfterThirdHalongTime.TotalSeconds, standardTime.A3FAverage, standardTime.A3FDeviation);
      value *= await this.GetCourseCorrectionAsync(race, standardTime, nameof(RaceStandardTimeMasterData.A3FAverage));
      return 100 - value;
    }

    public async Task<double> GetUntilA3HTimeDeviationValueAsync(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData standardTime)
    {
      if (standardTime.SampleCount == 0 || horse.AfterThirdHalongTime == default || race.Distance < 800)
      {
        return default;
      }

      var value = StatisticSingleArray.CalcDeviationValue((horse.ResultTime - horse.AfterThirdHalongTime).TotalSeconds / (race.Distance - 600), standardTime.UntilA3FAverage, standardTime.UntilA3FDeviation);
      value *= await this.GetCourseCorrectionAsync(race, standardTime, nameof(RaceStandardTimeMasterData.UntilA3FAverage));
      return 100 - value;
    }

    public async Task<double> GetTimeDeviationValueAsync(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData standardTime)
    {
      if (standardTime.SampleCount == 0 || horse.ResultTime == default || race.Distance == 0)
      {
        return default;
      }

      var weight = (58 - horse.RiderWeight / 10.0) * 0.7;
      var resultTimePerMeter = (double)horse.ResultTime.TotalSeconds / race.Distance;

      var value = StatisticSingleArray.CalcDeviationValue(resultTimePerMeter, standardTime.Average, standardTime.Deviation) + weight;
      value *= await this.GetCourseCorrectionAsync(race, standardTime, nameof(RaceStandardTimeMasterData.Average));
      return 100 - value;
    }

    private List<RaceStandardTimeMasterData>? _masterData;
    private async Task<RaceStandardTimeMasterData> LoadBaseStandardTimeFromFileAsync(RaceData race)
    {
      if (!File.Exists(InternalDataGenerator.BaseStandardTimeFileName))
      {
        return AnalysisUtil.DefaultStandardTime;
      }

      if (this._masterData == null)
      {
        Task.Delay(new System.Random().Next(10, 1000)).Wait();
        lock (this)
        {
          if (this._masterData == null)
          {
            this._masterData = new List<RaceStandardTimeMasterData>();
            var data = File.ReadAllLines(InternalDataGenerator.BaseStandardTimeFileName);
            foreach (var line in data)
            {
              var columns = line.Split(',');
              if (columns.Length < 19) continue;
              short.TryParse(columns[0], out var startYear);
              short.TryParse(columns[1], out var allCount);
              short.TryParse(columns[2], out var condition);
              short.TryParse(columns[3], out var trackType);
              short.TryParse(columns[4], out var trackOption);
              short.TryParse(columns[5], out var direction);
              double.TryParse(columns[6], out var average);
              double.TryParse(columns[7], out var median);
              double.TryParse(columns[8], out var deviation);
              double.TryParse(columns[9], out var a3hAverage);
              double.TryParse(columns[10], out var a3hMedian);
              double.TryParse(columns[11], out var a3hDeviation);
              double.TryParse(columns[12], out var ua3hAverage);
              double.TryParse(columns[13], out var ua3hMedian);
              double.TryParse(columns[14], out var ua3hDeviation);
              short.TryParse(columns[15], out var distance);
              short.TryParse(columns[16], out var distanceMax);
              short.TryParse(columns[17], out var ground);
              short.TryParse(columns[18], out var weather);

              this._masterData.Add(new RaceStandardTimeMasterData
              {
                SampleStartTime = new DateTime(startYear, 1, 1),
                SampleEndTime = new DateTime(startYear + 2, 1, 1),
                SampleCount = allCount,
                Course = RaceCourse.Nakayama,
                Condition = (RaceCourseCondition)condition,
                TrackType = (TrackType)trackType,
                TrackOption = (TrackOption)trackOption,
                CornerDirection = (TrackCornerDirection)direction,
                Average = average,
                Median = median,
                Deviation = deviation,
                A3FAverage = a3hAverage,
                A3FMedian = a3hMedian,
                A3FDeviation = a3hDeviation,
                UntilA3FAverage = ua3hAverage,
                UntilA3FMedian = ua3hMedian,
                UntilA3FDeviation = ua3hDeviation,
                Distance = distance,
                DistanceMax = distanceMax,
                Ground = (TrackGround)ground,
                Weather = (RaceCourseWeather)weather,
              });
            }
          }
        }
      }

      return await AnalysisUtil.GetRaceStandardTimeAsync(null, race, new Dictionary<RaceCourse, IReadOnlyList<RaceStandardTimeMasterData>> { { RaceCourse.Nakayama, this._masterData }, });
    }

    public double GetPciDeviationValue(RaceData race, double pci, RaceStandardTimeMasterData standardTime)
    {
      if (standardTime.SampleCount == 0 || pci == default || race.Distance == 0)
      {
        return default;
      }

      return StatisticSingleArray.CalcDeviationValue(pci, standardTime.PciAverage, standardTime.PciDeviation);
    }
  }
}
