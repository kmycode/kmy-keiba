using KmyKeiba.Data.Db;
using KmyKeiba.Shared;
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
  }
}
