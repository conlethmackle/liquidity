using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncfusionLiquidity.Shared
{
   public class OpenOrdersRequest
   {
      public string Venue { get; set; }
      public string PortfolioName { get; set; } = "VetLiquidationTest";
      public string InstanceName { get; set; } = "VetFairValueConfig";
   }
}
