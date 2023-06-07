using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Common.Models
{
   public class OrderBookSnapshot
   {
      [JsonPropertyName("symbol")]
      public string Symbol { get; set; }
      [JsonPropertyName("orderbook")]
      public OrderBook OrderBook { get; set; }


      public OrderBookSnapshot()
      {

      }

      public OrderBookSnapshot(string symbol, OrderBook book)
      {
         Symbol = symbol;
         OrderBook = book;
      }
   }
}
