using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.ML.Brain
{
  internal static class OptimizerManager
  {
    public static dynamic? Radam
    {
      get
      {
        if (!_isRadamChecked)
        {
          if (!PythonEngine.IsInitialized)
          {
            PythonEngine.Initialize();
          }
          try
          {
            _radam = Py.Import("keras_radam");
          }
          catch
          {
            _radam = null;
          }
          _isRadamChecked = true;
        }

        return _radam;
      }
    }
    private static dynamic? _radam;
    private static bool _isRadamChecked;
  }
}
