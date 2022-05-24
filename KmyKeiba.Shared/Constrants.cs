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

    public const string ApplicationVersion = "v1.0.0";
  }
}
