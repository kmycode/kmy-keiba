using KmyKeiba.Data.Db;
using KmyKeiba.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Data
{
  public class MyContext : MyContextBase
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public MyContext()
    {
      this.ConnectionString = "Data Source=" + Constrants.DatabasePath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      //base.OnConfiguring(optionsBuilder);
      //optionsBuilder.UseMySql(this.ConnectionString, null);
      optionsBuilder.UseSqlite(this.ConnectionString);
    }

    public async Task<int> SaveChangesAsync(int timeout = 0)
    {
      var isSucceed = false;
      var result = 0;
      var tryCount = 0;

      if (timeout == 0)
      {
        timeout = 1000 * 20 * 60;
      }

      while (!isSucceed)
      {
        try
        {
          result = await base.SaveChangesAsync();
          isSucceed = true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 5)
        {
          logger.Warn("保存でエラー", ex);

          tryCount += 100;
          if (tryCount > timeout)
          {
            logger.Error("保存に失敗しました");
            throw ex;
          }
          await Task.Delay(100);
        }
      }

      return result;
    }

    [Obsolete("WPFではUIスレッドにおいてTaskのWaitメソッドを実行できず、保存失敗時の待機処理を含めることができないため非常用。" +
      "特段の事情がない限り、基本はSaveChangesAsyncを使う")]
    public new int SaveChanges()
    {
      try
      {
        return base.SaveChanges();
      }
      catch (Exception ex)
      {
        logger.Error("同期メソッドで保存失敗", ex);
        throw new Exception(ex.Message, ex);
      }
    }
  }
}
