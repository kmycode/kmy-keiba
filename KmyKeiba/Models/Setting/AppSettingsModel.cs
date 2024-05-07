using KmyKeiba.Models.Race.AnalysisTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Setting
{
  public class AppSettingsModel
  {
    public static AppSettingsModel Instance => _instance ??= new();

    private static AppSettingsModel? _instance;

    public AnalysisTableConfigModel AnalysisTableConfig => AnalysisTableConfigModel.Instance;

    private AppSettingsModel() { }
  }
}
