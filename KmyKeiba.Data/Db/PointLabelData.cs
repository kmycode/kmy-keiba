using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class PointLabelData : AppDataBase
  {
    public string Name { get; set; } = string.Empty;

    public string Labels { get; set; } = string.Empty;

    private static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
    {
      Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
      NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    public IReadOnlyList<PointLabelItem> GetItems()
    {
      return JsonSerializer.Deserialize<IEnumerable<PointLabelItem>>(this.Labels, JsonOptions)?.ToArray() ??
        Array.Empty<PointLabelItem>();
    }

    public void SetItems(IEnumerable<PointLabelItem> items)
    {
      this.Labels = JsonSerializer.Serialize(items, JsonOptions);
    }
  }

  public class PointLabelItem
  {
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public MemoColor Color { get; set; }

    [JsonPropertyName("point")]
    public short Point { get; set; }
  }

  public enum MemoColor : short
  {
    Default = 0,
    Good = 1,
    Bad = 2,
    Warning = 3,
  }
}
