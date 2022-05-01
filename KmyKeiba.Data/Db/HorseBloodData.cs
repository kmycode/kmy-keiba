﻿using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  // [Index(nameof(Key), nameof(Code))]
  public class HorseBloodData : DataBase<HorseBlood>
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

    public int BornYear { get; set; }

    public HorseSex Sex { get; set; }

    public HorseBodyColor Color { get; set; }

    public HorseBloodFrom From { get; set; }

    public string ProductingName { get; set; } = string.Empty;

    public string FatherKey { get; set; } = string.Empty;

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
