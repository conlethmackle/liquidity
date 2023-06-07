using Common.Messages;
using Common.Models;
using OrderAndTradeProcessing;
using System;
using System.Timers;


namespace BlazorLiquidity.Server.OrdersAndTrades
{
  
   public class OrderPlacementHolderUI : IOrderPlacementHolder
   {
      public int ClientOid { get; set; }
      private System.Timers.Timer _placeTimer { get; set; }
      private System.Timers.Timer _cancelTimer { get; set; }
      private IOrderAndTradeProcessing _orderAndTradeProcessing { get; set; }
      public OrderPlacementState OrderState { get; set; }
      public string ExchangeOrderId { get; set; }
      public OwnOrderChange Order { get; set; }
      public string Venue { get; set; }
      public OrderPlacementHolderUI(IOrderAndTradeProcessing orderAndTradeProcessor)
      {
         _orderAndTradeProcessing = orderAndTradeProcessor;
      }

      public void Init(IOrderAndTradeProcessing orderAndTradeProcessing)
      {
         _orderAndTradeProcessing = orderAndTradeProcessing;
      }

      public void StartPlaceOrderTimer()
      {
         _placeTimer = new System.Timers.Timer();
         _placeTimer.Interval = 60000; // TODO
         _placeTimer.Elapsed +=  placeTimer_Elapsed;
         _placeTimer.Start();
      }

      public void StartCancelTimer()
      {
         _cancelTimer = new System.Timers.Timer();
         _cancelTimer.Interval = 60000; // TODO
         _cancelTimer.Elapsed += cancelTimer_Elapsed; ;
      }

      public void CancelPlaceOrderTimer()
      {
         _placeTimer.Stop();
         _placeTimer.Dispose();
      }

      public void CancelCancelOrderTimer()
      {
         _cancelTimer.Stop();
         _cancelTimer.Dispose();
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
