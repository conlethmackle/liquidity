using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramCommandServer.Services
{
   public interface ITelegramCommandListenerFactory
   {
      ITelegramCommandListener CreateListener();
   }
   public class TelegramCommandListenerFactory : ITelegramCommandListenerFactory
   {
      private readonly IServiceProvider _serviceProvider;

      public TelegramCommandListenerFactory(IServiceProvider serviceProvider)
      {
         _serviceProvider = serviceProvider;
      }

      public ITelegramCommandListener CreateListener()
      {
         return (ITelegramCommandListener)_serviceProvider?.GetService(typeof(ITelegramCommandListener));
      }
   }
}
