using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Common;
using Common.Messages;
using MessageBroker;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using TelegramAlertsServer.Router;

namespace TelegramAlertsServer.Services
{
   public interface ITelegramReceiver
   {
       
      
      public event Func<string, TelegramLiquidityAlert, Task> OnLiquidityProgressAlert;
      void Start();
   }

   public class MQReceiver : ITelegramReceiver
   {
      public event Func<string, TelegramLiquidityAlert, Task> OnConnectionAlert;
      public event Func<string, TelegramLiquidityAlert, Task> OnLiquidityProgressAlert;
      private readonly ILogger<MQReceiver> _logger;
      private readonly IMessageBroker _messageBroker;
      private readonly ITelegramAlertRouting _messageRouting;

      private JsonSerializerOptions _jsonSerializerOptions { get; set; }

      public MQReceiver(ILoggerFactory loggerFactory,
                        IMessageBroker messageBroker,
                        ITelegramAlertRouting messageRouting)
      {
         _logger = loggerFactory.CreateLogger<MQReceiver>();
         _messageBroker = messageBroker;
         _messageRouting = messageRouting;
         _jsonSerializerOptions = new JsonSerializerOptions()
         {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
         };
      }
      public void Start()
      {
         _messageRouting.Init();
         _messageBroker.SubscribeToTopicSubject(Constants.TELEGRAM_LIQUIDITY_ALERT, ProcessLiquidityAlerts);
      }

      private void ProcessLiquidityAlerts(string arg1, byte[] data)
      {
         using (var stream = new MemoryStream(data))
         {
            var response = Serializer.Deserialize<MessageBusReponse>(stream);
            var venue = response.FromVenue;
            switch (response.ResponseType)
            {
               case ResponseTypeEnums.CONNECTION_ALERT:
                  var alertData = JsonSerializer.Deserialize<TelegramLiquidityAlert>(response.Data, _jsonSerializerOptions);
                  if (alertData != null)
                     _messageRouting.ProcessConnectionAlert(venue, alertData);
                  else
                  {
                     _logger.LogError("Empty message received in TelegramAlert Server {MessageType}", response.ResponseType.ToString());
                  }
                  break;
               case ResponseTypeEnums.BALANCE_UPDATE:
               case ResponseTypeEnums.LIQUIDITY_PROGRESS_ALERT:
                  var progressData = JsonSerializer.Deserialize<TelegramLiquidityAlert>(response.Data, _jsonSerializerOptions);
                  if (progressData != null)
                     _messageRouting.ProcessLiquidityProgressAlert(venue, progressData);
                  else
                  {
                     _logger.LogError("Empty message received in TelegramAlert Server");
                  }
                  break;
                  break;
               
            }
         }
      }
   }
}
