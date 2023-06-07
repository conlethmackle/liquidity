using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class ResponseDataNonT
   {
      [JsonPropertyName("type")] //:"message",
      public string Type { get; set; }
      [JsonPropertyName("topic")] //:"/market/ticker:BTC-USDT",
      public string Topic { get; set; }
      [JsonPropertyName("subject")] // :"trade.ticker",
      public string Subject { get; set; }
      [JsonPropertyName("channelType")]
      public string ChannelType { get; set; }
      [JsonPropertyName("data")]
      public string Data { get; set; }
   }
   public class ResponseData<T>
   {
      [JsonPropertyName("type")] //:"message",
      public string Type { get; set; }
      [JsonPropertyName("topic")] //:"/market/ticker:BTC-USDT",
      public string Topic { get; set; }
      [JsonPropertyName("subject")] // :"trade.ticker",
      public string Subject { get; set; }
      [JsonPropertyName("channelType")]
      public string ChannelType { get; set; }
      [JsonPropertyName("data")]
      public T Data { get; set; }
   }
}
