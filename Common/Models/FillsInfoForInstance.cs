using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
   public class FillsInfoForInstance
   {
      public string Instance { get; set; }
      public string Account { get; set; }
      public int VenueId { get; set; }
      public int TotalFills { get; set; }
      public int DailyFills { get; set; }
      public decimal TotalLiquidated { get; set; }
      public decimal LiquidatedToday { get; set; }
      public decimal TotalStableEarned { get; set; }
      public decimal TotalStableEarnedToday { get; set; }

   }
}
