using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages
{
   public enum LiquidityAlertTypes
   {
      PUBLIC_WEBSOCKET = 1,
      PRIVATE_WEBSOCKET = 2,
      DAILY_LIQUIDATION_TARGET_REACHED = 3,
      LIQUIDATION_TARGET_REACHED = 4,
      INSUFFICIENT_BALANCE = 5,
      LIQUIDATION_COMPLETED = 6,
      LIQUIDATION_COMPLETED_FOR_TODAY = 7,
   }
   public class TelegramLiquidityAlert
   {
      public LiquidityAlertTypes SpecificAlert { get; set; }
      public string Message { get; set; }
      public string Venue { get; set; }
      public DateTime Time { get; set; }
   }
}
