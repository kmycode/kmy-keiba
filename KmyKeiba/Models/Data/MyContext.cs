using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Data
{
  public class MyContext : DbContext
  {
    public DbSet<SystemData>? SystemData { get; set; }

    public DbSet<RaceData>? Races { get; set; }

    public DbSet<RaceHorseData>? RaceHorses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql(@"server=localhost;database=kmykeiba;uid=root;pwd=takaki;", ServerVersion.AutoDetect(@"server=localhost;database=kmykeiba;uid=root;pwd=takaki;"));
  }
}
