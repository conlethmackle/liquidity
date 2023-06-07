using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
   public class OpenOrderErrorResponse
   {
      public string ErrorMessage { get; set; }
      public string Account { get; set; }
      public string Instance { get; set; }
   }
}
