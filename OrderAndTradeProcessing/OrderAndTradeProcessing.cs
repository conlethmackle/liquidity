using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Common.Messages;
using Common.Models;
using MessageBroker;
using Microsoft.Extensions.Logging;
using Common.Extensions;
using System.Linq;
using System.Threading.Tasks;
using OrderLedger;
using Common;
using BlazorLiquidity.Shared;
using Common.Models.DTOs;
using Common.Models.Entities;
using DataStore;
using CustomerIdAllocation;

namespace OrderAndTradeProcessing
{
   public interface IOrderAndTradeProcessing
   {
      event Action<OwnOrderChange, string, string> OnPlaceOrderFailure;
      event Action<OwnOrderChange, string, string> OnCancelOrderFailure;
      event Action <string> OnCancelAllOrderSuccess;
      event Action<string, List<OwnOrderChange>> OnOpenOrdersResponse;
      event Action<string, OwnOrderChange> OnNewOrder;
      event Action<string, OwnOrderChange> OnPartiallyFilledOrder;
      event Action<string, OwnOrderChange> OnCancelledOrder;
      event Action<string, OwnOrderChange> OnFilledOrder;
      event Action<string, TradeMsg> OnNewTrade;
      Task Init();
      void OrderUpdate(string venue, OwnOrderChange order);
      void OrderPlacementUpdate(string venue, OrderIdHolder holder);
      void OrderPlacementUpdateError(string venue, PlaceOrderErrorResponse errorRsp);
      void TradeUpdate(string venue, TradeMsg trade);
      int PlaceOrder(string venue, string symbol, decimal price, decimal quantity, TimeInForceEnum tif, bool isBuy, OrderTypeEnum orderType);
      void PlaceOrderUI(OrderDetails order);
      void CancelOrder(string venue,  string symbol, string customerOrderId, string exchangeOrderId);
      void ModifyOrder(string venue, string symbol, decimal price, decimal quantity);
      void CancelAllOrders();
      void CancelAllOrdersFromVenue(string venue);
      void GetOpenOrdersDirect(string venue, string[] coinPairs);
      void GetOpenOrdersDirectUI(OpenOrdersRequest request);
      Dictionary<string, List<OwnOrderChange>> GetOpenOrders();
      int GetOpenOrdersCount();
      List<OwnOrderChange> GetOpenOrdersFromVenue(string venue);
      List<OwnOrderChange> GetFilledOrders();
      List<OwnOrderChange> GetFilledOrdersFromVenue(string venue);
      Dictionary<string, List<OwnOrderChange>> GetCancelledOrders();
      List<OwnOrderChange> GetCancelledOrdersFromVenue(string venue);
      void OpenOrdersQueryResponse(string venue, List<OrderQueryResponse> orders);
      void CancelOrderPlacementResponse(string venue, SingleCancelledOrderId orderIdHolder);
      void CancelTimerExpired(int ClientOid, string venue, OwnOrderChange order);
      void CancelOrderError(string venue, CancelOrderResponseError error);
      void PlaceOrderTimerExpired(int ClientOid, string venue, OwnOrderChange order);
      void ConnectorStatusChange(string venue, bool status);
      void ConnectorLoginStatusChange(string venue, bool status);
      List<OwnOrderChange> GetProcessedOpenOrders();
      List<OwnOrderChange> GetProcessedOpenOrdersFromVenue(string venue);
      void OnPrivateLoginStatus(string venue, PrivateClientLoginStatus status);
      List<OwnOrderChange> GetPlacedOpenOrdersFromVenue(string venue);
      void OpenOrdersErrorResponse(string venue, OpenOrderErrorResponse errorResponse);
   }

   public class OrderTradeProcessing : IOrderAndTradeProcessing
   {
      private Dictionary<string, Dictionary<string, IOrderPlacementHolder>> _orderTracker = new Dictionary<string, Dictionary<string, IOrderPlacementHolder>>();
      private Dictionary<string, List<OwnOrderChange>> _cancelledOrdersTracker = new Dictionary<string, List<OwnOrderChange>>();
      private Dictionary<string, List<OwnOrderChange>> _filledOrdersTracker = new Dictionary<string, List<OwnOrderChange>>();
      private Dictionary<string, OrderTrackingStatusPerVenue> _venueOrderState   = new Dictionary<string, OrderTrackingStatusPerVenue>();
      private Dictionary<string, List<TradeMsg>> _tradeTracker = new Dictionary<string, List<TradeMsg>>();
      private Dictionary<string, bool>_openOrderRequestMap = new Dictionary<string, bool>();
      private readonly IMessageBroker _messageBroker;
      private readonly ILogger<OrderTradeProcessing> _logger;
      private readonly IOrderPlacementHolderFactory _orderPlacementHolderFactory;

      private readonly IPortfolioRepository _portfolioRepository;

      private readonly ICustomerIdAllocator _customerIdAllocator;
      //  private readonly IOrderLedgerProcessor _orderLedger;
      private const int PlacedOrderTimeout = 10000; //TODO
      private const int CancelledOrderTimeout = 10000;
      private const int CancelledOrderInitTimeout = 10000;
      private readonly string _account;
      private  List<VenueDTO> _venues = new List<VenueDTO>();
      private List<CoinPairDTO> _coinPairs = new List<CoinPairDTO>();
      private SPDTO _sp ;

      private readonly string _configName;

      public event Action<OwnOrderChange, string, string> OnPlaceOrderFailure;
      public event Action<OwnOrderChange, string, string> OnCancelOrderFailure;
      public event Action<string, List<OwnOrderChange>> OnOpenOrdersResponse;
      public event Action<string> OnCancelAllOrderSuccess;
      public event Action<string, OwnOrderChange> OnNewOrder;
      public event Action<string, OwnOrderChange> OnPartiallyFilledOrder;
      public event Action<string, OwnOrderChange> OnCancelledOrder;
      public event Action<string, OwnOrderChange> OnFilledOrder;
      public event Action<string, TradeMsg> OnNewTrade;

      private object _locker = new object();

      public OrderTradeProcessing(ILoggerFactory loggerFactory, 
                                  IMessageBroker messageBroker,
                                  IPortfolioRepository repository,
                                  //IOrderLedgerProcessor orderLedger,                                  
                                  StrategyStartConfig strategyStart,
                                  IOrderPlacementHolderFactory orderPlacementFactory,
                                  ICustomerIdAllocator customerIdAllocator)
      {
         _logger = loggerFactory.CreateLogger<OrderTradeProcessing>();
      //   _orderLedger = orderLedger;
         _messageBroker = messageBroker;
         _portfolioRepository = repository;
         _account = strategyStart.Account;
         _configName = strategyStart.ConfigName;
         _orderPlacementHolderFactory = orderPlacementFactory;
         _customerIdAllocator = customerIdAllocator;
      }

      public async Task Init()
      {
         _venues = await _portfolioRepository.GetVenues();
         _coinPairs = await _portfolioRepository.GetCoinPairs();
         _sp = await _portfolioRepository.GetPortfolioByName(_account);
      }

      public void OrderUpdate(string venue, OwnOrderChange order)
      {
         lock (_locker)
         {
            switch (order.Type)
            {
               case OwnOrderUpdateStatusEnum.OPEN:
                  _logger.LogInformation("Open  {Side} Order for orderId {OrderId} and Client OrderId {ClientOid}", order.IsBuy ? "Buy" : "Sell", order.OrderId, order.ClientOid);
                  HandleNewOrder(venue, order);
                  WriteOrder(order);
                  break;
               case OwnOrderUpdateStatusEnum.PARTIALY_FILLED:
                  _logger.LogInformation("PARTIALY_FILLED {Side} Order for orderId {OrderId} and Client OrderId {ClientOid}", order.IsBuy ? "Buy" : "Sell", order.OrderId, order.ClientOid);
                  HandlePartiallyFilledOrder(venue, order);
                  WriteOrder(order);
                  break;
               case OwnOrderUpdateStatusEnum.FILLED:
                  _logger.LogInformation("FILLED {Side} Order for orderId {OrderId} and Client OrderId {ClientOid}", order.IsBuy ? "Buy" : "Sell", order.OrderId, order.ClientOid);
                  HandleFilledOrder(venue, order);
                  WriteOrder(order);
                  break;
             
               case OwnOrderUpdateStatusEnum.CANCELED: // Not sure this happens for Binance. Might need to add exceptions
                  _logger.LogInformation("CANCELED Order for orderId {OrderId} and Client OrderId {ClientOid}", order.OrderId, order.ClientOid);
                  HandleCancelledOrder(venue, order);
                  WriteOrder(order);
                  break;
               case OwnOrderUpdateStatusEnum.MODIFIED:
                  HandleModifiedOrder(venue, order);
                  WriteOrder(order);
                  break;
               case OwnOrderUpdateStatusEnum.EXPIRED:
                  HandleExpiredOrder(venue, order);
                  WriteOrder(order);
                  break;
               default:
                  _logger.LogWarning("Unknown Order Type {OrderType}", order.Type);
                  break;
            }
         }
      }

      private void HandleExpiredOrder(string venue, OwnOrderChange order)
      {
         _logger.LogWarning("Expired Order message received for {ClientOid}", order.ClientOid);
      }

      private void HandleModifiedOrder(string venue, OwnOrderChange order)
      {
         throw new NotImplementedException();
      }

      private void HandleCancelledOrder(string venue, OwnOrderChange order)
      {
        // if (!venue.Equals(Constants.BINANCE)) return;
         HandleCancelledOrder(venue, order.ClientOid);         
      }

      private void HandleCancelledOrder(string venue, string clientOid)
      {
         var orderData = GetOrderFromTracker(venue, clientOid);
         if (orderData == null)
         {
            _logger.LogError("No such order Client Id in orders table {ClientOid}", clientOid);
            return;
         }
       
         var currentState = orderData.OrderState;
         if (currentState == OrderPlacementState.CANCELLING)
         {
            orderData.OrderState = OrderPlacementState.CANCELLED;
            orderData.CancelCancelOrderTimer();
            
            CancelOrderInTracker(venue, orderData);
            AddCancelledOrderToCancelledTracker(venue, orderData.Order);
            if (GetCountofCancellingOrders(venue) == 0)
            {
               var status = _venueOrderState[venue];
               status.Status = VenueState.ALL_GOOD;
               OnCancelAllOrderSuccess?.Invoke(venue);
            }

            // _orderLedger.AddOrder(orderData);
         }
         else
         {
            _logger.LogError("HandleCancelledOrder - Incorrect state of order {OrderState}", currentState.ToString());
            //throw new Exception($"HandleCancelledOrder - Incorrect state of order {currentState.ToString()}");
         }
      }

      private void HandleFilledOrder(string venue, OwnOrderChange order)
      {
         _logger.LogInformation("Got a filled order {ClientOid} and {OrderId}", order.ClientOid, order.OrderId);
         var orderData = GetOrderFromTracker(venue, order.ClientOid);
         if (orderData == null)
         {
            // Check for a trade
            if (_tradeTracker.ContainsKey(venue))
            {
               var xxx = _tradeTracker[venue];
               var chosen = xxx.Where(t => t.OrderId.Equals(order.OrderId));
               if (chosen.Any())
               {
                  _logger.LogInformation("{ClientOid} has been subject of a trade", order.OrderId);
                  return;
               }
               else
               {
                  _logger.LogError("No Entry for Client Order Id {ClientOid}", order.OrderId);
                  return;
               }
            }
            _logger.LogError("No Entry for Client Order Id {ClientOid}", order.ClientOid);
            return;
         }
         var currentState = orderData.OrderState;
         if (currentState == OrderPlacementState.OPEN || currentState == OrderPlacementState.ORDER_PARTIALLY_FILLED || currentState == OrderPlacementState.CREATED || currentState == OrderPlacementState.PENDING)
         {
            // Move to the filled table
            orderData.OrderState = OrderPlacementState.ORDER_FILLED;
            AddFilledOrderToFilledTracker(venue, order);
            CancelOrderInTracker(venue, orderData);
      //      _orderLedger.AddOrder(orderData);
         }
         else if (currentState == OrderPlacementState.CANCELLING || currentState == OrderPlacementState.CANCEL_INIT)
         {
            // Got a fill after a cancel was issued on the order
            orderData.OrderState = OrderPlacementState.ORDER_FILLED;
            AddFilledOrderToFilledTracker(venue, order);
            CancelOrderInTracker(venue, orderData);
            if (GetCountofCancellingOrders(venue) == 0)
            {
               var status = _venueOrderState[venue];
               status.Status = VenueState.ALL_GOOD;
               OnCancelAllOrderSuccess?.Invoke(venue);
            }

         }
         else
         {
            _logger.LogError("HandleFilledOrder - Incorrect state of order {OrderState}", currentState.ToString());
           // throw new Exception($"HandleFilledOrder - Incorrect state of order {currentState.ToString()}");
         }
      }


      private void HandlePartiallyFilledOrder(string venue, OwnOrderChange order)
      {
         var orderData = GetOrderFromTracker(venue, order.ClientOid);
         if (orderData != null)
         {
            var currentState = orderData.OrderState;
            if (currentState == OrderPlacementState.OPEN || currentState == OrderPlacementState.ORDER_PARTIALLY_FILLED || currentState == OrderPlacementState.CREATED)
            {
               orderData.OrderState = OrderPlacementState.ORDER_PARTIALLY_FILLED;
               ModifyOrderInTracker(venue, orderData);
               AddFilledOrderToFilledTracker(venue, order); // Not strictly true 
            }
            else if (currentState == OrderPlacementState.CANCELLING || currentState == OrderPlacementState.CANCEL_INIT)
            {
               orderData.OrderState = OrderPlacementState.ORDER_FILLED;
               AddFilledOrderToFilledTracker(venue, order);
               CancelOrderInTracker(venue, orderData);
               if (GetCountofCancellingOrders(venue) == 0)
               {
                  var status = _venueOrderState[venue];
                  status.Status = VenueState.ALL_GOOD;
                  OnCancelAllOrderSuccess?.Invoke(venue);
               }
            }
            else
            {
               _logger.LogError("HandlePartiallyFilledOrder - Incorrect state of order {OrderState}", currentState.ToString());
               //throw new Exception($"HandlePartiallyFilledOrder - Incorrect state of order {currentState.ToString()}");
            }
         }
         else
         {
            _logger.LogWarning("No such order id {ClientOid} - for parially filled order ", order.ClientOid);
            return;
         }
      }

      private void HandleNewOrder(string venue, OwnOrderChange order)
      {
         try
         {
            var orderData = GetOrderFromTracker(venue, order.ClientOid);
            if (orderData == null)
            {
               _logger.LogInformation("Order From UI");
               var holder = _orderPlacementHolderFactory.CreatePlacementHolder(this);
               holder.ClientOid = Int32.Parse(order.ClientOid);
               holder.Venue = venue;
               holder.OrderState = OrderPlacementState.OPEN;
               holder.ExchangeOrderId = order.OrderId;
               AddNewOrderToTracker(venue, holder);
               return;
            }
            orderData.ExchangeOrderId = order.OrderId;
            var currentState = orderData.OrderState;
            // Should be OPEN state
            if (currentState == OrderPlacementState.PENDING || currentState == OrderPlacementState.CREATED || currentState == OrderPlacementState.OPEN)
            {
               _logger.LogInformation("Handled new order - {ClientOid} and {OrderId}", order.ClientOid, order.OrderId);
               orderData.OrderState = OrderPlacementState.OPEN;
               orderData.Order = order;
               orderData.ExchangeOrderId = order.OrderId;
               orderData.CancelPlaceOrderTimer();
               orderData.ExchangeOrderId = order.OrderId;
               ModifyOrderInTracker(venue, orderData);
            }
            else
            {
               _logger.LogError("HandleNewOrder - Incorrect state of order {OrderState}", currentState.ToString());
               //throw new Exception($"HandleNewOrder - Incorrect state of order {currentState.ToString()}");
            }
         }
         catch(Exception e)
         {
            _logger.LogError(e, "Error Handling new order from {Venue} - {OrderId}", venue, order.ClientOid);
            throw;
         }        
      }

      public void OrderPlacementUpdate(string venue, OrderIdHolder holder)
      {
         // Not starting a timer for this - might add one later if need arises
         _logger.LogInformation("OrderPlacementUpdate - Order Placed with {ClientOid} and {OrderId}", holder.ClientOrderId, holder.OrderId);
         try
         {
           

            var orderData = GetOrderFromTracker(venue, holder.ClientOrderId);
            if (orderData == null)
            {
               // Check for a trade
               if (_tradeTracker.ContainsKey(venue))
               {
                  var tradeList = _tradeTracker[venue];
                  var chosen = tradeList.Where(t => t.OrderId.Equals(holder.OrderId));
                  if (chosen.Any())
                  {
                     _logger.LogInformation("{ClientOid} has been subject of a trade", holder.ClientOrderId);
                     return;
                  }
               }
               _logger.LogError("OrderPlacementUpdate - no entry for {ClientOid}", holder.ClientOrderId);
               return;
            }
            var currentState = orderData.OrderState;
            orderData.ExchangeOrderId = holder.OrderId;  
            if (venue.Equals(Constants.BINANCE)) // Seems that don't need the extra step with Binance.net
            {
               orderData.OrderState = OrderPlacementState.OPEN;
               orderData.CancelPlaceOrderTimer();
               ModifyOrderInTracker(venue, orderData);
            }
            else
            {

               // Should be PLACED_RESPONSE state
               if (currentState == OrderPlacementState.CREATED)
               {
                  if (venue.Equals(Constants.BINANCE)) // Seems that don't need the extra step with Binance.net
                  {
                     orderData.OrderState = OrderPlacementState.OPEN;

                  }
                  else
                     orderData.OrderState = OrderPlacementState.PENDING;
                  orderData.CancelPlaceOrderTimer();
                  ModifyOrderInTracker(venue, orderData);
               }
               else
               {
                  // Incorrect State - what's up??
                  _logger.LogError("OrderPlacementUpdate - Incorrect state of order {OrderState}", currentState.ToString());
                  // throw new Exception($"OrderPlacementUpdate - Incorrect state of order {currentState.ToString()}");
               }
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Handling new order from {Venue} - {OrderId}", venue, holder.ClientOrderId);
            throw;
         }
      }

      public void OrderPlacementUpdateError(string venue, PlaceOrderErrorResponse errorRsp)
      {
         lock (_locker)
         {
            try
            {
               var orderData = GetOrderFromTracker(venue, errorRsp.ClientOrderId);
               if (orderData != null)
               {
                  var currentState = orderData.OrderState;


                  orderData.OrderState = OrderPlacementState.FAILED_TO_PLACE_ORDER;
                  _logger.LogError("Order {CustomerOrderId} on {Exchange} has failed to be placed with Error {Error}",
                                    errorRsp.ClientOrderId, venue, errorRsp.ErrorMessage);
                  ModifyOrderInTracker(venue, orderData);
               }
               else
               {
                  _logger.LogError("No Entry for unplaced order {ClientOid}", errorRsp.ClientOrderId);
               }

            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error Handling new order from {Venue} - {OrderId}", venue, errorRsp.ClientOrderId);
               throw;
            }
         }
      }

      public void  TradeUpdate(string venue, TradeMsg trade)
      {
         _logger.LogInformation("************************** Got a Trade **************************** {OrderId}", trade.OrderId);
         if (AddToTradeTracker(venue, trade))
         {
            Task.Run(async () => await WriteToDb(venue, trade));
            OnNewTrade?.Invoke(venue, trade);
         }
      }

      private void WriteOrder(OwnOrderChange ownOrder)
      {
         Task.Run(async () => await WriteOrderToDb(ownOrder));
      }

      private async Task WriteOrderToDb(OwnOrderChange ownOrder)
      {
         try
         {
            var venueId = _venues.Where(x => x.VenueName == ownOrder.Venue).ToList().FirstOrDefault().VenueId;
            var coinPairId = _coinPairs.Where(x => x.Name == ownOrder.Symbol).ToList().FirstOrDefault()?.CoinPairId;
            if (coinPairId is null)
            {
               _logger.LogError("Error in Coin Pair lookup when writing order");
               throw new Exception("Error in Coin Pair lookup when writing order");
            }
            var order = new OrderDTO()
            {
               OrderId = ownOrder.OrderId,
               Price = ownOrder.Price,
               Quantity = ownOrder.Quantity,
               FilledQuantity = ownOrder.FilledQuantity,
               RemainingQuantity = ownOrder.RemainingQuantity,
               OrderTime = ownOrder.OrderTime,
               CoinPairId = (int)coinPairId,
               Instance = _configName,
               Account = _account,
               SPId = _sp.SPId,
               OrderType = ownOrder.OrderType,
               ClientOid = ownOrder.ClientOid,
               Symbol = ownOrder.Symbol,
               VenueId = venueId,
               IsBuy = ownOrder.IsBuy,
               Type =  ownOrder.Type,
               Status = ownOrder.Status,
               Timestamp = ownOrder.Timestamp,
            };

            var res = await _portfolioRepository.AddOrder(order);
         }
         catch (Exception e)
         {
            _logger.LogError("Error in Coin Pair lookup when writing trade");
         }


      }

      private async Task WriteToDb(string venue, TradeMsg tradeMsg)
      {
         try
         {
            var venueId = _venues.Where(x => x.VenueName == venue).ToList().FirstOrDefault().VenueId;
            var coinPairId = _coinPairs.Where(x => x.Name == tradeMsg.Symbol).ToList().FirstOrDefault()?.CoinPairId;
            if (coinPairId is null)
            {
               _logger.LogError("Error in Coin Pair lookup when writing trade");
               throw new Exception("Error in Coin Pair lookup when writing trade");
            }

            if (tradeMsg.FilledQuantity < 0)
            {
               tradeMsg.FilledQuantity = tradeMsg.FilledQuantity * -1;
            }
            var trade = new TradeDTO()
            {
               ExchangeTradeId = tradeMsg.TradeId.ToString(),
               VenueId = venueId,
               CoinPairId = (int)coinPairId,
               OrderId = tradeMsg.OrderId,
               DateCreated = tradeMsg.Timestamp,
               SPId = _sp.SPId,
               InstanceName = _configName,
               IsBuy = tradeMsg.IsBuy,
               Price = tradeMsg.Price,
               Quantity = tradeMsg.FilledQuantity,
               LeaveQuantity = tradeMsg.RemainingQuantity,
               Fee = tradeMsg.Fee
            };

            var res = await _portfolioRepository.AddTrade(trade);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Writing trade to db has failed - {Exception}", e.Message);
         }
        

      }

      private bool AddToTradeTracker(string venue, TradeMsg trade)
      {
         bool isAdded = false;
         if (_tradeTracker.ContainsKey(venue))
         {
            var tradeList = _tradeTracker[venue];
            tradeList.ForEach(t =>
            {
               if (t.TradeId.Equals(trade.TradeId))
               {
                  _logger.LogWarning("Not Adding Trade {TradeId} as already added", trade.TradeId);
                  isAdded = true;
               }
            });
            if (!isAdded)
            {
               tradeList.Add(trade);
               return true;
            }
            else
            {
               _logger.LogWarning("Not Adding Trade {TradeId} as already added", trade.TradeId);
               return false;
            }
         }
         else
         {
            var tradeList = new List<TradeMsg>();
            _tradeTracker.Add(venue, tradeList);
            tradeList.Add(trade);
         }
         _logger.LogInformation("====================== Trade at Price,Quantity {Price},{Quantity} Venue {Venue}", trade.Price, trade.Quantity, venue);
         return true;
      }

      private void AddFilledOrderToFilledTracker(string venue, OwnOrderChange order)
      {
         if (_filledOrdersTracker.ContainsKey(venue))
         {
            var ordersList = _filledOrdersTracker[venue];
            if (ordersList == null)
            {
               ordersList = new List<OwnOrderChange>();
            }
            ordersList.Add(order);
         }
         else
         {
            var ordersList = new List<OwnOrderChange>();
            ordersList.Add(order);
            _filledOrdersTracker.Add(venue, ordersList);
         }
      }

      private void AddCancelledOrderToCancelledTracker(string venue, OwnOrderChange order)
      {
         if (_cancelledOrdersTracker.ContainsKey(venue))
         {
            var ordersList = _cancelledOrdersTracker[venue];
            if (ordersList == null)
            {
               ordersList = new List<OwnOrderChange>();
            }
            ordersList.Add(order);
         }
         else
         {
            var ordersList = new List<OwnOrderChange>();
            ordersList.Add(order);
            _cancelledOrdersTracker.Add(venue, ordersList);
         }
      }

      private bool FindCancelledOrderinCancelledTracker(string venue, string clientOrderId)
      {
         if (_cancelledOrdersTracker.ContainsKey(venue))
         {
            var ordersList = _cancelledOrdersTracker[venue];
            if (ordersList.Where(o => o.ClientOid.Equals(clientOrderId)).ToList().Count > 0)
               return true;
            return false;
         }
         return false;
      }

      private int GetCountofCancellingOrders(string venue)
      {
         lock (_locker)
         {
            var venueOrderHolders = _orderTracker[venue];
            var vals = venueOrderHolders.Values.ToList();
            var count = vals.Where(o => o.OrderState == OrderPlacementState.CANCELLING).ToList().Count;
            return count;
         }
      }


      public int PlaceOrder(string venue, string symbol, decimal price, decimal quantity, TimeInForceEnum tif, bool isBuy, OrderTypeEnum orderType = OrderTypeEnum.LIMIT)
      {
        
            var status = GetVenueStatus(venue);
            if (status != VenueState.ALL_GOOD && status != VenueState.CONNECTOR_UP)
            {
               _logger.LogError("Not connected to {venue} order not being placed - status is {STATUS}", venue, status.ToString());
               return -1;
            }
            var clientOid = _customerIdAllocator.AllocateCustomerId(venue);

            var holder = _orderPlacementHolderFactory.CreatePlacementHolder(this);
            holder.ClientOid = clientOid;
            holder.Venue = venue;
            holder.OrderState = OrderPlacementState.CREATED;          

            MessageBusCommand msg = new MessageBusCommand();
            msg.CommandType = CommandTypesEnum.PLACE_ORDER;
            msg.Exchange = venue;
            msg.AccountName = _account;
            msg.IsPrivate = true;
            msg.InstanceName = _configName;

            var order = new PlaceOrderCmd();
            order.Symbol = symbol;
            order.ClientOrderId = clientOid.ToString();
            order.Price = price;
            order.Quantity = quantity;
            order.IsBuy = isBuy;
            order.OrderType = orderType;
            order.TimeInForce = tif;

            var data = JsonSerializer.Serialize(order);
            msg.Data = data;

            var orderChange = order.Convert();
            holder.Order = orderChange;

            var bytes = MessageBusCommand.ProtoSerialize(msg);
            _messageBroker.PublishToSubject(venue, bytes);
            // Stick this into the table 
            holder.StartPlaceOrderTimer();
            AddNewOrderToTracker(venue, holder);
            return clientOid;
         
      }

      public void PlaceOrderUI(OrderDetails order)
      {

      }
      public void GetOpenOrdersDirect(string venue, string[] symbols)
      {
         SetOpenOrderRequestStatus(venue, true);
         var coinPairs = String.Join(",", symbols);
         JsonSerializer.Serialize(coinPairs);
         MessageBusCommand msg = new MessageBusCommand()
         {
            CommandType = CommandTypesEnum.GET_OPEN_ORDERS,
            Exchange = venue,
            Data = coinPairs,
            AccountName = _account,
            InstanceName = _configName,
            IsPrivate = true,
         };
         var bytes = MessageBusCommand.ProtoSerialize(msg);
         _messageBroker.PublishToSubject(venue, bytes);
      }

      private void SetOpenOrderRequestStatus(string venue, bool status)
      {
          if (_openOrderRequestMap.ContainsKey(venue))
          {
            _openOrderRequestMap[venue] = status;
          }
          else
          {
            _openOrderRequestMap.Add(venue, status);
          }
      }

      private bool CheckOpenOrderRequestStatus(string venue)
      {
          if (_openOrderRequestMap.ContainsKey(venue))
            return _openOrderRequestMap[venue];
          else
          {
            _openOrderRequestMap.Add(venue, false);
            return false;
          }
      }

      private void OrderTimeoutsCallback(object state)
      {
         var orderPlacement = (OrderPlacementHolder)state;
      }

      public void CancelOrder(string venue, string symbol, string customerOrderId, string exchangeOrderId)
      {
         lock (_locker)
         {
            _logger.LogInformation("cancelling Order {CustomerOid} and exchange orderId {ExchangeOrderId}", customerOrderId, exchangeOrderId);
            var order = GetOrderFromTracker(venue, customerOrderId);
            if (order != null)
            {
               if (order.OrderState == OrderPlacementState.OPEN || order.OrderState == OrderPlacementState.PENDING ||
                   order.OrderState == OrderPlacementState.ORDER_PARTIALLY_FILLED)
               {
                  if (order.ExchangeOrderId == null)
                  {
                     _logger.LogInformation("Order not placed at exchange - so not sending a cancel yet");
                     return;
                  }
                  order.OrderState = OrderPlacementState.CANCELLING;

                  order.StartCancelTimer();

                  ModifyOrderInTracker(venue, order);

                  var holder = new OrderIdHolder()
                  {
                     ClientOrderId = customerOrderId,
                     OrderId = exchangeOrderId,
                     Symbol = symbol
                  };

                  MessageBusCommand msg = new MessageBusCommand()
                  {
                     CommandType = CommandTypesEnum.CANCEL_ORDER,
                     Exchange = venue,
                     Data = JsonSerializer.Serialize(holder),
                     AccountName = _account,
                     InstanceName = _configName,
                     IsPrivate = true
                  };
                  _logger.LogInformation("Sending Cancel Order to {Venue}", venue);
                  var bytes = MessageBusCommand.ProtoSerialize(msg);
                  _messageBroker.PublishToSubject(venue, bytes);
               }
            }
         }
      }

      public void ModifyOrder(string venue, string symbol, decimal price, decimal quantity)
      {
         throw new NotImplementedException();
      }

      public void CancelOrderPlacementResponse(string venue, SingleCancelledOrderId orderIdHolder)
      {
         // Cancel the associated timer
         lock (_locker)
         {
            if (_orderTracker.ContainsKey(venue))
            {
               var venueData = _orderTracker[venue];
               if (venueData.ContainsKey(orderIdHolder.ClientOrderId))
               {
                  var orderHolder = venueData[orderIdHolder.ClientOrderId];
                  if (venue.Equals(Constants.BINANCE))
                  {
                     orderHolder.OrderState = OrderPlacementState.CANCELLING;
                     HandleCancelledOrder(venue, orderIdHolder.ClientOrderId);
                     //orderHolder.OrderState = OrderPlacementState.CANCELLED;
                   //  CancelOrderInTracker(venue, orderHolder);
                   //  AddCancelledOrderToCancelledTracker(venue, orderHolder.Order);
                  }
                  else
                  {
                     //  var timer = orderHolder.OrderTimer.Change(Timeout.Infinite, Timeout.Infinite);
                     orderHolder.OrderState = OrderPlacementState.CANCELLING;
                     //   var newTimer = new Timer(OrderTimeoutsCallback, orderHolder, CancelledOrderTimeout, CancelledOrderTimeout);
                     //    orderHolder.OrderTimer = newTimer;
                  }
               }
               else
               {
                  if (FindCancelledOrderinCancelledTracker(venue, orderIdHolder.ClientOrderId))
                     return;
                  else
                    _logger.LogError("CancelOrderPlacementResponse - The Client OrderId {ClientOrderId} does not exist for Exchange {Venue}", orderIdHolder.ClientOrderId, venue);
                  //throw new Exception($"CancelOrderPlacementResponse - The Client OrderId {orderIdHolder.ClientOrderId} does not exist for Exchange {venue}");
               }
            }
            else
            {
               _logger.LogError("CancelOrderPlacementResponse - The Exchange {Venue} does not exist in the order tracker for Client OrderId {ClientOrderId}", venue, orderIdHolder.ClientOrderId);
               //throw new Exception($"CancelOrderPlacementResponse - The Exchange {venue} does not exist in the order tracker for Client OrderId {orderIdHolder.ClientOrderId}");
            }
         }
      }

      private void AddNewOrderToTracker(string venue,  IOrderPlacementHolder order)
      {
         lock (_locker)
         {
            try
            {
               // _logger.LogInformation("Adding order with ClientOid {ClientOid} OrderId {OrderId} as stored in order {OtherOrderId}", order.ClientOid, order.ExchangeOrderId, order?.Order.OrderId);

               var orderId = order.ClientOid.ToString();
               if (_orderTracker.ContainsKey(venue))
               {
                  var orderIdsTable = _orderTracker[venue];
                  if (orderIdsTable.ContainsKey(orderId))
                  {
                     // This shouldn't happen
                     _logger.LogInformation("Order Tracking already contains order {OrderId} for {Venue}", orderId,
                        venue);
                  }
                  else
                     orderIdsTable.Add(orderId, order);
               }
               else
               {
                  var venueEntry = new Dictionary<string, IOrderPlacementHolder>();
                  _orderTracker.Add(venue, venueEntry);
                  venueEntry.Add(orderId, order);
               }
            }
            catch (Exception e)
            {
               _logger.LogInformation("AddNewOrderToTracker - {Error}", e.Message);
            }
         }

      }

      private void ModifyOrderInTracker(string venue,  IOrderPlacementHolder order)
      {
         lock (_locker)
         {
            // _logger.LogInformation("Modifying order with ClientOid {ClientOid} OrderId {OrderId} as stored in order {OtherOrderId}", order.ClientOid, order.ExchangeOrderId, order.Order.OrderId);
            var orderId = order.ClientOid.ToString();
            if (_orderTracker.ContainsKey(venue))
            {
               var exchangeOrderBook = _orderTracker[venue];
               if (exchangeOrderBook.ContainsKey(orderId))
               {
                  var orderIdsEntry = exchangeOrderBook[orderId];
                  // probably should update the entry rather than replace
                  orderIdsEntry = order;
               }
               else
               {
                  _logger.LogError("ModifyOrderInTracker - No Entry for OrderId {OrderId} in orderTracking table ",
                     orderId);
                  throw new Exception($"ModifyOrderInTracker - No Entry for OrderId {orderId} in orderTracking table ");
               }
            }
            else
            {
               _logger.LogError("ModifyOrderInTracker - No Entry for Venue {Venue} in orderTracking table ", venue);
               throw new Exception($"ModifyOrderInTracker - No Entry for Venue {venue} in orderTracking table ");
            }
         }

      }     


      private void CancelOrderInTracker(string venue, IOrderPlacementHolder order)
      {
         lock (_locker)
         {
            var orderId = order.ClientOid.ToString();
            if (_orderTracker.ContainsKey(venue))
            {
               var exchangeOrderBook = _orderTracker[venue];
               if (exchangeOrderBook.ContainsKey(orderId))
               {

                  exchangeOrderBook.Remove(orderId);
               }
               else
               {
                  _logger.LogError("CancelOrderInTracker - No Entry for OrderId {OrderId} in orderTracking table ",
                     orderId);
                  throw new Exception($"CancelOrderInTracker - No Entry for OrderId {orderId} in orderTracking table ");
               }
            }
            else
            {
               _logger.LogError("CancelOrderInTracker - No Entry for Venue {Venue} in orderTracking table ", venue);
               throw new Exception($"CancelOrderInTracker - No Entry for Venue {venue} in orderTracking table ");
            }
         }
      }

      private IOrderPlacementHolder GetOrderFromTracker(string venue,  string orderId)
      {
         lock (_locker)
         {
            if (_orderTracker.ContainsKey(venue))
            {
               var orderIdsTable = _orderTracker[venue];
               if (orderIdsTable.ContainsKey(orderId))
               {
                  // probably should update the entry rather than replace
                  return orderIdsTable[orderId];
               }
               //   else
               //   {
               // _logger.LogError("GetOrderFromTracker - No Entry for OrderId {OrderId} in orderTracking table ", orderId);
               //          throw new Exception($"GetOrderFromTracker - No Entry for OrderId {orderId} in orderTracking table ");
               //  }
            }
            //  else
            //   {
            //    _logger.LogError("GetOrderFromTracker - No Entry for Venue {Venue} in orderTracking table ", venue);
            //        throw new Exception($"GetOrderFromTracker - No Entry for Venue {venue} in orderTracking table ");
            // }

            return null;
         }
      }     

      public void CancelTimerExpired(int ClientOid, string venue, OwnOrderChange order)
      {
         OnCancelOrderFailure?.Invoke(order, ClientOid.ToString(), venue);
      }

      public void PlaceOrderTimerExpired(int ClientOid, string venue, OwnOrderChange order)
      {
         OnPlaceOrderFailure?.Invoke(order, ClientOid.ToString(), venue);
      }

      public void CancelAllOrders()
      {
         foreach(var venue in _orderTracker.Keys)
         {
            CancelAllOrdersFromVenue(venue);
         }
      }

      public void CancelAllOrdersFromVenue(string venue)
      {
         lock (_locker)
         {
            if (_orderTracker.ContainsKey(venue))
            {
               if (_venueOrderState.ContainsKey(venue))
               {
                  var status = _venueOrderState[venue];
                  if (status.Status == VenueState.CANCEL_ALL_INFLIGHT)
                  {
                     _logger.LogInformation("Cancellation already in place for {Exchange}", venue);
                     return;
                  }
                 
                  var orders = _orderTracker[venue];

                 foreach (var order in orders.Values)
                  {
                     // Note - need to check, if the order has a non null exchange orderId, if it doesn't then this
                     // order was never placed and does not need cancelling
                     if (!string.IsNullOrEmpty(order.ExchangeOrderId) && order.Order != null)
                        CancelOrder(venue, order.Order.Symbol, order.ClientOid.ToString(), order.ExchangeOrderId);
                     else 
                        orders.Remove(order.ClientOid.ToString());
                  }
               //   if (orders.Count > 0)
                   //  status.Status = VenueState.CANCEL_ALL_INFLIGHT;
               }
            }
         }
      }

      public Dictionary<string, List<OwnOrderChange>> GetOpenOrders()
      {
         lock (_locker)
         {
            var resp = new Dictionary<string, List<OwnOrderChange>>();
            foreach (var venue in _orderTracker.Keys)
            {
               var orders = GetOpenOrdersFromVenue(venue);
               resp[venue] = orders;
            }

            return resp;
         }
      }

      public int GetOpenOrdersCount()
      {
         int count = 0;
         foreach (var venue in _orderTracker.Keys)
         {
            var orderCount = GetOpenOrdersFromVenue(venue).Count();
            count += orderCount;
         }
         return count;
      }

      public List<OwnOrderChange> GetProcessedOpenOrders()
      {
         List<OwnOrderChange> openProcessOrders = new();
         foreach (var venue in _orderTracker.Keys)
         {
            openProcessOrders.AddRange( GetProcessedOpenOrdersFromVenue(venue));
         }

         return openProcessOrders;
      }

      public List<OwnOrderChange> GetProcessedOpenOrdersFromVenue(string venue)
      {
         lock (_locker)
         {
            if (venue == null)
               throw new ArgumentException("GetOpenOrdersFromVenue null venue supplied");
            var emptyList = new List<OwnOrderChange>();
            if (_orderTracker.ContainsKey(venue) && _orderTracker[venue].Count > 0)
            {
               var venueMap = _orderTracker[venue];
               var orders = venueMap.Values.ToList().Where(o => (o.OrderState == OrderPlacementState.OPEN))
                                                     .Select(o => o.Order).ToList();
               return orders;
            }
            return emptyList;
         }
      }
      public List<OwnOrderChange> GetOpenOrdersFromVenue(string venue)
      {
         lock (_locker)
         {
            if (venue == null)
               throw new ArgumentException("GetOpenOrdersFromVenue null venue supplied");
            var emptyList = new List<OwnOrderChange>();
            if (_orderTracker.ContainsKey(venue) && _orderTracker[venue].Count > 0)
            {
               var venueMap = _orderTracker[venue];
               var orders = venueMap.Values.ToList().Where(o => (o.OrderState == OrderPlacementState.OPEN)
                                                     || (o.OrderState == OrderPlacementState.PENDING)
                                                     || (o.OrderState == OrderPlacementState.CREATED)).Select(o => o.Order).ToList();
               return orders;
            }
            return emptyList;
         }
      }

      public List<OwnOrderChange> GetPlacedOpenOrdersFromVenue(string venue)
      {
         lock (_locker)
         {
            if (venue == null)
               throw new ArgumentException("GetOpenOrdersFromVenue null venue supplied");
            var emptyList = new List<OwnOrderChange>();
            if (_orderTracker.ContainsKey(venue) && _orderTracker[venue].Count > 0)
            {
               var venueMap = _orderTracker[venue];
               //   var orders = venueMap.Values.ToList().Where(o => (o.OrderState == OrderPlacementState.OPEN)
               //                                      || (o.OrderState == OrderPlacementState.PENDING) || (o.OrderState == OrderPlacementState.ORDER_PARTIALLY_FILLED)).Select(o => o.Order).ToList();
               var orders = venueMap.Values.ToList().Where(o => (o.OrderState == OrderPlacementState.OPEN || (o.OrderState == OrderPlacementState.ORDER_PARTIALLY_FILLED))).Select(o => o.Order).ToList();
               //                                      || (o.OrderState == OrderPlacementState.PENDING) || (o.OrderState == OrderPlacementState.ORDER_PARTIALLY_FILLED)).Select(o => o.Order).ToList();
               return orders;
            }
            return emptyList;
         }
      }

      public List<OwnOrderChange> GetFilledOrders()
      {
         lock (_locker)
         {
            var resp = new List<OwnOrderChange>();
            foreach (var venue in _filledOrdersTracker.Keys)
            {
               resp.AddRange(GetFilledOrdersFromVenue(venue));
            }

            return resp;
         }
      }

      public List<OwnOrderChange> GetFilledOrdersFromVenue(string venue)
      {
         lock (_locker)
         {
            if (venue == null)
               throw new ArgumentException("GetFilledOrdersFromVenue null venue supplied");
            var emptyList = new List<OwnOrderChange>();
            if (_filledOrdersTracker.ContainsKey(venue) && _orderTracker[venue].Count > 0)
            {
               return _filledOrdersTracker[venue];
            }
            return emptyList;
         }
      }


      public Dictionary<string, List<OwnOrderChange>> GetCancelledOrders()
      {
         lock (_locker)
         {
            var resp = new Dictionary<string, List<OwnOrderChange>>();
            foreach (var venue in _cancelledOrdersTracker.Keys)
            {
               var orders = GetCancelledOrdersFromVenue(venue);
               resp[venue] = orders;
            }

            return resp;
         }
      }

      public List<OwnOrderChange> GetCancelledOrdersFromVenue(string venue)
      {
         lock (_locker)
         {
            if (venue == null)
               throw new ArgumentException("GetCancelledOrdersFromVenue null venue supplied");
            var emptyList = new List<OwnOrderChange>();
            if (_cancelledOrdersTracker.ContainsKey(venue) && _orderTracker[venue].Count > 0)
            {
               return _cancelledOrdersTracker[venue];
            }

            return emptyList;
         }
      }

      public void OpenOrdersQueryResponse(string venue, List<OrderQueryResponse> orders)
      {
         if (!CheckOpenOrderRequestStatus(venue)) return;
         lock (_locker)
         {
            List<OwnOrderChange> openOrders = new List<OwnOrderChange>();
            foreach (var order in orders)
            {
               OwnOrderChange change = new OwnOrderChange()
               {
                  ClientOid = order.CustomerOrderId,
                  FilledQuantity = order.FilledQuantity,
                  IsBuy = order.IsBuy,
                  OrderId = order.Id,
                  OrderTime = order.DateCreated,
                  OrderType = order.Type,
                  Price = order.Price,
                  Quantity = order.Quantity,
                  RemainingQuantity = order.RemainingQuantity, // TODO
                  Symbol = order.Symbol,
                  // Status   = order.
                  // Timestamp = order.
                  // TODO - too many fields not present

               };
               openOrders.Add(change);
               var p = _orderPlacementHolderFactory.CreatePlacementHolder(this);
               p.Venue = venue;
               p.OrderState = OrderPlacementState.OPEN;
               p.Order = change;
               p.ExchangeOrderId = change.OrderId;
               p.ClientOid = Int32.Parse(change.ClientOid);

           
               AddNewOrderToTracker(venue, p);

            }
            OnOpenOrdersResponse?.Invoke(venue, openOrders);
            SetOpenOrderRequestStatus(venue, false);
         }
      }

      public void ConnectorStatusChange(string venue, bool status)
      {
         lock (_locker)
         {

            OrderTrackingStatusPerVenue state = null;
            if (_venueOrderState.ContainsKey(venue))
               state = _venueOrderState[venue];
            else
               state = new OrderTrackingStatusPerVenue();

            if (status)
            {
               state.Status = VenueState.CONNECTOR_UP;
               _logger.LogInformation("{Venue} Connector is up", venue);
            }
            else
            {
               state.Status = VenueState.CONNECTOR_DOWN;
               _logger.LogInformation("{Venue} Connector is down", venue);
            }

            _venueOrderState[venue] = state;
         }
      }

      public void ConnectorLoginStatusChange(string venue, bool status)
      {
         lock (_locker)
         {
            OrderTrackingStatusPerVenue state = null;
            if (_venueOrderState.ContainsKey(venue))
               state = _venueOrderState[venue];
            else
               state = new OrderTrackingStatusPerVenue();

            if (status)
            {
               state.Status = VenueState.ALL_GOOD;
               _logger.LogInformation("Received Successful Login to {Venue}", venue);
            }
            else
            {
               state.Status = VenueState.CONNECTOR_DOWN;
               _logger.LogWarning("Received UnSuccessful Login to {Venue}", venue);
            }

            _venueOrderState[venue] = state;
         }
      }

      public VenueState GetVenueStatus(string venue)
      {
         lock (_locker)
         {
            if (_venueOrderState.ContainsKey(venue))
               return _venueOrderState[venue].Status;
            else
            {
               _logger.LogInformation("Waiting for Login to {Venue}", venue);
               var track = new OrderTrackingStatusPerVenue()
               {
                  Status = VenueState.CONNECTOR_DOWN,
               };
               _venueOrderState[venue] = track;
               return _venueOrderState[venue].Status;

            }
         }
         // _logger.LogError("{Venue} does not exist in the venue order state table", venue);
       //  throw new Exception($"{venue} does not exist in the venue order state table");
      }

      public void CancelOrderError(string venue, CancelOrderResponseError error)
      {
         _logger.LogError("Error in cancelling order from {Venue} with Error Message {Error}", venue, error.Error);
         HandleCancelledOrder(venue, error.ClientOid);
      }

      public void OnPrivateLoginStatus(string venue, PrivateClientLoginStatus status)
      {

      }

      public void GetOpenOrdersDirectUI(OpenOrdersRequest request)
      {
         throw new NotImplementedException();
      }

      public void OpenOrdersErrorResponse(string venue, OpenOrderErrorResponse errorResponse)
      {
         _logger.LogInformation("Sending another request for open orders to {Venue}", venue);
         GetOpenOrdersFromVenue(venue);
      }
   }
}
