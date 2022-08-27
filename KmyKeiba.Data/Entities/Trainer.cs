using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class Trainer : EntityBase
  {
    public string Code { get; set; } = string.Empty;

    public bool IsCentral => this.Belongs != HorseBelongs.Local;

    public short CentralFlag => this.IsCentral ? (short)1 : (short)0;

    public bool IsDeleted { get; set; }

    public HorseBelongs Belongs { get; set; }

    public HumanSex Sex { get; set; }
    
    public DateTime Issued { get; set; }

    public DateTime Deleted { get; set; }

    public DateTime Born { get; set; }

    public string Name { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    internal Trainer()
    {
    }

    public static Trainer FromJV(JVData_Struct.JV_CH_CHOKYOSI trainer)
    {
      short.TryParse(trainer.TozaiCD, out var belongs);
      short.TryParse(trainer.SexCD, out var sex);
      short.TryParse(trainer.DelKubun, out var deleted);

      return new Trainer
      {
        LastModified = trainer.head.MakeDate.ToDateTime(),
        DataStatus = trainer.head.DataKubun.ToDataStatus(),
        Code = trainer.ChokyosiCode.Trim(),
        IsDeleted = deleted != 0,
        Belongs = (HorseBelongs)belongs,
        Sex = (HumanSex)sex,
        Issued = trainer.IssueDate.ToDateTime(),
        Deleted = trainer.DelDate.ToDateTime(),
        Born = trainer.BirthDate.ToDateTime(),
        Name = trainer.ChokyosiName.Trim(),
      };
    }
  }
}
