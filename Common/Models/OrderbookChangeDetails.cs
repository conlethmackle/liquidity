using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Models
{
   public class OrderBookChanged
   {

      [JsonPropertyName("symbol")]
      public string Symbol { get; set; }
      [JsonPropertyName("data")]
      public OrderBookSide[] Data { get; set; }

      public OrderBookChanged()
      {
         Data = new OrderBookSide[]
         {
            new OrderBookSide(),
            new OrderBookSide()
         };       
         Data[0].Side = "buy";
         Data[1].Side = "sell";
      }
   }

   public class OrderBookSide
   {
      [JsonPropertyName("side")]
      public string Side { get; set; }
      [JsonPropertyName("remove")]
      public List<decimal> Remove { get; set; }
      [JsonPropertyName("add")]
      public List<LevelDetails> Add { get; set; }
      [JsonPropertyName("update")]
      public List<LevelDetails> Update { get; set; }
      public OrderBookSide()
      {
         Remove = new List<decimal>();
         Add = new List<LevelDetails>();
         Update = new List<LevelDetails>();
      }
   }

   public class LevelDetails
   {
      [JsonPropertyName("q")]
      public decimal Quantity { get; set; }
      [JsonPropertyName("p")]
      public decimal Price { get; set; }
      public LevelDetails(decimal price, decimal quantity)
      {
         Price = price;
         Quantity = quantity;
      }
   }

  
}
