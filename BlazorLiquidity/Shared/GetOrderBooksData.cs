using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorLiquidity.Shared
{
   public class GetOrderBooksData
   {
      public string Venue { get; set; }
      public string CoinPairs { get; set; }
      public string Instance { get; set; }
      public string Account { get; set; }
   }
}
