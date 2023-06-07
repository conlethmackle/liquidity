using Common.Messages;
using Common.Models;
using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderAndTradeProcessing
{
   public enum OrderPlacementState
   {
      CREATED = 1,
      PENDING = 9,
      OPEN = 10,
      CANCEL_INIT = 7,
      CANCELLING = 4,
      CANCELLED = 8,
      ORDER_FILLED = 5,
      ORDER_PARTIALLY_FILLED = 12,
      STALE_PENDING_CANCELLED_ORDER = 6,
      STALE_PENDING_OPEN_ORDER = 11,
      STALE_PENDING_CANCELLING_ORDER = 12,
      FAILED_TO_PLACE_ORDER = 13
   }
   public class OrderPlacementHolder : IOrderPlacementHolder
   {
      public int ClientOid { get; set; }
      private Timer _placeTimer { get; set; }
      private Timer _cancelTimer { get; set; }
      private IOrderAndTradeProcessing _orderAndTradeProcessing { get; set; }
      public OrderPlacementState OrderState { get; set; }
      public string ExchangeOrderId { get; set; }
      public OwnOrderChange Order { get; set; }
      public string Venue { get; set; }
      public OrderPlacementHolder(IOrderAndTradeProcessing orderAndTradeProcessor)
      {
         _orderAndTradeProcessing = orderAndTradeProcessor;
      }

      public void Init(IOrderAndTradeProcessing orderAndTradeProcessing)
      {
         _orderAndTradeProcessing = orderAndTradeProcessing;
      }

      public void StartPlaceOrderTimer()
      {
         _placeTimer = new Timer();
         _placeTimer.Interval = 60000; // TODO
         _placeTimer.Elapsed += placeTimer_Elapsed;
         _placeTimer.Start();
      }

      public void StartCancelTimer()
      {
         _cancelTimer = new Timer();
         _cancelTimer.Interval = 60000; // TODO
         _cancelTimer.Elapsed += cancelTimer_Elapsed; ;
      }

      public void CancelPlaceOrderTimer()
      {
         if (_placeTimer != null)
         {
            _placeTimer?.Stop();
            _placeTimer?.Dispose();
         }
      }

      public void CancelCancelOrderTimer()
      {
         if (_cancelTimer != null)
         {
            _cancelTimer.Stop();
            _cancelTimer.Dispose();
         }
      }

      private void placeTimer_Elapsed(object sender, ElapsedEventArgs e)
      {
         OrderState = OrderPlacementState.STALE_PENDING_OPEN_ORDER;
         _placeTimer?.Stop();
         _orderAndTradeProcessing.PlaceOrderTimerExpired(ClientOid, Venue, Order);
         _placeTimer?.Dispose();
      }


      private void cancelTimer_Elapsed(object sender, ElapsedEventArgs e)
      {
         OrderState = OrderPlacementState.STALE_PENDING_CANCELLED_ORDER;
         _cancelTimer?.Stop();
         _orderAndTradeProcessing.CancelTimerExpired(ClientOid, Venue, Order);
         _cancelTimer?.Dispose();
      }
   }
}
