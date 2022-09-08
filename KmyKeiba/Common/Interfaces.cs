using KmyKeiba.Data.Db;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KmyKeiba.Common
{
  public interface IHorseMarkSetter
  {
    ReactiveProperty<RaceHorseMark> Mark { get; }

    ICommand SetMarkCommand { get; }
  }
}
