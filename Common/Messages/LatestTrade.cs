using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages
{
   public class LatestTrade
   {
      public string Symbol { get; set; }
      public decimal Price { get; set; }
      public decimal Quantity { get; set; }
      public DateTime TradeTime { get; set; }
   }
}
