using KmyKeiba.Data.Db;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Documents;

namespace KmyKeiba.Shared
{
  internal static class DownloaderTaskDataExtensions
  {
    internal static DownloaderTaskData? FindOrDefault(uint id)
    {
      var task = DownloaderTaskData.LoadFile(ToFilePath(id));

      if (task == null) return null;

      task.Id = id;
      return task;
    }

    internal static DownloaderTaskData[] ToArray() => Enumerate().ToArray();

    internal static IEnumerable<DownloaderTaskData> Enumerate()
    {
      var files = Directory.GetFiles(Constrants.DownloaderTaskDataDir);

      foreach (var file in files.Select(f => Path.GetFileName(f)))
      {
        if (!uint.TryParse(file, out var id)) continue;

        var task = FindOrDefault(id);
        if (task != null)
        {
          yield return task;
        }
      }
    }

    internal static void Add(DownloaderTaskData data)
    {
      if (data.Id == default)
      {
        data.Id = GenerateId();
      }

      Save(data);
    }

    internal static void Save(DownloaderTaskData data)
    {
      if (data.Id == default)
      {
        throw new SaveDownloaderTaskDataException("存在しないタスクデータを保存しようとしました");
      }

      DownloaderTaskData.SaveFile(ToFilePath(data.Id), data);
    }

    internal static void Remove(uint id)
    {
      if (id == default) return;

      if (Exists(id))
      {
        File.Delete(ToFilePath(id));
      }
    }

    internal static void Remove(DownloaderTaskData data)
    {
      Remove(data.Id);
      data.Id = default;
    }

    internal static void RemoveRange(IEnumerable<DownloaderTaskData> dataList)
    {
      foreach (var data in dataList)
      {
        Remove(data.Id);
      }
    }

    internal static void RemoveAll()
    {
      var files = Directory.GetFiles(Constrants.DownloaderTaskDataDir);

      foreach (var file in files)
      {
        File.Delete(file);
      }
    }

    internal static bool Exists(uint id) => File.Exists(ToFilePath(id));

    private static uint GenerateId()
    {
      uint id;

      do
      {
        id = (uint)new Random().Next(1, int.MaxValue);
      }
      while (Exists(id));

      return id;
    }

    private static string ToFilePath(uint id) => Path.Combine(Constrants.DownloaderTaskDataDir, id.ToString());
  }
}
