using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace KmyKeiba.Shared
{
  internal static class Constrants
  {
    public static readonly string AppDataDir = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KMYsofts", "KMYKeiba");

    public static readonly string ScriptDir = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KMYsofts", "KMYKeiba", "script");

    public static readonly string AppDir = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KMYsofts", "KMYKeiba", "App");

    public static readonly string MLDir = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KMYsofts", "KMYKeiba", "ml");

    public static string DatabasePath
    {
      get
      {
        if (!string.IsNullOrEmpty(_databasePath))
        {
          return _databasePath;
        }

        var locationPath = Path.Combine(AppDataDir, "database-location.txt");
        if (File.Exists(locationPath))
        {
          var lines = File.ReadAllLines(locationPath);
          if (lines.Length > 0 && lines[0].EndsWith(".sqlite3"))
          {
            _databasePath = lines[0];
            return _databasePath;
          }
        }

        var basePath = Path.Combine(AppDataDir, "maindata.sqlite3");
        _databasePath = basePath;
        return _databasePath;
      }
    }
    private static string? _databasePath;

    public static readonly string ShutdownFilePath = Path.Combine(AppDataDir, "req_shutdown");

    public static readonly string DebugFilePath = Path.Combine(AppDataDir, "debug");

    public static readonly string RTHostFilePath = Path.Combine(AppDataDir, "rthost");

    public static readonly string RunningStyleTrainingFilePath = Path.Combine(AppDataDir, "runningstyle.mml");

    public static readonly string ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
  }
}
