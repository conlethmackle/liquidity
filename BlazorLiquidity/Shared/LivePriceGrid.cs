using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorLiquidity.Shared
{
   public class LivePriceGrid
   {
      public int Id { get; set; }
      public string Venue { get; set; }
      public string CoinPair { get; set; }
      public decimal Price { get; set; }
      public string Color { get; set; }
   }
}
