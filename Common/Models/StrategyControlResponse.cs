using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
   public enum StrategyProcessStatus
   {
      STARTED = 1,
      STOPPED = 2,
      EXITED = 3,
      UNEXPECTED_STOP=4
   }

   public class StrategyControlResponse
   {
      public string ConfigName { get; set; }
      public StrategyProcessStatus State { get; set; }
      public string Message { get; set; }
   }
}
