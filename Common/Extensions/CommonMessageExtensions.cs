using Common.Messages;
using Common.Models;
using Common.Models.DTOs;
using Common.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;


namespace Common.Extensions
{
   public static class CommonMessageExtensions
   {
      public static OwnOrderChange Convert(this PlaceOrderCmd cmd)
      {
         var o = new OwnOrderChange()
         {
            ClientOid = cmd.ClientOrderId,
            Price = cmd.Price,
            Quantity = cmd.Quantity,
            IsBuy = cmd.IsBuy,
            Symbol = cmd.Symbol
         };

         return o;
      }

      public static SPDTO Map(this SP sp)
      {
         var bals = new List<BalanceDTO>();
         //var dbBal = sp.Balances.ToList();
         //  dbBal.ForEach(b => bals.Add(b.Map()));

         var exchs = new List<ExchangeDetailsDTO>();
         //  var dbExchs = sp.Exchanges.ToList();
         //  dbExchs.ForEach(e => exchs.Add(e.Map()));
         if (sp == null)
            return new SPDTO();

         var p = new SPDTO()
         {
            SPId = sp.SPId,
            DateCreated = sp.DateCreated,
        //    Balances = bals,
            //Exchanges = exchs,
            Name = sp.Name
         };


         return p;
      }

      public static BalanceDTO Map(this Balance b)
      {
         // TOdo - will look at this closer when I have more
         // Data
         var dto = new BalanceDTO()
         {
            Available = b.Amount,
            //Currency = b.
         };
         return dto;
      }

      public static ExchangeDetailsDTO Map(this ExchangeDetails e)
      {
         var dto = new ExchangeDetailsDTO()
         {
            ExchangeDetailsId = e.ExchangeDetailsId,
            ApiKey = e.ApiKey.Map(),
            DateCreated = e.DateCreated,
            IsEnabled = e.IsEnabled,
            VenueId = e.VenueId,
            Venue = e.Venue.MapToDTO(),
            Name = e.Name,
            SPId = e.SPId,
            StrategySPSubscriptionConfigId = e.StrategySPSubscriptionConfigId,
            CoinPairs = e.CoinPairs,
            OpeningExchangeBalance = e.OpeningExchangeBalance.MapToDTO(),
            OpeningExchangeBalanceId = e.OpeningExchangeBalanceId
         };
         return dto;
      }

      public static ExchangeDetails MapFromDTO(this ExchangeDetailsDTO dto)
      {

         var exchangeDetails = new ExchangeDetails()
         {
            ExchangeDetailsId = dto.ExchangeDetailsId,
            Name = dto.Name,
            ApiKeyId = dto.ApiKeyId,
            ApiKey = dto.ApiKey.Map(),
            VenueId = dto.VenueId,
            DateCreated = dto.DateCreated,
            SPId = dto.SPId,
            StrategySPSubscriptionConfigId = dto.StrategySPSubscriptionConfigId,
            CoinPairs = dto.CoinPairs,
            OpeningExchangeBalanceId = dto.OpeningExchangeBalanceId,
            OpeningExchangeBalance = dto.OpeningExchangeBalance.MapFromDTO()
         };
         return exchangeDetails;
      }

      public static OpeningExchangeBalance MapFromDTO(this OpeningExchangeBalanceDTO dto)
      {
         var openingBalance = new OpeningExchangeBalance()
         {
            OpeningExchangeBalanceId = dto.OpeningExchangeBalanceId,
            Description = dto.Description,
            Created = dto.Created,
            LiquidatingToOpeningBalance = dto.LiquidatingToOpeningBalance,
            LiquidatingFromCurrency = dto.LiquidatingFromCurrency,
            LiquidatingToCurrency = dto.LiquidatingToCurrency,
            LiquidatingFromOpeningBalance = dto.LiquidatingFromOpeningBalance,
            AmountToBeLiquidated = dto.AmountToBeLiquidated,

         };
         return openingBalance;
      }

      public static OpeningExchangeBalanceDTO MapToDTO(this OpeningExchangeBalance bal)
      {
         var openingBalance = new OpeningExchangeBalanceDTO()
         {
            OpeningExchangeBalanceId = bal.OpeningExchangeBalanceId,
            Description = bal.Description,
            Created = bal.Created,
            LiquidatingToOpeningBalance = bal.LiquidatingToOpeningBalance,
            LiquidatingFromCurrency = bal.LiquidatingFromCurrency,
            LiquidatingToCurrency = bal.LiquidatingToCurrency,
            LiquidatingFromOpeningBalance = bal.LiquidatingFromOpeningBalance,
            AmountToBeLiquidated = bal.AmountToBeLiquidated,
         };
         return openingBalance;
      }

      public static VenueDTO MapToDTO(this Venue v)
      {
         if (v == null) return null;
         return new VenueDTO()
         {
            VenueId = v.VenueId,
            VenueName = v.VenueName,
         };
      }

      public static Venue MapFromDTO(this VenueDTO v)
      {
         return new Venue()
         {
            VenueId = v.VenueId,
            VenueName = v.VenueName,
         };
      }

      public static ApiKeyDTO Map(this ApiKey a)
      {
         return new ApiKeyDTO()
         {
            ApiKeyId = a.ApiKeyId,
            DateCreated = a.DateCreated,
            Key = a.Key,
            PassPhrase = a.PassPhrase,
            Secret = a.Secret,
            AccountName = a.AccountName,
            SubAccountName = a.SubAccountName,
            IsSubAccount = a.IsSubAccount,
            Password = a.Password,
            Description = a.Description,
         };
      }

      public static ApiKey Map(this ApiKeyDTO a)
      {
         return new ApiKey()
         {
            ApiKeyId = a.ApiKeyId,
            DateCreated = a.DateCreated,
            Key = a.Key,
            PassPhrase = a.PassPhrase,
            Secret = a.Secret,
            AccountName = a.AccountName,
            SubAccountName = a.SubAccountName,
            IsSubAccount = a.IsSubAccount,
            Password = a.Password,
            Description = a.Description,
         };
      }

      public static ConfigSetting Map(this RabbitMQSettingsDTO rabbitMQSettingsDTO)
      {
         var factorySettings = new RabbitConnectionSettings()
         {
            HostName = rabbitMQSettingsDTO.Url,
            Port = rabbitMQSettingsDTO.Port,
            UserName = rabbitMQSettingsDTO.UserName,
            Password = rabbitMQSettingsDTO.Password,
            VirtualHost = rabbitMQSettingsDTO.VirtualHost,
         };

         var config = new ConfigSetting()
         {
            Name = RabbitMQSettingsDTO.Name,
            Description = rabbitMQSettingsDTO.Description,
            Value = JsonSerializer.Serialize(factorySettings)
         };
         return config;
      }




      public static RabbitMQSettingsDTO MapToMQ(this ConfigSetting config)
      {
         var setting = JsonSerializer.Deserialize<RabbitConnectionSettings>(config.Value);
         var dto = new RabbitMQSettingsDTO()
         {
            Id = config.ConfigSettingId,
            Description = config.Description,
            UserName = setting.UserName,
            Password = setting.Password,
            Port = setting.Port,
            Url = setting.HostName,
            VirtualHost = setting.VirtualHost
         };
         return dto;
      }


      public static StrategyExchangeConfigDTO MapToDTO(this StrategySPSubscriptionConfig config)
      {
         var exchs = new List<ExchangeDetailsDTO>();
         if (config.ExchangeDetails != null)
         {
            var dbExchs = config.ExchangeDetails.ToList();
            dbExchs.ForEach(e => exchs.Add(e.Map()));
         }

         var dto = new StrategyExchangeConfigDTO()
         {
            SP = config.SP.Map(),
            ConfigName = config.ConfigName,
            Strategy = config.Strategy.MapToDTO(),
            StrategySPSubscriptionConfigId = config.StrategySPSubscriptionConfigId,
            ExchangeDetails = exchs,
            SPId = config.SPId,
            StrategyId = config.StrategyId,
         };
         return dto;
      }

      public static StrategyExchangeConfigDTO Copy(this StrategyExchangeConfigDTO config)
      {
         var exchs = new List<ExchangeDetailsDTO>();
         var dbExchs = config.ExchangeDetails.ToList();
         dbExchs.ForEach(e => exchs.Add(e));
         var dto = new StrategyExchangeConfigDTO()
         {
            SP = config.SP,
            ConfigName = config.ConfigName,
            Strategy = config.Strategy,
            StrategySPSubscriptionConfigId = config.StrategySPSubscriptionConfigId,
            ExchangeDetails = exchs,
            SPId = config.SPId,
            StrategyId = config.StrategyId,
         };
         return dto;
      }



      public static LiquidationStrategyConfigDTO MapToDTO(this LiquidationStrategyConfig config)
      {
         var dto = new LiquidationStrategyConfigDTO()
         {
            AmountDecimals = config.AmountDecimals,
            BatchSize = config.BatchSize,
            CancellationPolicyOnStart = config.CancellationPolicyOnStart,
            CoinPair = config.CoinPair,
            CoinPairId = config.CoinPairId,
            DateCreated = config.DateCreated,
            Id = config.Id,
            LongTimeInterval = config.LongTimeInterval,
            NumberOfCoins = config.NumberOfCoins,
            OrderSize = config.OrderSize,
            PercentageSpreadFromFV = config.PercentageSpreadFromFV,
            PercentageSpreadLowerThreshold = config.PercentageSpreadLowerThreshold,
            PriceDecimals = config.PriceDecimals,
            ShortTimeInterval = config.ShortTimeInterval,
            StrategySPSubscriptionConfig = config.StrategySPSubscriptionConfig.MapToDTO(),
            StrategySPSubscriptionConfigId = config.StrategySPSubscriptionConfigId,
            SubscriptionPrice = config.SubscriptionPrice,
            Symbol = config.Symbol,
            Venue = config.Venue,
         };
         return dto;
      }

      public static LiquidationStrategyConfig MapFromDTO(this LiquidationStrategyConfigDTO config)
      {
         var output = new LiquidationStrategyConfig()
         {
            AmountDecimals = config.AmountDecimals,
            BatchSize = config.BatchSize,
            CancellationPolicyOnStart = config.CancellationPolicyOnStart,
            CoinPair = config.CoinPair,
            CoinPairId = config.CoinPairId,
            DateCreated = config.DateCreated,
            Id = config.Id,
            LongTimeInterval = config.LongTimeInterval,
            NumberOfCoins = config.NumberOfCoins,
            OrderSize = config.OrderSize,
            PercentageSpreadFromFV = config.PercentageSpreadFromFV,
            PercentageSpreadLowerThreshold = config.PercentageSpreadLowerThreshold,
            PriceDecimals = config.PriceDecimals,
            ShortTimeInterval = config.ShortTimeInterval,
            StrategySPSubscriptionConfigId = config.StrategySPSubscriptionConfigId,
            SubscriptionPrice = config.SubscriptionPrice,
            Symbol = config.Symbol,
            Venue = config.Venue,

         };
         return output;
      }

      public static LiquidationStrategyConfigDTO Copy(this LiquidationStrategyConfigDTO config)
      {
         var output = new LiquidationStrategyConfigDTO()
         {
            StrategySPSubscriptionConfig = config.StrategySPSubscriptionConfig.Copy(),
            AmountDecimals = config.AmountDecimals,
            BatchSize = config.BatchSize,
            CancellationPolicyOnStart = config.CancellationPolicyOnStart,
            CoinPair = config.CoinPair,
            CoinPairId = config.CoinPairId,
            DateCreated = config.DateCreated,
            Id = config.Id,
            LongTimeInterval = config.LongTimeInterval,
            NumberOfCoins = config.NumberOfCoins,
            OrderSize = config.OrderSize,
            PercentageSpreadFromFV = config.PercentageSpreadFromFV,
            PercentageSpreadLowerThreshold = config.PercentageSpreadLowerThreshold,
            PriceDecimals = config.PriceDecimals,
            ShortTimeInterval = config.ShortTimeInterval,
            StrategySPSubscriptionConfigId = config.StrategySPSubscriptionConfigId,
            SubscriptionPrice = config.SubscriptionPrice,
            Symbol = config.Symbol,
         };
         return output;
      }

      public static CoinPairDTO MaptoDTO(this CoinPair coinPair)
      {
         var coinPairDTO = new CoinPairDTO()
         {
            CoinPairId = coinPair.CoinPairId,
            Name = coinPair.Name,
            PCoin = coinPair.PCoin.MapToDTO(),
            PCoinId = coinPair.PCoinId,
            SCoin = coinPair.SCoin.MapToDTO(),
            SCoinId = coinPair.SCoinId,
         };
         return coinPairDTO;
      }

      public static CoinPair MapFromDTO(this CoinPairDTO coinPairDto)
      {
         var coinPair = new CoinPair()
         {
            CoinPairId = coinPairDto.CoinPairId,
            Name = coinPairDto.Name,
            PCoin = coinPairDto.PCoin.MapFromDTO(),
            PCoinId = coinPairDto.PCoinId,
            SCoin = coinPairDto.SCoin.MapFromDTO(),
            SCoinId = coinPairDto.SCoinId,
         };
         return coinPair;
      }

      public static CoinDTO MapToDTO(this Coin coin)
      {
         var output = new CoinDTO()
         {
            CoinId = coin.CoinId,
            Name = coin.Name,
         };
         return output;
      }

      public static Coin MapFromDTO(this CoinDTO coin)
      {
         var output = new Coin()
         {
            CoinId = coin.CoinId,
            Name = coin.Name,
         };
         return output;
      }

      public static Strategy MapFromDTO(this StrategyDTO dto)
      {
         var output = new Strategy()
         {
            StrategyId = dto.StrategyId,
            StrategyName = dto.StrategyName,
         };
         return output;
      }

      public static StrategyDTO MapToDTO(this Strategy dto)
      {
         if (dto == null) return null;
         var output = new StrategyDTO()
         {
            StrategyId = dto.StrategyId,
            StrategyName = dto.StrategyName,
         };
         return output;
      }

      public static LiquidationConfigurationDTO MapToDTO(this LiquidationConfiguration config)
      {
         var output = new LiquidationConfigurationDTO()
         {
            Id = config.Id,
            SPId = config.SPId,
            SP = config.SP.Map(),
            StrategyId = config.StrategyId,
            Strategy = config.Strategy.MapToDTO(),
            StrategySPSubscriptionConfigId = config.StrategySPSubscriptionConfigId,
            StrategySPSubscriptionConfig = config.StrategySPSubscriptionConfig.MapToDTO(),
            CoinPairId = config.CoinPairId,
            CoinPair = config.CoinPair.MaptoDTO(),
            SubscriptionPrice = config.SubscriptionPrice,
            CoinAmount = config.CoinAmount,
            DailyLiquidationTarget = config.DailyLiquidationTarget,
            MaxOrderSize = config.MaxOrderSize,
            PercentageSpreadFromFV = config.PercentageSpreadFromFV,
            PercentageSpreadLowerThreshold = config.PercentageSpreadLowerThreshold,
            LiquidationOrderLoadingConfigurationId = config.LiquidationOrderLoadingConfigurationId,
            LiquidationOrderLoadingConfiguration = config.LiquidationOrderLoadingConfiguration.MapToDTO(),
            ShortTimeInterval = config.ShortTimeInterval,
            LongTimeInterval = config.LongTimeInterval,
            BatchSize = config.BatchSize,
            PriceDecimals = config.PriceDecimals,
            AmountDecimals = config.AmountDecimals,
            DateCreated = config.DateCreated,
            EndDate = config.EndDate.Date,
            StrategyState = config.StrategyState,
            OrderSize = config.OrderSize,
            MakerMode = config.MakerMode,
            StopOnDailyTargetReached = config.StopOnDailyTargetReached,
            TakerModeTimeInterval = config.TakerModeTimeInterval,
            CancelTimerInterval = config.CancelTimerInterval,
            LivePrice = 0,
            BalanceLiquidationFrom = 0,
            BalanceLiquidationTo = 0,
            DailyLiquidationFrom = 0,
            DailyLiquidationTo = 0,
            TotalFills = 0,
            DailyFills = 0,
         };
         List<string> exchangeNames = new List<string>();
         foreach (var ex in output.StrategySPSubscriptionConfig.ExchangeDetails)
         {
            exchangeNames.Add(ex.Name);
         }

         output.Exchanges = string.Join(',', exchangeNames);
         output.CurrencyLiquidatedFrom = output.CoinPair.PCoin.Name;
         output.CurrencyLiquidatedTo = output.CoinPair.SCoin.Name;
         output.NumDaysRemaining = (output.EndDate - DateTime.UtcNow).Days;
         if (output.NumDaysRemaining == 0)
            output.DailyLiquidationTarget = output.CoinAmount;
         else
            output.DailyLiquidationTarget =
               Math.Round(output.CoinAmount / output.NumDaysRemaining, 3, MidpointRounding.ToEven);

         return output;
      }

      public static LiquidationConfiguration MapFromDTO(this LiquidationConfigurationDTO config)
      {
         var output = new LiquidationConfiguration()
         {
            Id = config.Id,
            SPId = config.SPId,
            StrategyId = config.StrategyId,
            StrategySPSubscriptionConfigId = config.StrategySPSubscriptionConfigId,
            CoinPairId = config.CoinPairId,
            SubscriptionPrice = config.SubscriptionPrice,
            CoinAmount = config.CoinAmount,
            DailyLiquidationTarget = config.DailyLiquidationTarget,
            MaxOrderSize = config.MaxOrderSize,
            PercentageSpreadFromFV = config.PercentageSpreadFromFV,
            PercentageSpreadLowerThreshold = config.PercentageSpreadLowerThreshold,
            LiquidationOrderLoadingConfigurationId = config.LiquidationOrderLoadingConfigurationId,
            ShortTimeInterval = config.ShortTimeInterval,
            LongTimeInterval = config.LongTimeInterval,
            BatchSize = config.BatchSize,
            PriceDecimals = config.PriceDecimals,
            AmountDecimals = config.AmountDecimals,
            DateCreated = DateTime.UtcNow,
            EndDate = config.EndDate.ToUniversalTime(),
            StrategyState = config.StrategyState,
            OrderSize = config.OrderSize,
            MakerMode = config.MakerMode,
            StopOnDailyTargetReached = config.StopOnDailyTargetReached,
            TakerModeTimeInterval = config.TakerModeTimeInterval,
            CancelTimerInterval = config.CancelTimerInterval,
         };
         return output;
      }

      public static LiquidationOrderLoadingConfigurationDTO MapToDTO(this LiquidationOrderLoadingConfiguration config)
      {
         return new LiquidationOrderLoadingConfigurationDTO()
         {
            Id = config.Id,
            IsAuto = config.IsAuto,
            Name = config.Name,
            StartPercentage = config.StartPercentage,
            ScalingFactor = config.ScalingFactor
         };
      }


      public static LiquidationOrderLoadingConfiguration MapfromDTO(this LiquidationOrderLoadingConfigurationDTO config)
      {
         return new LiquidationOrderLoadingConfiguration()
         {
            Id = config.Id,
            IsAuto = config.IsAuto,
            Name = config.Name,
            StartPercentage = config.StartPercentage,
            ScalingFactor = config.ScalingFactor
         };
      }
   }
}
