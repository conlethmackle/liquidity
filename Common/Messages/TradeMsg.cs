using Common.Models;
using System;
using System.Text.Json.Serialization;

namespace Common.Messages
{
   public class TradeMsg
   {
      [JsonPropertyName("symbol")] //:"KCS-USDT",
      public string Symbol { get; set; }
      [JsonPropertyName("orderType")] //:"limit",
      public OrderTypeEnum OrderType { get; set; }
      [JsonPropertyName("isBuy")] //:"buy",
      public bool IsBuy { get; set; }
      [JsonPropertyName("tradeId")]
      public string TradeId { get; set; }
      [JsonPropertyName("orderId")] //:"5efab07953bdea00089965d2",
      public string OrderId { get; set; }
      [JsonPropertyName("type")] //:"open",
      public OwnOrderUpdateStatusEnum Type { get; set; }
      [JsonPropertyName("orderTime")] //:1593487481683297666,
      public DateTime OrderTime { get; set; }
      [JsonPropertyName("size")] //:"0.1",
      public decimal Quantity { get; set; }
      [JsonPropertyName("filledSize")] //:"0",
      public decimal FilledQuantity { get; set; }
      [JsonPropertyName("price")] //:"0.937",
      public decimal Price { get; set; }
      [JsonPropertyName("matchedPrice")]
      public decimal MatchedPrice { get; set; }
      [JsonPropertyName("matchedSize")]
      public decimal MatchedSize { get; set; }
      [JsonPropertyName("clientOid")] //:"1593487481000906",
      public string ClientOid { get; set; }
      [JsonPropertyName("remainSize")] //:"0.1",
      public decimal RemainingQuantity { get; set; }
      [JsonPropertyName("status")] //:"open",
      public string Status { get; set; }
      [JsonPropertyName("isTaker")]
      public bool IsTaker { get; set; }
      [JsonPropertyName("ts")] //:1593487481683297666
      public DateTime Timestamp { get; set; }
      [JsonPropertyName("fee")] //:1593487481683297666
      public decimal Fee { get; set; }
      [JsonPropertyName("feeRate")] //:"0",  //fee rate
      public decimal FeeRate { get; set; }
      [JsonPropertyName("feeCurrency")] //:"USDT",  // charge fee currency
      public string FeeCurrency { get; set; }
      public string Venue { get; set; }
      public string Account { get; set; }
      public string Instance { get; set; }
   }
}
