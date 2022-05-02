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
  [Index(nameof(Key), nameof(Code))]
  public class HorseBloodData : DataBase<HorseBlood>
  {
    /// <summary>
    /// 繁殖登録番号
    /// </summary>
    [StringLength(12)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    ///  血統登録番号
    /// </summary>
    [StringLength(16)]
    public string Code { get; set; } = string.Empty;

    [StringLength(72)]
    public string Name { get; set; } = string.Empty;

    public int BornYear { get; set; }

    public HorseSex Sex { get; set; }

    public HorseBodyColor Color { get; set; }

    public HorseBloodFrom From { get; set; }

    [StringLength(40)]
    public string ProductingName { get; set; } = string.Empty;

    [StringLength(12)]
    public string FatherKey { get; set; } = string.Empty;

    [StringLength(12)]
    public string MotherKey { get; set; } = string.Empty;

    public override void SetEntity(HorseBlood horse)
    {
      this.LastModified = horse.LastModified;
      this.DataStatus = horse.DataStatus;
      this.Key = horse.Key;
      this.Code = horse.Code;
      this.Name = horse.Name;
      this.BornYear = horse.BornYear;
      this.Sex = horse.Sex;
      this.Color = horse.Color;
      this.From = horse.From;
      this.ProductingName = horse.ProductingName;
      this.FatherKey = horse.FatherKey;
      this.MotherKey = horse.MotherKey;
    }
  }
}
