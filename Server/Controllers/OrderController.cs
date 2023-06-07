using SyncfusionLiquidity.Server.Receiver;
using SyncfusionLiquidity.Shared;
using Common.Messages;
using Common.Models.DTOs;
using DataStore;
using MessageBroker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderAndTradeProcessing;
using StrategyMessageListener;
using System.Text.Json;
using AccountBalanceManager;

namespace BlazorLiquidity.Server.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class OrderController : ControllerBase
   {
      ILogger<OrderController> _logger;
      private readonly IOrderAndTradeProcessing _orderProcessor;
      protected readonly IMessageBroker _messageBroker;
      protected readonly IPortFolioMessageReceiver _messageReceiver;
      protected readonly IPortfolioRepository _portfolioRepository;
      protected readonly IInventoryManager _inventoryManager;
      
         protected readonly Queue<OrderDetails> _orderQueue = new Queue<OrderDetails>();

      public OrderController(ILoggerFactory loggerFactory, 
                             IOrderAndTradeProcessing orderProcessor, 
                             IMessageBroker messageBroker, 
                             IPortfolioRepository portfolioRepository,
                             IPortFolioMessageReceiver messageReceiver,
                             IInventoryManager inventoryManager)
      {
         _logger = loggerFactory.CreateLogger<OrderController>();
         _orderProcessor = orderProcessor;   
         _portfolioRepository = portfolioRepository;
         _messageReceiver = messageReceiver;
         _messageBroker = messageBroker;
         _inventoryManager = inventoryManager;
         _messageReceiver.OnPrivateLoginStatus += OnPrivateLoginStatus;
      }

      private void OnPrivateLoginStatus(string venue, PrivateClientLoginStatus status)
      {
         if (status.IsLoggedIn)
         {
            if (_orderQueue.Count > 0)
            {
               var newOrder = _orderQueue.Dequeue();
             //  _orderProcessor.PlaceOrderUI(newOrder);
            }
         }
      }

      [HttpPost]
      [Route("openorders/request")]
      //public async Task<ActionResult> GetOpenOrders(string venue)
      public async Task<ActionResult> GetOpenOrders(OpenOrdersRequest request)
      {
         try
         {
            await _portfolioRepository.GetCoins();
            _logger.LogInformation("*********** Getting Open Orders for {Venue}", request.Venue);
       //     _orderProcessor.GetOpenOrdersDirectUI(request);
            return Ok();
         }
         catch(Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
                   "Error getting open orders ");
         }
      }

      [HttpPost]
      [Route("neworder/create")]
      public async Task<ActionResult> CreateMarketBuyOrder(OrderDetails newOrder)
      {
         try
         {
            if (newOrder == null)
               return BadRequest();
            // Need to log into the venue as a client
            var res = await _portfolioRepository.GetStrategyExchangeConfigEntry(newOrder.InstanceName);
            if (res == null)
            {
               return BadRequest($"No Entry for ConfigName = {newOrder.InstanceName}");
            }

            var exch = res.ExchangeDetails.Where(v => v.Venue.VenueName.Equals(newOrder.Venue)).ToList().FirstOrDefault();
            if (exch == null)
               return BadRequest($"No Entry for Venue = {newOrder.Venue}");

            _orderQueue.Enqueue(newOrder);
            LoginToVenue(newOrder.PortfolioName, newOrder.InstanceName, exch);

            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Creating Market Order {Error}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating Market Order ");
         }
      }

      private void LoginToVenue(string portfolioName, string instanceName, ExchangeDetailsDTO exchange)
      {
     
         MessageBusCommand msgPrivate = new MessageBusCommand()
         {
            AccountName = portfolioName,
            InstanceName = instanceName,
            CommandType = CommandTypesEnum.CONNECT_PRIVATE,
         };

         var apiKeys = exchange.ApiKey;
         PrivateConnectionLogon logon = new PrivateConnectionLogon()
         {
            SPName = portfolioName,
            ConfigInstance = instanceName,
            ApiKey = apiKeys.Key,
            PassPhrase = apiKeys.PassPhrase,
            Secret = apiKeys.Secret,
         };

         try
         {
            var data = JsonSerializer.Serialize(logon);
            msgPrivate.Data = data;
            var bytesRef = MessageBusCommand.ProtoSerialize(msgPrivate);
            _messageBroker.PublishToSubject(exchange.Venue.VenueName, bytesRef);
         }
         catch(Exception e)
         {
            _logger.LogError(e, "Error in sending Login command {Error}", e.Message);
         }

      }

      [HttpPost]
      [Route("Balances/get")]
      public async Task<ActionResult> GetBalancesFromVenue(GetBalanceRequest request)
      {
         try
         {
            _inventoryManager.GetOpeningBalancesDirect(request.Venue, request.InstanceName, request.PortfolioName);
           
            return Ok();
         }
         catch (Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting open orders ");
         }
      }
   }
}
