using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace KmyKeiba.Shared
{
  internal static class Constrants
  {
    public static readonly string AppDataPath = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KMYsofts", "KMYKeiba");

    public static readonly string DatabasePath = Path.Combine(AppDataPath, "maindata.sqlite3");

    public static readonly string ShutdownFilePath = Path.Combine(AppDataPath, "req_shutdown");

    public static readonly string DebugFilePath = Path.Combine(AppDataPath, "debug");

    public const string ApplicationVersion = "1.0.0 rc";
  }
}
