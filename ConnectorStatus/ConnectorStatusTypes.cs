using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorStatus
{
   public enum ConnectorStatusTypes
   {
      CONNECTOR_UP = 1,
      CONNECTOR_DOWN = 2,
      PUBLIC_CLIENT_UP = 3,
      PUBLIC_CLIENT_DOWN = 4,
      PRIVATE_CLIENT_UP = 5,
      PRIVATE_CLIENT_DOWN = 6,
   }
}
