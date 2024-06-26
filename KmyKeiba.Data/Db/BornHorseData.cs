﻿using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(Code), nameof(MFBreedingCode))]
  public class BornHorseData : DataBase<BornHorse>
  {
    [StringLength(16)]
    public string Code { get; set; } = string.Empty;

    public DateTime Entried { get; set; }

    public DateTime Retired { get; set; }

    public DateTime Born { get; set; }

    [StringLength(72)]
    public string Name { get; set; } = string.Empty;

    public HorseSex Sex { get; set; }

    public HorseType Type { get; set; }

    public HorseBodyColor Color { get; set; }

    [StringLength(12)]
    public string FatherBreedingCode { get; set; } = string.Empty;

    [StringLength(12)]
    public string MotherBreedingCode { get; set; } = string.Empty;

    [StringLength(12)]
    public string FFBreedingCode { get; set; } = string.Empty;

    [StringLength(12)]
    public string FMBreedingCode { get; set; } = string.Empty;

    [StringLength(12)]
    public string MFBreedingCode { get; set; } = string.Empty;

    [StringLength(12)]
    public string MMBreedingCode { get; set; } = string.Empty;

    [StringLength(12)]
    public string FFFBreedingCode { get; set; } = string.Empty;

    [StringLength(12)]
    public string FFMBreedingCode { get; set; } = string.Empty;

    [StringLength(12)]
    public string FMFBreedingCode { get; set; } = string.Empty;

    [StringLength(12)]
    public string FMMBreedingCode { get; set; } = string.Empty;

    [StringLength(12)]
    public string MFFBreedingCode { get; set; } = string.Empty;

    [StringLength(12)]
    public string MFMBreedingCode { get; set; } = string.Empty;

    [StringLength(12)]
    public string MMFBreedingCode { get; set; } = string.Empty;

    [StringLength(12)]
    public string MMMBreedingCode { get; set; } = string.Empty;

    public HorseBelongs Belongs { get; set; }

    [StringLength(8)]
    public string TrainerCode { get; set; } = string.Empty;

    [StringLength(16)]
    public string TrainerName { get; set; } = string.Empty;

    [StringLength(40)]
    public string InviteFrom { get; set; } = string.Empty;

    [StringLength(10)]
    public string OwnerCode { get; set; } = string.Empty;

    public override void SetEntity(BornHorse horse)
    {
      this.LastModified = horse.LastModified;
      this.DataStatus = horse.DataStatus;
      this.Code = horse.Code;
      this.Born = horse.Born;
      this.Sex = horse.Sex;
      this.Color = horse.Color;
      this.FatherBreedingCode = horse.FatherBreedingCode;
      this.MotherBreedingCode = horse.MotherBreedingCode;
      this.FFBreedingCode = horse.FFBreedingCode;
      this.FMBreedingCode = horse.FMBreedingCode;
      this.MFBreedingCode = horse.MFBreedingCode;
      this.MMBreedingCode = horse.MMBreedingCode;
      this.FFFBreedingCode = horse.FFFBreedingCode;
      this.FFMBreedingCode = horse.FFMBreedingCode;
      this.FMFBreedingCode = horse.FMFBreedingCode;
      this.FMMBreedingCode = horse.FMMBreedingCode;
      this.MFFBreedingCode = horse.MFFBreedingCode;
      this.MFMBreedingCode = horse.MFMBreedingCode;
      this.MMFBreedingCode = horse.MMFBreedingCode;
      this.MMMBreedingCode = horse.MMMBreedingCode;
    }
  }
}

