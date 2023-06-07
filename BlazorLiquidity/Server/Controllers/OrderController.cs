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
using BlazorLiquidity.Server.StrategyInstanceManager;
using Common.Models;
using Microsoft.AspNetCore.Authorization;

namespace BlazorLiquidity.Server.Controllers
{
   [Route("api/[controller]")]
   [Authorize()]
   [ApiController]
   public class OrderController : ControllerBase
   {
      ILogger<OrderController> _logger;
      private IOrderAndTradeProcessing _orderProcessor;
      protected readonly IMessageBroker _messageBroker;
      protected readonly IMessageReceiver _messageReceiver;
      protected readonly IPortfolioRepository _portfolioRepository;
      protected IInventoryManager _inventoryManager;
      protected readonly IConnectorStatusListener _connectorStatusListener;
      protected readonly IStrategyInstanceManager _instanceManager;
      protected readonly IStrategyInstance _strategyInstance;

      protected readonly Queue<OrderDetails> _orderQueue = new Queue<OrderDetails>();

      public OrderController(ILoggerFactory loggerFactory,
         IStrategyInstanceManager strategyInstanceManager,
         //  IOrderAndTradeProcessing orderProcessor,
         IMessageBroker messageBroker,
         IPortfolioRepository portfolioRepository,
         IMessageReceiver messageReceiver,
         //     IInventoryManager inventoryManager,
         IConnectorStatusListener connectorStatusListener)
      {
         _logger = loggerFactory.CreateLogger<OrderController>();
         _instanceManager = strategyInstanceManager;

         //    _orderProcessor = orderProcessor;
         _portfolioRepository = portfolioRepository;
         _messageReceiver = messageReceiver;
         _messageBroker = messageBroker;
         //     _inventoryManager = inventoryManager;
         _connectorStatusListener = connectorStatusListener;
         _messageReceiver.OnPrivateLoginStatus += OnPrivateLoginStatus;
      }

      private void OnPrivateLoginStatus(string venue, PrivateClientLoginStatus status)
      {
         if (status.IsLoggedIn)
         {
            if (_orderQueue.Count > 0)
            {
               var strategyInstance = _instanceManager.GetStrategyInstance(status.InstanceName);
               var orderProcessor = strategyInstance.GetOrderAndTradeProcessingManager();
               var newOrder = _orderQueue.Dequeue();
               orderProcessor.PlaceOrderUI(newOrder);
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
            var strategyInstance = _instanceManager.GetStrategyInstance(request.InstanceName);
            if (strategyInstance != null)
            {
               var orderProcessor = strategyInstance.GetOrderAndTradeProcessingManager();
               //  await _portfolioRepository.GetCoins();
               _logger.LogInformation("*********** Getting Open Orders for {Venue}", request.Venue);
               orderProcessor.GetOpenOrdersDirectUI(request);
               return Ok();
            }
            else
            {
               return StatusCode(StatusCodes.Status500InternalServerError,
                  "Error getting open orders ");
            }
         }
         catch (Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting open orders ");
         }
      }

      [HttpPost]
      [Route("neworder/create")]
      //[Authorize(Roles = "LiquidityCommands")]
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

            var exch = res.ExchangeDetails.Where(v => v.Venue.VenueName.Equals(newOrder.Venue)).ToList()
               .FirstOrDefault();
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
         catch (Exception e)
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
            var strategyInstance = _instanceManager.GetStrategyInstance(request.InstanceName);
            if (strategyInstance == null) return NotFound();
            var inventoryManager = strategyInstance.GetInventoryManager();
            inventoryManager.GetOpeningBalancesDirect(request.Venue, request.InstanceName, request.PortfolioName);
            return Ok();
         }
         catch (Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting open orders ");
         }
      }

      [HttpGet]
      [Route("connectionStatusAll/get")]
      public async Task<ActionResult<List<ConnectorStatusMsg>>> GetAllConnectorStatuses()
      {
         try
         {
            return Ok(_connectorStatusListener.GetAllConnectorStatuses());
         }
         catch (Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error GetAllConnectorStatuses ");
         }
      }

      [HttpGet]
      [Route("Trade/GetLatest")]
      public async Task<ActionResult<List<TradeDTO>>> GetLatestTrades(string InstanceName)
      {
         try
         {
            var trades = await _portfolioRepository.GetLatestTrades(InstanceName, 15);
            return Ok(trades);
         }
         catch (Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error GetLatestTrades ");
         }
      }

      [HttpGet]
      [Route("Trade/GetAll")]
      public async Task<ActionResult<List<TradeDTO>>> GetAllTrades(int SpId)
      {
         try
         {
            var trades = await _portfolioRepository.GetLatestTradesForSP(SpId);
            return Ok(trades);
         }
         catch (Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error GetLatestTrades ");
         }
      }


      [HttpGet]
      [Route("Trade/GetFills")]
      public async Task<ActionResult<List<FillsInfoForInstance>>> GetFills(string InstanceName)
      {
         try
         {
            var fillList = await _portfolioRepository.GetFillsInfoForInstance(InstanceName);
            return Ok(fillList);
         }
         catch (Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error GetFills ");
         }
      }


      [HttpPost]
      [Route("OrderBooks/GetForVenue")]
      public async Task<ActionResult> GetOrderBooksForVenue(GetOrderBooksData orderBookData)
      {
         try
         {
            _logger.LogInformation("Request to get OrderBook snapshot for {Venue}", orderBookData.Venue);
            GetOrderBooks(orderBookData);
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError("Error caught with OrderBooks/GetForVenue {Error}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error GetOrderBooksForVenue ");
         }
      }

      private void GetOrderBooks(GetOrderBooksData orderBookData)
      {
         try
         {
            MessageBusCommand msgPublic = new MessageBusCommand()
            {
               AccountName = orderBookData.Account,
               InstanceName = orderBookData.Instance,
               CommandType = CommandTypesEnum.GET_ORDERBOOK,
               Data = orderBookData.CoinPairs
            };
            var bytesRef = MessageBusCommand.ProtoSerialize(msgPublic);
            _messageBroker.PublishToSubject(orderBookData.Venue, bytesRef);
         }
         catch (Exception e)
         {
            _logger.LogError(e,"Error in sending GetOrderBooks {Error}", e.Message);
            throw;
         }
      }

      [HttpPost]
      [Route("LastTrade/GetForVenue")]
      public async Task<ActionResult> GetLastTradeFromVenue(GetOrderBooksData orderBookData)
      {
         try
         {
            _logger.LogInformation("*****************************************Request to get Last Trade  for {Venue}", orderBookData.Venue);
            GetLastTrade(orderBookData);
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError("Error caught with LastTrade/GetForVenue {Error}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error GetLastTradeFromVenue ");
         }
      }

      private void GetLastTrade(GetOrderBooksData orderBookData)
      {
         try
         {
            MessageBusCommand msgPublic = new MessageBusCommand()
            {
               AccountName = orderBookData.Account,
               InstanceName = orderBookData.Instance,
               CommandType = CommandTypesEnum.GET_LATEST_TRADES,
               Data = JsonSerializer.Serialize(orderBookData.CoinPairs)
            };
            var bytesRef = MessageBusCommand.ProtoSerialize(msgPublic);
            _messageBroker.PublishToSubject(orderBookData.Venue, bytesRef);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in sending GetOrderBooks {Error}", e.Message);
            throw;
         }
      }

      [HttpGet]
      [Route("Order/GetOrdersForInstance")]
      public async Task<ActionResult<List<TradeDTO>>> GetOrdersForInstance(string InstanceName)
      {
         try
         {
            var orders = await _portfolioRepository.GetOrdersForInstance(InstanceName);
            return Ok(orders);
         }
         catch (Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error GetOrdersForInstance ");
         }
      }

      [HttpGet]
      [Route("Order/GetAll")]
      public async Task<ActionResult<List<OrderDTO>>> GetAllOrders(int SpId)
      {
         try
         {
            var orders = await _portfolioRepository.GetOrdersForSP(SpId);
            return Ok(orders);
         }
         catch (Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error GetAllOrders ");
         }
      }
   }
}
