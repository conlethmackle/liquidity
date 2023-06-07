using System.Text.Json.Serialization;

namespace Okx.Models
{
   public class OkxTopAccountPush
   {
      [JsonPropertyName("uTime")]
      public ulong UTime { get; set; }
      [JsonPropertyName("totalEq")]
      public decimal TotalEq { get; set; }
      [JsonPropertyName("adjEq")]
      public decimal AdjEq { get; set; }
      [JsonPropertyName("isoEq")]
      public decimal IsoEq { get; set; }
      [JsonPropertyName("ordFroz")]
      public decimal OrdFroz { get; set; }
      [JsonPropertyName("imr")]
      public decimal Imr { get; set; }
      [JsonPropertyName("mmr")]
      public decimal Mmr { get; set; }
      [JsonPropertyName("notionalUsd")]
      public decimal NotionalUsd { get; set; }
      [JsonPropertyName("mgnRatio")]
      public decimal MgnRatio { get; set; }
      [JsonPropertyName("details")]
      public OkxDetailedAccountPush[] Details { get; set; }
   }
}
