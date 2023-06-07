using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
   
   public class CancelOrderResponseError
   {
      public string Error { get; set; }
      public string ClientOid { get; set; }
      public string OrderId { get; set; }      
      public string Account { get; set; }
      public string Instance { get; set; }
   }
}
