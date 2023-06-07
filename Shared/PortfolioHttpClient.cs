using Common.Models.DTOs;
using System.Net.Http.Json;
using Common.Messages;
using Microsoft.Extensions.Logging;
using System.Web.Mvc;
using System.Text.Json;
using System.Text;

namespace SyncfusionLiquidity.Shared
{
   public class PortfolioHttpClient
   {
         private readonly HttpClient _http;
         //private readonly ILogger<PortfolioHttpClient> _logger;

         public PortfolioHttpClient(HttpClient http)
         {
            _http = http;
          // _logger = loggerFactory.CreateLogger<PortfolioHttpClient>();
         }
    
         public async Task<List<SPDTO>> GetPortfolios()
         {         
            return await _http.GetFromJsonAsync<List<SPDTO>>("api/PortfolioManagement/portfolios");
         }

         public async Task<List<StrategyExchangeConfigDTO>> GetStrategyConfigsForSP(string name)
         {
            return await _http.GetFromJsonAsync<List<StrategyExchangeConfigDTO>>($"api/StrategyConfig/strategyexchangeconfig/get?sp={name}");        
         }

         public async Task<LiquidationStrategyConfigDTO> GetLiquidationStrategyConfig(int id)
         {
            return await _http.GetFromJsonAsync<LiquidationStrategyConfigDTO>($"api/StrategyConfig/liquidationStrategyConfigs/getById?strategyConfigId={id}");
         }

         public async Task<HttpResponseMessage> UpdateLiquidationStrategyConfig(LiquidationStrategyConfigDTO config)
         {
            var data = JsonSerializer.Serialize(config);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/StrategyConfig/liquidationStrategyConfigs/update", httpContent);
         }

         public async Task PlaceMarketOrder(OrderDetails cmd)
         {
            await _http.PostAsJsonAsync<OrderDetails>("api/Order/neworder/create", cmd);
         }

      //  public async Task GetOpenOrders(string venue)
         public async Task GetOpenOrders(OpenOrdersRequest request)
         {          
            await _http.PostAsJsonAsync<OpenOrdersRequest>($"api/Order/openorders/request", request);
         }

         public async Task SendStrategyProcessCommand(StrategyProcessDetails cmd)
         {
            await _http.PostAsJsonAsync<StrategyProcessDetails>("api/StrategyProcess/strategyprocess/command", cmd);
         }

         public async Task GetBalancesFromVenue(GetBalanceRequest request)
         {
            await _http.PostAsJsonAsync<GetBalanceRequest>($"api/Order/Balances/get", request);
         }

         public async Task<List<CoinPairDTO>> GetCoinPairs()
         {
            return await _http.GetFromJsonAsync<List<CoinPairDTO>>("api/PortfolioManagement/coinpairs/get");
         }



   }
}
