using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Common.Models.DTOs;
using DataStore;
using Common.Models.Entities;
using BlazorLiquidity.Server.RealTime;
using Microsoft.AspNetCore.SignalR;
using BlazorLiquidity.Server.Hubs;
using BlazorLiquidity.Shared;
using Common.Messages;
using MessageBroker;
using System.Text.Json;
using BlazorLiquidity.Server.StrategyInstanceManager;
using Common;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace PortfolioManagementAPI.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   [Authorize()]
   public class StrategyConfigController : ControllerBase
   {
      private readonly ILogger<ConfigController> _logger;
      private readonly IPortfolioRepository _repository;
      private readonly IMapper _mapper;
      private readonly IStrategyInstanceManager _strategyInstanceManager;
      protected readonly IHubContext<PortfolioHub, IPortfolioHub> _hub;
      private readonly IMessageBroker _messageBroker;
      private Dictionary<string, bool> _strategyInstanceTable = new();

      public StrategyConfigController(ILoggerFactory loggerFactory, 
                                      IPortfolioRepository repository, 
                                      IMapper mapper, 
                                      IStrategyInstanceManager strategyInstanceManager, 
                                      IHubContext<PortfolioHub, IPortfolioHub> hub,
                                      IMessageBroker broker)
      {
         _logger = loggerFactory.CreateLogger<ConfigController>();
         _repository = repository;
         _mapper = mapper;
         _strategyInstanceManager = strategyInstanceManager;
         _hub = hub;
         _messageBroker = broker;
      }

      [HttpPost]
      [Route("strategyexchangeconfig/create")]
      
      public async Task<ActionResult<StrategyExchangeConfigDTO>> ConfigureExchangesAndSymbols(StrategyExchangeConfigDTO configData)
      {
         try
         {
            if (configData == null)
               return BadRequest();

            var createdSettings = await _repository.CreateStrategyExchangeConfigEntry(configData);

            return CreatedAtAction("strategyexchangeconfig/get",
                new { id = createdSettings.StrategySPSubscriptionConfigId }, createdSettings);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating Strategy ");
         }
      }

      [HttpPost]
      [Route("exchangedetails/create")]
      public async Task<ActionResult<ExchangeDetailsDTO>> CreateExchangeDetailsEntry(ExchangeDetailsDTO entry)
      {
         try
         {
            if (entry == null)
               return BadRequest();
            var created = await _repository.CreateExchangeDetailsEntry(entry);
            return CreatedAtAction("exchangedetails/get",
               new { id = created.ExchangeDetailsId }, created);
         }
         catch (Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Exchange Details Entry ");
         }
      }

      [HttpGet]
      [Route("exchangedetails/get")]
      [ActionName("exchangedetails/get")]
      public async Task<ActionResult<List<ExchangeDetailsDTO>>> GetExchangeDetailsForConfigId(int StrategyConfigId)
      {
         try
         {
            if (StrategyConfigId != 0)
            {
               var res = await _repository.GetExchangeDetailsForStrategyConfigId(StrategyConfigId);
               return Ok(res);
            }
            return BadRequest();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting strategy config for {Account} with Error {Error}", StrategyConfigId, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error getting Strategy configs for {StrategyConfigId}");
         }
      }

      [HttpPut]
      [Route("exchangedetails/update")]
      public async Task<ActionResult> UpdateExchangeDetails(ExchangeDetailsDTO exchangeData)
      {
         try
         {
            if (exchangeData == null)
               return BadRequest();

            await _repository.UpdateExchangeDetails(exchangeData);

            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Strategy ");
         }
      }

      [HttpDelete]
      [Route("exchangedetails/delete")]
      public async Task<ActionResult> DeleteExchangeDetails(int Id)
      {
         try
         {
            await _repository.DeleteExchangeDetails(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error deleting ExchangeDetails");
         }
      }

      [HttpGet]
      [Route("strategyexchangeconfig/get")]
      [ActionName("strategyexchangeconfig/get")]
      public async Task<ActionResult<List<StrategyExchangeConfigDTO>>> GetAllStrategyConfigsForSP(string sp)
      {
         try
         {
            if (!string.IsNullOrEmpty(sp))
            {
               var res = await _repository.GetStrategyExchangeConfigsForSP(sp);
               return Ok(res);
            }
            return BadRequest();
         }
         catch(Exception e)
         {
            _logger.LogError(e, "Error getting strategy config for {Account} with Error {Error}", sp, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error getting Strategy configs for {sp}");
         }
      }

      [HttpGet]
      [Route("strategyexchangeconfig/getAll")]
      [ActionName("strategyexchangeconfig/getAll")]
      public async Task<ActionResult<List<StrategyExchangeConfigDTO>>> GetStrategyConfigs()
      {
         try
         {
           
               var res = await _repository.GetAllStrategyExchangeConfigs();
               return Ok(res);
            
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting all strategy configs Error {Error}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error getting Strategy configs ");
         }
      }

      [HttpGet]
      [Route("strategyexchangeconfigByName/get")]
      [ActionName("strategyexchangeconfigByName/get")]
      public async Task<ActionResult<StrategyExchangeConfigDTO>> GetStrategyConfigForSp(string configName)
      {
         try
         {
            if (!string.IsNullOrEmpty(configName))
            {
               var res = await _repository.GetStrategyExchangeConfigEntry(configName);
               return Ok(res);
            }
            return BadRequest();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting strategy config for {Account} with Error {Error}", configName, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error getting Strategy configs for {configName}");
         }
      }

      [HttpGet]
      [Route("strategyexchangeconfig/getById")]
      [ActionName("strategyexchangeconfig/getById")]
      public async Task<ActionResult<StrategyExchangeConfigDTO>> GetStrategyConfigForSp(int id)
      {
         try
         {
            var res = await _repository.GetStrategyExchangeConfigEntryById(id);
               return Ok(res);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting strategy config for  {Id} with Error {Error}", id, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error getting Strategy configs for {id}");
         }
      }

      [HttpPut]
      [Route("strategyexchangeconfig/update")]
      public async Task<ActionResult> UpdateExchangesAndSymbols(StrategyExchangeConfigDTO configData)
      {
         try
         {
            if (configData == null)
               return BadRequest();

            await _repository.UpdateStrategyExchangeConfigEntry(configData);

            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating Strategy ");
         }
      }

      [HttpDelete]
      [Route("strategyexchangeconfig/delete")]
      public async Task<ActionResult> DeleteStrategyConfig(int Id)
      {
         try
         {
            await _repository.DeleteStrategyExchangeConfig(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error deleting DeleteStrategyConfig");
         }
      }

      [HttpPost]
      [Route("strategy/create")]
      public async Task<ActionResult<StrategyDTO>> CreateStrategy(StrategyDTO strategyDTO)
      {
         try
         {
            if (!string.IsNullOrEmpty(strategyDTO.StrategyName))
            {
               var strategy = await _repository.AddStrategy(strategyDTO);
             
               return CreatedAtAction("getStrategies",
                  new { id = strategy.StrategyId }, strategy);
            }
            return BadRequest();
         }
         catch (Exception e)
         {
            _logger.LogError("Error creating new Strategy {Strategy}", strategyDTO.StrategyName);
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error creating new venue {strategyDTO.StrategyName}");
         }
      }

      [HttpGet]
      [Route("strategy/get")]
      [ActionName("getStrategies")]
      public async Task<ActionResult<IEnumerable<StrategyDTO>>> GetStrategies()
      {
         try
         {
            var dtoList = new List<StrategyDTO>();
            var strategies = await _repository.GetStrategies();
            if (strategies != null)
               strategies.ForEach(s => dtoList.Add(_mapper.Map<StrategyDTO>(s)));
            return Ok(dtoList);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting strategies from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating getting venue"); ;
         }
      }

      [HttpDelete]
      [Route("strategy/delete")]
      public async Task<ActionResult> DeleteStrategy(int Id)
      {
         try
         {
            await _repository.DeleteStrategy(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error deleting ExchangeDetails");
         }
      }

      [HttpPut]
      [Route("strategy/update")]
      public async Task<ActionResult> UpdateCoin(StrategyDTO strategy)
      {
         try
         {
            if (strategy == null)
               return BadRequest();

            await _repository.UpdateStrategy(strategy);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating CoinPair ");
         }
      }

      [HttpPost]
      [Route("liquidationStrategyConfigs/create")]
      public async Task<ActionResult<LiquidationStrategyConfigDTO>> CreateLiquidationStrategyConfig(LiquidationStrategyConfigDTO liquidationStrategyConfig)
      {
         try
         {
            if (liquidationStrategyConfig == null)
               return BadRequest();

            var createdSettings = await _repository.CreateLiquidationStrategyConfig(liquidationStrategyConfig);

            return CreatedAtAction("liquidationStrategyConfigs/get",
                new { id = createdSettings.Id }, createdSettings);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error creating LiquidityStrategyConfig {Error}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating Strategy ");
         }
      }

      [HttpPost("strategyInstance/create")]
      public async Task<ActionResult<RealTimeStartDTO>> CreateStrategyInstanceForUI(RealTimeStartDTO realTimeStartDto)
      {
         try
         {
            if (realTimeStartDto == null)
               return BadRequest();
            if (_strategyInstanceTable.ContainsKey(realTimeStartDto.Instance))
            {
               return Ok(realTimeStartDto);
            }
            var strategyInstant =  _strategyInstanceManager.CreateStrategyInstance(realTimeStartDto.SpName,  
                                                                  realTimeStartDto.Instance, 
                                                                  realTimeStartDto.ConfigId);
            _strategyInstanceTable.Add(realTimeStartDto.Instance, true);
            await strategyInstant.Item1.Init(realTimeStartDto.SpName, realTimeStartDto.Instance, realTimeStartDto.ConfigId, strategyInstant.Item2);
            strategyInstant.Item1.Run();
            return Ok(realTimeStartDto);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error starting Real Time Updater for  {Instance} {Error}", realTimeStartDto.Instance, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error starting Real Time Updater for {realTimeStartDto.Instance}");
         }
      }

      [HttpGet]
      [Route("strategyInstance/get")]
      [ActionName("strategyInstance/get")]
      public ActionResult<bool> GetStrategyInstance(string instance)
      {
         if (_strategyInstanceTable.ContainsKey(instance))
         {
            return Ok(true);
         }
         return Ok(false);
      }

      [HttpGet]
      [Route("liquidationStrategyConfigs/get")]
      [ActionName("liquidationStrategyConfigs/get")]
      public async Task<ActionResult<List<LiquidationStrategyConfigDTO>>> GetLiquidationStrategyConfigs()
      {
         try
         {
            var configs = await _repository.GetLiquidationStrategyConfigs();
            return Ok(configs);
         }
         catch(Exception e)
         {
            _logger.LogError(e, "Error getting LiquidationStrategies from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating getting LiquidationStrategies"); ;
         }
      }

      [HttpGet]
      [Route("liquidationStrategyConfigs/getById")]
      public async Task<ActionResult<LiquidationConfigurationDTO>> GetLiquidationStrategyConfig(int strategyConfigId)
      {
         try
         {
            var configs = await _repository.GetOpeningLiquidationSubscriptionForStrategySPSubscriptionId(strategyConfigId);
          
            // Now trigger an initialisation of the real time part
       
            //await _strategyInstanceManager.StartRealTimeUpdaterForInstance(strategyConfigId);

            return Ok(configs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting LiquidationStrategy from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating getting LiquidationStrategies"); ;
         }
      }

      [HttpPut]
      [Route("liquidationStrategyConfigs/update")]
      public async Task<ActionResult> UpdateLiquidationStrategyConfig(LiquidationStrategyConfigDTO liquidationStrategyConfig)
      {
         try
         {
            if (liquidationStrategyConfig == null)
               return BadRequest();

            await _repository.UpdateLiquidationStrategyConfig(liquidationStrategyConfig);
            BroadcastConfigChange(liquidationStrategyConfig);

            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating Strategy ");
         }
      }

      [HttpDelete]
      [Route("liquidationStrategyConfigs/delete")]
      public async Task<ActionResult> DeleteLiquidationStrategyConfig(int Id)
      {
         try
         {
            await _repository.DeleteLiquidationStrategyConfig(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error deleting ExchangeDetails");
         }
      }

      [HttpPost]
      [Route("venue/create")]
      public async Task<ActionResult<LiquidationStrategyConfigDTO>> CreateVenue(VenueDTO venue)
      {
         try
         {
            if (venue == null)
               return BadRequest();

            var createdSettings = await _repository.AddVenue(venue);

            return CreatedAtAction("venues/get",
               new { id = createdSettings.VenueId}, createdSettings);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error creating LiquidityStrategyConfig {Error}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Strategy ");
         }
      }

      [HttpGet]
      [Route("venues/get")]
      [ActionName("venues/get")]
      public async Task<ActionResult<IEnumerable<VenueDTO>>> GetVenues()
      {
         try
         {
            var venues = await _repository.GetVenues();
            return Ok(venues);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting strategies from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating getting venue"); ;
         }
      }

      [HttpDelete]
      [Route("venue/delete")]
      public async Task<ActionResult> DeleteVenue(int Id)
      {
         try
         {
            await _repository.DeleteVenue(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error deleting ExchangeDetails");
         }
      }

      [HttpPut]
      [Route("venue/update")]
      public async Task<ActionResult> UpdateVenue(VenueDTO venue)
      {
         try
         {
            if (venue == null)
               return BadRequest();

            await _repository.UpdateVenue(venue);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating CoinPair ");
         }
      }

      [HttpPost]
      [Route("neworder/create")]
      public async Task<ActionResult<LiquidationStrategyConfigDTO>> CreateMarketBuyOrder(LiquidationStrategyConfigDTO liquidationStrategyConfig)
      {
         try
         {
            if (liquidationStrategyConfig == null)
               return BadRequest();

            var createdSettings = await _repository.CreateLiquidationStrategyConfig(liquidationStrategyConfig);

            return CreatedAtAction("liquidationStrategyConfigs/get",
                new { id = createdSettings.Id }, createdSettings);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error creating LiquidityStrategyConfig {Error}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating Strategy ");
         }
      }

      private void BroadcastConfigChange(LiquidationStrategyConfigDTO liquidationStrategyConfig)
      {
        try
        {
            var strategyConfigData = new StrategyConfigChangeData()
            {
               StrategyConfigChangeType = StrategyConfigChangeType.LIQUIDATION,
               InstanceName = liquidationStrategyConfig.ConfigName
            };

            var configChangeUpdate = new ConfigChangeUpdate()
            {
               ConfigChangeType = ConfigChangeType.STRATEGY,
               Data = JsonSerializer.Serialize(strategyConfigData)

            };
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.CONFIG_UPDATE_STATUS,
               Data = JsonSerializer.Serialize(configChangeUpdate)
               
            };
       
            PublishHelper.PublishToTopic(Constants.CONFIG_UPDATE_TOPIC, response, _messageBroker);
        }
        catch (Exception e)
        { 
           _logger.LogError("Error cancelling Order {Error}", e.Message); 
           throw;
        }
      }

      [HttpPost]
      [Route("apiKey/create")]
      public async Task<ActionResult<ApiKeyDTO>> CreateApiKey(ApiKeyDTO apiKeyDto)
      {
         try
         {
            if (apiKeyDto == null) return BadRequest();

            var apiKey = await _repository.CreateApiKey(apiKeyDto);

               return CreatedAtAction("getApiKeys",
                  new { id = apiKey.ApiKeyId }, apiKey);
         }
         catch (Exception e)
         {
            _logger.LogError("Error creating new ApiKey for  {Description}", apiKeyDto.Description);
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error creating new apiKey {apiKeyDto.Description}");
         }
      }

      [HttpGet]
      [Route("apiKeys/get")]
      [ActionName("getApiKeys")]
      public async Task<ActionResult<IEnumerable<ApiKeyDTO>>> GetApiKeys()
      {
         try
         {
            var dtoList = new List<ApiKeyDTO>();
            var apiKeys = await _repository.GetApiKeys();
            return Ok(apiKeys);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting apiKeys from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating getting apiKeys"); ;
         }
      }

      [HttpDelete]
      [Route("apiKey/delete")]
      public async Task<ActionResult> DeleteApiKey(int Id)
      {
         try
         {
            await _repository.DeleteApiKey(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error deleting ApiKey");
         }
      }

      [HttpPut]
      [Route("apiKey/update")]
      public async Task<ActionResult> UpdateApiKey(ApiKeyDTO apiKeyDto)
      {
         try
         {
            if (apiKeyDto == null)
               return BadRequest();

            await _repository.UpdateApiKey(apiKeyDto);
            return Ok();
         }
         catch (Exception)
         {
            _logger.LogError("Error updating ApiKey Id={Id}", apiKeyDto.ApiKeyId);
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error updating ApiKey {apiKeyDto.ApiKeyId}");
         }
      }

      [HttpPost]
      [Route("exchangeBalances/Create")]
      public async Task<ActionResult<OpeningExchangeBalanceDTO>> CreateExchangeBalance(OpeningExchangeBalanceDTO exchangeBalanceDTO)
      {
         try
         {
            if (exchangeBalanceDTO == null) return BadRequest();

            var balance = await _repository.CreateExchangeBalance(exchangeBalanceDTO);
            var created = CreatedAtAction("getExchangeBalances",
               new { id = balance.OpeningExchangeBalanceId }, balance);
            return created;
         }
         catch (Exception e)
         {
            _logger.LogError("Error creating new ExchangeBalance for  {Description} ", 
                                        exchangeBalanceDTO.Description);
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error creating new exchange balance {exchangeBalanceDTO.Description}");
         }
      }

      [HttpGet]
      [Route("exchangeBalances/get")]
      [ActionName("getExchangeBalances")]
      public async Task<ActionResult<IEnumerable<OpeningExchangeBalanceDTO>>> GetExchangeBalances()
      {
         try
         {
            var dtoList = new List<OpeningExchangeBalanceDTO>();
            var balances = await _repository.GetExchangeBalances();
            return Ok(balances);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting Exchange Balances from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating getting Exchange Balances"); ;
         }
      }

      [HttpGet]
      [Route("exchangeBalances/getById")]
      [ActionName("getExchangeBalancesById")]
      public async Task<ActionResult<IEnumerable<OpeningExchangeBalanceDTO>>> GetExchangeBalancesById(int id)
      {
         try
         {
            var dtoList = new List<OpeningExchangeBalanceDTO>();
            var balances =  _repository.GetExchangeBalancesForInstance(id);
            return Ok(balances);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting Exchange Balances for instance {Instance} from db {Error} - {Stacktrace}", 
                                   id, e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error creating getting Exchange Balances for exchange details id = {id}"); ;
         }
      }

      [HttpPut]
      [Route("exchangeBalances/update")]
      public async Task<ActionResult> UpdateExchangeBalance(OpeningExchangeBalanceDTO dto)
      {
         try
         {
            if (dto == null)
               return BadRequest();

            await _repository.UpdateExchangeBalance(dto);
            return Ok();
         }
         catch (Exception)
         {
            _logger.LogError("Error updating Exchange Balance {Id}", dto.OpeningExchangeBalanceId);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating Exchange Balance ");
         }
      }

      [HttpDelete]
      [Route("exchangeBalances/delete")]
      public async Task<ActionResult> DeleteExchangeBalance(int Id)
      {
         try
         {
            await _repository.DeleteExchangeBalance(Id);
            return Ok();
         }
         catch (Exception)
         {
            _logger.LogError("Error deleting Exchange Balance {Id}", Id);
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting Exchange Balance {Id}");
         }
      }

   }
}
