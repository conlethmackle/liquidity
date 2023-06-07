using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binance.Net.Clients;
using Binance.Net.Objects;
using Binance.Net.Enums;
using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Binance.Config;
using Binance.Models;
using Common.Models;
using Binance.Net.Objects.Models.Spot;
using System.Net.Http;
using Binance.Net.Interfaces;
using Common;
using Common.Messages;
using ConnectorStatus;

namespace Binance.RestApi
{
   public interface IBinanceRestApiClient
   {
      void Init(PrivateConnectionLogon accountsConfig, string statusTopic);
      Task<Result<string>> GetListenKey();
      Task<Result<OrderIdHolder>> PlaceSpotLimitOrder(BinanceSpotOrderPlacement order);
      Task<Result<OrderIdHolder>> PlaceSpotMarketOrder(BinanceSpotOrderPlacement order);
      Task<Result<SingleCancelledOrderId>> CancelSpotOrder(OrderIdHolder cancel);
      Task<Result<List<SingleCancelledOrderId>>> CancelAllSpotOrders(string? symbol);
      Task<Result<IEnumerable<BinanceOrder>>> GetOpenSpotOrders(string? symbol);
      Task<Result<BinanceAccountInfo>> GetBalances();
      Task<Result<BinanceOrderBook>> GetOrderBook(string symbol);
      Task<Result<IEnumerable<BinanceSymbol>>> GetAvailablePairs();
      Task<List<IBinanceRecentTrade>> GetTradeHistory(string symbol, int limit);
      Task KeepUserStreamAlive(string listenKey);
   }

   public class BinanceRestApiClient : IBinanceRestApiClient
   {
      private readonly ILogger<BinanceRestApiClient> _logger;
      private BinanceClient _client = null;
      private BinanceClient _publicClient = null;
      private readonly IConnectorClientStatus _clientStatus;
      private readonly string _restEndpoint;
      private readonly string _apiKey;
      private readonly string _secret;
      private string _statusTopic { get; set; }
      private string _account { get; set; }
      private string _instance { get; set; }
      private int? _receiveWindowSize = 5000;
      public BinanceRestApiClient(ILoggerFactory factory, 
                                  IOptions<PrivateConnectionConfig> config,
                                  IConnectorClientStatus clientStatus)
      {
         _logger = factory.CreateLogger<BinanceRestApiClient>();
         _restEndpoint = config.Value.RestEndpoint;
         _clientStatus = clientStatus;
         try
         {
            _publicClient = new BinanceClient(new BinanceClientOptions()
            {
               SpotApiOptions = new BinanceApiClientOptions
               {
                  BaseAddress = _restEndpoint
               }
            });
            _clientStatus.UpdatePublicRestApiStatus(true, Constants.BINANCE, "All Good with public RestApi Binance");
         }
         catch (Exception e)
         {
            _logger.LogError("Error Creating client for public RestApi Binance {Error}", e.Message);
            _clientStatus.UpdatePublicRestApiStatus(false, Constants.BINANCE, $"Error Creating client for public RestApi Binance - {e.Message}");
         }
        
         // TODO - ENCRYPT !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
      }

      public void Init(PrivateConnectionLogon accountsConfig, string statusTopic)
      {
         // need to inject this !!!!!
         try
         {
            _statusTopic = statusTopic;
            _account = accountsConfig.SPName;
            _instance = accountsConfig.ConfigInstance;
            _client = new BinanceClient(new BinanceClientOptions()
            {
               ApiCredentials = new ApiCredentials(accountsConfig.ApiKey, accountsConfig.Secret),
               SpotApiOptions = new BinanceApiClientOptions
               {
                  BaseAddress = _restEndpoint
               }
            });
            _clientStatus.UpdatePrivateRestApiStatus(_statusTopic, _account, _instance, true, "All Good with private RestApi Bitfinex");

         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error creating private RestApi endpoint Binance - error {Error}", e.Message);
            _clientStatus.UpdatePrivateRestApiStatus(_statusTopic, _account, _instance, false, $"Error with private RestApi Binance{e.Message}");
            throw;
         }
      }

      public async Task<Result<string>> GetListenKey()
      {
         var result = await _client.SpotApi.Account.StartUserStreamAsync();
         if (result.Success)
         {
            return Result.OK(result.Data);
         }
    //     _logger.LogError(result.Error.Message);
         return Result.Fail<string>(result.Error.Message);        
      } 

      public async Task<Result<IEnumerable<BinanceSymbol>>> GetAvailablePairs()
      {
         var result = await _client.SpotApi.ExchangeData.GetExchangeInfoAsync();
         if (result.Success)
         {
            var symbols = result.Data.Symbols;
            
            return Result.OK(symbols);
         }
         
         return Result.Fail<IEnumerable<BinanceSymbol>>(result.Error.Message);
      }

      public async Task KeepUserStreamAlive(string listenKey)
      {
         await _client.SpotApi.Account.KeepAliveUserStreamAsync(listenKey);
      }

      public async Task<Result<OrderIdHolder>> PlaceSpotLimitOrder(BinanceSpotOrderPlacement order)
      {
         var result = await _client.SpotApi.Trading.PlaceOrderAsync(order.Symbol, 
                                                                    order.Side ,
                                                                    SpotOrderType.Limit, 
                                                                    order.Quantity, 
                                                                    order.QuoteQuantity, 
                                                                    order.CustomerOrderId, 
                                                                    order.Price, order.Tif, 
                                                                    order.StopPrice, 
                                                                    order.IcebergQuantity, 
                                                                    Binance.Net.Enums.OrderResponseType.Result, 
                                                                    _receiveWindowSize);
         if (result.Success)
         {
            var resp = result.Data;
            var orderIdHolder = new OrderIdHolder()
            {
               ClientOrderId = resp.ClientOrderId,
               OrderId = resp.Id.ToString()
            };
            return Result.OK(orderIdHolder);            
         }
     //    _logger.LogError(result.Error.Message);
         return Result.Fail<OrderIdHolder>(result.Error.Message);
      }

      public async Task<Result<OrderIdHolder>> PlaceSpotMarketOrder(BinanceSpotOrderPlacement order)
      {
         
         var result = await _client.SpotApi.Trading.PlaceOrderAsync(order.Symbol,
                                                                    order.Side,
                                                                    SpotOrderType.Market,
                                                                    order.Quantity,
                                                                    order.QuoteQuantity,
                                                                    order.CustomerOrderId,
                                                                    null, null,
                                                                  //  order.Price, order.Tif,
                                                                    order.StopPrice,
                                                                    order.IcebergQuantity,
                                                                    Binance.Net.Enums.OrderResponseType.Result,
                                                                    _receiveWindowSize);
         if (result.Success)
         {
            var resp = result.Data;
            var orderIdHolder = new OrderIdHolder()
            {
               ClientOrderId = resp.ClientOrderId,
               OrderId = resp.Id.ToString()
            };
            return Result.OK(orderIdHolder);
         }
         //    _logger.LogError(result.Error.Message);
         return Result.Fail<OrderIdHolder>(result.Error.Message);
      }

      public async Task<Result<SingleCancelledOrderId>> CancelSpotOrder(OrderIdHolder cancel)
      {
         var result = await _client.SpotApi.Trading.CancelOrderAsync(cancel.Symbol, Int64.Parse(cancel.OrderId), cancel.ClientOrderId);
         var exchangeInfo = await _client.SpotApi.ExchangeData.GetExchangeInfoAsync();
         if (result.Success)
         {          
            var cancelledOrderDetails = new SingleCancelledOrderId();
            cancelledOrderDetails.ClientOrderId = result.Data.OriginalClientOrderId;
            cancelledOrderDetails.OrderId = result.Data.Id.ToString();           
            return Result.OK(cancelledOrderDetails);
         }
         _logger.LogError(result.Error.Message);
         return Result.Fail<SingleCancelledOrderId>(result.Error.Message);
      }

      public async Task<Result<List<SingleCancelledOrderId>>> CancelAllSpotOrders(string? symbol)
      {
         var result = await _client.SpotApi.Trading.CancelAllOrdersAsync(symbol);
         if (result.Success)
         {
            var cancelledOrders = new List<SingleCancelledOrderId>();
            var resp = result.Data;
            foreach(var order in resp)
            {
               var cancelledOrderDetails = new SingleCancelledOrderId();
               cancelledOrderDetails.ClientOrderId = order.ClientOrderId;
               cancelledOrderDetails.OrderId = order.Id.ToString();

               cancelledOrders.Add(cancelledOrderDetails);
            }
            return Result.OK(cancelledOrders);
         }
    //     _logger.LogError(result.Error.Message);
         return Result.Fail<List<SingleCancelledOrderId>>(result.Error.Message);
      }

      public async Task<Result<IEnumerable<BinanceOrder>>> GetOpenSpotOrders(string? symbol)
      {
         
         var result = await _client.SpotApi.Trading.GetOpenOrdersAsync(symbol);
         if (result.Success)
         {
            return Result.OK(result.Data);
         }
  //       _logger.LogError(result.Error.Message);
         return Result.Fail<IEnumerable<BinanceOrder>> (result.Error.Message);
      }

      public async Task<Result<BinanceAccountInfo>> GetBalances()
      {
         var result = await _client.SpotApi.Account.GetAccountInfoAsync();
         if (result.Success)
         {
            return Result.OK(result.Data);
         }
  //       _logger.LogError(result.Error.Message);
         return Result.Fail<BinanceAccountInfo>(result.Error.Message);
      }

      public async Task<Result<BinanceOrderBook>> GetOrderBook(string symbol)
      {
         var result = await _publicClient.SpotApi.ExchangeData.GetOrderBookAsync(symbol);
         if (result.Success)
         {
            return Result.OK(result.Data);
         }
  //       _logger.LogError(result.Error.Message);
         return Result.Fail<BinanceOrderBook>(result.Error.Message);
      }

      public async Task<List<IBinanceRecentTrade>> GetTradeHistory(string symbol, int limit)
      {
         try
         {
            var res = await _publicClient.SpotApi.ExchangeData.GetTradeHistoryAsync(symbol, limit);
            if (res.Success)
            {
               var trades = res.Data.ToList();
               return trades;
            }
            else
            {
               _logger.LogError("Error in Getting latest trades - {Error}", res.Error);
               return null;
            }

         }
         catch (Exception e)
         {

            _logger.LogError(e, "Error in Getting latest trades {Error}", e.Message);
            return null;
         }
      }
   }
}
