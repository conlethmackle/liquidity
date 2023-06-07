using KuCoin.Config;
using KuCoin.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Common.Models;
using Common.Messages;

namespace KuCoin.RestApi
{
   public interface IKuCoinRestApiClient
   {
      Task<Result<WebsocketTokenAndServer>> GetPrivateWebsocketInfo();
      Task<Result<WebsocketTokenAndServer>> GetPublicWebsocketInfo();
      Task<Result<OrderBookSnapShotContainer>> GetFullOrderBookSnapshot(string symbol);
      Task<Result<OrderIdHolder>> PlaceSingleOrder(KuCoinOrderPlacement order);
      Task<Result<OrderIdHolder>> PlaceSingleMarketOrder(KuCoinOrderMarketOrderPlacement order);
      Task<Result<SingleCancelledOrderId>> CancelOrder(string orderId);
      Task<Result<CancelAllCancelledOrderIds>> CancelAllOrders(string symbol);
      Task<Result<KuCoinAccount[]>> GetAccounts();
      Task<Result<KuCoinOrderQueryResponse[]>> GetOpenOrdersList(string symbol);
      Task<Result<KuCoinCoinPairRefData[]>> GetReferenceData();

      Task<Result<OrderFillResponse[]>> GetRecentFills();

      void Configure(PrivateConnectionLogon config);
   }

   public class KuCoinRestApiClient : IKuCoinRestApiClient
   {
      private readonly ILogger<KuCoinRestApiClient> _logger;
      private readonly KuCoinConnectionConfig _config;
      private readonly HttpClient _client;
      private const string OK_Response = "200000";
      private JsonSerializerOptions _jsonSerializerOptions { get; set; }

      private PrivateConnectionLogon _apiKeyData { get; set; } = null;
      public KuCoinRestApiClient(ILoggerFactory loggerFactory, HttpClient client, IOptions<KuCoinConnectionConfig> privateConfig)
      {
         _logger = loggerFactory.CreateLogger<KuCoinRestApiClient>();
         _config = privateConfig.Value;
         _client = client;
         _jsonSerializerOptions = new JsonSerializerOptions()
         {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
         };
      }

      public void Configure(PrivateConnectionLogon config)
      {
         _apiKeyData = config;
      }

      private void GetPrivateHeaders(HttpRequestMessage messageHandler, string httpVerb, string method, string extra = "")
      {
         
         DateTimeOffset now = DateTimeOffset.UtcNow;
         long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
         var body = unixTimeMilliseconds.ToString() + httpVerb + "/" + method  + extra;
         var signature = Convert.ToBase64String(HashHMAC(new UTF8Encoding().GetBytes(_apiKeyData.Secret),
                  new UTF8Encoding().GetBytes(body)));

         var passphrase = Convert.ToBase64String(HashHMAC(new UTF8Encoding().GetBytes(_apiKeyData.Secret),
                  new UTF8Encoding().GetBytes(_apiKeyData.PassPhrase)));

         messageHandler.Headers.Add("KC-API-SIGN", signature);
         messageHandler.Headers.Add("KC-API-TIMESTAMP", unixTimeMilliseconds.ToString());
         messageHandler.Headers.Add("KC-API-KEY", _apiKeyData.ApiKey);
         messageHandler.Headers.Add("KC-API-PASSPHRASE", passphrase);
         messageHandler.Headers.Add("KC-API-KEY-VERSION", "2");
         messageHandler.Content = new StringContent(
                              extra,
                              System.Text.Encoding.UTF8,
                              "application/json");


         if (httpVerb.Equals("POST"))
         {
            messageHandler.Content = new StringContent(
                                 extra,
                                 System.Text.Encoding.UTF8,
                                 "application/json");
         }
      }

      private static byte[] HashHMAC(byte[] key, byte[] message)
      {
         var hash = new HMACSHA256(key);
         return hash.ComputeHash(message);
      }

      private void HandleErrorResponse(string responseContent)
      {
         if (IsResponseAnError(responseContent))
         {
            var errorResponse = JsonSerializer.Deserialize<KuCoinRestApiErrorMsg>(responseContent);
            var exception = new Exception(errorResponse.Msg);
            throw exception;
         }
      }
      private static bool IsResponseAnError(string response)
      {
         string strRegex = $".*?code.*?:.*{OK_Response}.*?";

         var regex = new Regex(strRegex);

         if (regex.IsMatch(response))
            return false;
         return true;
      }

      public async Task<Result<WebsocketTokenAndServer>> GetPrivateWebsocketInfo()
      {
         try
         {
            
            HttpClient client = new HttpClient();
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Post,
               RequestUri = new Uri(_config.Url + _config.PrivateWebSocketEndpoint),
            };

          
            GetPrivateHeaders(messageHandler, "POST", _config.PrivateWebSocketEndpoint,  "{}");
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  HandleErrorResponse(responseContent);
                  var data = JsonSerializer.Deserialize<WebsocketEndpoints>(responseContent, _jsonSerializerOptions);
                  return Result.OK(data.Data);
               }
               var msg = $"Error in data returned from Kucoin GetPrivateWebsocketInfo {response.StatusCode}";
               _logger.LogError("Error in data returned from Kucoin {Endpoint} {StatusCode}", "GetPrivateWebsocketInfo", response.StatusCode);
               return Result.Fail<WebsocketTokenAndServer>(msg);
            }
         }
         catch(Exception e)
         {
            _logger.LogError("Error connecting to {URL}{Endpoint} with error {Error}",
                               _config.Url, _config.PrivateWebSocketEndpoint, e.Message);
            throw;
         }
      }

      public async Task<Result<WebsocketTokenAndServer>> GetPublicWebsocketInfo()
      {
         try
         {
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Post,
               RequestUri = new Uri(_config.Url + _config.PublicWebSocketEndpoint),
            };
            
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  HandleErrorResponse(responseContent);
                  var data = JsonSerializer.Deserialize<WebsocketEndpoints>(responseContent, _jsonSerializerOptions);
                  return Result.OK(data.Data);
               }
               var msg = $"Error in data returned from Kucoin GetPublicWebsocketInfo {response.StatusCode}";
               _logger.LogError("Error in data returned from Kucoin {Endpoint} {StatusCode}", _config.PublicWebSocketEndpoint, response.StatusCode);
               return Result.Fail<WebsocketTokenAndServer>(msg);             
            }
         }
         catch (Exception e)
         {
            _logger.LogError("Error connecting to {URL}{Endpoint} with error {Error}",
                               _config.Url, _config.PublicWebSocketEndpoint, e.Message);
            throw;
         }
      }

      public async Task<Result<OrderBookSnapShotContainer>> GetFullOrderBookSnapshot(string symbol)
      {
         try
         {
            //var x = "api/v3/market/orderbook/level2?symbol=BTC-USDT";
            var endpoint = "api/v1/market/orderbook/level2_100"+ $"?symbol={symbol}";

            //var endpoint = "api/v3/market/orderbook/level2" + $"?symbol={symbol}";
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Get,
               RequestUri = new Uri(_config.Url + endpoint),
            };
            
           // GetPrivateHeaders(messageHandler, "GET", y);
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  HandleErrorResponse(responseContent);
                  
                  var data = JsonSerializer.Deserialize<OrderBookSnapShotContainer>(responseContent, _jsonSerializerOptions);
                  return Result.OK(data);
               }
               var msg = $"Error in data returned from Kucoin {endpoint} {response.StatusCode}";
               _logger.LogError("Error in data returned from Kucoin {Endpoint} {StatusCode}", endpoint, response.StatusCode);
               return Result.Fail<OrderBookSnapShotContainer>(msg);
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error connecting to {URL}/api/v3/market/orderbook/level2 with error {Error}",
                               _config.Url,  e.Message);
            throw;
         }
      }
 
      public async Task<Result<OrderIdHolder>> PlaceSingleOrder(KuCoinOrderPlacement order)
      {
         try
         {
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Post,
               RequestUri = new Uri(_config.Url + "api/v1/orders"),
            };
            var body = JsonSerializer.Serialize(order);
            GetPrivateHeaders(messageHandler, "POST", "api/v1/orders", body);
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  if (IsResponseAnError(responseContent))
                  {
                     var errorResponse = JsonSerializer.Deserialize<KuCoinRestApiErrorMsg>(responseContent);
                     return Result.Fail<OrderIdHolder>(errorResponse.Msg);
                 }
                     
                  var data = JsonSerializer.Deserialize<SingleOrderPlacementResponse>(responseContent, _jsonSerializerOptions);
                  return Result.OK(data.Data);             
               }
               var msg = $"Error in data returned from Kucoin /api/v1/orders {response.StatusCode}";
               _logger.LogError("Error in data returned from Kucoin {Endpoint} {StatusCode}", "/api/v1/orders", response.StatusCode);
               return Result.Fail<OrderIdHolder>(msg);
            }
         }
         catch (Exception e)
         {
            var errorMsg = $"Error connecting to {_config.Url}/api/v1/orders with error {e.Message}";
            _logger.LogError("Error connecting to {URL}{Endpoint} with error {Error}",
                               _config.Url, "/api/v1/orders", e.Message);
            return Result.Fail<OrderIdHolder>(errorMsg);
         }
      }

      public async Task<Result<OrderIdHolder>> PlaceSingleMarketOrder(KuCoinOrderMarketOrderPlacement order)
      {
         try
         {
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Post,
               RequestUri = new Uri(_config.Url + "api/v1/orders"),
            };
            var body = JsonSerializer.Serialize(order);
            GetPrivateHeaders(messageHandler, "POST", "api/v1/orders", body);
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  if (IsResponseAnError(responseContent))
                  {
                     var errorResponse = JsonSerializer.Deserialize<KuCoinRestApiErrorMsg>(responseContent);
                     return Result.Fail<OrderIdHolder>(errorResponse.Msg);
                  }

                  var data = JsonSerializer.Deserialize<SingleOrderPlacementResponse>(responseContent, _jsonSerializerOptions);
                  return Result.OK(data.Data);
               }
               var msg = $"Error in data returned from Kucoin /api/v1/orders {response.StatusCode}";
               _logger.LogError("Error in data returned from Kucoin {Endpoint} {StatusCode}", "/api/v1/orders", response.StatusCode);
               return Result.Fail<OrderIdHolder>(msg);
            }
         }
         catch (Exception e)
         {
            var errorMsg = $"Error connecting to {_config.Url}/api/v1/orders with error {e.Message}";
            _logger.LogError("Error connecting to {URL}{Endpoint} with error {Error}",
               _config.Url, "/api/v1/orders", e.Message);
            return Result.Fail<OrderIdHolder>(errorMsg);
         }
      }

      public async Task<Result<SingleCancelledOrderId>> CancelOrder(string orderId)
      {
         try
         {
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Delete,
               RequestUri = new Uri(_config.Url + $"api/v1/orders/{orderId}"),
               
            };
            
            GetPrivateHeaders(messageHandler, "DELETE", $"api/v1/orders/{orderId}", "");
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  if (IsResponseAnError(responseContent))
                  {
                     var errorResponse = JsonSerializer.Deserialize<KuCoinRestApiErrorMsg>(responseContent);
                     return Result.Fail<SingleCancelledOrderId>(errorResponse.Msg);
                  }
                
                  var data = JsonSerializer.Deserialize<SingleCancelOrderResponseContainer>(responseContent, _jsonSerializerOptions);
                  return Result.OK(data.CancelledOrderId);
               }
               var msg = $"Error in data returned from Kucoin /api/v1/orders {response.StatusCode}";
               _logger.LogError("Error in data returned from Kucoin {Endpoint} {StatusCode}", "/api/v1/orders", response.StatusCode);
               return Result.Fail<SingleCancelledOrderId>(msg);
            }
         }
         catch (Exception e)
         {
            _logger.LogError("Error connecting to {URL}{Endpoint} with error {Error}",
                               _config.Url, "/api/v1/orders", e.Message);
            throw;
         }
      }

      public async Task<Result<CancelAllCancelledOrderIds>> CancelAllOrders(string symbol)
      {
         try
         {
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Delete,
               RequestUri = new Uri(_config.Url + $"api/v1/orders/symbol={symbol}"),
            };

            GetPrivateHeaders(messageHandler, "DELETE", $"api/v1/orders/symbol={symbol}", "");
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  if (IsResponseAnError(responseContent))
                  {
                     var errorResponse = JsonSerializer.Deserialize<KuCoinRestApiErrorMsg>(responseContent);
                     return Result.Fail<CancelAllCancelledOrderIds>(errorResponse.Msg);
                  }
                  //HandleErrorResponse(responseContent);
                  var data = JsonSerializer.Deserialize<CancelAllOrderResponseContainer>(responseContent, _jsonSerializerOptions);
                  return Result.OK(data.CancelledOrderIds);
               }
               var msg = $"Error in data returned from Kucoin /api/v1/orders CancelAllOrders {response.StatusCode}";
               _logger.LogError("Error in data returned from Kucoin {Endpoint} CancelAllOrders {StatusCode}", "/api/v1/orders", response.StatusCode);
               return Result.Fail<CancelAllCancelledOrderIds>(msg);
            }
         }
         catch (Exception e)
         {
            _logger.LogError("Error connecting to {URL}{Endpoint} with error {Error}",
                               _config.Url, "/api/v1/orders", e.Message);
            throw;
         }
      }

      public async Task<Result<KuCoinOrderQueryResponse[]>> GetOpenOrdersList(string symbol)
      {
         try
         {
            var cmd = $"api/v1/orders?status=active&symbol={symbol}";
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Get,
               RequestUri = new Uri(_config.Url + cmd),
            };
           
            GetPrivateHeaders(messageHandler, "GET", cmd, "");
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  if (IsResponseAnError(responseContent))
                  {
                     var errorResponse = JsonSerializer.Deserialize<KuCoinRestApiErrorMsg>(responseContent);
                     return Result.Fail<KuCoinOrderQueryResponse[]>(errorResponse.Msg);
                  }
                  // HandleErrorResponse(responseContent);
                  var dataRsp = JsonSerializer.Deserialize<KuCoinCoinOrderQueryResponseData>(responseContent, _jsonSerializerOptions);                
                  var data = dataRsp.Data.Items;
                  return Result.OK(data);
               }
               var msg = $"Error in data returned from Kucoin /api/v1/orders CancelAllOrders {response.StatusCode}";
               _logger.LogError("Error in data returned from Kucoin {Endpoint} CancelAllOrders {StatusCode}", "/api/v1/orders", response.StatusCode);
               return Result.Fail<KuCoinOrderQueryResponse[]>(msg);             
            }
         }
         catch(Exception e)
         {
            _logger.LogError("Error connecting to {URL}{Endpoint} with error {Error}",
                               _config.Url, "/api/v1/orders", e.Message);
            throw;
         }
      }

      public async Task<Result<KuCoinAccount[]>> GetAccounts()
      {
         try
         {
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Get,
               RequestUri = new Uri(_config.Url + "api/v1/accounts"),
            };
            GetPrivateHeaders(messageHandler, "GET", "api/v1/accounts", "");
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  if (IsResponseAnError(responseContent))
                  {
                     var errorResponse = JsonSerializer.Deserialize<KuCoinRestApiErrorMsg>(responseContent);
                     return Result.Fail<KuCoinAccount[]>(errorResponse.Msg);
                  }
                  //HandleErrorResponse(responseContent);
                  var responseData = JsonSerializer.Deserialize<AccountContainer>(responseContent, _jsonSerializerOptions);  
                  var data = responseData.Data;
                  return Result.OK(data);
                 
               }
               var msg = $"Error in data returned from Kucoin api/v1/accounts {response.StatusCode}";
               _logger.LogError("Error in data returned from Kucoin {Endpoint} {StatusCode}", "api/v1/accounts", response.StatusCode);
               return Result.Fail<KuCoinAccount[]>(msg);
            }
         }
         catch (Exception e)
         {
            _logger.LogError("Error connecting to {URL}{Endpoint} with error {Error}",
                               _config.Url, _config.PrivateWebSocketEndpoint, e.Message);
            throw;
         }
      }

      public async Task<Result<KuCoinCoinPairRefData[]>> GetReferenceData()
      {
         try
         {
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Get,
               RequestUri = new Uri(_config.Url + "api/v1/symbols"),
            };
          
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  if (IsResponseAnError(responseContent))
                  {
                     var errorResponse = JsonSerializer.Deserialize<KuCoinRestApiErrorMsg>(responseContent);
                     return Result.Fail<KuCoinCoinPairRefData[]>(errorResponse.Msg);
                  }

                  var responseData = JsonSerializer.Deserialize<RefDataContainer>(responseContent, _jsonSerializerOptions);   
                  var data = responseData.Data;
                  return Result.OK(data);                  
               }
               var msg = $"Error in data returned from Kucoin api/v1/symbols {response.StatusCode}";
               _logger.LogError("Error in data returned from Kucoin {Endpoint} {StatusCode}", "api/v1/symbols", response.StatusCode);
               return Result.Fail<KuCoinCoinPairRefData[]>(msg);
            }
         }
         catch (Exception e)
         {
            _logger.LogError("Error connecting to {URL}{Endpoint} with error {Error}",
                               _config.Url, "api/v1/symbols", e.Message);
            throw;
         }
      }

      public async Task<Result<OrderFillResponse[]>> GetRecentFills()
      {
         try
         {
            var cmd = $"api/v1/fills";
            HttpRequestMessage messageHandler = new HttpRequestMessage()
            {
               Method = HttpMethod.Get,
               RequestUri = new Uri(_config.Url + cmd),
            };

            GetPrivateHeaders(messageHandler, "GET", cmd, "");
            using (HttpResponseMessage response = await _client.SendAsync(messageHandler))
            {
               if (response.IsSuccessStatusCode)
               {
                  string responseContent = await response.Content.ReadAsStringAsync();
                  if (IsResponseAnError(responseContent))
                  {
                     var errorResponse = JsonSerializer.Deserialize<KuCoinRestApiErrorMsg>(responseContent);
                     return Result.Fail<OrderFillResponse[]>(errorResponse.Msg);
                  }
                  var responseData = JsonSerializer.Deserialize<OrderFillResponseData>(responseContent, _jsonSerializerOptions);            
                  var data = responseData.Data.Items;
                  return Result.OK(data);                 
               }
               var msg = $"Error in data returned from Kucoin api/v1/fills {response.StatusCode}";
               _logger.LogError("Error in data returned from Kucoin {Endpoint} {StatusCode}", "api/v1/fills", response.StatusCode);
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError("Error connecting to {URL}{Endpoint} with error {Error}",
                               _config.Url, "api/v1/fills", e.Message);
            throw;
         }
      }
   }     
}
