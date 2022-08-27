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
    public string Code { get; set; } = string.Empty;

    public string Memo { get; set; } = string.Empty;
  }

  [Flags]
  public enum HorseCheckType : short
  {
    Unset = 0,
    CheckBlood = 1,
  }
}
