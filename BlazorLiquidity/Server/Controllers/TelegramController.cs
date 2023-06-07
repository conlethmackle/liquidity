using AutoMapper;
using Common.Messages;
using Common.Models.DTOs;
using DataStore;
using MessageBroker;
using Microsoft.AspNetCore.Mvc;
using PortfolioManagementAPI.Controllers;
using System.Text.Json;
using Common;
using Microsoft.AspNetCore.Authorization;

namespace BlazorLiquidity.Server.Controllers
{
   [Authorize(Roles = "LiquidityCommands, Administrator")]
   [Route("api/[controller]")]
   [ApiController]
   public class TelegramController :  ControllerBase
   {
      private readonly ILogger<TelegramController> _logger;
      private readonly IPortfolioRepository _repository;
      private readonly IMapper _mapper;
      private readonly IMessageBroker _messageBroker;

      public TelegramController(ILoggerFactory loggerFactory, IPortfolioRepository repository, IMapper mapper,
                                 IMessageBroker messageBroker)
      {
         _logger = loggerFactory.CreateLogger<TelegramController>();
         _repository = repository;
         _mapper = mapper;
         _messageBroker = messageBroker;
      }

      [HttpPost]
      [Route("create_alert")]
   //   [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<TelegramAlertDTO>> CreateAlert(TelegramAlertDTO alertDTO)
      {
         try
         {
            if (alertDTO == null)
               return BadRequest();

            var createdAlert = await _repository.CreateTelegramAlerts(alertDTO);
            BroadcastConfigChange(TelegramConfigChangeType.ALERT_CHANGE);

            return CreatedAtAction("get_alerts",
               new { id = createdAlert.TelegramAlertId }, createdAlert);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Telegram Alert");
         }
      }

      [HttpGet]
      [Route("get_alerts")]
      [ActionName("get_alerts")]
 //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<IEnumerable<TelegramAlertDTO>>> GetAlerts()
      {
         try
         {
            var alerts = await _repository.GetTelegramAlerts();
            return Ok(alerts);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting telegram alerts from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting telegram alerts");
         }
      }

      [HttpPut]
      [Route("update_alert")]
 //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> UpdateAlert(TelegramAlertDTO alertDTO)
      {
         try
         {
            if (alertDTO == null)
               return BadRequest();
            await _repository.UpdateTelegramAlert(alertDTO);
            BroadcastConfigChange(TelegramConfigChangeType.ALERT_CHANGE);
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating telegram alert to db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating telegram alerts");
         }
      }

      [HttpDelete]
      [Route("delete_alert")]
//      [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> DeleteAlert(int Id)
      {
         try
         {
            await _repository.DeleteTelegramAlert(Id);
            BroadcastConfigChange(TelegramConfigChangeType.ALERT_CHANGE);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting Telegram alert {Id}");
         }
      }

      [HttpPost]
      [Route("create_alert_category")]
  //    [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<TelegramAlertCategoryDTO>> CreateAlertCategory(TelegramAlertCategoryDTO alertCategoryDTO)
      {
         try
         {
            if (alertCategoryDTO == null)
               return BadRequest();

            var createdAlert = await _repository.CreateTelegramAlertCategory(alertCategoryDTO);

            return CreatedAtAction("get_alerts",
               new { id = createdAlert.Id }, createdAlert);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Telegram Alert");
         }
      }

      [HttpGet]
      [Route("get_alert_categories")]
      [ActionName("get_alert_categories")]
      public async Task<ActionResult<IEnumerable<TelegramAlertCategoryDTO>>> GetAlertCategories()
      {
         try
         {
            var alertCategories = await _repository.GetTelegramAlertCategories();
            return Ok(alertCategories);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting telegram alerts from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting telegram alerts");
         }
      }

      [HttpPut]
      [Route("update_alert_category")]
 //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> UpdateAlertCategory(TelegramAlertCategoryDTO alertCategoryDTO)
      {
         try
         {
            if (alertCategoryDTO == null)
               return BadRequest();
            await _repository.UpdateTelegramAlertCategory(alertCategoryDTO);
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating telegram alert category to db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating telegram alert category");
         }
      }

      [HttpDelete]
      [Route("delete_alert_category")]
//      [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> DeleteAlertCategory(int Id)
      {
         try
         {
            await _repository.DeleteTelegramAlertCategory(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting Telegram alert category {Id}");
         }
      }

      [HttpPost]
      [Route("create_alert_to_channel")]
  //    [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<TelegramAlertToChannelDTO>> CreateAlertToChannel(TelegramAlertToChannelDTO alertToChannelDTO)
      {
         try
         {
            if (alertToChannelDTO == null)
               return BadRequest();

            var createdAlert = await _repository.CreateTelegramAlertToChannel(alertToChannelDTO);
            BroadcastConfigChange(TelegramConfigChangeType.ALERT_TO_CHANNEL);

            return CreatedAtAction("get_alerts_to_channels",
               new { id = createdAlert.TelegramAlertToChannelId }, createdAlert);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Telegram Alert To Channel");
         }
      }

      [HttpGet]
      [Route("get_alerts_to_channels")]
      [ActionName("get_alerts_to_channels")]
      public async Task<ActionResult<IEnumerable<TelegramAlertToChannelDTO>>> GetAlertToChannels()
      {
         try
         {
            var alertToChannelDTOs = await _repository.GetTelegramAlertsToChannels();
            
            return Ok(alertToChannelDTOs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting telegram alerts to channels from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting telegram alerts to channels");
         }
      }

      [HttpPut]
      [Route("update_alert_to_channel")]
//      [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> UpdateAlertToChannel(TelegramAlertToChannelDTO alertCategoryDTO)
      {
         try
         {
            if (alertCategoryDTO == null)
               return BadRequest();
            await _repository.UpdateTelegramAlertToChannel(alertCategoryDTO);
            BroadcastConfigChange(TelegramConfigChangeType.ALERT_TO_CHANNEL);
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating telegram alert to channel to db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating telegram alert to channel");
         }
      }

      [HttpDelete]
      [Route("delete_alert_to_channel")]
 //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> DeleteAlertToChannel(int Id)
      {
         try
         {
            await _repository.DeleteTelegramAlertToChannel(Id);
            BroadcastConfigChange(TelegramConfigChangeType.ALERT_TO_CHANNEL);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting Telegram alert to channel {Id}");
         }
      }

      [HttpPost]
      [Route("create_channel")]
  //    [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<TelegramChannelDTO>> CreateChannel(TelegramChannelDTO alertToChannelDTO)
      {
         try
         {
            if (alertToChannelDTO == null)
               return BadRequest();

            var createdAlert = await _repository.CreateTelegramChannel(alertToChannelDTO);
            BroadcastConfigChange(TelegramConfigChangeType.CHANNEL_CHANGE);

            return CreatedAtAction("get_channels",
               new { id = createdAlert.TelegramChannelId }, createdAlert);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Telegram Alert To Channel");
         }
      }

      [HttpGet]
      [Route("get_channels")]
      [ActionName("get_channels")]
      public async Task<ActionResult<IEnumerable<TelegramChannelDTO>>> GetChannels()
      {
         try
         {
            var alertToChannelDTOs = await _repository.GetTelegramChannels();
            return Ok(alertToChannelDTOs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting telegram channels from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting telegram channels");
         }
      }

      [HttpGet]
      [Route("get_command_channels")]
      [ActionName("get_command_channels")]
      public async Task<ActionResult<IEnumerable<TelegramChannelDTO>>> GetCommandChannels()
      {
         try
         {
            var alertToChannelDTOs = await _repository.GetCommandTelegramChannels();
            return Ok(alertToChannelDTOs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting command telegram channels from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting command telegram channels");
         }
      }

      [HttpPut]
      [Route("update_channel")]
 //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> UpdateChannel(TelegramChannelDTO alertCategoryDTO)
      {
         try
         {
            if (alertCategoryDTO == null)
               return BadRequest();
            await _repository.UpdateTelegramChannel(alertCategoryDTO);
            BroadcastConfigChange(TelegramConfigChangeType.CHANNEL_CHANGE);
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating telegram alert to channel to db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating telegram alert to channel");
         }
      }

      [HttpDelete]
      [Route("delete_channel")]
 //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> DeleteChannel(int Id)
      {
         try
         {
            await _repository.DeleteTelegramChannel(Id);
            BroadcastConfigChange(TelegramConfigChangeType.CHANNEL_CHANGE);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting Telegram alert to channel {Id}");
         }
      }

      [HttpPost]
      [Route("create_command_to_user")]
   //   [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<TelegramCommandToUserDTO>> CreateCommandToUser(TelegramCommandToUserDTO alertToChannelDTO)
      {
         try
         {
            if (alertToChannelDTO == null)
               return BadRequest();

            var createdAlert = await _repository.CreateTelegramCommandToUser(alertToChannelDTO);
            BroadcastConfigChange(TelegramConfigChangeType.USER_TO_COMMAND);

            return CreatedAtAction("get_commands_to_users",
               new { id = createdAlert.TelegramCommandToUserId }, createdAlert);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Telegram Command To User");
         }
      }

      [HttpGet]
      [Route("get_commands_to_users")]
      [ActionName("get_commands_to_users")]
      public async Task<ActionResult<IEnumerable<TelegramCommandToUserDTO>>> GetCommandsToUsers()
      {
         try
         {
            var alertToChannelDTOs = await _repository.GetTelegramCommandToUsers();
            return Ok(alertToChannelDTOs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting telegram commands to users from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting telegram commands to users");
         }
      }

      [HttpPut]
      [Route("update_command_to_user")]
 //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> UpdateCommandToUser(TelegramCommandToUserDTO alertCategoryDTO)
      {
         try
         {
            if (alertCategoryDTO == null)
               return BadRequest();
            await _repository.UpdateTelegramCommandToUser(alertCategoryDTO);
            BroadcastConfigChange(TelegramConfigChangeType.USER_TO_COMMAND);
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating telegram command to user to db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating telegram command to user ");
         }
      }

      [HttpDelete]
      [Route("delete_command_to_user")]
  //    [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> DeleteCommandToUser(int Id)
      {
         try
         {
            await _repository.DeleteTelegramCommandToUser(Id);
            BroadcastConfigChange(TelegramConfigChangeType.USER_TO_COMMAND);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting Telegram command to user {Id}");
         }
      }

      [HttpPost]
      [Route("create_command_type")]
 //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<TelegramCommandTypeDTO>> CreateCommandType(TelegramCommandTypeDTO alertToChannelDTO)
      {
         try
         {
            if (alertToChannelDTO == null)
               return BadRequest();

            var createdAlert = await _repository.CreateTelegramCommandType(alertToChannelDTO);

            return CreatedAtAction("get_commands_types",
               new { id = createdAlert.TelegramCommandTypeId }, createdAlert);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Telegram Command Type");
         }
      }

      [HttpGet]
      [Route("get_commands_types")]
      [ActionName("get_commands_types")]
      public async Task<ActionResult<IEnumerable<TelegramCommandTypeDTO>>> GetCommandTypes()
      {
         try
         {
            var alertToChannelDTOs = await _repository.GetTelegramCommandTypes();
            return Ok(alertToChannelDTOs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting telegram command types from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting telegram command types");
         }
      }

      [HttpPut]
      [Route("update_command_type")]
  //    [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> UpdateCommandType(TelegramCommandTypeDTO alertCategoryDTO)
      {
         try
         {
            if (alertCategoryDTO == null)
               return BadRequest();
            await _repository.UpdateTelegramCommandType(alertCategoryDTO);
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating telegram command type to db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating telegram command type ");
         }
      }

      [HttpDelete]
      [Route("delete_command_type")]
  //    [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> DeleteCommandType(int Id)
      {
         try
         {
            await _repository.DeleteTelegramCommandType(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting Telegram command type {Id}");
         }
      }

      [HttpPost]
      [Route("create_command")]
 //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<TelegramCommandDTO>> CreateCommand(TelegramCommandDTO alertToChannelDTO)
      {
         try
         {
            if (alertToChannelDTO == null)
               return BadRequest();

            var createdAlert = await _repository.CreateTelegramCommand(alertToChannelDTO);
            BroadcastConfigChange(TelegramConfigChangeType.COMMAND_CHANGE);
            return CreatedAtAction("get_commands",
               new { id = createdAlert.TelegramCommandId }, createdAlert);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Telegram Command");
         }
      }

      [HttpGet]
      [Route("get_commands")]
      [ActionName("get_commands")]
      public async Task<ActionResult<IEnumerable<TelegramCommandDTO>>> GetCommands()
      {
         try
         {
            var alertToChannelDTOs = await _repository.GetTelegramCommands();
            return Ok(alertToChannelDTOs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting telegram commands from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting telegram commands");
         }
      }

      [HttpPut]
      [Route("update_command")]
  //    [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> UpdateCommand(TelegramCommandDTO alertCategoryDTO)
      {
         try
         {
            if (alertCategoryDTO == null)
               return BadRequest();
            await _repository.UpdateTelegramCommand(alertCategoryDTO);
            BroadcastConfigChange(TelegramConfigChangeType.COMMAND_CHANGE);
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating telegram command to db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating telegram command");
         }
      }

      [HttpDelete]
      [Route("delete_command")]
  //    [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> DeleteCommand(int Id)
      {
         try
         {
            await _repository.DeleteTelegramCommand(Id);
            BroadcastConfigChange(TelegramConfigChangeType.COMMAND_CHANGE);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting Telegram command {Id}");
         }
      }

      [HttpPost]
      [Route("create_subscriber_to_channel")]
  //    [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<TelegramSubscriberToChannelDTO>> CreateSubscriberToChannel(TelegramSubscriberToChannelDTO alertToChannelDTO)
      {
         try
         {
            if (alertToChannelDTO == null)
               return BadRequest();

            alertToChannelDTO.TelegramChannel = null;
            alertToChannelDTO.TelegramUser = null;
            var createdAlert = await _repository.CreateTelegramSubscriberToChannel(alertToChannelDTO);
            BroadcastConfigChange(TelegramConfigChangeType.USER_TO_CHANNEL);

            return CreatedAtAction("get_subscribers_to_channels",
               new { id = createdAlert.TelegramSubscriberToChannelId }, createdAlert);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Telegram Subscriber to Channel");
         }
      }

      [HttpGet]
      [Route("get_subscribers_to_channels")]
      [ActionName("get_subscribers_to_channels")]
      public async Task<ActionResult<IEnumerable<TelegramSubscriberToChannelDTO>>> GetSubscribersToChannels()
      {
         try
         {
            var alertToChannelDTOs = await _repository.GetTelegramSubscriberToChannels();
            return Ok(alertToChannelDTOs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting telegram subscribers to channels from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting telegram subscribers to channels");
         }
      }

      [HttpPut]
      [Route("update_subscriber_to_channel")]
 //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> UpdateSubscriberToChannel(TelegramSubscriberToChannelDTO alertCategoryDTO)
      {
         try
         {
            if (alertCategoryDTO == null)
               return BadRequest();
            await _repository.UpdateTelegramSubscriberToChannel(alertCategoryDTO);
            BroadcastConfigChange(TelegramConfigChangeType.USER_TO_CHANNEL);
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating Telegram Subscriber to Channel to db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating Telegram Subscriber to Channel");
         }
      }

      [HttpDelete]
      [Route("delete_subscriber_to_channel")]
 //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> DeleteSubscriberToChannel(int Id)
      {
         try
         {
            await _repository.DeleteTelegramSubscriberToChannel(Id);
            BroadcastConfigChange(TelegramConfigChangeType.USER_TO_CHANNEL);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting Telegram Subscriber to Channel {Id}");
         }
      }

      [HttpPost]
      [Route("create_user")]
 //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<TelegramAlertDTO>> CreateUser(TelegramUserDTO data)
      {
         try
         {
            if (data == null)
               return BadRequest();

            var createdUser = await _repository.CreateTelegramUser(data);
            BroadcastConfigChange(TelegramConfigChangeType.USER_CHANGE);

            return CreatedAtAction("get_users",
               new { id = createdUser.Id }, createdUser);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Telegram User");
         }
      }

      [HttpGet]
      [Route("get_users")]
      [ActionName("get_users")]
      public async Task<ActionResult<IEnumerable<TelegramUserDTO>>> GetUsers()
      {
         try
         {
            var users = await _repository.GetTelegramUsers();
            return Ok(users);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting telegram users from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting telegram users");
         }
      }

      [HttpPut]
      [Route("update_user")]
 //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> UpdateUser(TelegramUserDTO userDTO)
      {
         try
         {
            if (userDTO == null)
               return BadRequest();
            await _repository.UpdateTelegramUser(userDTO);
            BroadcastConfigChange(TelegramConfigChangeType.USER_CHANGE);
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating telegram user to db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating telegram user");
         }
      }

      [HttpDelete]
      [Route("delete_user")]
 //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> DeleteUser(int Id)
      {
         try
         {
            await _repository.DeleteTelegramUser(Id);
            BroadcastConfigChange(TelegramConfigChangeType.USER_CHANGE);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting Telegram User {Id}");
         }
      }

      [HttpPost]
      [Route("create_alert_behaviour_type")]
      //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<TelegramAlertBehaviourTypeDTO>> CreateTelegramAlertBehaviourType(TelegramAlertBehaviourTypeDTO alertToChannelDTO)
      {
         try
         {
            if (alertToChannelDTO == null)
               return BadRequest();

            var createdAlert = await _repository.CreateTelegramAlertBehaviourType(alertToChannelDTO);
           
            return CreatedAtAction("get_alert_behaviour_types",
               new { id = createdAlert.Id }, createdAlert);
         }
         catch (Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Telegram Behaviour Type");
         }
      }

      [HttpGet]
      [Route("get_alert_behaviour_types")]
      [ActionName("get_alert_behaviour_types")]
      public async Task<ActionResult<IEnumerable<TelegramAlertBehaviourTypeDTO>>> GetTelegramAlertBehaviourTypes()
      {
         try
         {
            var alertToChannelDTOs = await _repository.GetTelegramAlertBehaviourTypes();
            return Ok(alertToChannelDTOs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting telegram behaviour types from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting telegram behaviour types");
         }
      }

      [HttpPut]
      [Route("update_alert_behaviour_type")]
      //    [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> UpdateTelegramAlertBehaviourType(TelegramAlertBehaviourTypeDTO alertCategoryDTO)
      {
         try
         {
            if (alertCategoryDTO == null)
               return BadRequest();
            await _repository.UpdateTelegramAlertBehaviourType(alertCategoryDTO);
            
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating telegram command to db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating telegram command");
         }
      }

      [HttpDelete]
      [Route("delete_alert_behaviour_type")]
      //    [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> DeleteTelegramAlertBehaviourType(int Id)
      {
         try
         {
            await _repository.DeleteTelegramAlertBehaviourType(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting Telegram Behaviour Type {Id}");
         }
      }


      [HttpPost]
      [Route("create_alert_behaviour")]
      //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<TelegramAlertBehaviourDTO>> CreateTelegramAlertBehaviour(TelegramAlertBehaviourDTO alertToChannelDTO)
      {
         try
         {
            if (alertToChannelDTO == null)
               return BadRequest();

            var createdAlert = await _repository.CreateTelegramAlertBehaviour(alertToChannelDTO);

            return CreatedAtAction("get_alert_behaviours",
               new { id = createdAlert.Id }, createdAlert);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Telegram Behaviour Type");
         }
      }

      [HttpGet]
      [Route("get_alert_behaviours")]
      [ActionName("get_alert_behaviours")]
      public async Task<ActionResult<IEnumerable<TelegramAlertBehaviourDTO>>> GetTelegramAlertBehaviours()
      {
         try
         {
            var alertToChannelDTOs = await _repository.GetTelegramAlertBehaviours();
            return Ok(alertToChannelDTOs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting telegram behaviour from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting telegram behaviour");
         }
      }

      [HttpPut]
      [Route("update_alert_behaviour")]
      //    [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> UpdateTelegramAlertBehaviour(TelegramAlertBehaviourDTO alertCategoryDTO)
      {
         try
         {
            if (alertCategoryDTO == null)
               return BadRequest();
            await _repository.UpdateTelegramAlertBehaviour(alertCategoryDTO);
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating telegram alert behaviour to db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating telegram alert behaviour");
         }
      }

      [HttpDelete]
      [Route("delete_alert_behaviour")]
      //    [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> DeleteTelegramAlertBehaviour(int Id)
      {
         try
         {
            await _repository.DeleteTelegramAlertBehaviour(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting Telegram Alert Behaviour {Id}");
         }
      }

      private void BroadcastConfigChange(TelegramConfigChangeType changeType)
      {
         try
         {
            var configChange = new TelegramConfigChange()
            {
               ChangeType = changeType
            };

            
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.TELEGRAM_CONFIG_CHANGE,
               Data = JsonSerializer.Serialize(configChange)

            };

            PublishHelper.PublishToTopic(Constants.TELEGRAM_CONFIG_CHANGE_TOPIC, response, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogError("Error cancelling Order {Error}", e.Message);
            throw;
         }
      }
   }
}
