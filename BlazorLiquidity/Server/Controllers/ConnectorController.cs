using Common;
using Common.Messages;
using MessageBroker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Common.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace PortfolioManagementAPI.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   [Authorize(Roles = "Administrator")]
   public class ConnectorController : ControllerBase
   {
      private readonly ILogger<ConnectorController> _logger;
      private readonly IMessageBroker _messageBroker;
      public ConnectorController(ILoggerFactory loggerFactory, IMessageBroker messageBroker)
      {
         _logger = loggerFactory.CreateLogger<ConnectorController>();
         _messageBroker = messageBroker;
      }

      [HttpPost]
      [Route("connect/private")]
      public  ActionResult ConnectPrivate(string exchange, string spName)
      {
         try
         {
            if (exchange == null || spName == null)
               return BadRequest();

            var ccc = new ConnectorConnectMsg()
            {
               Exchange = exchange,
               IsConnect = true,
               IsPrivate = true,
               SPName = spName
            };

            var msg = new MessageBusCommand()
            {
               CommandType = CommandTypesEnum.DISCONNECT_PRIVATE,
               AccountName = spName,
               Exchange = exchange,
               Data = JsonSerializer.Serialize(ccc)
            };

            var bytesRef = MessageBusCommand.ProtoSerialize(msg);
            _messageBroker.PublishToSubject(Constants.CONNECTOR_COMMAND, bytesRef);

            return Ok("");
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error connecting/disconnecting from {exchange}");
         }
      }

      [HttpPost]
      [Route("disconnect/private")]
      public ActionResult DisconnectPrivate(string exchange, string spName)
      {
         try
         {
            if (exchange == null || spName == null)
               return BadRequest();

            var ccc = new ConnectorConnectMsg()
            {
               Exchange = exchange,
               IsConnect = false,
               IsPrivate = true,
               SPName = spName
            };

            var msg = new MessageBusCommand()
            {
               CommandType = CommandTypesEnum.DISCONNECT_PRIVATE,
               AccountName = spName,
               Exchange = exchange,
               Data = JsonSerializer.Serialize(ccc)
            };

            var bytesRef = MessageBusCommand.ProtoSerialize(msg);
            _messageBroker.PublishToSubject(Constants.CONNECTOR_COMMAND, bytesRef);

            return Ok("");
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error connecting/disconnecting from {exchange}");
         }
      }

      [HttpPost]
      [Route("connect/public")]
      public ActionResult ConnectPublic(string exchange)
      {
         try
         {
            if (exchange == null )
               return BadRequest();

            var ccc = new ConnectorConnectMsg()
            {
               Exchange = exchange,
               IsConnect = true,
               IsPrivate = false,
               SPName = ""
            };

            var msg = new MessageBusCommand()
            {
               CommandType = CommandTypesEnum.DISCONNECT_PRIVATE,
               AccountName = "",
               Exchange = exchange,
               Data = JsonSerializer.Serialize(ccc)
            };

            var bytesRef = MessageBusCommand.ProtoSerialize(msg);
            _messageBroker.PublishToSubject(Constants.CONNECTOR_COMMAND, bytesRef);

            return Ok("");
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error connecting/disconnecting from {exchange}");
         }
      }

      [HttpPost]
      [Route("disconnect/public")]
      public ActionResult DisconnectPublic(string exchange)
      {
         try
         {
            if (exchange == null)
               return BadRequest();

            var ccc = new ConnectorConnectMsg()
            {
               Exchange = exchange,
               IsConnect = false,
               IsPrivate = false,
               SPName = ""
            };

            var msg = new MessageBusCommand()
            {
               CommandType = CommandTypesEnum.DISCONNECT_PRIVATE,
               AccountName = "",
               Exchange = exchange,
               Data = JsonSerializer.Serialize(ccc)
            };

            var bytesRef = MessageBusCommand.ProtoSerialize(msg);
            _messageBroker.PublishToSubject(Constants.CONNECTOR_COMMAND, bytesRef);

            return Ok("");
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error connecting/disconnecting from {exchange}");
         }
      }

      [HttpPost]
      [Route("getStatus/public")]
      public ActionResult GetPublicStatus(VenueDTO venue)
      {
         try
         {
            if (venue == null)
               return BadRequest();

            try
            {
               var msg = new MessageBusCommand()
               {
                  CommandType = CommandTypesEnum.GET_PUBLIC_STATUS,
                  Exchange = venue.VenueName,
                  Data = venue.VenueName
               };

               var bytesRef = MessageBusCommand.ProtoSerialize(msg);
               _messageBroker.PublishToSubject(venue.VenueName, bytesRef);
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error in SendPublicStatusEnquiry - {Error}", e.Message);
            }

            return Ok("");
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error GetPublicStatus from {venue.VenueName}");
         }
      }

   }
}