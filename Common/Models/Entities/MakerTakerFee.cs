using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class MakerTakerFee
   {
      public int Id { get; set; }
      public int VenueId { get; set; }
      public Venue Venue { get; set; }
      public MarketType Mode { get; set; }
      public decimal MakerPercentage { get; set; }
      public decimal TakerPercentage { get; set; }
   }
}
