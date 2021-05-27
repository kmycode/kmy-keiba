using KmyKeiba.Data.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Data
{
  public class MyContext : MyContextBase
  {
    public MyContext()
    {
      this.ConnectionString = "server=localhost;database=kmykeiba;uid=root;pwd=takaki;";
    }
  }
}
