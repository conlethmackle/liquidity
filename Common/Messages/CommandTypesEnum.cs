using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages
{
   public enum CommandTypesEnum
   {
      PLACE_ORDER = 1,
      PLACE_BULK_ORDERS = 2,
      CANCEL_ORDER = 3,
      CANCEL_ALL = 4,
      GET_ORDERBOOK = 5,
      GET_ACCOUNT_BALANCE = 6,
      GET_OPEN_ORDERS = 7,
      GET_REFERENCE_DATA = 8,
      MODIFY_ORDER = 9,
      CONNECTOR_STATUS_UPDATE = 10,
      GET_CONNECTOR_STATUS = 11,
      CONNECT_PRIVATE = 11,
      CONNECT_PUBLIC = 12,
      DISCONNECT_PRIVATE = 13,
      DISCONNECT_PUBLIC = 14,
      PLACE_MARKET_ORDER = 15,
      START_STRATEGY = 16,
      STOP_STRATEGY = 17,
      LIST_STRATEGIES = 18,
      
      GET_LATEST_TRADES = 19,
      GET_PUBLIC_STATUS = 20,
     

   }
}
