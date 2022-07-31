using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class HorseBloodInfo : EntityBase
  {
    /// <summary>
    ///  繁殖登録番号
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 系統ID
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// 系統名
    /// </summary>
    public string FamilyName { get; set; } = string.Empty;

    public static HorseBloodInfo FromJV(JVData_Struct.JV_BT_KEITO uma)
    {
      return new()
      {
        LastModified = uma.head.MakeDate.ToDateTime(),
        DataStatus = uma.head.DataKubun.ToDataStatus(),
        Key = uma.HansyokuNum.Trim(),
        FamilyId = uma.KeitoId.Trim(),
        FamilyName = uma.KeitoName.Trim(),
      };
    }
  }
}
