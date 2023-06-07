using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class BasicResponseData
   {
      [JsonPropertyName("type")] //:"message",
      public string Type { get; set; }
      [JsonPropertyName("id")]
      public string Id { get; set; }
      [JsonPropertyName("topic")] //:"/market/ticker:BTC-USDT",
      public string Topic { get; set; }
      [JsonPropertyName("subject")] // :"trade.ticker",
      public string Subject { get; set; }
     
   }
}
