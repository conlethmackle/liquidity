using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Common.Models
{  
   public class OrderBook
   {
      public OrderBook()
      {
         Bid = new List<Level>();
         Ask = new List<Level>();
      }
      [JsonPropertyName("bids")]
      public List<Level> Bid { get; set; }
      [JsonPropertyName("asks")]
      public List<Level> Ask { get; set; }
   }

   public class Level
   {
      public Level()
      {

      }
      public Level(decimal p, decimal q)
      {
            Price = p;
            Quantity = q;
      }
      [JsonPropertyName("price")]
      public decimal Price { get; set; }
      [JsonPropertyName("quantity")]
      public decimal Quantity { get; set; }
   }
}
