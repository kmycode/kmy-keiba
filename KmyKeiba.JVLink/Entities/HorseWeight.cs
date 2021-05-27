using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class HorseWeight : EntityBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public IReadOnlyList<Info> Infos { get; set; } = new Info[] { };

    public struct Info
    {
      public int HorseNumber { get; init; }

      public short Weight { get; init; }

      public short WeightDiff { get; init; }
    }

    internal HorseWeight()
    {
    }

    internal static HorseWeight FromJV(JVData_Struct.JV_WH_BATAIJYU weight)
    {
      var infos = new List<Info>();
      foreach (var w in weight.BataijyuInfo)
      {
        int.TryParse(w.Umaban, out int number);
        short.TryParse(w.BaTaijyu.Trim(), out short wei);
        short.TryParse(w.ZogenSa.Trim(), out short weightDiff);
        if (w.ZogenFugo != "+")
        {
          weightDiff *= -1;
        }

        var info = new Info
        {
          HorseNumber = number,
          Weight = wei,
          WeightDiff = weightDiff,
        };
        infos.Add(info);
      }

      var obj = new HorseWeight
      {
        LastModified = weight.head.MakeDate.ToDateTime(),
        RaceKey = weight.id.ToRaceKey(),
        DataStatus = weight.head.DataKubun.ToDataStatus(),
        Infos = infos,
      };
      return obj;
    }

    public override int GetHashCode() => this.RaceKey.GetHashCode();
  }
}
