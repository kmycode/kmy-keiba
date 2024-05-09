using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  public class FinderConfigModel
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public static FinderConfigModel Instance => _instance ??= new();
    private static FinderConfigModel? _instance;

    public ReactiveCollection<FinderConfigItem> Configs => FinderConfigUtil.Configs;

    public ReactiveProperty<FinderConfigItem?> SelectedConfig { get; } = new();

    public ReactiveProperty<string> ErrorMessage { get; } = new();

    private FinderConfigModel()
    {
    }

    public async Task<FinderConfigItem?> AddConfigAsync(string? name = null, string? configString = null)
    {
      var config = new FinderConfigData
      {
        Name = name ?? string.Empty,
        Config = configString ?? string.Empty,
      };

      try
      {
        this.ErrorMessage.Value = string.Empty;

        using var db = new MyContext();

        return await FinderConfigUtil.AddAsync(db, config);
      }
      catch (Exception ex)
      {
        logger.Error("検索条件保存でエラー発生", ex);
        this.ErrorMessage.Value = "検索条件の保存でエラーが発生しました";
        return null;
      }
    }

    public async Task<bool> RemoveConfigAsync(FinderConfigItem configItem)
    {
      try
      {
        this.ErrorMessage.Value = string.Empty;

        using var db = new MyContext();

        await FinderConfigUtil.RemoveAsync(db, configItem);
        return true;
      }
      catch (Exception ex)
      {
        logger.Error("検索条件削除でエラー発生", ex);
        this.ErrorMessage.Value = "検索条件の削除でエラーが発生しました";
        return false;
      }
    }

    public async Task<bool> UpConfigAsync(FinderConfigItem configItem)
    {
      try
      {
        this.ErrorMessage.Value = string.Empty;

        using var db = new MyContext();

        await FinderConfigUtil.UpAsync(db, configItem);
        return true;
      }
      catch (Exception ex)
      {
        logger.Error("検索条件上へ移動でエラー発生", ex);
        this.ErrorMessage.Value = "検索条件の移動でエラーが発生しました";
        return false;
      }
    }

    public async Task<bool> DownConfigAsync(FinderConfigItem configItem)
    {
      try
      {
        this.ErrorMessage.Value = string.Empty;

        using var db = new MyContext();

        await FinderConfigUtil.DownAsync(db, configItem);
        return true;
      }
      catch (Exception ex)
      {
        logger.Error("検索条件下へ移動でエラー発生", ex);
        this.ErrorMessage.Value = "検索条件の移動でエラーが発生しました";
        return false;
      }
    }
  }
}
