using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderAndTradeProcessing
{
   public enum VenueState
   {
      INITIAL = 1,
      CANCEL_ALL_INFLIGHT=2,
      CONNECTOR_DOWN=3,
      CONNECTOR_UP=4,
      ALL_GOOD=5
   }
   public class OrderTrackingStatusPerVenue
   {
      public VenueState Status { get; set; }
   }
}
