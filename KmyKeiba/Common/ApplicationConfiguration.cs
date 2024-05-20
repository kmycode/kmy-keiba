using KmyKeiba.Models.Race;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Common
{
  internal class ApplicationConfiguration
  {
    public static ReactiveProperty<ApplicationConfiguration> Current { get; } = new(new ApplicationConfiguration());

    public const int ExpansionMemoGroupSize = 8;
  }
}
