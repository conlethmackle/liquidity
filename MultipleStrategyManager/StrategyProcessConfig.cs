using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql.Logging;

namespace MultipleStrategyManager
{
   public class StrategyProcessConfig
   {
      public string AccountName { get; set; }
      public string StrategyName { get; set; }
      public string ConfigName { get; set; }
      public string FileLogging { get; set; }
      public string ConsoleLogging { get; set; }
   }
}
