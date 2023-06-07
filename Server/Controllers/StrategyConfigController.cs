using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Common.Models.DTOs;
using DataStore;
using Common.Models.Entities;
using SyncfusionLiquidity.Server.RealTime;
using Microsoft.AspNetCore.SignalR;
using SyncfusionLiquidity.Server.Hubs;
using SyncfusionLiquidity.Shared;
using Common.Messages;
using MessageBroker;
using System.Text.Json;
using Common;
using Common.Extensions;

namespace PortfolioManagementAPI.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class StrategyConfigController : ControllerBase
   {
      private readonly ILogger<ConfigController> _logger;
      private readonly IPortfolioRepository _repository;
      private readonly IMapper _mapper;
      private readonly IRealTimeUpdater _realTimeUpdater;
      protected readonly IHubContext<PortfolioHub, IPortfolioHub> _hub;
      private readonly IMessageBroker _messageBroker;

      public StrategyConfigController(ILoggerFactory loggerFactory, 
                                      IPortfolioRepository repository, 
                                      IMapper mapper, 
                                      IRealTimeUpdater realTimeUpdater, 
                                      IHubContext<PortfolioHub, IPortfolioHub> hub,
                                      IMessageBroker broker)
      {
         _logger = loggerFactory.CreateLogger<ConfigController>();
         _repository = repository;
         _mapper = mapper;
         _realTimeUpdater = realTimeUpdater;
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

      [HttpPost]
      [Route("strategies/create")]
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
      [Route("strategies/get")]
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

      [HttpGet]
      [Route("liquidationStrategyConfigs/get")]
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
      public async Task<ActionResult<LiquidationStrategyConfigDTO>> GetLiquidationStrategyConfig(int strategyConfigId)
      {
         try
         {
            var configs = await _repository.GetLiquidationStrategyConfigByStrategyConfigId(strategyConfigId);
          
            // Now trigger an initialisation of the real time part
       
            await _realTimeUpdater.Init(strategyConfigId);

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
   }
}
