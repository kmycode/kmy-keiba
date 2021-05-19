using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class RaceHorse
  {
    public uint Id { get; set; }

    /// <summary>
    /// 出場するレースID
    /// </summary>
    public string RaceKey { get; set; } = string.Empty;

    /// <summary>
    /// 名前
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 番号
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// 枠番
    /// </summary>
    public int FrameNumber { get; set; }

    /// <summary>
    /// 着順
    /// </summary>
    public int ResultOrder { get; set; }

    /// <summary>
    /// 人気
    /// </summary>
    public int Popular { get; set; }

    internal static RaceHorse FromJV(JVData_Struct.JV_SE_RACE_UMA uma)
    {
      int.TryParse(uma.Umaban.Trim(), out int num);
      int.TryParse(uma.Wakuban.Trim(), out int wakuNum);
      int.TryParse(uma.KakuteiJyuni.Trim(), out int result);
      int.TryParse(uma.Ninki.Trim(), out int pop);

      var horse = new RaceHorse
      {
        RaceKey = uma.id.ToRaceKey(),
        Name = uma.Bamei.Trim(),
        Number = num,
        FrameNumber = wakuNum,
        ResultOrder = result,
        Popular = pop,
      };
      return horse;
    }

    public override int GetHashCode() => this.Name.GetHashCode();
  }
}
