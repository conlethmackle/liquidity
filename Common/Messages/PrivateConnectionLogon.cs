using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages
{
   public class PrivateConnectionLogon
   {
      public int SPId { get; set; }
      public string SPName { get; set; }
      public string ConfigInstance { get; set; }
      public int ExchangeDetailsId { get; set; }
      public string ApiKey { get; set; }
      public string Secret { get; set; }
      public string PassPhrase { get; set; }
   }
}
