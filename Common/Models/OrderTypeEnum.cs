using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
   public enum OrderTypeEnum
   {
      LIMIT = 1,
      MARKET = 2,      
      STOPLOSS = 3,
      STOPLOSSLIMIT = 4,
      TAKEPROFIT = 5,
      TAKEPROFITLIMIT = 6,
      LIMITMAKER = 7,
      FILLORKILL = 8,
      IOC = 9,
      POST_ONLY = 10,
      OPTIMAL_LIMIT_IOC = 11,
   }
}
