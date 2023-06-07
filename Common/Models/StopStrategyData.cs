using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
   public enum StopType
   {
      SINGLE_INSTANCE = 1,
      BY_PID = 2,
      BY_INSTANCE = 3,
   }
   public class StopStrategyData
   {
      public StopType Method { get; set; }
      public int Pid { get; set; }
      public string InstanceName { get; set; }
      public string AccountName  { get; set; }
   }
}
