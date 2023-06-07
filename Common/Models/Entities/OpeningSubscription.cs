using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class OpeningSubscription
   {
     public int Id { get; set; }
     public int LiquidationTrackerId { get; set; }
     public LiquidationTracker LiquidationTracker { get; set; }
     public int VenueId { get; set; }
     public Venue Venue { get; set; }
     public int CoinId { get; set; }
     public Coin Coin { get; set; }
     public decimal SubscriptionPrice { get; set; }
     public decimal InitialCoinAmount { get; set; }
     public decimal ProjectedNominal { get; set; }
   }
}
