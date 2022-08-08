using KmyKeiba.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.ExNumber
{
  public class ExternalNumberConfigModel
  {
    public static ExternalNumberConfigModel Default { get; } = new();

    private ExternalNumberConfigModel()
    {
    }
  }
}
