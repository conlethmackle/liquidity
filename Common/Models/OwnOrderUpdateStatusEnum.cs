using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
   public enum OwnOrderUpdateStatusEnum
   {
      OPEN = 1,
      CANCELED = 2,
      FILLED = 3,
      MATCHED = 4,
      MODIFIED = 5,
      PARTIALY_FILLED = 6,
      EXPIRED = 7
   }
}
