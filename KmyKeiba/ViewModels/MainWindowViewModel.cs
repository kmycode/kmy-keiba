using Prism.Mvvm;
using JVDTLabLib;
using NVDTLabLib;
using KmyKeiba.Models.JVLib;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using KmyKeiba.Models.Entities;

namespace KmyKeiba.ViewModels
{
  public class MainWindowViewModel : BindableBase
  {
    private string _title = "Prism Application";
    public string Title
    {
      get { return _title; }
      set { SetProperty(ref _title, value); }
    }

    public unsafe MainWindowViewModel()
    {
      var context = new MyContext();
      context.Database.Migrate();
      context.Dispose();

      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

      var link = new NVLink();
      var r = link.NVInit("UNKNOWN");
      //link.NVSetUIProperties();
      var readCount = 0;
      var downloadCount = 0;
      var rr = link.NVOpen("RACE", "20210517000000", 4, ref readCount, ref downloadCount, out string lastFileTimestamp);
      // var rr = link.NVRTOpen("0B15", "20210518");

      var buffSize = 110000;
      object buff = new byte[buffSize];

      for (var i = 0; i < 1000000; i++)
      {
        var rrr = link.NVGets(ref buff, buffSize, out string fileName);
        var d = Encoding.GetEncoding(932).GetString((byte[])buff);

        if (rrr == -403)
        {
          link.NVFiledelete(fileName);
          continue;
        }
        if (rrr == -3)
        {
          var downloaded = link.NVStatus();
          Task.Delay(100).Wait();
          continue;
        }
        if (rrr == -503)
        {
          continue;
        }
        if (rrr <= 0 && rrr != -1)
        {
          break;
        }

        var spec = d.Substring(0, 2);

        if (spec == "RA")
        {
          var data = new JVData_Struct.JV_RA_RACE();
          data.SetDataB(ref d);
          if (data.id.JyoCD == "36")
          {
            var race = Race.FromJV(data);
            Debug.WriteLine($"{race.Name}");
          }
        }
        else if (spec == "YS")
        {
          var data = new JVData_Struct.JV_YS_SCHEDULE();
          data.SetDataB(ref d);
          if (data.id.JyoCD == "36")
          {
            Debug.WriteLine($"{data.JyusyoInfo![0].Hondai.Trim()}");
          }
        }

        // Marshal.FreeCoTaskMem(&buff);
        // Marshal.FreeCoTaskMem(new IntPtr(&buff));

        // var data = new JVData_Struct.JV_H1_HYOSU_ZENKAKE();
        // data.SetDataB(ref d);
      }

      var buff2 = (byte[])buff;
      Array.Resize(ref buff2, 0);

      link.NVClose();
    }

    private void Do1()
    {

      var link = new JVLink();
      var r = link.JVInit("UNKNOWN");
      var readCount = 0;
      var downloadCount = 0;
      var rr = link.JVOpen("RACE", "20210506000000", 2, ref readCount, ref downloadCount, out string lastFileTimestamp);

      var buffSize = 110000;
      object buff = new byte[buffSize];

      for (var i = 0; i < 1000000; i++)
      {
        var rrr = link.JVGets(ref buff, buffSize, out string fileName);
        var d = Encoding.GetEncoding(932).GetString((byte[])buff);

        if (rrr == -403)
        {
          link.JVFiledelete(fileName);
          continue;
        }
        if (rrr == -3)
        {
          Task.Delay(100).Wait();
          continue;
        }
        if (rrr == -503)
        {
          continue;
        }
        if (rrr <= 0 && rrr != -1)
        {
          break;
        }

        var spec = d.Substring(0, 2);

        if (spec == "RA")
        {
          var data = new JVData_Struct.JV_RA_RACE();
          data.SetDataB(ref d);
          if (data.id.JyoCD == "36")
          {
            if (!string.IsNullOrWhiteSpace(data.RaceInfo.Hondai))
            {
              Debug.WriteLine(data.RaceInfo.Hondai);
            }
          }
        }
      }
    }
  }
}
