using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AccountBalanceManager;
using Common;
using Common.Messages;
using Common.Models;
using MessageBroker;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using StrategyMessageListener;

namespace TelegramCommandServer.Services
{
   public interface ITelegramMessageReceiver
   {
      void Start();
      event Func<TelegramConfigChange, Task> OnTelegramConfigChange;
      void Subscribe(string topic);
   }

   public class TelegramMessageReceiver : ITelegramMessageReceiver
   {
      public event Func<TelegramConfigChange, Task> OnTelegramConfigChange;
      private readonly ILogger<TelegramMessageReceiver> _logger;
      private readonly IMessageBroker _messageBroker;

      private JsonSerializerOptions _jsonSerializerOptions { get; set; }

      private readonly IInventoryManager _inventoryMgr;

         public TelegramMessageReceiver(ILoggerFactory loggerFactory,
         IMessageBroker messageBroker,
         IInventoryManager inventoryMgr)
      {
         _logger = loggerFactory.CreateLogger<TelegramMessageReceiver>();
         _messageBroker = messageBroker;
         _inventoryMgr = inventoryMgr;
      }

      public void Start()
      {
         _messageBroker.SubscribeToTopicSubject(Constants.TELEGRAM_CONFIG_CHANGE_TOPIC, ProcessCommands);
      }

      private void ProcessCommands(string subject, byte[] data)
      {
         using (var stream = new MemoryStream(data))
         {
            var response = Serializer.Deserialize<MessageBusReponse>(stream);
            var venue = response.FromVenue;
            //_logger.LogInformation("Received a message of type {MessageType}", response.ResponseType.ToString());
            switch (response.ResponseType)
            {
               case ResponseTypeEnums.TELEGRAM_CONFIG_CHANGE:
                  _logger.LogInformation("Received a message of type {MessageType}", response.ResponseType.ToString());
                  var configData = JsonSerializer.Deserialize<TelegramConfigChange>(response.Data);
                  OnTelegramConfigChange?.Invoke(configData);
                  break;
               case ResponseTypeEnums.GET_BALANCE_RESPONSE:
                  _logger.LogInformation("Received a message of type {MessageType}", response.ResponseType.ToString());
                  var balances = JsonSerializer.Deserialize<ExchangeBalance[]>(response.Data, _jsonSerializerOptions);
                  _inventoryMgr.UpdateForTelegram(response.FromVenue, balances, response.JobNo);
                  break;
               case ResponseTypeEnums.BALANCE_UPDATE:
                  _logger.LogInformation("Received a message of type {MessageType}", response.ResponseType.ToString());
                  var balance = JsonSerializer.Deserialize<ExchangeBalance>(response.Data, _jsonSerializerOptions);
                  _inventoryMgr.Update(response.FromVenue, balance);
                  break;
            }
         }
      }

      public void Subscribe(string topic)
      {
         _messageBroker.SubscribeToTopicSubject(topic, ProcessCommands);
      }
   }
}
