using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages
{
   public class PrivateClientLoginStatus
   {
      public bool IsLoggedIn { get; set; }
      public string AccountName { get; set; }
      public string InstanceName { get; set; }
      public string Message { get; set; }
   }
}
