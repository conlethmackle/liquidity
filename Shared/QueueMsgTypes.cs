using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncfusionLiquidity.Shared
{
   public enum QueueMsgTypes
   {
      FAIRVALUEUPDATE = 1,
      OPENINGBALANCE = 2,
      BALANCEUPDATE = 3,
      NEWORDER = 4,
      CANCELLEDORDER = 5,
      PARTIALLYFILLEDORDER = 6,
      FILLEDORDER = 7,
      TRADE = 8,
      OPEN_ORDERS_RESPONSE = 9,
      STRATEGY_ALIVE_PING = 10,
      STRATEGY_STARTED = 11,
   }
}
