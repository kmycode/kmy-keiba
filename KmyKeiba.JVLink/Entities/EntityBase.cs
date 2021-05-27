using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public interface IEntityBase
  {
    public DateTime LastModified { get; set; }

    public RaceDataStatus DataStatus { get; set; }
  }

  public abstract class EntityBase : IEntityBase
  {
    public DateTime LastModified { get; set; }

    /// <summary>
    ///  データが更新された理由
    /// </summary>
    public RaceDataStatus DataStatus { get; set; } = RaceDataStatus.Unknown;
  }
}
