using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorLiquidity.Shared
{
   public class StrategyProcessDetails
   {
      public string AccountName { get; set; }
      public string ConfigName { get; set; }
      public int StrategyConfigId { get; set; }
      public bool Enable { get; set; }
   }
}
