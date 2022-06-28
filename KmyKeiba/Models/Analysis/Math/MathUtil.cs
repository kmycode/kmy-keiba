using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis.Math
{
  internal static class MathUtil
  {
    public static double AvoidNan(double num)
    {
      if (!double.IsNaN(num))
      {
        return num;
      }
      return default;
    }
  }
}
