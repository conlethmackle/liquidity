using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitfinex.Config;
using Common.Messages;
using Common.Models;
using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Bitfinex;
using Common.Models;
using Bitfinex.Net.Objects.Models;
using System.Net.Http;
using Bitfinex.Models;
using Bitfinex.Net.Clients;
using Bitfinex.Net.Enums;
using Bitfinex.Net.Objects;
using Bitfinex.Net.Objects.Models;
using Bitfinex.Net.Objects.Models.V1;
using Common.Messages;
using ConnectorStatus;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;

namespace Bitfinex.RestApi
{
   public interface IBitfinexRestApiClient
   {
      Task Init(PrivateConnectionLogon accountsConfig, string statusTopic);
      
      Task<Result<OrderIdHolder>> PlaceSpotLimitOrder(BitfinexSpotOrderPlacement order);
      Task<Result<OrderIdHolder>> PlaceSpotMarketOrder(BitfinexSpotOrderPlacement order);
      Task<Result<SingleCancelledOrderId>> CancelSpotOrder(OrderIdHolder cancel);
      Task<Result<string>> CancelAllSpotOrders();
      Task<Result<IEnumerable<BitfinexOrder>>> GetOpenSpotOrders(string? symbol);
      Task<Result<List<ExchangeBalance>>> GetBalances();
      Task<Result<BitfinexOrderBook>> GetOrderBook(string symbol);

      Task<List<string>> GetSymbols();
      Task<List<BitfinexTradeSimple>> GetTradeHistory(string symbol, int limit);


   }

   public class BitfinexRestApiClient : IBitfinexRestApiClient
   {
      private readonly ILogger<BitfinexRestApiClient> _logger;
      private BitfinexClient _client = null;
      private readonly IConnectorClientStatus _clientStatus;
      private BitfinexClient _publicClient = null;
      private readonly string _publicRestEndpoint;
      private readonly string _privateRestEndpoint;
      private readonly string _apiKey;
      private readonly string _secret;
      private int? _receiveWindowSize = 5000;
      private string _statusTopic { get; set; }
      private string _account { get; set; }
      private string _instance { get; set; }
      public BitfinexRestApiClient(ILoggerFactory factory, 
                                   IOptions<PrivateConnectionConfig> config,
                                   IConnectorClientStatus clientStatus)
      {
         _logger = factory.CreateLogger<BitfinexRestApiClient>();
         _publicRestEndpoint = config.Value.PublicRestEndpoint;
         _privateRestEndpoint = config.Value.PrivateRestEndpoint;
         _clientStatus = clientStatus;
         try
         {
            _publicClient = new BitfinexClient(new BitfinexClientOptions()
            {
               SpotApiOptions = new RestApiClientOptions()
               {
                  BaseAddress = _publicRestEndpoint
               }
            });
            _clientStatus.UpdatePublicRestApiStatus(true, "All Good with public RestApi Bitfinex");
         }
         catch (Exception e)
         {
            _logger.LogError("Error Creating client for public RestApi Bitfinex {Error}", e.Message);
            _clientStatus.UpdatePublicRestApiStatus(false, $"Error Creating client for public RestApi Bitfinex {e.Message}" );
         }
         
         
         // TODO - ENCRYPT !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
      }

      public async Task Init(PrivateConnectionLogon accountsConfig, string statusTopic)
      {
         // need to inject this !!!!!
         try
         {
            _statusTopic = statusTopic;
            _account = accountsConfig.SPName;
            _instance = accountsConfig.ConfigInstance;

            _client = new BitfinexClient(new BitfinexClientOptions()
            {
               ApiCredentials = new ApiCredentials(accountsConfig.ApiKey, accountsConfig.Secret),
               SpotApiOptions = new RestApiClientOptions()
               {
                  BaseAddress = _privateRestEndpoint
               }
            });
            _clientStatus.UpdatePrivateRestApiStatus(_statusTopic, _account, _instance, true, "All Good with private RestApi Bitfinex");
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error creating private RestApi endpoint Bitfinex - error {Error}", e.Message);
            _clientStatus.UpdatePrivateRestApiStatus(_statusTopic, _account, _instance, false, $"Error with private Bitfinex RestApi {e.Message}");
            throw;
         }
      }

      public async Task<List<BitfinexTradeSimple>> GetTradeHistory(string symbol, int limit)
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
               _logger.LogError("Error in Getting latest trades - {Error}", res.Error.Message);
               _clientStatus.UpdatePublicRestApiStatus(false, $"Error getting in public RestApi Bitfinex - {res.Error.Message}");
               return null;
            }
            
         }
         catch (Exception e)
         {
            _clientStatus.UpdatePublicRestApiStatus(false, $"Error getting in public RestApi Bitfinex - {e.Message}");

            _logger.LogError(e, "Error in Getting latest trades {Error}", e.Message);
            return null;
         }
      }

      public async Task<List<string>> GetSymbols()
      {
         try
         {
            var res = await _publicClient.SpotApi.ExchangeData.GetSymbolsAsync();
            if (res.Success)
            {
               var symbols = res.Data;
               return symbols.ToList();
            }
            else
            {
               _logger.LogError("Error in Getting Symbols - {Error}", res.Error.Message);
               _clientStatus.UpdatePublicRestApiStatus(false, $"Error getting Symbols RestApi Bitfinex - {res.Error.Message}");
               return null;
            }
         }
         catch (Exception e)
         {
            _clientStatus.UpdatePublicRestApiStatus(false, $"Error getting in in Getting Symbols RestApi Bitfinex - {e.Message}");
            _logger.LogError(e, "Error in Getting Symbols Bitfinex {Error}", e.Message);
            return null;
         }
      }



      public async Task<Result<OrderIdHolder>> PlaceSpotLimitOrder(BitfinexSpotOrderPlacement order)
      {
         var result = await _client.SpotApi.Trading.PlaceOrderAsync(order.Symbol,
                                                                    order.Side,
                                                                    OrderType.Limit,
                                                                    order.Quantity,
                                                                    (decimal)order?.Price,null,null,null, Int32.Parse(order.CustomerOrderId)
                                                                    );
      
         if (result.Success)
         {
            var resp = result.Data;
            
            var orderIdHolder = new OrderIdHolder()
            {
               ClientOrderId = resp.Data.ClientOrderId.ToString(),
               OrderId = resp.Data.Id.ToString()
            };
            return Result.OK(orderIdHolder);
         }
         //    _logger.LogError(result.Error.Message);
         return Result.Fail<OrderIdHolder>(result.Error.Message);
      }

      public async Task<Result<OrderIdHolder>> PlaceSpotMarketOrder(BitfinexSpotOrderPlacement order)
      {
         var result = await _client.SpotApi.Trading.PlaceOrderAsync(order.Symbol,
            order.Side,
            OrderType.ExchangeMarket,
            order.Quantity,
            (decimal)order?.Price, null, null, null, Int32.Parse(order.CustomerOrderId)
         );
         if (result.Success)
         {
            var resp = result.Data;

            var orderIdHolder = new OrderIdHolder()
            {
               ClientOrderId = resp.Data.ClientOrderId.ToString(),
               OrderId = resp.Data.Id.ToString()
            };
            return Result.OK(orderIdHolder);
         }
        
         return Result.Fail<OrderIdHolder>(result.Error.Message);
      }

      public async Task<Result<SingleCancelledOrderId>> CancelSpotOrder(OrderIdHolder cancel)
      {
         var orderId = Int64.Parse(cancel.OrderId);
         var clientOid = Int32.Parse(cancel.ClientOrderId);


         var result = await _client.SpotApi.Trading.CancelOrderAsync(orderId);
        
         if (result.Success)
         {
            var cancelledOrderDetails = new SingleCancelledOrderId();
            cancelledOrderDetails.ClientOrderId = result?.Data?.Data.ClientOrderId.ToString();
            cancelledOrderDetails.OrderId = result.Data.Data.Id.ToString();
            return Result.OK(cancelledOrderDetails);
         }
         _logger.LogError(result.Error.Message);
         return Result.Fail<SingleCancelledOrderId>(result.Error.Message);
      }

      public async Task<Result<string>> CancelAllSpotOrders()
      {
         var result = await _client.SpotApi.Trading.CancelAllOrdersAsync();
         if (result.Success)
         {
            if (result.Data.Result == "All orders cancelled")
               return Result.OK(result.Data.Result);
         }
         //     _logger.LogError(result.Error.Message);
         result.Error.Message = "Error cancelling all orders";
         return Result.Fail<string>(result.Error.Message);
      }

      public async Task<Result<IEnumerable<BitfinexOrder>>> GetOpenSpotOrders(string? symbol)
      {
         var result = await _client.SpotApi.Trading.GetOpenOrdersAsync();
         if (result.Success)
         {
            return Result.OK(result.Data);
         }
         //       _logger.LogError(result.Error.Message);
         return Result.Fail<IEnumerable<BitfinexOrder>>(result.Error.Message);
      }

      public async Task<Result<List<ExchangeBalance>>> GetBalances()
      {
         var result = await _client.SpotApi.Account.GetBalancesAsync();
         if (result.Success)
         {
            var exchangeBalances = new List<ExchangeBalance>();
            var balances = result.Data;
            foreach (var bitfinexWallet in balances)
            {
               if (bitfinexWallet.Type == WalletType.Exchange)
               {
                  exchangeBalances.Add(new ExchangeBalance()
                  {
                     Available = (decimal)bitfinexWallet?.Available,
                     Total = (decimal)bitfinexWallet?.Total,
                     Currency = bitfinexWallet.Asset
                  });
               }
            }
           
            return Result.OK(exchangeBalances);
         }
         //       _logger.LogError(result.Error.Message);
         return Result.Fail<List<ExchangeBalance>>(result.Error.Message);
      }

      public async Task<Result<BitfinexOrderBook>> GetOrderBook(string symbol)
      {
         
         var result = await _publicClient.SpotApi.ExchangeData.GetOrderBookAsync(symbol, Precision.PrecisionLevel3);
         if (result.Success)
         {
            return Result.OK(result.Data);
         }
         //       _logger.LogError(result.Error.Message);
         return Result.Fail<BitfinexOrderBook>(result?.Error.Message);
      }
   }
}
