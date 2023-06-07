using System.Text.Json.Serialization;

namespace Okx.Models
{
   public class OkxOrderPlacement
   {
      [JsonPropertyName("instId")]
      public string InstId { get; set; }
      [JsonPropertyName("tdMode")]
      public string TdMode { get; set; }
      [JsonPropertyName("ccy")]
      public string MarginCurrency { get; set; }
      [JsonPropertyName("clOrdId")]
      public string ClientOrderId { get; set; }
      [JsonPropertyName("tag")]
      public string OrderTag { get; set; }
      [JsonPropertyName("side")]
      public string Side { get; set; }
      [JsonPropertyName("posSide")]
      public string PositionSide { get; set; }
      [JsonPropertyName("ordType")]
      public string OrderType { get; set; }
      [JsonPropertyName("sz")]
      public decimal Quantity { get; set; }
      [JsonPropertyName("px")]
      public decimal Price { get; set; }
      [JsonPropertyName("reduceOnly")]
      public bool ReduceOnly { get; set; }
      [JsonPropertyName("tgtCcy")]
      public string TargetCurrency { get; set; }
      [JsonPropertyName("banAmend")]
      public string BanAmend { get; set; }
      [JsonPropertyName("tpTriggerPx")]
      public string TpTriggerPx { get; set; }
      [JsonPropertyName("tpOrdPx")]
      public string TpOrdPx { get; set; }
      [JsonPropertyName("slTriggerPx")]
      public string SlTriggerPx { get; set; }
      [JsonPropertyName("slOrdPx")]
      public decimal SlOrdPx { get; set; }
      [JsonPropertyName("tpTriggerPxType")]
      public string TpTriggerPxType { get; set; }
      [JsonPropertyName("slTriggerPxType")]
      public string SlTriggerPxType { get; set; }
      [JsonPropertyName("quickMgnType")]
      public string QuickMgnType { get; set; }
   }
}
