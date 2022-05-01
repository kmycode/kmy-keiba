using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class HorseBlood : EntityBase
  {
    /// <summary>
    /// 繁殖登録番号
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    ///  血統登録番号
    /// </summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public short BornYear { get; set; }

    public short ImportYear { get; set; }

    public HorseSex Sex { get; set; }

    public HorseBodyColor Color { get; set; }

    public HorseBloodFrom From { get; set; }

    public string ProductingName { get; set; } = string.Empty;

    public string FatherKey { get; set; } = string.Empty;

    public string MotherKey { get; set; } = string.Empty;

    public static HorseBlood FromJV(JVData_Struct.JV_HN_HANSYOKU uma)
    {
      short.TryParse(uma.SexCD, out var sex);
      short.TryParse(uma.KeiroCD, out var color);
      short.TryParse(uma.HansyokuMochiKubun, out var from);
      short.TryParse(uma.BirthYear, out var year);
      short.TryParse(uma.ImportYear, out var year2);

      return new()
      {
        LastModified = uma.head.MakeDate.ToDateTime(),
        DataStatus = uma.head.DataKubun.ToDataStatus(),
        Key = uma.HansyokuNum.Trim(),
        Code = uma.KettoNum.Trim(),
        BornYear = year,
        ImportYear = year2,
        Name = uma.Bamei.Trim(),
        Sex = (HorseSex)sex,
        Color = (HorseBodyColor)color,
        From = (HorseBloodFrom)from,
        FatherKey = uma.HansyokuFNum.Trim(),
        MotherKey = uma.HansyokuMNum.Trim(),
        ProductingName = uma.SanchiName.Trim(),
      };
    }
  }

  public enum HorseBloodFrom : short
  {
    MyCountry = 0, // 内国産
    BringIn = 1,  // 持ち込み
    ImportA = 2,  // 輸入内国産扱い
    ImportB = 3,
    Others = 9,
  }
}
