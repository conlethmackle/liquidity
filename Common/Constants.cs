using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
   public static class Constants
   {
# region RabbitMQSubjects
      public const string KUCOIN = "KuCoin_Exchange";
      public const string BITFINEX = "Bitfinex_Exchange";
      public const string BINANCE = "Binance_Exchange";
      public const string OKX = "Okx_Exchange";
      public const string TESTER = "TestSender";
      public const string SINKS = "Sinks";
      public const string CONNECTOR_COMMAND = "Connector_Command";
      public const string TOPIC_EXCHANGE = "topic_exchange";
      public const string ORDERBOOK_TOPIC = ".orderbooks";
      public const string LAST_TRADED_PRICE_TOPIC = ".last_traded";
      public const string ORDERS_TOPIC = ".orders";
      public const string TRADES_TOPIC = ".trades";
      public const string BALANCES_TOPIC = ".balances";
      public const string STATUS_TOPIC = ".status";
      public const string CONNECTOR_ALIVE_TOPIC = "connectorAlive";
      public const string CONNECTOR_PUBLIC_CONNECTION_TOPIC = "connector_public_connection_topic";
      public const string STRATEGY_CONTROL = "strategy_control";
      public const string STRATEGY_CONTROL_RESPONSE = "strategy_control_response";
      public const string MULTI_STRATEGY_MANAGER = "multiple_strategy_manager";
      public const string STRATEGY_ALIVE_TOPIC = "strategy_alive_topic";
      public const string CONFIG_UPDATE_TOPIC = "config_update_topic";
      public const string SCHEDULE_UPDATE_TOPIC = "schedule_update_topic";
      public const string TELEGRAM_CONFIG_CHANGE_TOPIC = "telegram_config_change_topic";

      public const string TELEGRAM_LIQUIDITY_ALERT = "telegram_lquidity_alert";

      #endregion
   }

   public  static class StrategyNames
   {
      public const string MarketMaking = "MarketMaking";
      public const string Hedging = "Hedging";
      public const string FairValueLiquidation = "FairValueLiquidation";
      public const string MultiVenueFairValueLiquidation = "MultiVenueFairValueLiquidation";
      public const string LiquidationPriceStrategy = "LiquidationPriceStrategy";
   }
}
