using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorStatus
{
   public class SingleClientConnectionStatus
   {
      public bool RestApi { get; set; }
      public bool WebSocket { get; set; }
      public string? ErrorMsg { get; set; }
   }
}
