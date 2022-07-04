using KmyKeiba.ML.Brain;
using KmyKeiba.ML.Script;
using KmyKeiba.Shared;
using Python.Runtime;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Kmykeiba.ML
{
  public static class Program
  {
    public static void Main(string[] args)
    {
      Console.WriteLine("============ Start ML Program ============");

      //args = new string[] { "training", "local", };

      if (args.Length == 0)
      {
        Error("No arguments");
      }

      var command = args[0];
      var profile = args.Length >= 2 ? args[1] : string.Empty;

      if (command == "training")
      {
        Task.Run(async () =>
        {
          try
          {
            await KerasModel.FromSourceAsync(profile);
            PythonEngine.Shutdown();
          }
          catch (Exception ex)
          {
            Error(ex.Message + "\n" + ex.StackTrace);
          }
        }).Wait();
      }

      if (command == "predict")
      {
        Task.Run(async () =>
        {
          try
          {
            await KerasModel.PredictAsync(profile);
            PythonEngine.Shutdown();
          }
          catch (Exception ex)
          {
            Error(ex.Message + "\n" + ex.StackTrace);
          }
        }).Wait();
      }
    }

    public static void Error(string text)
    {
      File.WriteAllText(Path.Combine(Constrants.MLDir, "error.txt"), text);
      Console.WriteLine(text);
      //Console.ReadKey();
      PythonEngine.Shutdown();
      Environment.Exit(-2);
    }
  }
}
