using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Logics
{
  class MainModel
  {
    public ObservableCollection<Race> Races { get; } = new();

    public MainModel()
    {

    }
  }
}
