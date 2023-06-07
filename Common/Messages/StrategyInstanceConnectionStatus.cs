using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages
{
   public class StrategyInstanceConnectionStatus
   {
      public string InstanceName { get; set; }
      public bool Status { get; set; }
      public string ErrorMsg { get; set; }
   }
}
