using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(Key))]
  public class HorseBloodInfoData : DataBase<HorseBloodInfo>
  {
    /// <summary>
    ///  繁殖登録番号
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 系統ID
    /// </summary>
    [StringLength(32)]
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// 系統名
    /// </summary>
    [StringLength(40)]
    public string FamilyName { get; set; } = string.Empty;

    public override void SetEntity(HorseBloodInfo horse)
    {
      this.LastModified = horse.LastModified;
      this.DataStatus = horse.DataStatus;
      this.Key = horse.Key;
      this.FamilyId = horse.FamilyId;
      this.FamilyName = horse.FamilyName;
    }
  }
}
