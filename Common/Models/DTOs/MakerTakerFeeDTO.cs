using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class MakerTakerFeeDTO
   {
      public int Id { get; set; }
      public int VenueId { get; set; }
      public VenueDTO Venue { get; set; }
      public MarketType Mode { get; set; }
      public decimal MakerPercentage { get; set; }
      public decimal TakerPercentage { get; set; }
   }
}
