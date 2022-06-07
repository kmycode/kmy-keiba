using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

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

    public static readonly string DatabasePath = Path.Combine(AppDataDir, "maindata.sqlite3");

    public static readonly string ShutdownFilePath = Path.Combine(AppDataDir, "req_shutdown");

    public static readonly string DebugFilePath = Path.Combine(AppDataDir, "debug");

    public static readonly string RTHostFilePath = Path.Combine(AppDataDir, "rthost");

    public static readonly string RunningStyleTrainingFilePath = Path.Combine(AppDataDir, "runningstyle.mml");

    public const string ApplicationVersion = "1.1.2";
  }
}
