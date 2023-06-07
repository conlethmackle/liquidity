using MessageBroker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Messages;
using System.Text.Json;
using Common.Models;

namespace MultipleStrategyManager
{
   public interface IStrategyMessageReceiver
   {
      void Start();
   }
   public class StrategyMessageReceiver : IStrategyMessageReceiver
   {
      private readonly IStrategyMonitor _monitor;
      private readonly ILogger<StrategyMessageReceiver> _logger;
      private readonly IMessageBroker _messageBroker;

      public StrategyMessageReceiver(ILoggerFactory loggerFactory,
                                    IMessageBroker messageBroker,
                                    IStrategyMonitor strategyMonitor)
      {
         _logger = loggerFactory.CreateLogger<StrategyMessageReceiver>();
         _monitor = strategyMonitor;
         _messageBroker = messageBroker;
      }

      public void Start()
      {
         _messageBroker.SubscribeToSubject(Constants.STRATEGY_CONTROL, ProcessCommands);
      }

      private void ProcessCommands(string subject, byte[] data)
      {
         var msg = MessageBusCommand.ProtoDeserialize<MessageBusCommand>(data);
         switch (msg.CommandType)
         {
            case CommandTypesEnum.START_STRATEGY:
               var startData = JsonSerializer.Deserialize<StartStrategyData>(msg.Data);
               _monitor.StartStrategy(startData);
               break;
            case CommandTypesEnum.STOP_STRATEGY:
               var stopData = JsonSerializer.Deserialize<StopStrategyData>(msg.Data);
               _monitor.StopStrategy(stopData);
               break;
            case CommandTypesEnum.LIST_STRATEGIES:
               break;
            default:
               break;
         }
      }
   }
}
