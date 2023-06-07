using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.Models
{
   public enum OrderBookState
   {
      INITIAL = 1,
      CACHING = 2,
      REPLAYING_CACHE = 3,
      REALTIME = 4
   }
}
