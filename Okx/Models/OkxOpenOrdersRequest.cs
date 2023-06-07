using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Okx.Models
{
   public class OkxOpenOrdersRequest
   {
      public OkxInstType InstType { get; set; }
      public string InstId { get; set; }
      public OkxOrderType[] OrderTypes { get; set; }
      
   }
}
