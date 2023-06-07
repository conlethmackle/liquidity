using Common.Messages;
using KuCoin.Models;
using System;
using Common.Models;
using CryptoExchange.Net.Converters;

namespace KuCoin.Extensions
{
   public static class CloneForKuCoin
   {
      public static KuCoinOrderPlacement Clone(this PlaceOrderCmd cmd)
      {
         var order = new KuCoinOrderPlacement()
         {
            Hidden = false,
            ClientOid = cmd.ClientOrderId,
            Price = cmd.Price,
            Side = cmd.IsBuy ? "buy" : "sell",
            Quantity = cmd.Quantity,
            Iceberg = cmd.Iceberg,
            VisibleSize = cmd.VisibleSize,
            Symbol = cmd.Symbol,
            TradeType = "TRADE",
         };
         //order.TimeInForce = cmd.TimeInForce.ToString();
         order.TimeInForce = "GTC";
         //order.Type = cmd.OrderType ==  OrderTypeEnum.LIMIT ? "limit" : "market";
         order.Type =  "limit";
         return order;
      }

      public static KuCoinOrderMarketOrderPlacement CloneForMarket(this PlaceOrderCmd cmd)
      {
         var order = new KuCoinOrderMarketOrderPlacement()
         {
            Hidden = false,
            ClientOid = cmd.ClientOrderId,
            Side = cmd.IsBuy ? "buy" : "sell",
            Quantity = cmd.Quantity,
            Iceberg = cmd.Iceberg,
            VisibleSize = cmd.VisibleSize,
            Symbol = cmd.Symbol,
            TradeType = "TRADE",
            
         };
         //order.TimeInForce = cmd.TimeInForce.ToString();
         order.TimeInForce = "IOC";
         //order.Type = cmd.OrderType ==  OrderTypeEnum.LIMIT ? "limit" : "market";
         order.Type = "market";
         return order;
      }

      public static ExchangeBalance Clone(this KuCoinAccount acc)
      {
         return new ExchangeBalance()
         {
            AccountId = acc.Id,
            Available = Decimal.Parse(acc.Available),
            Total = Decimal.Parse(acc.Balance),
            Currency = acc.Currency,
            Hold = Decimal.Parse(acc.Hold),
            Type = acc.AccountType
         };
       
      }

      public static OwnOrderChange Clone(this PrivateOrderChangeEvent o)
      {
         var orderMsg=  new OwnOrderChange()
         {
            ClientOid = o.ClientOid,
            FilledQuantity = Decimal.Parse(o.FilledQuantity),
            OrderId = o.OrderId,
            OrderTime = DateTimeConverter.ConvertFromNanoseconds((long)o.OrderTime),
            OrderType = o.OrderType.Equals("limit") ? OrderTypeEnum.LIMIT : OrderTypeEnum.MARKET, // Need ability to add the other order types 
           // Price = Decimal.Parse(o.Price), // Need to think about this if ever use market orders
            Quantity = Decimal.Parse(o.Quantity),
            RemainingQuantity = Decimal.Parse(o.RemainingQuantity),
            IsBuy = o.Side.Equals("buy") ? true : false,
            Status = o.Status,
            Symbol = o.Symbol,
            Timestamp = DateTimeConverter.ConvertFromNanoseconds((long)o.TS)            
         };

         if (o.Price != null) orderMsg.Price = Decimal.Parse(o.Price);

         if (o.Type.Equals("open"))
            orderMsg.Type = OwnOrderUpdateStatusEnum.OPEN;
         else if (o.Type.Equals("filled") && Decimal.Parse(o.RemainingQuantity) != 0)
            orderMsg.Type = OwnOrderUpdateStatusEnum.PARTIALY_FILLED;
         else if (o.Type.Equals("match") && Decimal.Parse(o.RemainingQuantity) == 0)
            orderMsg.Type = OwnOrderUpdateStatusEnum.FILLED;
         else if (o.Type.Equals("canceled"))
            orderMsg.Type = OwnOrderUpdateStatusEnum.CANCELED;
         else if (o.Type.Equals("update"))
            orderMsg.Type = OwnOrderUpdateStatusEnum.MODIFIED;

         return orderMsg;
      }

      public static TradeMsg CreateTradeMsg (this PrivateOrderChangeEvent e)
      {
         var tradeMsg = new TradeMsg()
         {
            ClientOid = e.ClientOid,
            FilledQuantity = Decimal.Parse(e.FilledQuantity),
            IsBuy = e.Side.Equals("buy") ? true : false,
            IsTaker = e.Liquidity.Equals("taker") ? true : false,
            
            Quantity = Decimal.Parse(e.Quantity),
            MatchedPrice = e.MatchedPrice,
            MatchedSize = e.MatchedSize,
            OrderId = e.OrderId,
            OrderTime = DateTimeConverter.ConvertFromNanoseconds((long)e.OrderTime) ,
            OrderType = e.OrderType.Equals("limit") ? OrderTypeEnum.LIMIT : OrderTypeEnum.MARKET,
            RemainingQuantity = Decimal.Parse(e.RemainingQuantity),
            Status = e.Status,
            Symbol = e.Symbol,
            Timestamp = DateTimeConverter.ConvertFromNanoseconds((long)e.TS),
            Type = OwnOrderUpdateStatusEnum.MATCHED,
            TradeId = e.TradeId
            
         };

         if (e.Price != null)
            tradeMsg.Price = Decimal.Parse(e.Price);
       
         return tradeMsg;
      }

      public static ExchangeBalance Clone(this AccountBalanceNotice b)
      {
         return new ExchangeBalance()
         {
            AccountId = b.AccountId,
            Available = Decimal.Parse(b.Available),
            Currency = b.Currency,
            Hold = Decimal.Parse(b.Hold),
            Total = Decimal.Parse(b.Total)
         };       
      }

      public static OrderQueryResponse Clone(this KuCoinOrderQueryResponse q)
      {
         var o = new OrderQueryResponse()
         {
            CustomerOrderId = q.ClientOid,
            Id  = q.Id, 
            IsBuy = q.Side.Equals("buy") ? true : false,
            Symbol = q.Symbol,
            Type  = q.Type.Equals("limit") ? OrderTypeEnum.LIMIT : OrderTypeEnum.MARKET,
            IsOpen = q.IsActive,
            Price = q.Price, 
            Quantity = q.Quantity,              
            Hidden = q.Hidden,     
            Iceberg = q.Iceberg,     
            VisibleSize = q.VisibleSize,    
            DateCreated = DateTimeConverter.ConvertFromNanoseconds((long)q.CreatedAt)
         };
         if (q.TimeInForce.Equals("GTC"))
            o.TimeInForce = TimeInForceEnum.GTC;
         else if (q.TimeInForce.Equals("GTT"))
            o.TimeInForce = TimeInForceEnum.GTT;
         else if (q.TimeInForce.Equals("IOC"))
            o.TimeInForce = TimeInForceEnum.IOC;
         else if (q.TimeInForce.Equals("FOK"))
            o.TimeInForce = TimeInForceEnum.FOK;

         return o;
      }

      public static TickerReferenceData Clone(this KuCoinCoinPairRefData r)
      {
         return new TickerReferenceData()
         {
            Symbol = r.Symbol,
            EnableTrading = r.EnableTrading,
            FeeCurrency = r.FeeCurrency,
            MaxQuantity = r.BaseMaxSize,
            MinQuantity = r.BaseMinSize,
            PriceIncrement = r.PriceIncrement,
            QuantityIncrement = r.BaseIncrement            
         };
      }
   }
}
