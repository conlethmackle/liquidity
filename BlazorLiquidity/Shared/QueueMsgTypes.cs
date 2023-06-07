using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorLiquidity.Shared
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
      STRATEGY_CONTROL_MSG = 11,
      
      CONNECTOR_STATUS = 12,
      START_OF_DAY = 13,
      PUBLIC_CONNECTOR_STATUS = 14,
      PRIVATE_CONNECTOR_STATUS = 15
   }
}
