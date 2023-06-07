using MessageBroker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Timers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Messages;
using Common;

namespace ConnectorCommon
{
   
   public interface IConnectorAliveHandler
   {
      void Init();
   }

   public class ConnectorAliveHandler : IConnectorAliveHandler
   {
      private readonly ILogger<ConnectorAliveHandler> _logger;
      private readonly IMessageBroker _messageBroker;
      public readonly string ExchangeName;
      private readonly int _interval;
      private System.Timers.Timer _privatePingTimer { get; set; }
      public ConnectorAliveHandler(ILoggerFactory loggerFactory, 
                                   IMessageBroker messageBroker, 
                                   IOptions<ExchangeGenericConfig> genericExchangeConfig)
      {
         _logger = loggerFactory.CreateLogger<ConnectorAliveHandler>();
         _messageBroker = messageBroker;
         ExchangeName = genericExchangeConfig.Value.ExchangeTopic;
         _privatePingTimer = new System.Timers.Timer();
         _privatePingTimer.Interval = genericExchangeConfig.Value.ConnectorAlivePingIntervalMs;
         _privatePingTimer.Elapsed += SendPingCallback;         
      }

      public void Init()
      {
         _privatePingTimer.Enabled = true;
      }

      private void SendPingCallback(object sender, ElapsedEventArgs e)
      {
         _privatePingTimer.Enabled = false;
        var msg = new MessageBusReponse()
         {
            FromVenue = ExchangeName,
            IsPrivate = false,
            ResponseType = ResponseTypeEnums.CONNECTOR_PING,
            Data = "ping",
            AccountName = ""
         };

         var routingKey = Constants.CONNECTOR_ALIVE_TOPIC + "." + ExchangeName;
         PublishHelper.PublishToTopic(routingKey, msg, _messageBroker);
         _privatePingTimer.Enabled = true;
      }
   }
}
