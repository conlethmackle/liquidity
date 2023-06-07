using Common.Models.DTOs;
using System.Net.Http.Json;
using Common.Messages;
using Microsoft.Extensions.Logging;
using System.Web.Mvc;
using System.Text.Json;
using System.Text;
using Common.Models;

namespace BlazorLiquidity.Shared
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

      public async Task CreatePortfolio(SPDTO sp)
      {
         await _http.PostAsJsonAsync<SPDTO>("api/PortfolioManagement/portfolios", sp);
      }

      public async Task<List<SPDTO>> GetPortfolios()
      {
         return await _http.GetFromJsonAsync<List<SPDTO>>("api/PortfolioManagement/portfolios");
      }

      public async Task<List<SPDTO>> GetSubFundsByFundId(int FundId)
      {
         return await _http.GetFromJsonAsync<List<SPDTO>>($"api/PortfolioManagement/portfoliosGetByFundId?FundId={FundId}");
      }

      public async Task<HttpResponseMessage> UpdatePortfolio(SPDTO sp)
      {
         var data = JsonSerializer.Serialize(sp);
         StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
         return await _http.PutAsync($"api/PortfolioManagement/portfolios/update", httpContent);
      }

      public async Task<HttpResponseMessage> DeletePortfolio(SPDTO sp)
      {
         try
         {
            return await _http.DeleteAsync($"api/PortfolioManagement/portfolios/delete?Id={sp.SPId}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw e;
         }
      }

      public async Task<StrategyExchangeConfigDTO> GetStrategyConfigById(int id)
      {
         return await _http.GetFromJsonAsync<StrategyExchangeConfigDTO>(
            $"api/StrategyConfig/strategyexchangeconfig/getById?id={id}");
      }

      public async Task<List<StrategyExchangeConfigDTO>> GetStrategyConfigsForSP(string name)
      {
         return await _http.GetFromJsonAsync<List<StrategyExchangeConfigDTO>>(
            $"api/StrategyConfig/strategyexchangeconfig/get?sp={name}");
      }

      public async Task<List<StrategyExchangeConfigDTO>> GetAllStrategyConfigs()
      {
         return await _http.GetFromJsonAsync<List<StrategyExchangeConfigDTO>>(
            $"api/StrategyConfig/strategyexchangeconfig/getAll");
      }

      public async Task<HttpResponseMessage> DeleteStrategyExchangeConfig(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/StrategyConfig/strategyexchangeconfig/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw e;
         }
      }

      public async Task<LiquidationConfigurationDTO> GetLiquidationStrategyConfig(int id)
      {
         return await _http.GetFromJsonAsync<LiquidationConfigurationDTO>(
            $"api/LiquidationSubscription/liquidationsubscriptions/getByConfigId?id={id}");
      }

      public async Task<HttpResponseMessage> UpdateLiquidationStrategyConfig(LiquidationStrategyConfigDTO config)
      {
         var data = JsonSerializer.Serialize(config);
         StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
         return await _http.PutAsync($"api/StrategyConfig/liquidationStrategyConfigs/update", httpContent);
      }

      public async Task<List<LiquidationStrategyConfigDTO>> GetLiquidationStrategyConfigs()
      {
         return await _http.GetFromJsonAsync<List<LiquidationStrategyConfigDTO>>(
            "api/StrategyConfig/liquidationStrategyConfigs/get");
      }

      public async Task CreateLiquidationStrategyConfig(LiquidationStrategyConfigDTO strategyConfig)
      {
         await _http.PostAsJsonAsync<LiquidationStrategyConfigDTO>(
            "api/StrategyConfig/liquidationStrategyConfigs/create", strategyConfig);
      }

      public async Task<HttpResponseMessage> DeleteLiquidationStrategyConfig(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/StrategyConfig/liquidationStrategyConfigs/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw e;
         }
      }

      public async Task PlaceMarketOrder(OrderDetails cmd)
      {
         await _http.PostAsJsonAsync<OrderDetails>("api/Order/neworder/create", cmd);
      }


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

      public async Task<List<CoinDTO>> GetCoins()
      {
         return await _http.GetFromJsonAsync<List<CoinDTO>>("api/PortfolioManagement/coins/get");
      }

      public async Task CreateCoin(CoinDTO coin)
      {
         await _http.PostAsJsonAsync<CoinDTO>("api/PortfolioManagement/coin/create", coin);
      }

      public async Task<HttpResponseMessage> DeleteCoin(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/PortfolioManagement/coin/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw e;
         }
      }

      public async Task<HttpResponseMessage> UpdateCoin(CoinDTO coin)
      {
         var data = JsonSerializer.Serialize(coin);
         StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
         return await _http.PutAsync($"api/PortfolioManagement/coin/update", httpContent);
      }

      public async Task<List<CoinPairDTO>> GetCoinPairs()
      {
         return await _http.GetFromJsonAsync<List<CoinPairDTO>>("api/PortfolioManagement/coinpairs/get");
      }

      public async Task CreateCoinPair(CoinPairDTO coinPair)
      {
         await _http.PostAsJsonAsync<CoinPairDTO>("api/PortfolioManagement/coinpairs/create", coinPair);
      }

      public async Task<HttpResponseMessage> DeleteCoinPair(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/PortfolioManagement/coinpairs/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw e;
         }
      }

      public async Task<HttpResponseMessage> UpdateCoinPair(CoinPairDTO coinPair)
      {
         var data = JsonSerializer.Serialize(coinPair);
         StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
         return await _http.PutAsync($"api/PortfolioManagement/coinpair/update", httpContent);
      }

      public async Task CreateStrategyInstance(StrategyExchangeConfigDTO instanceData)
      {
         await _http.PostAsJsonAsync<StrategyExchangeConfigDTO>($"api/StrategyConfig/strategyexchangeconfig/create",
            instanceData);
      }

      public async Task<StrategyExchangeConfigDTO> GetStrategyConfigData(string spName)
      {
         return await _http.GetFromJsonAsync<StrategyExchangeConfigDTO>(
            $"api/StrategyConfig/strategyexchangeconfigByName/get?configName={spName}");
      }

      public async Task<StrategyExchangeConfigDTO> GetStrategyConfigDataById(int Id)
      {
         return await _http.GetFromJsonAsync<StrategyExchangeConfigDTO>(
            $"api/StrategyConfig/strategyexchangeconfigByName/getById?Id={Id}");
      }

      public async Task CreateStrategy(StrategyDTO strategy)
      {
         await _http.PostAsJsonAsync<StrategyDTO>("api/StrategyConfig/strategy/create", strategy);
      }

      public async Task<List<StrategyDTO>> GetStrategies()
      {
         return await _http.GetFromJsonAsync<List<StrategyDTO>>("api/StrategyConfig/strategy/get");
      }

      public async Task<HttpResponseMessage> UpdateStrategy(StrategyDTO coin)
      {
         var data = JsonSerializer.Serialize(coin);
         StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
         return await _http.PutAsync($"api/StrategyConfig/strategy/update", httpContent);
      }

      public async Task<HttpResponseMessage> DeleteStrategy(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/StrategyConfig/strategy/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw e;
         }
      }

     

      public async Task<HttpResponseMessage> CreateExchangeDetailsEntry(ExchangeDetailsDTO exchange)
      {
         return await _http.PostAsJsonAsync<ExchangeDetailsDTO>("api/StrategyConfig/exchangedetails/create", exchange);
      }

      public async Task<HttpResponseMessage> UpdateExchangeDetails(ExchangeDetailsDTO exchangeDetails)
      {
         var data = JsonSerializer.Serialize(exchangeDetails);
         StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
         return await _http.PutAsync($"api/StrategyConfig/exchangedetails/update", httpContent);
      }

      public async Task<HttpResponseMessage> DeleteExchangeDetails(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/StrategyConfig/exchangedetails/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw e;
         }
      }

      public async Task CreateVenue(VenueDTO venue)
      {
         await _http.PostAsJsonAsync("api/StrategyConfig/venue/create", venue);
      }

      public async Task<List<VenueDTO>> GetVenues()
      {
         return await _http.GetFromJsonAsync<List<VenueDTO>>("api/StrategyConfig/venues/get");
      }

      public async Task<HttpResponseMessage> DeleteVenue(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/StrategyConfig/venue/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw e;
         }
      }

      public async Task<HttpResponseMessage> UpdateVenue(VenueDTO venue)
      {
         var data = JsonSerializer.Serialize(venue);
         StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
         return await _http.PutAsync($"api/StrategyConfig/venue/update", httpContent);
      }


      public async Task<HttpResponseMessage> CreateApiKey(ApiKeyDTO apiKeyDto)
      {
         return await _http.PostAsJsonAsync("api/StrategyConfig/apiKey/create", apiKeyDto);
      }

      public async Task<List<ApiKeyDTO>> GetApiKeys()
      {
         return await _http.GetFromJsonAsync<List<ApiKeyDTO>>("api/StrategyConfig/apiKeys/get");
      }

      public async Task<HttpResponseMessage> DeleteApiKey(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/StrategyConfig/apiKey/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw e;
         }
      }

      public async Task<HttpResponseMessage> UpdateApiKey(ApiKeyDTO apiKey)
      {
         var data = JsonSerializer.Serialize(apiKey);
         StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
         return await _http.PutAsync($"api/StrategyConfig/apiKey/update", httpContent);
      }

      public async Task<List<ConnectorStatusMsg>> GetAllConnectorStatuses()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<ConnectorStatusMsg>>("api/Order/connectionStatusAll/get");
         }
         catch (Exception e)
         {
            throw e;
         }
      }

      public async Task StartRealTimeUpdater(RealTimeStartDTO realTimeStartDto)
      {
         try
         {
            var response = await _http.PostAsJsonAsync("api/StrategyConfig/strategyInstance/create", realTimeStartDto);
         }
         catch (Exception e)
         {
            throw e;
         }
      }

      public async Task<bool> IsRealTimeUpdaterStarted(string instance)
      {
         try
         {
            return await _http.GetFromJsonAsync<bool>(
               $"api/StrategyConfig/strategyInstance/get?instance={instance}");
         }
         catch (Exception e)
         {
            throw e;
            throw;
         }
      }

      public async Task<List<TradeDTO>> GetLatestTrades(string InstanceName)
      {
         try
         {
            return await _http.GetFromJsonAsync<List<TradeDTO>>(
               $"api/Order/Trade/GetLatest?InstanceName={InstanceName}");
         }
         catch (Exception e)
         {
            
            throw;
         }
      }

      public async Task<List<TradeDTO>> GetAllTrades(int SpId)
      {
         try
         {
            return await _http.GetFromJsonAsync<List<TradeDTO>>(
               $"api/Order/Trade/GetAll?SpId={SpId}");
         }
         catch (Exception e)
         {

            throw;
         }
      }

      public async Task<List<FillsInfoForInstance>> GetFills(string InstanceName)
      {
         try
         {
            return await _http.GetFromJsonAsync<List<FillsInfoForInstance>>(
               $"api/Order/Trade/GetFills?InstanceName={InstanceName}");
         }
         catch (Exception e)
         {

            throw;
         }
      }

      public async Task<List<LiquidationConfigurationDTO>> GetOpeningLiquidationSubscriptions()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<LiquidationConfigurationDTO>>(
               $"api/LiquidationSubscription/liquidationsubscriptions/get");
         }
         catch (Exception e)
         {

            throw;
         }
      }

      public async Task<List<LiquidationConfigurationDTO>> GetOpeningLiquidationSubscriptionsForSp(
         string spName)
      {
         try
         {
            return await _http.GetFromJsonAsync<List<LiquidationConfigurationDTO>>(
               $"api/LiquidationSubscription/liquidationsubscriptions/getInstancesForSp?spName={spName}");
         }
         catch (Exception e)
         {

            throw;
         }

      }

      public async Task<List<LiquidationConfigurationDTO>> GetOpeningLiquidationSubscription(int Id)
      {
         try
         {
            return await _http.GetFromJsonAsync<List<LiquidationConfigurationDTO>>(
               $"api/LiquidationSubscription/liquidationsubscriptions/getById?Id={Id}");
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task CreateOpeningLiquidationSubscription(LiquidationConfigurationDTO subscription)
      {
         await _http.PostAsJsonAsync<LiquidationConfigurationDTO>(
            "api/LiquidationSubscription/liquidationsubscriptions/create", subscription);
      }

      public async Task<HttpResponseMessage> UpdateOpeningLiquidationSubscription(
         LiquidationConfigurationDTO update)
      {
         try
         {
            var data = JsonSerializer.Serialize(update);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/LiquidationSubscription/liquidationsubscriptions/update", httpContent);
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteOpeningLiquidationSubscription(int id)
      {
         try
         {
            return await _http.DeleteAsync($"api/LiquidationSubscription/liquidationsubscriptions/delete?Id={id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<List<ExchangeCoinpairMappingDTO>> GetExchangeCoinPairs()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<ExchangeCoinpairMappingDTO>>(
               $"api/PortfolioManagement/exchangecoinpairs/get");
         }
         catch (Exception e)
         {

            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateExchangeCoinPair(ExchangeCoinpairMappingDTO coin)
      {
         var data = JsonSerializer.Serialize(coin);
         StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
         return await _http.PutAsync($"api/PortfolioManagement/exchangecoinpairs/update", httpContent);
      }

      
      public async Task CreateExchangeCoinPair(ExchangeCoinpairMappingDTO coinPair)
      {
         await _http.PostAsJsonAsync<ExchangeCoinpairMappingDTO>("api/PortfolioManagement/exchangecoinpairs/create", coinPair);
      }

      public async Task<HttpResponseMessage> DeleteExchangeCoinPair(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/PortfolioManagement/exchangecoinpairs/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<List<ExchangeCoinMappingsDTO>> GetExchangeCoins()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<ExchangeCoinMappingsDTO>>(
               $"api/PortfolioManagement/exchangecoins/get");
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateExchangeCoin(ExchangeCoinMappingsDTO coin)
      {
         var data = JsonSerializer.Serialize(coin);
         StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
         return await _http.PutAsync($"api/PortfolioManagement/exchangecoins/update", httpContent);
      }

      public async Task CreateExchangeCoinPair(ExchangeCoinMappingsDTO coin)
      {
         await _http.PostAsJsonAsync<ExchangeCoinMappingsDTO>("api/PortfolioManagement/exchangecoins/create", coin);
      }

      public async Task<HttpResponseMessage> DeleteExchangeCoin(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/PortfolioManagement/exchangecoins/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task GetOrderBooks(string venue, string coinPairs, string account, string instance)
      {
         try
         {
            var orderBookData = new GetOrderBooksData()
            {
               Account = account,
               Venue = venue,
               CoinPairs = coinPairs,
               Instance = instance
            };
            await _http.PostAsJsonAsync<GetOrderBooksData>("api/Order/OrderBooks/GetForVenue", orderBookData);
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task GetLastTrades(string venue, string coinPairs, string account, string instance)
      {
         try
         {
            var orderBookData = new GetOrderBooksData()
            {
               Account = account,
               Venue = venue,
               CoinPairs = coinPairs,
               Instance = instance
            };
            await _http.PostAsJsonAsync<GetOrderBooksData>("api/Order/LastTrade/GetForVenue", orderBookData);
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<List<FairValueConfigForUiDTO>> GetFairValueConfigs()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<FairValueConfigForUiDTO>>(
               $"api/FairValueConfigUI/get");
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateFairValueConfig(FairValueConfigForUiDTO config)
      {
         try
         {
            var data = JsonSerializer.Serialize(config);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/FairValueConfigUI/update", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine($"UpdateFairValueConfig error {e.Message}");
            throw;
         }
      }

      public async Task CreateFairValueConfig(FairValueConfigForUiDTO config)
      {
         await _http.PostAsJsonAsync<FairValueConfigForUiDTO>("api/FairValueConfigUI/create", config);
      }

      public async Task<HttpResponseMessage> DeleteFairValueConfig(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/FairValueConfigUI/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<List<MakerTakerFeeDTO>> GetMakerTakerFees()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<MakerTakerFeeDTO>>(
               $"api/MakerTakerFee/get");
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateMakerTakerFees(MakerTakerFeeDTO config)
      {
         try
         {
            var data = JsonSerializer.Serialize(config);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/MakerTakerFee/update", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine($"UpdateMakerTakerFees error {e.Message}");
            throw;
         }
      }

      public async Task CreateMakerTakerFee(MakerTakerFeeDTO config)
      {
         await _http.PostAsJsonAsync<MakerTakerFeeDTO>("api/MakerTakerFee/create", config);
      }

      public async Task<HttpResponseMessage> DeleteMakerTakerFee(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/MakerTakerFee/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<List<OrderDTO>> GetOrdersForInstance(string InstanceName)
      {
         try
         {
            return await _http.GetFromJsonAsync<List<OrderDTO>>(
               $"api/Order/Order/GetOrdersForInstance?InstanceName={InstanceName}");
         }
         catch (Exception e)
         {

            throw;
         }
      }
      public async Task<List<OrderDTO>> GetAllOrdersForSp(int SpId)
      {
         try
         {
            return await _http.GetFromJsonAsync<List<OrderDTO>>(
               $"api/Order/Order/GetAll?SpId={SpId}");
         }
         catch (Exception e)
         {

            throw;
         }
      }

      public async Task GetPublicStatus(VenueDTO venue)
      {
         await _http.PostAsJsonAsync<VenueDTO>("api/Connector/getStatus/public", venue);
      }

      public async Task<HttpResponseMessage> CreateTelegramAlert(TelegramAlertDTO input)
      {
         try
         {
            return await _http.PostAsJsonAsync<TelegramAlertDTO>("api/Telegram/create_alert", input);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<TelegramAlertDTO>> GetTelegramAlerts()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<TelegramAlertDTO>>("api/Telegram/get_alerts");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateTelegramAlert(TelegramAlertDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/Telegram/update_alert", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteTelegramAlert(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/Telegram/delete_alert?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> CreateTelegramAlertCategory(TelegramAlertCategoryDTO input)
      {
         try
         {
            return await _http.PostAsJsonAsync<TelegramAlertCategoryDTO>("api/Telegram/create_alert_category", input);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<TelegramAlertCategoryDTO>> GetTelegramAlertCategories()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<TelegramAlertCategoryDTO>>("api/Telegram/get_alert_categories");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateTelegramAlertCategory(TelegramAlertCategoryDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/Telegram/update_alert_category", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteTelegramAlertCategory(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/Telegram/delete_alert_category?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> CreateTelegramAlertToChannel(TelegramAlertToChannelDTO input)
      {
         try
         {
            return await _http.PostAsJsonAsync<TelegramAlertToChannelDTO>("api/Telegram/create_alert_to_channel", input);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<TelegramAlertToChannelDTO>> GetTelegramAlertsToChannels()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<TelegramAlertToChannelDTO>>("api/Telegram/get_alerts_to_channels");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateTelegramAlertToChannel(TelegramAlertToChannelDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/Telegram/update_alert_to_channel", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteTelegramAlertToChannel(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/Telegram/delete_alert_to_channel?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> CreateTelegramChannel(TelegramChannelDTO input)
      {
         try
         {
            return await _http.PostAsJsonAsync<TelegramChannelDTO>("api/Telegram/create_channel", input);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<TelegramChannelDTO>> GetTelegramChannels()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<TelegramChannelDTO>>("api/Telegram/get_channels");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<TelegramChannelDTO>> GetCommandTelegramChannels()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<TelegramChannelDTO>>("api/Telegram/get_command_channels");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateTelegramChannel(TelegramChannelDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/Telegram/update_channel", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteTelegramChannel(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/Telegram/delete_channel?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> CreateTelegramCommandToUser(TelegramCommandToUserDTO input)
      {
         try
         {
            return await _http.PostAsJsonAsync<TelegramCommandToUserDTO>("api/Telegram/create_command_to_user", input);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<TelegramCommandToUserDTO>> GetTelegramCommandToUsers()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<TelegramCommandToUserDTO>>("api/Telegram/get_commands_to_users");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateTelegramCommandToUser(TelegramCommandToUserDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/Telegram/update_command_to_user", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteTelegramCommandToUser(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/Telegram/delete_command_to_user?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> CreateTelegramCommandType(TelegramCommandTypeDTO input)
      {
         try
         {
            return await _http.PostAsJsonAsync<TelegramCommandTypeDTO>("api/Telegram/create_command_type", input);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<TelegramCommandTypeDTO>> GetTelegramCommandTypes()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<TelegramCommandTypeDTO>>("api/Telegram/get_commands_types");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateTelegramCommandType(TelegramCommandTypeDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/Telegram/update_command_type", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteTelegramCommandType(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/Telegram/delete_command_type?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> CreateTelegramCommand(TelegramCommandDTO input)
      {
         try
         {
            return await _http.PostAsJsonAsync<TelegramCommandDTO>("api/Telegram/create_command", input);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<TelegramCommandDTO>> GetTelegramCommands()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<TelegramCommandDTO>>("api/Telegram/get_commands");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateTelegramCommand(TelegramCommandDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/Telegram/update_command", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteTelegramCommand(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/Telegram/delete_command?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> CreateTelegramSubscriberToChannel(TelegramSubscriberToChannelDTO input)
      {
         try
         {
            return await _http.PostAsJsonAsync<TelegramSubscriberToChannelDTO>("api/Telegram/create_subscriber_to_channel", input);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<TelegramSubscriberToChannelDTO>> GetSubscribersToChannels()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<TelegramSubscriberToChannelDTO>>("api/Telegram/get_subscribers_to_channels");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateSubscriberToChannel(TelegramSubscriberToChannelDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/Telegram/update_subscriber_to_channel", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteSubscriberToChannel(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/Telegram/delete_subscriber_to_channel?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> CreateTelegramUser(TelegramUserDTO input)
      {
         try
         {
            return await _http.PostAsJsonAsync<TelegramUserDTO>("api/Telegram/create_user", input);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<TelegramUserDTO>> GetTelegramUsers()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<TelegramUserDTO>>("api/Telegram/get_users");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateTelegramUser(TelegramUserDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/Telegram/update_user", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteTelegramUser(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/Telegram/delete_user?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> CreateLiquidationOrderLoadingConfiguration(
         LiquidationOrderLoadingConfigurationDTO dto)
      {
         try
         {
            return await _http.PostAsJsonAsync<LiquidationOrderLoadingConfigurationDTO>("api/LiquidationSubscription/liquidationorderloading/create", dto);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<LiquidationOrderLoadingConfigurationDTO>> GetLiquidationOrderLoadingConfiguration()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<LiquidationOrderLoadingConfigurationDTO>>("api/LiquidationSubscription/liquidationorderloading/get");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateLiquidationOrderLoadingConfiguration(LiquidationOrderLoadingConfigurationDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/LiquidationSubscription/liquidationorderloading/update", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteLiquidationOrderLoadingConfiguration(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/LiquidationSubscription/liquidationorderloading/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> CreateLiquidationManualOrderLoading(
         LiquidationManualOrderLoadingDTO dto)
      {
         try
         {
            return await _http.PostAsJsonAsync<LiquidationManualOrderLoadingDTO>("api/LiquidationSubscription/liquidationmanualorderloading/create", dto);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<LiquidationManualOrderLoadingDTO>> GetLiquidationManualOrderLoading()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<LiquidationManualOrderLoadingDTO>>("api/LiquidationSubscription/liquidationmanualorderloading/get");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateLiquidationManualOrderLoadingConfiguration(LiquidationManualOrderLoadingDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/LiquidationSubscription/liquidationmanualorderloading/update", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteLiquidationManualOrderLoadingConfiguration(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/LiquidationSubscription/liquidationmanualorderloading/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> CreateExchangeOpeningBalance(OpeningExchangeBalanceDTO dto)
      {
        return await _http.PostAsJsonAsync("api/StrategyConfig/exchangeBalances/Create", dto);
      }

      public async Task<List<OpeningExchangeBalanceDTO>> GetOpeningExchangeBalances()
      {
         return await _http.GetFromJsonAsync<List<OpeningExchangeBalanceDTO>>("api/StrategyConfig/exchangeBalances/get");
      }

      public async Task<List<OpeningExchangeBalanceDTO>> GetOpeningExchangeBalancesForInstance(int InstanceId)
      {
         return await _http.GetFromJsonAsync<List<OpeningExchangeBalanceDTO>>($"api/StrategyConfig/exchangeBalances/getById?{InstanceId}");
      }


      public async Task<HttpResponseMessage> DeleteOpeningExchangeBalance(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/StrategyConfig/exchangeBalances/delete?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw e;
         }
      }

      public async Task<HttpResponseMessage> UpdateOpeningExchangeBalance(OpeningExchangeBalanceDTO dto)
      {
         var data = JsonSerializer.Serialize(dto);
         StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
         return await _http.PutAsync($"api/StrategyConfig/exchangeBalances/update", httpContent);
      }

      public async Task<HttpResponseMessage> CreateFund(FundDTO input)
      {
         try
         {
            return await _http.PostAsJsonAsync<FundDTO>("api/Fund/create_fund", input);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<FundDTO>> GetFunds()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<FundDTO>>("api/Fund/get_funds");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<FundDTO> GetFundById(int id)
      {
         try
         {
            return await _http.GetFromJsonAsync<FundDTO>($"api/Fund/get_fundById?id={id}");
         }
         catch (Exception e)
         {
            Console.WriteLine($"FFs what is going on here - {e.StackTrace} ");
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateFund(FundDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/Fund/update_fund", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteFund(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/Fund/delete_fund?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> CreateLocation(LocationDTO input)
      {
         try
         {
            return await _http.PostAsJsonAsync<LocationDTO>("api/Fund/create_location", input);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<LocationDTO>> GetLocations()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<LocationDTO>>("api/Fund/get_locations");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateLocation(LocationDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/Fund/update_location", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteLocation(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/Fund/delete_location?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> CreateTelegramAlertBehaviourType(TelegramAlertBehaviourTypeDTO input)
      {
         try
         {
            return await _http.PostAsJsonAsync<TelegramAlertBehaviourTypeDTO>("api/Telegram/create_alert_behaviour_type", input);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<TelegramAlertBehaviourTypeDTO>> GetTelegramAlertBehaviourTypes()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<TelegramAlertBehaviourTypeDTO>>("api/Telegram/get_alert_behaviour_types");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateTelegramAlertBehaviourType(TelegramAlertBehaviourTypeDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/Telegram/update_alert_behaviour_type", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteTelegramAlertBehaviourType(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/Telegram/delete_alert_behaviour_type?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }

      public async Task<HttpResponseMessage> CreateTelegramAlertBehaviour(TelegramAlertBehaviourDTO input)
      {
         try
         {
            return await _http.PostAsJsonAsync<TelegramAlertBehaviourDTO>("api/Telegram/create_alert_behaviour", input);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<List<TelegramAlertBehaviourDTO>> GetTelegramAlertBehaviours()
      {
         try
         {
            return await _http.GetFromJsonAsync<List<TelegramAlertBehaviourDTO>>("api/Telegram/get_alert_behaviours");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> UpdateTelegramAlertBehaviour(TelegramAlertBehaviourDTO input)
      {
         try
         {
            var data = JsonSerializer.Serialize(input);
            StringContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            return await _http.PutAsync($"api/Telegram/update_alert_behaviour", httpContent);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      public async Task<HttpResponseMessage> DeleteTelegramAlertBehaviour(int Id)
      {
         try
         {
            return await _http.DeleteAsync($"api/Telegram/delete_alert_behaviour?Id={Id}",
               new CancellationToken());
         }
         catch (Exception e)
         {
            throw;
         }
      }
   }
}
