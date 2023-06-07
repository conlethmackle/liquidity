using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages
{
   public class ConnectorConnectMsg
   {
      public string Exchange { get; set; }
      public bool IsPrivate { get; set; }
      public string? SPName { get; set; }
      public bool IsConnect { get; set; }
   }
}
