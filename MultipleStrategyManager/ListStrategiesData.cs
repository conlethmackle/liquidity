using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleStrategyManager
{
   public enum ListFilter
   {
      RUNNING_INSTANCES = 1,
      BY_PORTFOLIO = 2,
      ALL_INSTANCES = 3
   }
   public class ListStrategiesData
   {
      public ListFilter Filter { get; set; }
      public int PortfolioId { get; set; }
   }
}
