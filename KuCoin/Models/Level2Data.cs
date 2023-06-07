using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KuCoin.Models
{
   public class Level2Changes
   {
      [JsonPropertyName("sequenceStart")] //:1545896669105,
      public Int64 SequenceStart { get; set; }
      [JsonPropertyName("sequenceEnd")] //:1545896669106,
      public Int64 SequenceEnd { get; set; }
      [JsonPropertyName("symbol")] //:"BTC-USDT" //,
      public string Symbol { get; set; }
      [JsonPropertyName("changes")] //:{
      public OrderBookChanges Changes { get; set; }
      //  "asks":[["6","1","1545896669105"]],           //price, size, sequence
      //   "bids":[["4","1","1545896669106"]]
   }

   public class OrderBookChanges
   {
      [JsonPropertyName("asks")]
      public List<List<string>> Asks { get; set; } = new List<List<string>>()
      {
 
      };
      [JsonPropertyName("bids")]
      public List<List<string>> Bids { get; set; } = new List<List<string>>()
      {
      };
   }

   

}
