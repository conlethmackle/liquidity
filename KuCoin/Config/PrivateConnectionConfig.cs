using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.Config
{
  
   public class KuCoinConnectionConfig
   {
      public const string PrivateConfig = "KuCoinConnectionConfig";
      public string Url { get; set; }
      public string PublicWebSocketEndpoint { get; set; }
      public string PrivateWebSocketEndpoint { get; set; }
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
