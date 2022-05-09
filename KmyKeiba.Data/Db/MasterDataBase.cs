using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public abstract class MasterDataBase
  {
    public uint Id { get; set; }

    public DateTime LastModified { get; set; }

    public ushort Version { get; set; }
  }
}
