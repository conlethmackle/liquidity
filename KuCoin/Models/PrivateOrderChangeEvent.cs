using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class PrivateOrderChangeEvent
   {
      [JsonPropertyName("symbol")] //:"KCS-USDT",
      public string Symbol { get; set; }
      [JsonPropertyName("orderType")] //:"limit",
      public string OrderType { get; set; }
      [JsonPropertyName("side")] //:"buy",
      public string Side { get; set; }
      [JsonPropertyName("orderId")] //:"5efab07953bdea00089965d2",
      public string OrderId { get; set; }
      [JsonPropertyName("type")] //:"open",
      public string Type { get; set; }
      [JsonPropertyName("orderTime")] //:1593487481683297666,
      public long OrderTime { get; set; }
      [JsonPropertyName("size")] //:"0.1",
      public string Quantity { get; set; }
      [JsonPropertyName("filledSize")] //:"0",
      public string FilledQuantity { get; set; }
      [JsonPropertyName("price")] //:"0.937",
      public string Price { get; set; }
      [JsonPropertyName("clientOid")] //:"1593487481000906",
      public string ClientOid { get; set; }
      [JsonPropertyName("remainSize")] //:"0.1",
      public string RemainingQuantity { get; set; }
      [JsonPropertyName("status")] //:"open",
      public string Status { get; set; }
      [JsonPropertyName("ts")] //:1593487481683297666
      public UInt64 TS { get; set; }
      [JsonPropertyName("tradeId")]
      public string TradeId { get; set; }
      [JsonPropertyName("matchedPrice")]
      public decimal MatchedPrice { get; set; }
      [JsonPropertyName("matchedSize")]
      public decimal MatchedSize { get; set; }
      [JsonPropertyName("liquidity")]
      public string Liquidity { get; set; }
      
   }
}
