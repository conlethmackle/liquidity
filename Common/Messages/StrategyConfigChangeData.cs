using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages
{
   public class StrategyConfigChangeData
   {
      public StrategyConfigChangeType StrategyConfigChangeType { get; set; }
      public string InstanceName { get; set; }
   }
}
