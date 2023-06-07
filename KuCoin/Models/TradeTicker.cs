using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KuCoin.Models
{
   public class TradeTicker
   {
      [JsonPropertyName("sequence")] //:"1545896668986", // Sequence number
      public string Sequence { get; set; }
      [JsonPropertyName("price")] //:"0.08",             // Last traded price
      public string Price { get; set; }
      [JsonPropertyName("size")] //:"0.011",             //  Last traded amount
      public string Quantity { get; set; }
      [JsonPropertyName("bestAsk")] //:"0.08",          // Best ask price
      public string BestAsk { get; set; }
      [JsonPropertyName("bestAskSize")] //:"0.18",      // Best ask size
      public string BestAskQuantity { get; set; }
      [JsonPropertyName("bestBid")] //:"0.049",         // Best bid price
      public string BestBidPrice { get; set; }
      [JsonPropertyName("bestBidSize")] //:"0.036"     // Best bid size
      public string BestBidQuantity { get; set; }
      [JsonPropertyName("time")]
      public Int64 Time { get; set; }
   }
}
