using Binance.Models;
using Binance.Net.Objects.Models.Spot.Socket;
using Binance.Net.Enums;
using Common.Messages;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binance.Net.Objects.Models.Spot;
using CryptoExchange.Net.Converters;

namespace Binance.Extensions
{
   public static class CloneForBinance
   {
      public static BinanceSpotOrderPlacement CloneForLimit(this PlaceOrderCmd cmd)
      {
         var order = new BinanceSpotOrderPlacement()
         {
            
            CustomerOrderId = cmd.ClientOrderId,
            Price = cmd.Price,
            Side = cmd.IsBuy ? OrderSide.Buy : OrderSide.Sell,
            Quantity = cmd.Quantity,
            Symbol = cmd.Symbol,
            OrderType = SpotOrderType.Limit            
         };

         switch(cmd.TimeInForce)
         {
            case TimeInForceEnum.GTC:
               order.Tif = TimeInForce.GoodTillCanceled;
               break;
            case TimeInForceEnum.IOC:
               order.Tif = TimeInForce.ImmediateOrCancel;
               break;
            case TimeInForceEnum.FOK:
               order.Tif = TimeInForce.FillOrKill;
               break;
            default:
               // Might need to get a logger in here
               order.Tif = TimeInForce.GoodTillCanceled;
               break;
         }
         return order;
      }

      public static BinanceSpotOrderPlacement CloneForMarket(this PlaceOrderCmd cmd)
      {
         var order = new BinanceSpotOrderPlacement()
         {

            CustomerOrderId = cmd.ClientOrderId,
            Price = cmd.Price,
            Side = cmd.IsBuy ? OrderSide.Buy : OrderSide.Sell,
            Quantity = cmd.Quantity,
            Symbol = cmd.Symbol,
            OrderType = SpotOrderType.Market
         };

         switch (cmd.TimeInForce)
         {
            case TimeInForceEnum.GTC:
               order.Tif = TimeInForce.GoodTillCanceled;
               break;
            case TimeInForceEnum.IOC:
               order.Tif = TimeInForce.ImmediateOrCancel;
               break;
            case TimeInForceEnum.FOK:
               order.Tif = TimeInForce.FillOrKill;
               break;
            default:
               // Might need to get a logger in here
               order.Tif = TimeInForce.GoodTillCanceled;
               break;
         }
         return order;
      }

      public static ExchangeBalance Clone(this BinanceStreamBalance b)
      {
         return new ExchangeBalance()
         {
            AccountId = "",
            Available = b.Available,
            Currency = b.Asset,
            Hold = b.Locked,
            Total = b.Total
         };
      }

      public static ExchangeBalance Clone(this BinanceBalance b)
      {
         return new ExchangeBalance()
         {
            AccountId = "",
            Currency = b.Asset,
            Hold = b.Locked,
            Total = b.Total,
            Available = b.Available
         };
      }

      public static OrderQueryResponse Clone(this BinanceOrder q)
      {
         var queryResponse = new OrderQueryResponse()
         {
            CustomerOrderId = q.ClientOrderId, 
            Id  = q.Id.ToString(),
            Symbol = q.Symbol,           
            IsOpen = (bool)q.IsWorking,
            IsBuy = q.Side == OrderSide.Buy ? true : false,
            Price = q.Price,
            Quantity = q.Quantity,
            Hidden = false,   // TODO 
            Iceberg = q.IcebergQuantity > 0? true : false,
            VisibleSize = q.IcebergQuantity ?? 0,
            DateCreated = q.CreateTime,
            
         };

         switch(q.Type)
         {
            case SpotOrderType.Limit:
               queryResponse.Type = OrderTypeEnum.LIMIT;
               break;
            case SpotOrderType.Market:
               queryResponse.Type = OrderTypeEnum.MARKET;
               break;
            case SpotOrderType.TakeProfit:
               queryResponse.Type = OrderTypeEnum.TAKEPROFIT;
               break;
            case SpotOrderType.LimitMaker:
               queryResponse.Type = OrderTypeEnum.LIMITMAKER;
               break;
            case SpotOrderType.StopLoss:
               queryResponse.Type = OrderTypeEnum.STOPLOSS;
               break;
            case SpotOrderType.TakeProfitLimit:
               queryResponse.Type = OrderTypeEnum.TAKEPROFITLIMIT;
               break;
            case SpotOrderType.StopLossLimit:
               queryResponse.Type = OrderTypeEnum.STOPLOSSLIMIT;
               break;
         }

         switch(q.TimeInForce)
         {
            case TimeInForce.FillOrKill:
               queryResponse.TimeInForce = TimeInForceEnum.FOK;
               break;
            case TimeInForce.GoodTillCanceled:
               queryResponse.TimeInForce = TimeInForceEnum.GTC;
               break;
            case TimeInForce.GoodTillCrossing:
               // Will this ever be used???
               break;
            case TimeInForce.GoodTillExpiredOrCanceled:
               queryResponse.TimeInForce = TimeInForceEnum.GTT;
               break;
            case TimeInForce.ImmediateOrCancel:
               queryResponse.TimeInForce = TimeInForceEnum.IOC;
               break;
         }
         return queryResponse;
      }

      public static OwnOrderChange Clone(this BinanceStreamOrderUpdate u)
      {
         var order = new OwnOrderChange()
         {
            OrderId = u.Id.ToString(),
            ClientOid = u.ClientOrderId,
            Price = u.Price,
            Quantity = u.Quantity,
           
            IsBuy = u.Side == Net.Enums.OrderSide.Buy ? true : false,
            OrderTime = u.CreateTime,
            Timestamp = u.EventTime,
            RemainingQuantity = u.Quantity - u.QuantityFilled,
            FilledQuantity = u.QuantityFilled,
            Symbol = u.Symbol,
         };

         switch(u.Type)
         {
            case Net.Enums.SpotOrderType.Limit:
               order.OrderType = OrderTypeEnum.LIMIT;
               break;
            case Net.Enums.SpotOrderType.LimitMaker:
               order.OrderType = OrderTypeEnum.LIMITMAKER;
               break;
            case Net.Enums.SpotOrderType.Market:
               order.OrderType = OrderTypeEnum.MARKET;
               break;
            case Net.Enums.SpotOrderType.StopLoss:
               order.OrderType = OrderTypeEnum.STOPLOSS;
               break;
            case Net.Enums.SpotOrderType.StopLossLimit:
               order.OrderType = OrderTypeEnum.STOPLOSSLIMIT;
               break;
            case Net.Enums.SpotOrderType.TakeProfit:
               order.OrderType = OrderTypeEnum.TAKEPROFIT;
               break;
            case Net.Enums.SpotOrderType.TakeProfitLimit:
               order.OrderType = OrderTypeEnum.TAKEPROFITLIMIT;
               break;
         }

         switch (u.Status)
         {
            case Net.Enums.OrderStatus.New:
               order.Type = OwnOrderUpdateStatusEnum.OPEN;              
               break;
            case Net.Enums.OrderStatus.PartiallyFilled:
               order.Type = OwnOrderUpdateStatusEnum.PARTIALY_FILLED;
               break;
            case Net.Enums.OrderStatus.Filled:
               order.Type = OwnOrderUpdateStatusEnum.FILLED;
               break;
            case Net.Enums.OrderStatus.Canceled:
               order.ClientOid = u.OriginalClientOrderId;
               order.Type = OwnOrderUpdateStatusEnum.CANCELED;
               break;
            case Net.Enums.OrderStatus.Expired:
               order.Type = OwnOrderUpdateStatusEnum.EXPIRED;
               break;          
         }        
         return order;
      }

      public static TradeMsg CreateTradeMsg(this BinanceStreamOrderUpdate u)
      {
         var tradeMsg = new TradeMsg()
         {
            OrderId = u.Id.ToString(),
            ClientOid = u.ClientOrderId.ToString(),
            FilledQuantity = u.QuantityFilled,
            IsBuy = u.Side == Net.Enums.OrderSide.Buy ? true : false,
            IsTaker = u.BuyerIsMaker,
            Price = u.LastPriceFilled,
            Quantity = u.LastQuantityFilled,          
            OrderTime = u.CreateTime,
            //OrderType = u.OrderType.Equals("limit") ? OrderTypeEnum.LIMIT : OrderTypeEnum.MARKET,
            RemainingQuantity = u.Quantity - u.LastQuantityFilled,
            
            Symbol = u.Symbol,
            Timestamp = u.EventTime,
            Type = OwnOrderUpdateStatusEnum.MATCHED,
            TradeId = u.TradeId.ToString()
         };
         

         switch (u.Type)
         {
            case Net.Enums.SpotOrderType.Limit:
               tradeMsg.OrderType = OrderTypeEnum.LIMIT;
               break;
            case Net.Enums.SpotOrderType.LimitMaker:
               tradeMsg.OrderType = OrderTypeEnum.LIMITMAKER;
               break;
            case Net.Enums.SpotOrderType.Market:
               tradeMsg.OrderType = OrderTypeEnum.MARKET;
               break;
            case Net.Enums.SpotOrderType.StopLoss:
               tradeMsg.OrderType = OrderTypeEnum.STOPLOSS;
               break;
            case Net.Enums.SpotOrderType.StopLossLimit:
               tradeMsg.OrderType = OrderTypeEnum.STOPLOSSLIMIT;
               break;
            case Net.Enums.SpotOrderType.TakeProfit:
               tradeMsg.OrderType = OrderTypeEnum.TAKEPROFIT;
               break;
            case Net.Enums.SpotOrderType.TakeProfitLimit:
               tradeMsg.OrderType = OrderTypeEnum.TAKEPROFITLIMIT;
               break;
         }
         
         return tradeMsg;
      }

      public static OrderBookSnapshot Clone(this BinanceOrderBook binanceBook)
      {
         var orderbook = new OrderBook();
         var bids = binanceBook.Bids.ToList();
         var asks = binanceBook.Asks.ToList();
         asks.ForEach(a => orderbook.Ask.Add(new Level(a.Price, a.Quantity)));
         bids.ForEach(a => orderbook.Bid.Add(new Level(a.Price, a.Quantity)));
         var orderBookSnapshot = new OrderBookSnapshot()
         {
            Symbol = binanceBook.Symbol,
            OrderBook = orderbook
         };
         return orderBookSnapshot;
      }
   }
}
