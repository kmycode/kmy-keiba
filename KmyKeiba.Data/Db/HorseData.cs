using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  // [Index(nameof(Code), nameof(FatherBreedingCode), nameof(MotherBreedingCode))]
  public class HorseData : DataBase<Horse>
  {
    public string Code { get; set; } = string.Empty;

    public DateTime Entried { get; set; }

    public DateTime Retired { get; set; }

    public DateTime Born { get; set; }

    public string Name { get; set; } = string.Empty;

    public HorseSex Sex { get; set; }

    public HorseType Type { get; set; }

    public HorseBodyColor Color { get; set; }

    public string FatherBreedingCode { get; set; } = string.Empty;

    public string MotherBreedingCode { get; set; } = string.Empty;

    public string FFBreedingCode { get; set; } = string.Empty;

    public string FMBreedingCode { get; set; } = string.Empty;

    public string MFBreedingCode { get; set; } = string.Empty;

    public string MMBreedingCode { get; set; } = string.Empty;

    public string FFFBreedingCode { get; set; } = string.Empty;

    public string FFMBreedingCode { get; set; } = string.Empty;

    public string FMFBreedingCode { get; set; } = string.Empty;

    public string FMMBreedingCode { get; set; } = string.Empty;

    public string MFFBreedingCode { get; set; } = string.Empty;

    public string MFMBreedingCode { get; set; } = string.Empty;

    public string MMFBreedingCode { get; set; } = string.Empty;

    public string MMMBreedingCode { get; set; } = string.Empty;

    public HorseBelongs Belongs { get; set; }

    public string TrainerCode { get; set; } = string.Empty;

    public string TrainerName { get; set; } = string.Empty;

    public string InviteFrom { get; set; } = string.Empty;

    public override void SetEntity(Horse horse)
    {
      this.LastModified = horse.LastModified;
      this.DataStatus = horse.DataStatus;
      this.Code = horse.Code;
      this.Entried = horse.Entried;
      this.Retired = horse.Retired;
      this.Born = horse.Born;
      this.Name = horse.Name;
      this.Sex = horse.Sex;
      this.Color = horse.Color;
      this.Type = horse.Type;
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
      this.Belongs = horse.Belongs;
      this.TrainerCode = horse.TrainerCode;
      this.TrainerName = horse.TrainerName;
      this.InviteFrom = horse.InviteFrom;
    }
  }
}

