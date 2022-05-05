using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public abstract class MyContextBase : DbContext
  {
    public DbSet<SystemData>? SystemData { get; set; }

    #region Data

    public DbSet<RaceData>? Races { get; set; }

    public DbSet<RaceHorseData>? RaceHorses { get; set; }

    public DbSet<HorseData>? Horses { get; set; }

    public DbSet<HorseBloodData>? HorseBloods { get; set; }

    public DbSet<SingleOddsTimeline>? SingleOddsTimelines { get; set; }

    public DbSet<FrameNumberOddsData>? FrameNumberOdds { get; set; }

    public DbSet<QuinellaOddsData>? QuinellaOdds { get; set; }

    public DbSet<QuinellaPlaceOddsData>? QuinellaPlaceOdds { get; set; }

    public DbSet<ExactaOddsData>? ExactaOdds { get; set; }

    public DbSet<TrioOddsData>? TrioOdds { get; set; }

    public DbSet<TrifectaOddsData>? TrifectaOdds { get; set; }

    public DbSet<RefundData>? Refunds { get; set; }

    public DbSet<LearningDataCache>? LearningDataCaches { get; set; }

    public DbSet<TrainingData>? Trainings { get; set; }

    public DbSet<WoodtipTrainingData>? WoodtipTrainings { get; set; }

    #endregion

    #region Analysis

    public DbSet<RaceHorseAnalysisData>? RaceHorseAnalysis { get; set; }

    #endregion

    protected string ConnectionString { get; set; } = string.Empty;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // => optionsBuilder.UseMySql($@"server=localhost;database=kmykeiba;uid=root;pwd=takaki;", ServerVersion.AutoDetect(@"server=localhost;database=kmykeiba;uid=root;pwd=takaki;"));
        => optionsBuilder.UseMySql(this.ConnectionString, ServerVersion.AutoDetect(this.ConnectionString));
  }
}
