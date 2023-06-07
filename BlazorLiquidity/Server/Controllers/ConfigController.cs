using AutoMapper;
using Common.Models.DTOs;
using DataStore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioManagementAPI.Controllers
{
   [Route("api/[controller]")]
   [Authorize()]
   [ApiController]
   public class ConfigController : ControllerBase
   {
      private readonly ILogger<ConfigController> _logger;
      private readonly IPortfolioRepository _repository;
      private readonly IMapper _mapper;

      public ConfigController(ILoggerFactory loggerFactory, IPortfolioRepository repository, IMapper mapper)
      {
         _logger = loggerFactory.CreateLogger<ConfigController>();
         _repository = repository;
         _mapper = mapper;
      }


      [HttpPost]
      [Route("rabbitmq/create")]
      [Authorize(Roles = "Administrator")]
      public async Task<ActionResult<RabbitMQSettingsDTO>> CreateMQSettings(RabbitMQSettingsDTO rabbitSettings)
      {
         try
         {
            if (rabbitSettings == null)
               return BadRequest();

            var createdSettings = await _repository.CreateMQSettings(rabbitSettings);
         
            return CreatedAtAction("getRabbitMqSettings",
                new { id = createdSettings.Id }, createdSettings);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating MQ Settings");
         }
      }

      [HttpGet]
      [Route("rabbitmq/get")]
      [ActionName("getRabbitMqSettings")]
      public async Task<ActionResult<RabbitMQSettingsDTO>> GetMQSettings()
      {
         try
         {            
            var setting = await _repository.GetMQSettingsAsync();           
            return Ok(setting);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting mq settings from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error getting mq settings");
         }
      }

      [HttpPut]
      [Route("rabbitmq/update")]
      [Authorize(Roles = "Administrator")]
      public async Task<ActionResult> UpdateMQSettings(RabbitMQSettingsDTO rabbitSettings)
      {
         try
         {
            if (rabbitSettings == null)
               return BadRequest();
            await _repository.UpdateMQSettings(rabbitSettings);
            return Ok();
         }
         catch(Exception e)
         {
            _logger.LogError(e, "Error updating mq settings from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting mq settings");
         }
      }

      [HttpDelete]
      [Route("rabbitmq/delete")]
      [Authorize(Roles = "Administrator")]
      public async Task<ActionResult> DeleteMQSettings(int Id)
      {
         try
         {
            await _repository.DeleteMQSettings(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting DeleteMQSettings {Id}");
         }
      }
   }
}
