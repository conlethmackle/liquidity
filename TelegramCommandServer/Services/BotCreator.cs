using DataStore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TelegramCommandServer.Services
{
   public interface IBotCreator
   {
      Task Init();
   }

   public class BotCreator : IBotCreator
   {
      private readonly ILogger<BotCreator> _logger;
      private readonly IPortfolioRepository _repository;
      private readonly ITelegramCommandListenerFactory _listenerFactory;

      public BotCreator(ILoggerFactory loggerFactory,
         IPortfolioRepository repository,
         ITelegramCommandListenerFactory listenerFactory)
      {
         _logger = loggerFactory.CreateLogger<BotCreator>();
         _repository = repository;
         _listenerFactory = listenerFactory;
      }

      public async Task Init()
      {
         var commandChannels = await _repository.GetCommandTelegramChannels();
         foreach (var channel in commandChannels)
         {
            var listener = _listenerFactory.CreateListener();
            await listener.Init(channel);
            await listener.ListenForMessagesAsync();
         }
      }
   }
}
