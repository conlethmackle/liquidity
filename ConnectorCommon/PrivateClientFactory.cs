using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorCommon
{
   public interface IPrivateClientFactory
   {
      IPrivateClient CreatePrivateClient();
   }

   public class PrivateClientFactory : IPrivateClientFactory
   {
      private readonly IServiceProvider _serviceProvider;

      public PrivateClientFactory(IServiceProvider serviceProvider)
      {
         _serviceProvider = serviceProvider;
      }

      public IPrivateClient CreatePrivateClient()
      {         
         return (IPrivateClient)_serviceProvider.GetService(typeof(IPrivateClient));
      }
   }
}
