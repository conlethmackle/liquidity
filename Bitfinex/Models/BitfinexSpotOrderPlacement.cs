using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitfinex.Net.Enums;
using Bitfinex.Net.Objects.Models.V1;


namespace Bitfinex.Models
{
   public class BitfinexSpotOrderPlacement
   {
      public string CustomerOrderId { get; set; }
      public OrderSide Side { get; set; }
      public string Symbol { get; set; }
     // public SpotOrderType OrderType { get; set; }
      public decimal? Price { get; set; }
      public decimal Quantity { get; set; }
      public OrderType OrderType { get; set; }
      //   public TimeInForce Tif { get; set; }

   }
}
