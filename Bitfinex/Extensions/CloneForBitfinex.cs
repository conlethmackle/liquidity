using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitfinex.Models;
using Bitfinex.Net.Enums;
using Bitfinex.Net.Objects.Models;
using Common.Messages;
using Common.Models;

namespace Bitfinex.Extensions
{
   public static class CloneForBitfinex
   {
      public static OrderQueryResponse Clone(this BitfinexOrder q)
      {

         var queryResponse = new OrderQueryResponse()
         {
            CustomerOrderId = q.ClientOrderId.ToString(),
            Id = q.Id.ToString(),
            Symbol = q.Symbol,
            IsOpen = (q.Status == OrderStatus.Active || q.Status == OrderStatus.PartiallyFilled) ? true : false,
            IsBuy = q.Side == OrderSide.Buy ? true : false,
            Price = q.Price,
            Quantity = q.Quantity,
            DateCreated = q.CreateTime,
         };

         switch (q.Type)
         {
            case OrderType.ExchangeLimit:
               queryResponse.Type = OrderTypeEnum.LIMIT;
               queryResponse.TimeInForce = TimeInForceEnum.GTC;
               break;
            case OrderType.ExchangeMarket:
               queryResponse.Type = OrderTypeEnum.MARKET;
               queryResponse.TimeInForce = TimeInForceEnum.IOC;
               break;
            case OrderType.ExchangeStopLimit:
               queryResponse.Type = OrderTypeEnum.STOPLOSS;
               queryResponse.TimeInForce = TimeInForceEnum.GTC;
               break;

            case OrderType.ExchangeFillOrKill:
               queryResponse.Type = OrderTypeEnum.FILLORKILL;
               queryResponse.TimeInForce = TimeInForceEnum.FOK;
               break;
         }

         return queryResponse;
      }

      public static BitfinexSpotOrderPlacement CloneForMarket(this PlaceOrderCmd cmd)
      {
         var order = new BitfinexSpotOrderPlacement()
         {
            CustomerOrderId = cmd.ClientOrderId,
            Price = cmd.Price,
            Side = cmd.IsBuy ? OrderSide.Buy : OrderSide.Sell,
            Quantity = cmd.Quantity,
            Symbol = cmd.Symbol,
            OrderType = OrderType.ExchangeMarket
         };
         return order;
      }

      public static BitfinexSpotOrderPlacement CloneForLimit(this PlaceOrderCmd cmd)
      {
         var order = new BitfinexSpotOrderPlacement()
         {
            CustomerOrderId = cmd.ClientOrderId,
            Price = cmd.Price,
            Side = cmd.IsBuy ? OrderSide.Buy : OrderSide.Sell,
            Quantity = cmd.Quantity,
            Symbol = cmd.Symbol,
            OrderType = OrderType.ExchangeLimit
         };
         return order;
      }

      public static OwnOrderChange CloneForOwnOrder(this BitfinexOrder u)
      {
         var order = new OwnOrderChange()
         {
            OrderId = u.Id.ToString(),
            ClientOid = u.ClientOrderId.ToString(),
            Price = u.Price,
            Quantity = u.Quantity,
            IsBuy = u.Side == Net.Enums.OrderSide.Buy ? true : false,
            OrderTime = u.CreateTime,
            Timestamp = u.UpdateTime,
            RemainingQuantity = u.Quantity - u.QuantityRemaining,
            FilledQuantity = u.Quantity,
            Symbol = u.Symbol,
         };

         switch (u.Type)
         {
            case OrderType.ExchangeLimit:
            case OrderType.Limit:

               order.OrderType = OrderTypeEnum.LIMIT;
               break;

            case OrderType.Market:
            case OrderType.ExchangeMarket:
               order.OrderType = OrderTypeEnum.MARKET;
               break;
            case OrderType.ExchangeStopLimit:
            case OrderType.StopLimit:
               order.OrderType = OrderTypeEnum.STOPLOSSLIMIT;
               break;
            case OrderType.Stop:
            case OrderType.ExchangeStop:
               order.OrderType = OrderTypeEnum.STOPLOSS;
               break;

         }

         switch (u.Status)
         {
            case OrderStatus.Active:
               order.Type = OwnOrderUpdateStatusEnum.OPEN;
               break;
            case OrderStatus.PartiallyFilled:
               order.Type = OwnOrderUpdateStatusEnum.PARTIALY_FILLED;
               break;
            case OrderStatus.Executed:
               order.Type = OwnOrderUpdateStatusEnum.FILLED;
               break;
            case OrderStatus.Canceled:
               order.ClientOid = u.ClientOrderId.ToString();
               order.Type = OwnOrderUpdateStatusEnum.CANCELED;
               break;
            default:
               throw new Exception($"New Order status type {u.Status.ToString()}");

         }

         return order;
      }

      public static TradeMsg CreateTradeMsg(this BitfinexTradeDetails u)
      {
        
         var tradeMsg = new TradeMsg()
         {
            OrderId = u.OrderId.ToString(),
            FilledQuantity = u.Quantity,
            IsBuy = u.Quantity > 0?true:false,
            // IsBuy = u. == Net.Enums.OrderSide.Buy ? true : false, where the f is the side??????
            IsTaker = u.Maker == true ? false : true,
            Price = u.Price,
            Quantity = u.Quantity,
            OrderTime = u.Timestamp,
            //OrderType = u.OrderType.Equals("limit") ? OrderTypeEnum.LIMIT : OrderTypeEnum.MARKET,
            //RemainingQuantity = u.Quantity - u.LastQuantityFilled,
            Symbol = u.Symbol,
            Timestamp = u.Timestamp,
            Type = OwnOrderUpdateStatusEnum.MATCHED,
            TradeId = u.Id.ToString()
         };
         return tradeMsg;
      }

      public static ExchangeBalance Clone(this BitfinexWallet b)
      {
         var bal = new ExchangeBalance();
         bal.Currency = b.Asset;
         bal.Total = b.Total;
         bal.Hold = 0;
         if (b.Available == null)
            bal.Available = b.Total;
         else
         {
            bal.Available = (decimal)b.Available;
         }
         return bal;

      }
   }
}
