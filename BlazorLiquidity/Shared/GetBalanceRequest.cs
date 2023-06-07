using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorLiquidity.Shared
{
   public class GetBalanceRequest
   {
      public string Venue { get; set; }
      public string PortfolioName { get; set; } = "VetLiquidationTest";
      public string InstanceName { get; set; } = "VetFairValueConfig";
   }
}
