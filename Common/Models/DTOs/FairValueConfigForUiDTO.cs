using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models.Entities;

namespace Common.Models.DTOs
{
   public class FairValueConfigForUiDTO
   {
      public int Id { get; set; }
      public int VenueId { get; set; }
      public Venue Venue { get; set; }
      public int CoinPairId { get; set; }
      public CoinPair CoinPair { get; set; }
      public int UpdateIntervalSecs { get; set; }
   }
}
