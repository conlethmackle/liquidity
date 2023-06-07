using Binance.Net.Enums;
using System.Text.Json.Serialization;

namespace Binance.Models
{
   public class BinanceSpotOrderPlacement
   {
      public string CustomerOrderId { get; set; }     
      public OrderSide Side { get; set; }      
      public string Symbol { get; set; }      
      public SpotOrderType OrderType { get; set; }
      public decimal? Price { get; set; }       
      public decimal Quantity { get; set; }
      public TimeInForce Tif { get; set; }
      public decimal? QuoteQuantity { get; set; }
      public decimal? StopPrice { get; set; }
      public decimal? IcebergQuantity { get; set; }
   }
}
