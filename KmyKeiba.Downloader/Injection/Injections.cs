using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Downloader.Injection
{
  internal interface ICentralSoftwareIdGetter
  {
    string InitializationKey { get; }
  }

  internal interface ILocalSoftwareIdGetter
  {
    string InitializationKey { get; }
  }
}
