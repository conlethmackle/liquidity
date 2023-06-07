using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorCommon
{
   public class ExchangeGenericConfig
   {
      public const string GenericConfig = "ExchangeGenericConfig";
      public string ExchangeName { get; set; }
      public string ExchangeTopic { get; set; }
      public int ConnectorAlivePingIntervalMs { get; set; } = 1000;
   }
}
