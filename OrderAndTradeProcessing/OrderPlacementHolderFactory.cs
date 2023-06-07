using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderAndTradeProcessing
{
   public interface IOrderPlacementHolderFactory
   {
      IOrderPlacementHolder CreatePlacementHolder(IOrderAndTradeProcessing processor);
   }
   public class OrderPlacementHolderFactory : IOrderPlacementHolderFactory
   {
      private readonly IServiceProvider _serviceProvider;

      public OrderPlacementHolderFactory(IServiceProvider serviceProvider)
      {
         _serviceProvider = serviceProvider;
      }

      public IOrderPlacementHolder CreatePlacementHolder(IOrderAndTradeProcessing processor)
      {
         return (IOrderPlacementHolder)_serviceProvider.GetService(typeof(IOrderPlacementHolder ));
      }
   }
}
