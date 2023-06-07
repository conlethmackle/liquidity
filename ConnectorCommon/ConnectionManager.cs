using Common.Messages;
using Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Timers;

namespace ConnectorCommon
{
   public interface IConnectionManager
   {
      Task Init();
   }

   public class ConnectionManager : IConnectionManager
   {
      private readonly ILogger<ConnectionManager> _logger;
      private readonly IPublicClient _publicClient;      
      private readonly IPrivateClientFactory _privateClientFactory;
      private Dictionary<string, IPrivateClient> _privateClients = new Dictionary<string, IPrivateClient>();
      private readonly IMessageBusProcessor _messageBusProcessor;   
      private readonly IConnectorAliveHandler _connectorAliveHandler;
      private readonly ExchangeGenericConfig _exchangeGenericConfig;
      private System.Timers.Timer _reInitTimer;
      private const int ReinitTimerPeriod = 5000;

      public ConnectionManager(ILoggerFactory loggerFactory, 
                               IPublicClient publicClient, 
                               IPrivateClientFactory privateClientFactory,                             
                               IMessageBusProcessor messageBusProcessor,
                               IConnectorAliveHandler aliveHandler,
                               IOptions<ExchangeGenericConfig> genericExchangeConfig
                               )
      {
         _logger = loggerFactory.CreateLogger<ConnectionManager>();     
         _publicClient = publicClient;
         _privateClientFactory = privateClientFactory;
         _messageBusProcessor = messageBusProcessor;
         _connectorAliveHandler = aliveHandler;
         _exchangeGenericConfig = genericExchangeConfig.Value;
         _messageBusProcessor.OnOrderPlacementCommand += PlaceOrder;
         _messageBusProcessor.OnGetBalancesCommand += GetBalances;
         _messageBusProcessor.OnBulkOrderPlacementCommand += BulkOrderPlacementCommand;
         _messageBusProcessor.OnCancelAllOrdersCommand += CancelAllOrdersCommand;
         _messageBusProcessor.OnOrderCancelCommand += CancelOrderCommand;
         _messageBusProcessor.OnGetOrderBookCommand += GetOrderBookCommand;
         _messageBusProcessor.OnOpenOrderQueryCommand += OpenOrderQueryCommand;
         _messageBusProcessor.OnGetRefDataCommand += GetRefDataCommand;
         _messageBusProcessor.OnPrivateConnectCommand += OnPrivateConnectionCmd;
         _messageBusProcessor.OnPublicConnectCommand += OnPublicConnectionCmd;
         _messageBusProcessor.OnMarketOrderPlacementCommand += OnMarketOrderPlacementCommand;
         _messageBusProcessor.OnGetLatestTradesCommand += OnGetLatestTradesCommand;

         _connectorAliveHandler.Init();
      }

      private void OnGetLatestTradesCommand(string instanceName, string symbol)
      {
         try
         {
            _logger.LogInformation("Got Get Orderbook Command from {Place} for {Symbol}", instanceName, symbol);
            _publicClient.GetLatestTrades(instanceName, symbol);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting orderbook for {Symbol} - {Error}", symbol, e.Message);
         }
      }

      private async Task OnPublicConnectionCmd()
      {
         await _publicClient.Init();
      }

      private async Task OnPrivateConnectionCmd(string account, PrivateConnectionLogon msg)
      {
         // Might be a reconnect
         if (_privateClients.ContainsKey(account))
         {
            var client = _privateClients[account];
            await client.Init(msg);
         }
         else
         {
            var privateClient = _privateClientFactory.CreatePrivateClient();
            if (privateClient is null)
            {
               _logger.LogCritical("Unable to create private clients");
               throw new Exception("Unable to create private clients");
            }
            _privateClients.Add(account, privateClient);
            await privateClient.Init(msg);
         }
      }

      private async Task GetRefDataCommand()
      {
         try
         {
            await _publicClient.GetReferenceData();
         }
         catch (Exception e)
         {
            _logger.LogError("{Error}", e.Message);
            // throw;
         }
      }

      private async Task OpenOrderQueryCommand(string account, string[] symbols)
      {
         try
         {
            _logger.LogInformation("**************** Received get open orders query ************************");
            if (!_privateClients.ContainsKey(account))
            {
               _logger.LogError("Unable to find client for {Account}", account);
               throw new Exception($"");
            }
            var client = _privateClients[account];
            if (client.LoggedInState())
            {
               await client.GetOpenOrders(symbols);
            }
            else
               _logger.LogError("Error - attempting operation before logged in");
         }
         catch (Exception e)
         {
            _logger.LogError("{Error}", e.Message);
            // throw;
         }
      }

      private void  GetOrderBookCommand(string instanceName, string symbol)
      {
         try
         {
            _logger.LogInformation("Got Get Orderbook Command from {Place} for {Symbol}", instanceName, symbol);
            _publicClient.GetOrderBook(instanceName, symbol);
         }
         catch(Exception e)
         {
            _logger.LogError(e, "Error getting orderbook for {Symbol} - {Error}", symbol, e.Message);
         }
      }

      private async Task CancelOrderCommand(string account, OrderIdHolder orderDetails)
      {
         try
         {
            if (!_privateClients.ContainsKey(account))
            {
               _logger.LogError("Unable to find client for {Account}", account);
               throw new Exception($"");
            }
            var client = _privateClients[account];
            if (client.LoggedInState())
               await client.CancelOrder(orderDetails);
            else
               _logger.LogError("Error - attempting operation before logged in");

         }
         catch (Exception e)
         {
            _logger.LogError("{Error}", e.Message);
            // throw;
         }
      }

      private async Task CancelAllOrdersCommand(string account, string symbol)
      {
         try
         {
            if (!_privateClients.ContainsKey(account))
            {
               _logger.LogError("Unable to find client for {Account}", account);
               throw new Exception($"Unable to find client for {account}");
            }
            var client = _privateClients[account];
            if (client.LoggedInState())
               await client.CancelAllOrdersCommand(symbol);
            else
               _logger.LogError("Error - attempting operation before logged in");
         }
         catch (Exception e)
         {
            _logger.LogError("{Error}", e.Message);
            // throw;
         }
      }

      private Task BulkOrderPlacementCommand(string arg1, PlaceOrderCmd[] arg2)
      {
         throw new NotImplementedException();
      }

      private async Task GetBalances(string instanceName, int jobNo)
      {
         try
         {
            if (!_privateClients.ContainsKey(instanceName))
            {
               _logger.LogError("Unable to find client for {Account}", instanceName);
               throw new Exception($"Unable to find client for {instanceName}");
            }
            var client = _privateClients[instanceName];
            if (client.LoggedInState())
               await client.GetBalances(jobNo);
            else
               _logger.LogError("Error - attempting operation before logged in");
         }
         catch (Exception e)
         {
            _logger.LogError("{Error}", e.Message);
            // throw;
         }
      }

      private async Task PlaceOrder(string account, PlaceOrderCmd cmd)
      {
         try
         {
            if (!_privateClients.ContainsKey(account))
            {
               _logger.LogError("Unable to find client for {Account}", account);
               throw new Exception($"");
            }
            var client = _privateClients[account];
            await client.PlaceOrder(cmd);

         }
         catch(Exception e)
         {
            _logger.LogError("{Error}", e.Message);
            // throw;
         }
      }

      private async Task OnMarketOrderPlacementCommand(string account, PlaceOrderCmd cmd)
      {
         try
         {
            if (!_privateClients.ContainsKey(account))
            {
               _logger.LogError("Unable to find client for {Account}", account);
               throw new Exception($"");
            }
            var client = _privateClients[account];
            await client.PlaceMarketOrder(cmd);

         }
         catch (Exception e)
         {
            _logger.LogError("{Error}", e.Message);
            // throw;
         }
      }

      public async Task Init()
      {
         try
         {
            await _publicClient.Init();
            _messageBusProcessor.Start(_exchangeGenericConfig.ExchangeName);
         }
         catch (Exception e)
         {
            // An error in initialising the public side of connection
            // Start a timer
            _reInitTimer.Start();
            _reInitTimer.Interval = ReinitTimerPeriod;
            _reInitTimer.Elapsed += ReconnectToPublicSide;
         }
      }

      private void ReconnectToPublicSide(object sender, ElapsedEventArgs e)
      {
         _reInitTimer.Stop();
         _logger.LogInformation("Attempting to reconnect to public rest api or websocket");
         Task.Run(async () =>
         {
            await _publicClient.Init();
            _messageBusProcessor.Start(_exchangeGenericConfig.ExchangeName);
         });
        
      }
   }
}
