using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class RiderData
  {
    public string Code { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string DisplayName => !string.IsNullOrWhiteSpace(this.FullName) ? this.FullName : this.Name;
  }
}
