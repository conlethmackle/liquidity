using SyncfusionLiquidity.Server.Receiver;
using SyncfusionLiquidity.Shared;
using Common;
using Common.Extensions;
using Common.Messages;
using Common.Models;
using MessageBroker;
using OrderAndTradeProcessing;
using System.Text.Json;

namespace SyncfusionLiquidity.Server.OrdersAndTrades
{
  

   public class OrdersAndTradeForUI : IOrderAndTradeProcessing
   {
      private Dictionary<string, Dictionary<string, IOrderPlacementHolder>> _orderTracker = new Dictionary<string, Dictionary<string, IOrderPlacementHolder>>();
      private Dictionary<string, List<OwnOrderChange>> _cancelledOrdersTracker = new Dictionary<string, List<OwnOrderChange>>();
      private Dictionary<string, List<OwnOrderChange>> _filledOrdersTracker = new Dictionary<string, List<OwnOrderChange>>();
      private Dictionary<string, OrderTrackingStatusPerVenue> _venueOrderState = new Dictionary<string, OrderTrackingStatusPerVenue>();
      private Dictionary<string, List<TradeMsg>> _tradeTracker = new Dictionary<string, List<TradeMsg>>();
      private Dictionary<string, bool> _openOrderRequestMap = new Dictionary<string, bool>();
      private readonly IMessageBroker _messageBroker;
     
      private readonly ILogger<OrderTradeProcessing> _logger;
      private readonly IOrderPlacementHolderFactory _orderPlacementHolderFactory;
      //  private readonly IOrderLedgerProcessor _orderLedger;
      private const int PlacedOrderTimeout = 10000; //TODO
      private const int CancelledOrderTimeout = 10000;
      private const int CancelledOrderInitTimeout = 10000;
      private readonly string _account;

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

      public OrdersAndTradeForUI(ILoggerFactory loggerFactory,
                                  IMessageBroker messageBroker,
                             //     IPortFolioMessageReceiver messageReceiver,
                                  StrategyStartConfig strategyStart,
                                  IOrderPlacementHolderFactory orderPlacementFactory)
      {
         _logger = loggerFactory.CreateLogger<OrderTradeProcessing>();
         //   _orderLedger = orderLedger;
         _messageBroker = messageBroker;
         _account = strategyStart.Account;
         _configName = strategyStart.ConfigName;
         _orderPlacementHolderFactory = orderPlacementFactory;
         
      }

      public void OnPrivateLoginStatus(string venue, PrivateClientLoginStatus status)
      {
         OrderTrackingStatusPerVenue state = null;
         if (_venueOrderState.ContainsKey(venue))
         {
            state = _venueOrderState[venue];
            state.Status = status.IsLoggedIn ? VenueState.ALL_GOOD : VenueState.CONNECTOR_DOWN;          
         }
         else
         {
            state = new OrderTrackingStatusPerVenue()
            {
               Status = status.IsLoggedIn ? VenueState.ALL_GOOD : VenueState.CONNECTOR_DOWN
            };
            _venueOrderState[venue] = state;
         }
      }

      public void OrderUpdate(string venue, OwnOrderChange order)
      {
         lock (_locker)
         {
            switch (order.Type)
            {
               case OwnOrderUpdateStatusEnum.OPEN:
                  _logger.LogInformation("HandleNewOrder");
                  HandleNewOrder(venue, order);
                  break;
               case OwnOrderUpdateStatusEnum.PARTIALY_FILLED:
                  _logger.LogInformation("HandlePartiallyFilledOrder");
                  HandlePartiallyFilledOrder(venue, order);
                  break;
               case OwnOrderUpdateStatusEnum.FILLED:
                  _logger.LogInformation("HandleFilledOrder");
                  HandleFilledOrder(venue, order);
                  break;
               // case OwnOrderUpdateStatusEnum.MATCHED: // Is this an actual one??
               //    break;
               case OwnOrderUpdateStatusEnum.CANCELED:
                  _logger.LogInformation("HandleCancelledOrder");
                  HandleCancelledOrder(venue, order);
                  break;
               case OwnOrderUpdateStatusEnum.MODIFIED:
                  HandleModifiedOrder(venue, order);
                  break;
               case OwnOrderUpdateStatusEnum.EXPIRED:
                  HandleExpiredOrder(venue, order);
                  break;
               default:
                  _logger.LogInformation("Whoops - got an unhandled one {Type}", order.Type.ToString());
                  break;
            }
         }
      }

      private void HandleExpiredOrder(string venue, OwnOrderChange order)
      {
         
      }

      private void HandleModifiedOrder(string venue, OwnOrderChange order)
      {
         
      }

      private void HandleCancelledOrder(string venue, OwnOrderChange order)
      {
         var orderData = GetOrderFromTracker(venue, order.ClientOid);
         if (orderData != null)
         {
            orderData.OrderState = OrderPlacementState.CANCELLED;
            order.Status = orderData.OrderState.ToString();
            if (orderData == null)
            {
               _logger.LogError("No such order Client Id in orders table {ClientOid}", order.ClientOid);
               return;
            }
            CancelOrderInTracker(venue, orderData);
            OnCancelledOrder?.Invoke(venue, order);
         }
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
         var orderData = GetOrderFromTracker(venue, order.ClientOid);
        
         if (orderData == null)
         {
            // Check for a trade
            if (_tradeTracker.ContainsKey(venue))
            {
               var xxx = _tradeTracker[venue];
               var chosen = xxx.Where(t => t.ClientOid.Equals(order.ClientOid));
               if (chosen.Any())
               {
                  _logger.LogInformation("{ClientOid} has been subject of a trade", order.ClientOid);
                  return;
               }
               else
               {
                  _logger.LogError("No Entry for Client Order Id {ClientOid}", order.ClientOid);
                  return;
               }
               
            }
            _logger.LogError("No Entry for Client Order Id {ClientOid} in tradeTable or OrderTable", order.ClientOid);
            return;
         }
         var currentState = orderData.OrderState;
         if (currentState == OrderPlacementState.OPEN || currentState == OrderPlacementState.ORDER_PARTIALLY_FILLED)
         {
            // Move to the filled table
            orderData.OrderState = OrderPlacementState.ORDER_FILLED;
            order.Status = orderData.OrderState.ToString();
            AddFilledOrderToFilledTracker(venue, order);
            CancelOrderInTracker(venue, orderData);
            OnFilledOrder?.Invoke(venue, order);
            //      _orderLedger.AddOrder(orderData);
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
            if (currentState == OrderPlacementState.OPEN || currentState == OrderPlacementState.ORDER_PARTIALLY_FILLED)
            {
               orderData.OrderState = OrderPlacementState.ORDER_PARTIALLY_FILLED;
               order.Status = orderData.OrderState.ToString();
               ModifyOrderInTracker(venue, orderData);
               AddFilledOrderToFilledTracker(venue, order); // Not strictly true 
               OnPartiallyFilledOrder?.Invoke(venue, order);
            }
            else
            {
               _logger.LogError("HandlePartiallyFilledOrder - Incorrect state of order {OrderState} ClientOid {ClientOid} Exchange OrderId {OrderId}", currentState.ToString(), order.ClientOid, order.OrderId);
              // throw new Exception($"HandlePartiallyFilledOrder - Incorrect state of order {currentState.ToString()}");
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
            var holder = _orderPlacementHolderFactory.CreatePlacementHolder(this);
            holder.Venue = venue;
            holder.OrderState = OrderPlacementState.OPEN;           
            holder.Order = order;
            holder.ExchangeOrderId = order.OrderId;
            order.Venue = venue;
            order.Status = holder.OrderState.ToString();

            if (Guid.TryParse(order.ClientOid, out var guid))
               holder.ClientOid = guid;
            else
            {
               _logger.LogInformation("Error parsing ClientOid - this is in the wrong format - not a GUID {ClientOid}", order.ClientOid);
               return;
            }
          
            // Stick this into the table 
            holder.StartPlaceOrderTimer();
            AddNewOrderToTracker(venue, holder);
            OnNewOrder?.Invoke(venue, order);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Handling new order from {Venue} - {OrderId}", venue, order.ClientOid);
            throw;
         }
      }

      

      public void TradeUpdate(string venue, TradeMsg trade)
      {
         _logger.LogInformation("************************** Got a Trade ****************************");
         AddToTradeTracker(venue, trade);
         OnNewTrade?.Invoke(venue, trade);
      }

      private void AddToTradeTracker(string venue, TradeMsg trade)
      {
         if (_tradeTracker.ContainsKey(venue))
         {
            var tradeList = _tradeTracker[venue];
            tradeList.Add(trade);
         }
         else
         {
            var tradeList = new List<TradeMsg>();
            _tradeTracker.Add(venue, tradeList);
            tradeList.Add(trade);
         }
         _logger.LogInformation("====================== Trade at Price,Quantity {Price},{Quantity} Venue {Venue}", trade.Price, trade.Quantity, venue);


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


      public Guid PlaceOrder(string venue, string symbol, decimal price, decimal quantity, TimeInForceEnum tif, bool isBuy, OrderTypeEnum orderType = OrderTypeEnum.LIMIT)
      {

         var status = GetVenueStatus(venue);
         if (status != VenueState.ALL_GOOD)
         {
            _logger.LogError("Not connected to {venue} order not being placed", venue);
            return Guid.Empty;
         }
         var clientOid = Guid.NewGuid();
         var holder = _orderPlacementHolderFactory.CreatePlacementHolder(this);
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

      public void PlaceOrderUI(OrderDetails placeOrder)
      {
         var status = GetVenueStatus(placeOrder.Venue);
         if (status != VenueState.ALL_GOOD)
         {
            _logger.LogError("Not connected to {venue} order not being placed", placeOrder.Venue);
            return;
         }
         var clientOid = Guid.NewGuid();
         var holder = _orderPlacementHolderFactory.CreatePlacementHolder(this);
         holder.Venue = placeOrder.Venue;
         holder.OrderState = OrderPlacementState.CREATED;

         MessageBusCommand msg = new MessageBusCommand();
         msg.CommandType = CommandTypesEnum.PLACE_ORDER;
         msg.Exchange = placeOrder.Venue;
         msg.AccountName = placeOrder.PortfolioName;
         msg.IsPrivate = true;
         msg.InstanceName = placeOrder.InstanceName;

         var order = new PlaceOrderCmd();
         order.Symbol = placeOrder.Symbol;
         order.ClientOrderId = clientOid.ToString();
        
         order.Quantity = placeOrder.Quantity;
         order.IsBuy = placeOrder.Side;
         order.OrderType = placeOrder.OrderType;
         order.TimeInForce = placeOrder.TimeInForce;

         var data = JsonSerializer.Serialize(order);
         msg.Data = data;

         var orderChange = order.Convert();
         holder.Order = orderChange;

         var bytes = MessageBusCommand.ProtoSerialize(msg);
         _messageBroker.PublishToSubject(placeOrder.Venue, bytes);
         // Stick this into the table 
         holder.StartPlaceOrderTimer();
         AddNewOrderToTracker(placeOrder.Venue, holder);

      }
      public void GetOpenOrdersDirect(string venue)
      {
         SetOpenOrderRequestStatus(venue, true);
         MessageBusCommand msg = new MessageBusCommand()
         {
            CommandType = CommandTypesEnum.GET_OPEN_ORDERS,
            Exchange = venue,
            Data = "",
            AccountName = _account,
            InstanceName = _configName,
            IsPrivate = true,
         };
         var bytes = MessageBusCommand.ProtoSerialize(msg);
         _messageBroker.PublishToSubject(venue, bytes);
      }

      private void OrderTimeoutsCallback(object state)
      {
         var orderPlacement = (OrderPlacementHolder)state;
      }

      public void CancelOrder(string venue, string symbol, string customerOrderId, string exchangeOrderId)
      {
         lock (_locker)
         {
            var order = GetOrderFromTracker(venue, customerOrderId);
            if (order != null)
            {
               if (order.OrderState == OrderPlacementState.OPEN ||
                   order.OrderState == OrderPlacementState.ORDER_PARTIALLY_FILLED)
               {
                  order.OrderState = OrderPlacementState.CANCEL_INIT;

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

            if (venue.Equals(Constants.BINANCE))
            {
               var orderData = GetOrderFromTracker(venue, orderIdHolder.ClientOrderId);
               if (orderData != null)
               {
                  orderData.OrderState = OrderPlacementState.CANCELLED;
                  var order = orderData.Order;
                  order.Status = orderData.OrderState.ToString();
                  if (orderData == null)
                  {
                     _logger.LogError("No such order Client Id in orders table {ClientOid}", order.ClientOid);
                     return;
                  }
                  CancelOrderInTracker(venue, orderData);
                  OnCancelledOrder?.Invoke(venue, order);
               }
            }
            else
            {
               if (_orderTracker.ContainsKey(venue))
               {
                  var venueData = _orderTracker[venue];
                  if (venueData != null)
                  {
                     if (venueData.ContainsKey(orderIdHolder.ClientOrderId))
                     {
                        var orderHolder = venueData[orderIdHolder.ClientOrderId];
                        //  var timer = orderHolder.OrderTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        orderHolder.OrderState = OrderPlacementState.CANCELLING;
                        //   var newTimer = new Timer(OrderTimeoutsCallback, orderHolder, CancelledOrderTimeout, CancelledOrderTimeout);
                        //    orderHolder.OrderTimer = newTimer;
                     }
                     else
                     {
                        _logger.LogError("CancelOrderPlacementResponse - The Client OrderId {ClientOrderId} does not exist for Exchange {Venue}", orderIdHolder.ClientOrderId, venue);
                        //throw new Exception($"CancelOrderPlacementResponse - The Client OrderId {orderIdHolder.ClientOrderId} does not exist for Exchange {venue}");
                     }
                  }
               }
               else
               {
                  _logger.LogError("CancelOrderPlacementResponse - The Exchange {Venue} does not exist in the order tracker for Client OrderId {ClientOrderId}", venue, orderIdHolder.ClientOrderId);
                  throw new Exception($"CancelOrderPlacementResponse - The Exchange {venue} does not exist in the order tracker for Client OrderId {orderIdHolder.ClientOrderId}");
               }
            }
         }
      }

      private void AddNewOrderToTracker(string venue, IOrderPlacementHolder order)
      {
         _logger.LogInformation("Adding order with ClientOid {ClientOid}", order.ClientOid);

         var orderId = order.ClientOid.ToString();
         if (_orderTracker.ContainsKey(venue))
         {
            var orderIdsTable = _orderTracker[venue];
            if (orderIdsTable.ContainsKey(orderId))
            {
               // This shouldn't happen
               _logger.LogError("Order Tracking already contains order {OrderId} for {Venue}", orderId, venue);
               //throw new Exception($"Order Tracking already contains order {orderId} for {venue}");
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

      private void ModifyOrderInTracker(string venue, IOrderPlacementHolder order)
      {

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
               _logger.LogError("ModifyOrderInTracker - No Entry for OrderId {OrderId} in orderTracking table ", orderId);
               throw new Exception($"ModifyOrderInTracker - No Entry for OrderId {orderId} in orderTracking table ");
            }
         }
         else
         {
            _logger.LogError("ModifyOrderInTracker - No Entry for Venue {Venue} in orderTracking table ", venue);
            throw new Exception($"ModifyOrderInTracker - No Entry for Venue {venue} in orderTracking table ");
         }

      }


      private void CancelOrderInTracker(string venue, IOrderPlacementHolder order)
      {
         _logger.LogInformation("Removing order {OrderId} from tracker", order.ClientOid);
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
               _logger.LogError("CancelOrderInTracker - No Entry for OrderId {OrderId} in orderTracking table ", orderId);
               throw new Exception($"CancelOrderInTracker - No Entry for OrderId {orderId} in orderTracking table ");
            }
         }
         else
         {
            _logger.LogError("CancelOrderInTracker - No Entry for Venue {Venue} in orderTracking table ", venue);
            throw new Exception($"CancelOrderInTracker - No Entry for Venue {venue} in orderTracking table ");
         }
      }

      private IOrderPlacementHolder GetOrderFromTracker(string venue, string orderId)
      {

         if (_orderTracker.ContainsKey(venue))
         {
            var orderIdsTable = _orderTracker[venue];
            if (orderIdsTable.ContainsKey(orderId))
            {
               // probably should update the entry rather than replace
               return orderIdsTable[orderId];
            }
            else
            {
               _logger.LogError("GetOrderFromTracker - No Entry for OrderId {OrderId} in orderTracking table ", orderId);
               //          throw new Exception($"GetOrderFromTracker - No Entry for OrderId {orderId} in orderTracking table ");
            }
         }
         else
         {
            _logger.LogError("GetOrderFromTracker - No Entry for Venue {Venue} in orderTracking table ", venue);
            //        throw new Exception($"GetOrderFromTracker - No Entry for Venue {venue} in orderTracking table ");
         }

         return null;
      }

      public void CancelTimerExpired(Guid ClientOid, string venue, OwnOrderChange order)
      {
         OnCancelOrderFailure?.Invoke(order, ClientOid.ToString(), venue);
      }

      public void PlaceOrderTimerExpired(Guid ClientOid, string venue, OwnOrderChange order)
      {
         OnPlaceOrderFailure?.Invoke(order, ClientOid.ToString(), venue);
      }

      public void CancelAllOrders()
      {
         foreach (var venue in _orderTracker.Keys)
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
               var status = _venueOrderState[venue];
               status.Status = VenueState.CANCEL_ALL_INFLIGHT;
               var orders = _orderTracker[venue];
               foreach (var order in orders.Values)
               {
                  CancelOrder(venue, order.Order.Symbol, order.ClientOid.ToString(), order.ExchangeOrderId);
               }
            }
         }
      }

      public Dictionary<string, List<OwnOrderChange>> GetOpenOrders()
      {
         var resp = new Dictionary<string, List<OwnOrderChange>>();
         foreach (var venue in _orderTracker.Keys)
         {
            var orders = GetOpenOrdersFromVenue(venue);
            resp[venue] = orders;
         }
         return resp;
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

      public Dictionary<string, List<OwnOrderChange>> GetFilledOrders()
      {
         var resp = new Dictionary<string, List<OwnOrderChange>>();
         foreach (var venue in _filledOrdersTracker.Keys)
         {
            var orders = GetFilledOrdersFromVenue(venue);
            resp[venue] = orders;
         }
         return resp;
      }

      public List<OwnOrderChange> GetFilledOrdersFromVenue(string venue)
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

      public Dictionary<string, List<OwnOrderChange>> GetCancelledOrders()
      {
         var resp = new Dictionary<string, List<OwnOrderChange>>();
         foreach (var venue in _cancelledOrdersTracker.Keys)
         {
            var orders = GetCancelledOrdersFromVenue(venue);
            resp[venue] = orders;
         }
         return resp;
      }

      public List<OwnOrderChange> GetCancelledOrdersFromVenue(string venue)
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

      public void OpenOrdersQueryResponse(string venue, List<OrderQueryResponse> orders)
      {
         
         if (!CheckOpenOrderRequestStatus(venue)) return;
         _logger.LogInformation("Received Open Orders Response with {Count} orders", orders.Count);
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

               var p = _orderPlacementHolderFactory.CreatePlacementHolder(this);
               p.Venue = venue;
               p.OrderState = OrderPlacementState.OPEN;
               p.Order = change;
               p.ExchangeOrderId = change.OrderId;
               p.ClientOid = Guid.Parse(change.ClientOid);

               AddNewOrderToTracker(venue, p);
               openOrders.Add(change);

            }
            OnOpenOrdersResponse?.Invoke(venue, openOrders);
         }
      }

      public void ConnectorStatusChange(string venue, bool status)
      {
         OrderTrackingStatusPerVenue state = null;
         if (_venueOrderState.ContainsKey(venue))
            state = _venueOrderState[venue];
         else
            state = new OrderTrackingStatusPerVenue();

         if (status)
            state.Status = VenueState.CONNECTOR_UP;
         else
            state.Status = VenueState.CONNECTOR_DOWN;
         _venueOrderState[venue] = state;
      }

      public void ConnectorLoginStatusChange(string venue, bool status)
      {
         OrderTrackingStatusPerVenue state = null;
         if (_venueOrderState.ContainsKey(venue))
            state = _venueOrderState[venue];
         else
            state = new OrderTrackingStatusPerVenue();

         if (status)
            state.Status = VenueState.ALL_GOOD;
         else
            state.Status = VenueState.CONNECTOR_DOWN;
         _venueOrderState[venue] = state;
      }

      public VenueState GetVenueStatus(string venue)
      {
         if (_venueOrderState.ContainsKey(venue))
            return _venueOrderState[venue].Status;
         else
         {
            var newVenue = new OrderTrackingStatusPerVenue()
            {
               Status = VenueState.CONNECTOR_DOWN
            };
            _venueOrderState[venue] = newVenue;
            return newVenue.Status;
         }       
      }

      public void CancelOrderError(string venue, CancelOrderResponseError error)
      {
         _logger.LogError("Error in cancelling order from {Venue} with Error Message {Error}", venue, error.Error);
         HandleCancelledOrder(venue, error.ClientOid);
      }

      public void OrderPlacementUpdate(string venue, OrderIdHolder holder)
      {
         lock (_locker)
         {
            _logger.LogInformation("OrderPlacementUpdate - Order Placed with {ClientOid} and {OrderId}",
               holder.ClientOrderId, holder.OrderId);
            try
            {
               var orderData = GetOrderFromTracker(venue, holder.ClientOrderId);
               if (orderData == null)
               {
                  // Check for a trade
                  if (_tradeTracker.ContainsKey(venue))
                  {
                     var xxx = _tradeTracker[venue];
                     var chosen = xxx.Where(t => t.ClientOid.Equals(holder.ClientOrderId));
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
                     _logger.LogError("OrderPlacementUpdate - Incorrect state of order {OrderState}",
                        currentState.ToString());
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

      public List<OwnOrderChange> GetProcessedOpenOrdersFromVenue(string venue)
      {
         throw new NotImplementedException();
      }

      public void GetOpenOrdersDirectUI(OpenOrdersRequest request)
      {
         SetOpenOrderRequestStatus(request.Venue, true);
         MessageBusCommand msg = new MessageBusCommand()
         {
            CommandType = CommandTypesEnum.GET_OPEN_ORDERS,
            Exchange = request.Venue,
            Data = "",
            AccountName = request.PortfolioName,
            InstanceName = request.InstanceName,
            IsPrivate = true,
         };
         var bytes = MessageBusCommand.ProtoSerialize(msg);
         _messageBroker.PublishToSubject(request.Venue, bytes);
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

      public List<OwnOrderChange> GetPlacedOpenOrdersFromVenue(string venue)
      {
         return new List<OwnOrderChange>();
      }

      public void PlaceOrderUI(BlazorLiquidity.Shared.OrderDetails order)
      {
         throw new NotImplementedException();
      }

      public void GetOpenOrdersDirectUI(BlazorLiquidity.Shared.OpenOrdersRequest request)
      {
         throw new NotImplementedException();
      }
   }

   
}
