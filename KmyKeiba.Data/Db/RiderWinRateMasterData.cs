using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(RiderCode))]
  public class RiderWinRateMasterData : AppDataBase
  {
    public string RiderCode { get; set; } = string.Empty;

    public short Year { get; set; }

    public short Month { get; set; }

    public short Distance { get; set; }

    public short DistanceMax { get; set; }

    public short AllTurfCount { get; set; }

    public short FirstTurfCount { get; set; }

    public short SecondTurfCount { get; set; }

    public short ThirdTurfCount { get; set; }

    public short AllDirtCount { get; set; }

    public short FirstDirtCount { get; set; }

    public short SecondDirtCount { get; set; }

    public short ThirdDirtCount { get; set; }

    public short AllTurfSteepsCount { get; set; }

    public short FirstTurfSteepsCount { get; set; }

    public short SecondTurfSteepsCount { get; set; }

    public short ThirdTurfSteepsCount { get; set; }

    public short AllDirtSteepsCount { get; set; }

    public short FirstDirtSteepsCount { get; set; }

    public short SecondDirtSteepsCount { get; set; }

    public short ThirdDirtSteepsCount { get; set; }
  }
}
