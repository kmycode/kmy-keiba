using KmyKeiba.Data.Db;
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
    public MyContext()
    {
      //this.ConnectionString = "server=localhost;database=kmykeiba;uid=root;pwd=takaki;";

      var path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KMYsofts", "KMYKeiba");
      Directory.CreateDirectory(path);
      this.ConnectionString = "Data Source=" + Path.Combine(path, "maindata.sqlite3");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      //base.OnConfiguring(optionsBuilder);
      //optionsBuilder.UseMySql(this.ConnectionString, null);
      optionsBuilder.UseSqlite(this.ConnectionString);
    }
  }
}
