using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class SystemData
  {
    [Key]
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
  }
}
