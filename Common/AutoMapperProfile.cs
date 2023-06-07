using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Common.Models.DTOs;
using Common.Models.Entities;

namespace Common
{
   public class AutoMapperProfile : Profile
   {
      public AutoMapperProfile()
      {
         CreateMap<SP, SPDTO>().ReverseMap();
         CreateMap<ApiKey, ApiKeyDTO>().ReverseMap();
         CreateMap<Coin, CoinDTO>().ReverseMap();
         CreateMap<CoinPair, CoinPairDTO>().ReverseMap();
         CreateMap<CompletedOrder, CompletedOrderDTO>().ReverseMap();
         CreateMap<ExchangeDetails, ExchangeDetailsDTO>().ReverseMap();
         CreateMap<OpenOrder, OpenOrderDTO>().ReverseMap();
         CreateMap<Trade, TradeDTO>().ReverseMap(); 
         CreateMap<Balance, BalanceDTO>().ReverseMap();
         CreateMap<Venue, VenueDTO>().ReverseMap();
         CreateMap<Strategy, StrategyDTO>().ReverseMap();
         CreateMap<ExchangeCoinpairMapping, ExchangeCoinpairMappingDTO>().ReverseMap();
         CreateMap<LiquidationStrategyConfig, LiquidationStrategyConfigDTO>().ReverseMap();
         CreateMap<StrategySPSubscriptionConfig, StrategyExchangeConfigDTO>().ReverseMap();
         CreateMap<LiquidationConfiguration, LiquidationConfigurationDTO>().ReverseMap();
         CreateMap<ExchangeCoinMappings, ExchangeCoinMappingsDTO>().ReverseMap();
         CreateMap<ExchangeCoinpairMapping, ExchangeCoinpairMappingDTO>().ReverseMap();
         CreateMap<FairValueConfigForUI, FairValueConfigForUiDTO>().ReverseMap();
         CreateMap<Order, OrderDTO>().ReverseMap();
         CreateMap<MakerTakerFee, MakerTakerFeeDTO>().ReverseMap();

         CreateMap<TelegramAlert, TelegramAlertDTO>().ReverseMap();
         CreateMap<TelegramAlertCategory, TelegramAlertCategoryDTO>().ReverseMap();
         CreateMap<TelegramAlertToChannel, TelegramAlertToChannelDTO>().ReverseMap();
         CreateMap<TelegramChannel, TelegramChannelDTO>().ReverseMap();
         CreateMap<TelegramCommand, TelegramCommandDTO>().ReverseMap();
         CreateMap<TelegramCommandToUser, TelegramCommandToUserDTO>().ReverseMap();
         CreateMap<TelegramCommandType, TelegramCommandTypeDTO>().ReverseMap();
         CreateMap<TelegramSubscriberToChannel, TelegramSubscriberToChannelDTO>().ReverseMap();
         CreateMap<TelegramUser, TelegramUserDTO>().ReverseMap();
         CreateMap<LiquidationOrderLoadingConfiguration, LiquidationOrderLoadingConfigurationDTO>().ReverseMap();
         CreateMap<LiquidationManualOrderLoading, LiquidationManualOrderLoadingDTO>().ReverseMap();
         CreateMap<OpeningExchangeBalance, OpeningExchangeBalanceDTO>().ReverseMap();

         CreateMap<Fund, FundDTO>().ReverseMap();
         CreateMap<Location, LocationDTO>().ReverseMap();
         CreateMap<TelegramAlertBehaviour, TelegramAlertBehaviourDTO>().ReverseMap();
         CreateMap<TelegramAlertBehaviourType, TelegramAlertBehaviourTypeDTO>().ReverseMap();
      }
   }
}
