﻿using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  public class TrainingAnalysisData
  {
    public IReadOnlyList<TrainingRow> Trainings { get; }

    public IReadOnlyList<WoodtipTrainingData> WoodtipTrainings { get; }

    public TrainingAnalysisData(IReadOnlyList<TrainingData> trainings, IReadOnlyList<WoodtipTrainingData> woodtipTrainings)
    {
      var trainingStatistics = new[]
      {
        new StatisticSingleArray(trainings.Select(t => (double)t.FirstLapTime).Where(d => d > 0).ToArray()),
        new StatisticSingleArray(trainings.Select(t => (double)t.SecondLapTime).Where(d => d > 0).ToArray()),
        new StatisticSingleArray(trainings.Select(t => (double)t.ThirdLapTime).Where(d => d > 0).ToArray()),
        new StatisticSingleArray(trainings.Select(t => (double)t.FourthLapTime).Where(d => d > 0).ToArray()),
      };
      var woodtipStatistics = new[]
      {
        new StatisticSingleArray(woodtipTrainings.Select(t => (double)t.Lap1Time).Where(d => d > 0).ToArray()),
        new StatisticSingleArray(woodtipTrainings.Select(t => (double)t.Lap2Time).Where(d => d > 0).ToArray()),
        new StatisticSingleArray(woodtipTrainings.Select(t => (double)t.Lap3Time).Where(d => d > 0).ToArray()),
        new StatisticSingleArray(woodtipTrainings.Select(t => (double)t.Lap4Time).Where(d => d > 0).ToArray()),
        new StatisticSingleArray(woodtipTrainings.Select(t => (double)t.Lap5Time).Where(d => d > 0).ToArray()),
        new StatisticSingleArray(woodtipTrainings.Select(t => (double)t.Lap6Time).Where(d => d > 0).ToArray()),
        new StatisticSingleArray(woodtipTrainings.Select(t => (double)t.Lap7Time).Where(d => d > 0).ToArray()),
        new StatisticSingleArray(woodtipTrainings.Select(t => (double)t.Lap8Time).Where(d => d > 0).ToArray()),
        new StatisticSingleArray(woodtipTrainings.Select(t => (double)t.Lap9Time).Where(d => d > 0).ToArray()),
        new StatisticSingleArray(woodtipTrainings.Select(t => (double)t.Lap10Time).Where(d => d > 0).ToArray()),
      };
      var trainingRows = trainings.Select(t => new TrainingRow(t, trainingStatistics)).ToArray();
      var woodtipTrainingRows = woodtipTrainings.Select(t => new TrainingRow(t, woodtipStatistics)).ToArray();

      this.Trainings = trainingRows
        .Concat(woodtipTrainingRows)
        .OrderByDescending(t => t.StartTime)
        .Take(100)
        .ToArray();
    }

    public class TrainingRow
    {
      public TrainingCenter Center { get; }

      public bool IsWoodtip { get; }

      public WoodtipCourse WoodtipCourse { get; }

      public WoodtipDirection WoodtipDirection { get; }

      public DateTime StartTime { get; }

      public IReadOnlyList<LapTimeData> LapTimes { get; private set; } = Array.Empty<LapTimeData>();

      private ValueComparation GetComparation(short value, StatisticSingleArray statistic)
      {
        // サンプルが少なすぎると表示が変になる
        if (statistic.Values.Length < 3)
        {
          return ValueComparation.Standard;
        }

        if (value >= statistic.GetPositionValue(0.85))
        {
          return ValueComparation.Bad;
        }
        if (value <= statistic.GetPositionValue(0.15))
        {
          return ValueComparation.Good;
        }
        return ValueComparation.Standard;
      }

      private void InitializeValueComparations(IReadOnlyList<StatisticSingleArray> statistics, IReadOnlyList<short> lapTimes)
      {
        if (statistics.Count < lapTimes.Count)
        {
          return;
        }

        var lts = new List<LapTimeData>();
        for (var i = 0; i < lapTimes.Count; i++)
        {
          lts.Add(new LapTimeData
          {
            LapTime = lapTimes[i],
            LapTimeComparation = this.GetComparation(lapTimes[i], statistics[i]),
          });
        }
        this.LapTimes = lts;
      }

      public TrainingRow(TrainingData data, IReadOnlyList<StatisticSingleArray> statistics)
      {
        this.StartTime = data.StartTime;
        this.Center = data.Center;
        var lapTimes = new short[]
        {
          data.FirstLapTime,
          data.SecondLapTime,
          data.ThirdLapTime,
          data.FourthLapTime,
        }.Where(d => d > 0).ToArray();

        this.InitializeValueComparations(statistics, lapTimes);
      }

      public TrainingRow(WoodtipTrainingData data, IReadOnlyList<StatisticSingleArray> statistics)
      {
        this.StartTime = data.StartTime;
        this.Center = data.Center;
        this.WoodtipCourse = data.Course;
        this.WoodtipDirection = data.Direction;
        this.IsWoodtip = true;
        var lapTimes = new short[]
        {
          data.Lap1Time,
          data.Lap2Time,
          data.Lap3Time,
          data.Lap4Time,
          data.Lap5Time,
          data.Lap6Time,
          data.Lap7Time,
          data.Lap8Time,
          data.Lap9Time,
          data.Lap10Time,
        }.Where(d => d > 0).ToArray();

        this.InitializeValueComparations(statistics, lapTimes);
      }

      public struct LapTimeData
      {
        public short LapTime { get; init; }

        public ValueComparation LapTimeComparation { get; init; }
      }
    }
  }
}
