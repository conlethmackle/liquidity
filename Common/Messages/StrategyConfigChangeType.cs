using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages
{
   public enum StrategyConfigChangeType
   {
      LIQUIDATION = 1,
      MARKET_MAKING = 2,
      FAIRVALUEMAKERTAKER = 3,
      LIQUIDATION_MANUAL_ORDER_SIZES = 4,
      LIQUIDATION_AUTO_ORDER_SIZES = 5,
   }
}
