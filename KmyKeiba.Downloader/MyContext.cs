using KmyKeiba.Data.Db;
using KmyKeiba.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Downloader
{
  public class MyContext : MyContextBase
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public MyContext()
    {
      //this.ConnectionString = "server=localhost;database=kmykeiba;uid=root;pwd=takaki;";

      Directory.CreateDirectory(Constrants.AppDataDir);
      this.ConnectionString = "Data Source=" + Constrants.DatabasePath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      //base.OnConfiguring(optionsBuilder);
      //optionsBuilder.UseMySql(this.ConnectionString, null);
      optionsBuilder.UseSqlite(this.ConnectionString);
    }

    protected override Task<IDbContextTransaction> GenerateTransactionAsync()
    {
      return this.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
    }

    public async Task<int> SaveChangesAsync()
    {
      var isSucceed = false;
      var result = 0;
      var tryCount = 0;
      while (!isSucceed)
      {
        try
        {
          result = await base.SaveChangesAsync();
          isSucceed = true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 5)
        {
          logger.Warn("保存処理でエラー", ex);

          tryCount++;
          if (tryCount > 10 * 20 * 60)
          {
            logger.Error("保存に失敗しました");
            throw ex;
          }
          await Task.Delay(100);
        }
      }

      return result;
    }

    public new int SaveChanges()
    {
      return Task.Run(async () => await this.SaveChangesAsync()).Result;
    }
  }
}
