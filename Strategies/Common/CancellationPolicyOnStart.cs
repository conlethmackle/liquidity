using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategies.Common
{
   internal enum  CancellationPolicyOnStart
   {
      CANCEL_ALL_ORDERS = 1,
      GET_OPEN_ORDERS = 2,
   }
}
