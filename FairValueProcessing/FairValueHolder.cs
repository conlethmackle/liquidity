using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairValueProcessing
{
   public class FairValueHolder
   {
      public string Venue { get; set; }
      public decimal BestBid { get; set; }
      public decimal BestAsk { get; set; }
      public decimal LastTrade { get; set; }
      public bool IsBidValid { get; set; } = false;
      public bool IsAskValid { get; set; } = false;
      public bool IsLastTradeValid { get; set; } = false;
      public bool IsFairValueValid { get; set; } = false;
      public bool IsBest { get; set; } = false;
      public decimal FairValue { get; set; } = 0;

      public bool CalculateFairValue()
      {
         if (IsBidValid && IsAskValid && IsLastTradeValid)
            IsFairValueValid = true;
         FairValue = (BestBid + BestAsk + LastTrade) / 3;
         return IsFairValueValid;
      }

      
   }
}
