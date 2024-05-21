using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class CheckHorseData : AppDataBase
  {
    public HorseCheckType Type { get; set; }

    [StringLength(20)]
    public string Key { get; set; } = string.Empty;

    [StringLength(20)]
    [Obsolete("V5.0.0以降使わないが、データベースマイグレーションの関係で削除すると古いデータが残る可能性あり")]
    public string Code { get; set; } = string.Empty;

    public string Memo { get; set; } = string.Empty;
  }

  [Flags]
  public enum HorseCheckType : short
  {
    Unset = 0,
    CheckBlood = 1,
    CheckRace = 2,
  }
}
