using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Shared
{
  public static class InjectionManager
  {
    private static readonly Dictionary<string, InstanceItem> instances = new();

    public const string TimeDeviationValueCalculator = "KmyKeiba.Models.Injection.Private.TimeDeviationValueCalculator";

    public const string Buyer = "KmyKeiba.Models.Injection.Private.KscBuyer";

    public const string CentralSoftwareIdGetter = "KmyKeiba.Downloader.Injection.Private.CentralSoftwareIdGetter";

    public static T? GetInstance<T>(string typeName) where T : class
    {
      var exists = instances.TryGetValue(typeName, out var instance);
      if (exists && instance.Instance is T obj)
      {
        return obj;
      }

      if (!exists)
      {
        T? newObj = null;
        var type = Type.GetType(typeName);
        if (type != null)
        {
          newObj = Activator.CreateInstance(type) as T;
        }

        instances.Add(typeName, new InstanceItem
        {
          Instance = newObj,
        });
        return newObj;
      }

      return null;
    }

    private struct InstanceItem
    {
      public object? Instance { get; init; }
    }
  }
}
