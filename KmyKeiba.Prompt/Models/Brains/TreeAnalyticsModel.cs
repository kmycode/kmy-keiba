using Numpy;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Prompt.Models.Brains
{
  class TreeAnalyticsModel
  {
    private dynamic module;
    private dynamic tree;
    private List<(float[,] Data, float[] Results)> data = new();

    public TreeAnalyticsModel(int maxDepth)
    {
      this.module = Py.Import("sklearn.tree");
      this.tree = module.DecisionTreeRegressor(max_depth: maxDepth);
    }

    public void AddData(float[,] data, float[] results)
    {
      this.data.Add((data, results));
    }

    public void Fit()
    {
      if (!this.data.Any())
      {
        return;
      }

      var arr = new float[this.data.Sum((d) => d.Data.GetLength(0)), this.data[0].Data.GetLength(1)];
      var arr2 = new float[this.data.Sum((d) => d.Results.Length)];

      for (var i = 0; i < arr.GetLength(0); i++)
      {
        for (var j = 0; j < arr.GetLength(1); j++)
        {
          if (float.IsNaN(arr[i, j]))
          {
            this.data.Clear();
            return;
          }
        }
      }

      var x = 0;
      var y = 0;
      foreach (var d in this.data)
      {
        for (var a = 0; a < d.Data.GetLength(0); a++)
        {
          for (var b = 0; b < d.Data.GetLength(1); b++)
          {
            arr[x, b] = d.Data[a, b];
          }
          x++;
        }
        for (var a = 0; a < d.Results.Length; a++)
        {
          arr2[y] = d.Results[a];
          y++;
        }
      }

      var args = new Dictionary<string, object>
      {
        { "x", np.array(arr) },
        { "y", np.array(arr2) },
      };
      InvokeMethod("fit", args);
      // this.tree.fit(np.array(data).ToPython(), ToPython(results));
    }

    public void ExportAsDot(string fileName, string[] featureNames)
    {
      if (!this.data.Any())
      {
        return;
      }

      this.module.export_graphviz(this.tree, out_file: fileName, feature_names: new PyList(featureNames.Select((n) => new PyString(n)).ToArray()), filled: true);
    }

    public PyObject InvokeMethod(string method, Dictionary<string, object> args)
    {
      var pyargs = ToTuple(new object[]
      {
                args.FirstOrDefault().Value
      });

      var kwargs = new PyDict();

      bool skip = true;
      foreach (var item in args)
      {
        if (skip)
        {
          skip = false;
          continue;
        }

        if (item.Value != null && !string.IsNullOrWhiteSpace(item.Value.ToString()))
        {
          kwargs[item.Key] = ToPython(item.Value);
        }
      }

      if (args.Count > 0)
        return this.tree.InvokeMethod(method, pyargs, kwargs);
      else
        return this.tree.InvokeMethod(method, null, null);
    }

    internal static PyObject ToPython(object? obj)
    {
      if (obj == null) return Runtime.None;
      switch (obj)
      {
        // basic types
        case int o: return new PyInt(o);
        case float o: return new PyFloat(o);
        case long o: return new PyLong(o);
        case double o: return new PyFloat(o);
        case string o: return new PyString(o);
        case bool o: return ConverterExtension.ToPython(o);

        // sequence types
        case Array o: return ToList(o);
        // special types from 'ToPythonConversions'
        /*
        case Shape o: return ToTuple(o.Dimensions);
        case ValueTuple<int> o: return ToTuple(o);
        case ValueTuple<int, int> o: return ToTuple(o);
        case ValueTuple<int, int, int> o: return ToTuple(o);
        case Slice o: return o.ToPython();
        case Sequence o: return o.PyInstance;
        case StringOrInstance o: return o.PyObject;
        case KerasFunction o: return o.PyObject;
        case Base o: return o.PyInstance;
        */
        case PythonObject o: return o.PyObject;
        case PyObject o: return o;
        default: throw new NotImplementedException($"Type is not yet supported: { obj.GetType().Name}. Add it to 'ToPythonConversions'");
      }
    }

    protected static PyList ToList(Array input)
    {
      var array = new PyObject[input.Length];
      for (int i = 0; i < input.Length; i++)
      {
        array[i] = ToPython(input.GetValue(i));
      }

      return new PyList(array);
    }

    protected static PyTuple ToTuple(Array input)
    {
      var array = new PyObject[input.Length];
      for (int i = 0; i < input.Length; i++)
      {
        array[i] = ToPython(input.GetValue(i));
      }

      return new PyTuple(array);
    }
  }
}
