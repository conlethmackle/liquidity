using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorLiquidity.Shared
{
   public class BalanceUpdate
   {
      public string Venue { get; set; }
      public string Currency { get; set; }
      public ExchangeBalance Balance { get; set; }
   }
}
