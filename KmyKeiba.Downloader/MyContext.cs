using KmyKeiba.Data.Db;
using KmyKeiba.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Downloader
{
  public class MyContext : MyContextBase
  {
    public MyContext()
    {
      //this.ConnectionString = "server=localhost;database=kmykeiba;uid=root;pwd=takaki;";

      Directory.CreateDirectory(Constrants.AppDataPath);
      this.ConnectionString = "Data Source=" + Constrants.DatabasePath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      //base.OnConfiguring(optionsBuilder);
      //optionsBuilder.UseMySql(this.ConnectionString, null);
      optionsBuilder.UseSqlite(this.ConnectionString);
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
          // TODO: log
          tryCount++;
          if (tryCount > 10 * 20 * 60)
          {
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
