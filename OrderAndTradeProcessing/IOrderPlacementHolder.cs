using Common.Models;
using System;

namespace OrderAndTradeProcessing
{
   public interface IOrderPlacementHolder
   {
      void Init(IOrderAndTradeProcessing orderAndTradeProcessing);
      int ClientOid { get; set; }
      string ExchangeOrderId { get; set; }
      OwnOrderChange Order { get; set; }
      OrderPlacementState OrderState { get; set; }
      string Venue { get; set; }

      void CancelCancelOrderTimer();
      void CancelPlaceOrderTimer();
      void StartCancelTimer();
      void StartPlaceOrderTimer();
   }
}