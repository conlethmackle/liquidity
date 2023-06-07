using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategies.Common
{
   public interface IStrategy
   {
      Task Initialise();
      public void OnFairValueUpdate();
      public void OnOrderStatusUpdate();
      public void OnCancelStrategyOrders();
      public void OnCheckStaleOrdersPending();
   }
}
