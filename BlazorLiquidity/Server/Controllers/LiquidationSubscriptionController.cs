using AccountBalanceManager;
using BlazorLiquidity.Shared;
using Common.Messages;
using Common.Models.DTOs;
using ConnectorStatus;
using DataStore;
using MessageBroker;
using Microsoft.AspNetCore.Mvc;
using OrderAndTradeProcessing;
using StrategyMessageListener;
using System.Text.Json;
using AutoMapper;
using Common.Models.Entities;
using Common;
using Microsoft.AspNetCore.Authorization;

namespace BlazorLiquidity.Server.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   [Authorize()]
   public class LiquidationSubscriptionController : Controller
   {
      private readonly ILogger<LiquidationSubscriptionController> _logger;
      private readonly IPortfolioRepository _repository;
      private readonly IMapper _mapper;
      private readonly IMessageBroker _messageBroker;

      public LiquidationSubscriptionController(ILoggerFactory loggerFactory,
                                               IPortfolioRepository repository,
                                               IMapper mapper,
                                               IMessageBroker broker)
      {
         _logger = loggerFactory.CreateLogger<LiquidationSubscriptionController>();
         _repository = repository;
         _mapper = mapper;
         _messageBroker = broker;
      }

      [HttpGet]
      [Route("liquidationsubscriptions/get")]
      [ActionName("liquidationsubscriptions/get")]
      public async Task<ActionResult<IEnumerable<LiquidationConfigurationDTO>>> GetOpeningLiquidationSubscriptions()
      {
         try
         {
            var subs = await _repository.GetOpeningLiquidationSubscriptions();
            return Ok(subs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error GetOpeningLiquidationSubscriptions - {Message}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error GetOpeningLiquidationSubscriptions");
         }
        
      }

      [HttpGet]
      [Route("liquidationsubscriptions/getInstancesForSp")]
      [ActionName("liquidationsubscriptions/getInstancesForSp")]
      public async Task<ActionResult<IEnumerable<LiquidationConfigurationDTO>>> GetOpeningLiquidationSubscriptionsForSp(string spName)
      {
         try
         {
            var subs = await _repository.GetOpeningLiquidationSubscriptionsForSp(spName);
            return Ok(subs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error GetOpeningLiquidationSubscriptionsForSp - {Message}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error GetOpeningLiquidationSubscriptionsForSp");
         }

      }

      [HttpGet]
      [Route("liquidationsubscriptions/getById")]
      [ActionName("liquidationsubscriptions/getById")]
      public async Task<ActionResult<LiquidationConfigurationDTO>> GetOpeningLiquidationSubscriptionById(int id)
      {
         try
         {
            var sub = await _repository.GetOpeningLiquidationSubscription(id);
            return Ok(sub);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error GetOpeningLiquidationSubscriptionById - {Message}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error GetOpeningLiquidationSubscriptionById");
         }
      }

      [Route("liquidationsubscriptions/getByConfigId")]
      [ActionName("liquidationsubscriptions/getByConfigId")]
      public async Task<ActionResult<LiquidationConfigurationDTO>> GetOpeningLiquidationSubscriptionByConfigId(int id)
      {
         try
         {
            var sub = await _repository.GetOpeningLiquidationSubscriptionForStrategySPSubscriptionId(id);
            return Ok(sub);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error GetOpeningLiquidationSubscriptionByConfigId - {Message}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error GetOpeningLiquidationSubscriptionByConfigId");
         }
      }

      [HttpPost]
      [Route("liquidationsubscriptions/create")]
      public async Task<ActionResult<LiquidationConfigurationDTO>> CreateLiquidationOpeningSubscription(LiquidationConfigurationDTO subscriptionDto)
      {
         try
         {
            if (subscriptionDto == null)
               return BadRequest();

            var createdSubscription = await _repository.CreateOpeningLiquidationSubscription(subscriptionDto);

            return CreatedAtAction("liquidationsubscriptions/get",
                new { id = createdSubscription.Id }, createdSubscription);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error creating new CreateLiquidationOpeningSubscription");
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating new CreateLiquidationOpeningSubscription");
         }
      }

      [HttpPut]
      [Route("liquidationsubscriptions/update")]
      public async Task<ActionResult> UpdateLiquidation(LiquidationConfigurationDTO sub)
      {
         try
         {
            if (sub == null)
               return BadRequest();

            await _repository.UpdateOpeningLiquidationSubscription(sub);
            var updatedConfig =
               await _repository.GetOpeningLiquidationSubscriptionForStrategySPSubscriptionId(
                  sub.StrategySPSubscriptionConfigId);
            BroadcastConfigChange(updatedConfig);
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in  UpdateLiquidation {Error}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating LiquidationOpeningSubscription ");
         }
      }

      [HttpDelete] 
      [Route("liquidationsubscriptions/delete")]
      public async Task<ActionResult> DeleteOpeningLiquidationSubscription(int Id)
      {
         try
         {
            await _repository.DeleteOpeningLiquidationSubscription(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error DeletePortfolio ");
         }
      }

      [HttpPost]
      [Route("liquidationorderloading/create")]
      public async Task<ActionResult<LiquidationOrderLoadingConfigurationDTO>> CreateLiquidationOrderLoadingConfiguration(LiquidationOrderLoadingConfigurationDTO subscriptionDto)
      {
         try
         {
            if (subscriptionDto == null)
               return BadRequest();

            var createdSubscription = await _repository.CreateLiquidationOrderLoadingConfiguration(subscriptionDto);

            return CreatedAtAction("liquidationorderloading/get",
                new { id = createdSubscription.Id }, createdSubscription);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error creating new CreateLiquidationOrderLoadingConfiguration");
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating new CreateLiquidationOrderLoadingConfiguration");
         }
      }

      [HttpGet]
      [Route("liquidationorderloading/get")]
      [ActionName("liquidationorderloading/get")]
      public async Task<ActionResult<IEnumerable<LiquidationOrderLoadingConfigurationDTO>>> GetLiquidationOrderLoadingConfiguration()
      {
         try
         {
            var subs = await _repository.GetLiquidationOrderLoadingConfiguration();
            return Ok(subs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error GetLiquidationOrderLoadingConfiguration - {Message}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error GetLiquidationOrderLoadingConfiguration");
         }

      }

      [HttpPut]
      [Route("liquidationorderloading/update")]
      public async Task<ActionResult> UpdateLiquidationOrderLoadingConfiguration(LiquidationOrderLoadingConfigurationDTO sub)
      {
         try
         {
            if (sub == null)
               return BadRequest();

            await _repository.UpdateLiquidationOrderLoadingConfiguration(sub);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating UpdateLiquidationOrderLoadingConfiguration ");
         }
      }

      [HttpDelete] 
      [Route("liquidationorderloading/delete")]
      public async Task<ActionResult> DeleteLiquidationOrderLoadingConfiguration(int Id)
      {
         try
         {
            await _repository.DeleteLiquidationOrderLoadingConfiguration(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error DeleteLiquidationOrderLoadingConfiguration ");
         }
      }

      [HttpPost]
      [Route("liquidationmanualorderloading/create")]
      public async Task<ActionResult<LiquidationManualOrderLoadingDTO>> CreateLiquidationManualOrderLoading(LiquidationManualOrderLoadingDTO dto)
      {
         try
         {
            if (dto == null)
               return BadRequest();

            var createdSubscription = await _repository.CreateLiquidationManualOrderLoading(dto);

            return CreatedAtAction("liquidationmanualorderloading/get",
                new { id = createdSubscription.Id }, createdSubscription);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error creating new CreateLiquidationManualOrderLoading");
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating new CreateLiquidationManualOrderLoading");
         }
      }

      [HttpGet]
      [Route("liquidationmanualorderloading/get")]
      [ActionName("liquidationmanualorderloading/get")]
      public async Task<ActionResult<IEnumerable<LiquidationManualOrderLoadingDTO>>> GetLiquidationManualOrderLoading()
      {
         try
         {
            var subs = await _repository.GetLiquidationManualOrderLoading();
            return Ok(subs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error GetLiquidationManualOrderLoading - {Message}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error GetLiquidationManualOrderLoading");
         }

      }


      [HttpPut]
      [Route("liquidationmanualorderloading/update")]
      public async Task<ActionResult> UpdateLiquidationManualOrderLoadingConfiguration(LiquidationManualOrderLoadingDTO sub)
      {
         try
         {
            if (sub == null)
               return BadRequest();

            await _repository.UpdateLiquidationManualOrderLoadingConfiguration(sub);
            BroadcastConfigChange(sub);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating UpdateLiquidationOrderLoadingConfiguration ");
         }
      }

      [HttpDelete]
      [Route("liquidationmanualorderloading/delete")]
      public async Task<ActionResult> DeleteLiquidationManualOrderLoadingConfiguration(int Id)
      {
         try
         {
            await _repository.DeleteLiquidationOrderLoadingConfiguration(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error DeleteLiquidationManualOrderLoadingConfiguration ");
         }
      }

      private void BroadcastConfigChange(LiquidationConfigurationDTO liquidationStrategyConfig)
      {
         try
         {
            var strategyConfigData = new StrategyConfigChangeData()
            {
               StrategyConfigChangeType = StrategyConfigChangeType.LIQUIDATION,
               InstanceName = liquidationStrategyConfig.StrategySPSubscriptionConfig.ConfigName
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

      private void BroadcastConfigChange(LiquidationManualOrderLoadingDTO config)
      {
         try
         {
            var strategyConfigData = new StrategyConfigChangeData()
            {
               StrategyConfigChangeType = StrategyConfigChangeType.LIQUIDATION_MANUAL_ORDER_SIZES,
               InstanceName = config.StrategyInstance.ConfigName
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
