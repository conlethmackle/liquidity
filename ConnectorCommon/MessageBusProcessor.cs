using Common.Messages;
using Common.Models;
using MessageBroker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConnectorCommon
{
  
   public interface IMessageBusProcessor
   {
      //event EventHandler<PlaceOrderCmd> OnOrderPlacement;
      event Func<string, PlaceOrderCmd, Task> OnOrderPlacementCommand;
      event Func<string, int, Task> OnGetBalancesCommand;
      event Func<string, PlaceOrderCmd[], Task> OnBulkOrderPlacementCommand;
      event Func<string, OrderIdHolder, Task> OnOrderCancelCommand;
      event Func<string, string, Task> OnCancelAllOrdersCommand;
      event Func<string, string[], Task> OnOpenOrderQueryCommand;
      event Action<string, string> OnGetOrderBookCommand;
      event Action<string, string> OnGetLatestTradesCommand;
      event Func<Task> OnGetRefDataCommand;
      event Func<string, PrivateConnectionLogon, Task> OnPrivateConnectCommand;
      event Func<Task> OnPublicConnectCommand;
      event Func<string, PlaceOrderCmd, Task> OnMarketOrderPlacementCommand;
      event Action<string> OnGetPublicStatus;
      void Start(string subject);
   }

   public class MessageBusProcessor : IMessageBusProcessor
   {
      //public event EventHandler<PlaceOrderCmd> OnOrderPlacement;
      private readonly IMessageBroker _messageBroker;
      private readonly ILogger<MessageBusProcessor> _logger;

      public event Func<string, PlaceOrderCmd, Task> OnOrderPlacementCommand;
      public event Func<string, int, Task> OnGetBalancesCommand;
      public event Func<string, PlaceOrderCmd[], Task> OnBulkOrderPlacementCommand;
      public event Func<string, OrderIdHolder, Task> OnOrderCancelCommand;
      public event Func<string, string, Task> OnCancelAllOrdersCommand;
      public event Func<string, string[], Task> OnOpenOrderQueryCommand;
      public event Action<string, string> OnGetOrderBookCommand;
      public event Action<string, string> OnGetLatestTradesCommand;
      public event Func<Task> OnGetRefDataCommand;
      public event Func<string, PrivateConnectionLogon, Task> OnPrivateConnectCommand;
      public event Func<Task> OnPublicConnectCommand;
      public event Action<string> OnGetPublicStatus;
      public event Func<string, PlaceOrderCmd, Task> OnMarketOrderPlacementCommand;
      private bool _initialised = false;
      public MessageBusProcessor(ILoggerFactory loggerFactory, IMessageBroker messageBroker)
      {
         _logger = loggerFactory.CreateLogger<MessageBusProcessor>();
         _messageBroker = messageBroker;
      }

      public void Start(string subject)
      {
         try
         {
            if (!_initialised)
            {
               _messageBroker.SubscribeToSubject(subject, ProcessCommands);
               _initialised = true;
            }
         }
         catch(Exception e)
         {
            _logger.LogError(e, "Error in RabbitMQ subscribe to suject {Subject} {Error}", subject, e.Message);
            throw;
         }
      }

      private void ProcessCommands(string subject, byte[] data)
      {
         var msg = MessageBusCommand.ProtoDeserialize<MessageBusCommand>(data);
         _logger.LogInformation("Received a {MessageType}", msg.CommandType.ToString());
         try
         {
            switch (msg.CommandType)
            {
               // Private Commands
               case CommandTypesEnum.PLACE_ORDER:
                  var orderData = JsonSerializer.Deserialize<PlaceOrderCmd>(msg.Data);
                  OnOrderPlacementCommand?.Invoke(msg.InstanceName, orderData);
                  break;
               case CommandTypesEnum.PLACE_BULK_ORDERS:
                  var bulkOrders = JsonSerializer.Deserialize<PlaceOrderCmd[]>(msg.Data);
                  OnBulkOrderPlacementCommand?.Invoke(msg.InstanceName, bulkOrders);
                  break;
               case CommandTypesEnum.GET_ACCOUNT_BALANCE:
                  OnGetBalancesCommand?.Invoke(msg.InstanceName, msg.JobNo);
                  break;
               case CommandTypesEnum.CANCEL_ALL:
                  var symbol = msg.Data;
                  OnCancelAllOrdersCommand?.Invoke(msg.InstanceName, symbol);
                  break;
               case CommandTypesEnum.CANCEL_ORDER:
                  var orderIds = JsonSerializer.Deserialize<OrderIdHolder>(msg.Data);
                  OnOrderCancelCommand?.Invoke(msg.InstanceName, orderIds);
                  break;
               case CommandTypesEnum.GET_OPEN_ORDERS:
                  var coinPairs = msg.Data;
                  var pairOfCoins = coinPairs.Split(",");
                  OnOpenOrderQueryCommand?.Invoke(msg.InstanceName, pairOfCoins);
                  break;
               case CommandTypesEnum.GET_ORDERBOOK:
                  var pairs = msg.Data;
                  var symbols = pairs.Split(",");
                  foreach (var coinpair in symbols)
                  {
                     OnGetOrderBookCommand?.Invoke(msg.InstanceName, coinpair);
                  }
                  break;
               case CommandTypesEnum.GET_REFERENCE_DATA:
                  OnGetRefDataCommand?.Invoke();
                  break;
               case CommandTypesEnum.CONNECT_PRIVATE:
                  var cmd = JsonSerializer.Deserialize<PrivateConnectionLogon>(msg.Data);
                  _logger.LogInformation("Login request from {Account} {Instance}", msg.AccountName, msg.InstanceName);
                  OnPrivateConnectCommand?.Invoke(msg.InstanceName, cmd);
                  break;
               case CommandTypesEnum.CONNECT_PUBLIC:
                  OnPublicConnectCommand?.Invoke();
                  break;
               case CommandTypesEnum.PLACE_MARKET_ORDER:
                  var marketOrderData = JsonSerializer.Deserialize<PlaceOrderCmd>(msg.Data);
                  OnMarketOrderPlacementCommand?.Invoke(msg.InstanceName, marketOrderData);
                  break;
               case CommandTypesEnum.GET_LATEST_TRADES:
                  var coinPair = msg.Data;
                  OnGetLatestTradesCommand?.Invoke(msg.InstanceName, coinPair);
                  break;
               case CommandTypesEnum.GET_PUBLIC_STATUS:
                  var venue = msg.Data;
                  _logger.LogInformation("Received a GET_PUBLIC_STATUS request");
                  OnGetPublicStatus?.Invoke(venue);
                  break;
            }
         }
         catch (Exception e)
         {
            _logger.LogInformation("Error in Process Message {MsgType} - Error {Error}",
                           msg.CommandType.ToString(), e.Message);
            
         }
         

         }
      }
}
