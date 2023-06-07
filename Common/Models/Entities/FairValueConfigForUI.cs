using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class FairValueConfigForUI
   {
      public int Id { get; set; }
      public int VenueId { get; set; }
      public Venue Venue { get; set; }
      public int CoinPairId { get; set; }
      public CoinPair CoinPair { get; set; }
      public int UpdateIntervalSecs { get; set; }
   }
}
