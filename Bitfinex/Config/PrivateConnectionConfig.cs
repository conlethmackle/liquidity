using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitfinex.Config
{
   public class PrivateConnectionConfig
   {
      public const string PrivateConfig = "PrivateConnectionConfig";
      public string RestEndpoint { get; set; }
     
      public string PublicRestEndpoint { get; set; }
      public string PrivateRestEndpoint { get; set; }
      public string WebSocketEndpoint { get; set; }
      public string PublicWSEndpoint { get; set; }
      public string PrivateWSEndpoint { get; set; }
      public UInt64 ReconnectIntervalMilliSecs { get; set; }
      public UInt64 OrderbookUpdateInterval { get; set; }
      public SubAccountsConfig[] Accounts { get; set; }
   }

   public class SubAccountsConfig
   {
      public string Name { get; set; }
      public string ApiKey { get; set; }
      public string SecretKey { get; set; }
      public string PassPhrase { get; set; }
   }
}
