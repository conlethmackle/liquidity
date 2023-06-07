using AutoMapper;
using Common.Extensions;
using Common.Models.DTOs;
using Common.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Messages;
using Common.Models;
using ExchangeBalance = Common.Models.Entities.OpeningExchangeBalance;

namespace DataStore
{
   public interface IPortfolioRepository
   {
      Task<List<SPDTO>> GetPortfolios();
      Task<List<SPDTO>> GetPortfoliosByFundId(int FundId);
      Task<SPDTO> CreatePortfolio(SPDTO portfolio);
      Task<SPDTO> GetPortfolioByName(string name);
      Task DeletePortfolio(int Id);
      Task DeletePortfolioByName(string name);
      Task UpdatePortfolio(SPDTO sp);

      Task<Coin> AddCoin(CoinDTO coin);
      Task<List<Coin>> GetCoins();
      Task DeleteCoin(int Id);
      Task UpdateCoin(CoinDTO coin);
      Task<CoinPair> AddCoinPair(CoinPairDTO coinPair);
      Task<List<CoinPairDTO>> GetCoinPairs();
      Task DeleteCoinPair(int Id);
      Task UpdateCoinPair(CoinPairDTO coinPair);
      Task<VenueDTO> AddVenue(VenueDTO venueDTO);
      Task<List<VenueDTO>> GetVenues();
      Task UpdateVenue(VenueDTO venue);
      Task DeleteVenue(int Id);
      Task<TradeDTO> AddTrade(TradeDTO tradeDTO);
      Task<List<TradeDTO>> GetLatestTrades(string InstanceName, int numberToTake);
      Task<List<TradeDTO>> GetLatestTradesForSP(int SpId);
      Task<List<TradeDTO>> GetTradesOnDateForInstance(string instance, DateTime date);

      Task<RabbitMQSettingsDTO> CreateMQSettings(RabbitMQSettingsDTO settingsDTO);
      Task<RabbitMQSettingsDTO> GetMQSettingsAsync();
      RabbitMQSettingsDTO GetMQSettings();
      Task UpdateMQSettings(RabbitMQSettingsDTO settingsDTO);
      Task DeleteMQSettings(int Id);

      Task<StrategyExchangeConfigDTO> CreateStrategyExchangeConfigEntry(StrategyExchangeConfigDTO strategyConfig);
      Task<ExchangeDetailsDTO> CreateExchangeDetailsEntry(ExchangeDetailsDTO entry);
      Task<List<ExchangeDetailsDTO>> GetExchangeDetailsForStrategyConfigId(int ConfigId);
      Task UpdateExchangeDetails(ExchangeDetailsDTO exchangeDetailsDTO);
      Task DeleteExchangeDetails(int Id);
      Task UpdateStrategyExchangeConfigEntry(StrategyExchangeConfigDTO strategyConfigDTO);
      Task<StrategyExchangeConfigDTO> GetStrategyExchangeConfigEntry(string configName);
      Task<StrategyExchangeConfigDTO> GetStrategyExchangeConfigEntryById(int Id);
      Task<List<StrategyExchangeConfigDTO>> GetStrategyExchangeConfigsForSP(string accountName);
      Task<List<StrategyExchangeConfigDTO>> GetAllStrategyExchangeConfigs();
      Task DeleteStrategyExchangeConfig(int Id);
      Task<StrategyDTO> AddStrategy(StrategyDTO strategyDTO);
      Task<List<StrategyDTO>> GetStrategies();
      Task UpdateStrategy(StrategyDTO strategy);
      Task DeleteStrategy(int Id);
      Task<ExchangeCoinpairMappingDTO> AddExchangeCoinPairLookup(ExchangeCoinpairMappingDTO coinPairDTO);
      Task<List<ExchangeCoinpairMappingDTO>> GetExchangeCoinPairLookups();
      Task<ExchangeCoinpairMappingDTO> GetExchangeCoinPairSymbolFromGenericCoinPairSymbol(string exchange, string genericSymbol);
      Task<ExchangeCoinpairMappingDTO> GetGenericSymbolFromExchangeCoinPairSymbol(string exchange, string exchangeSymbol);
      Task UpdateExchangeCoinPair(ExchangeCoinpairMappingDTO coinPairDTO);
      Task DeleteExchangeCoinPair(int Id);
      Task<LiquidationStrategyConfigDTO> CreateLiquidationStrategyConfig(LiquidationStrategyConfigDTO config);
      Task UpdateLiquidationStrategyConfig(LiquidationStrategyConfigDTO config);
      Task<List<LiquidationStrategyConfigDTO>> GetLiquidationStrategyConfigs();
      Task<LiquidationStrategyConfigDTO> GetLiquidationStrategyConfigByStrategyConfigId(int id);
     
      Task DeleteLiquidationStrategyConfig(int Id);
      Task<ApiKeyDTO> CreateApiKey(ApiKeyDTO apiKey);
      Task<List<ApiKeyDTO>> GetApiKeys();
      Task UpdateApiKey(ApiKeyDTO apiKey);
      Task DeleteApiKey(int Id);

      Task<LiquidationConfigurationDTO> GetOpeningLiquidationSubscriptionsForInstance(string instance);
      Task<List<LiquidationConfigurationDTO>> GetOpeningLiquidationSubscriptions();
      Task<List<LiquidationConfigurationDTO>> GetOpeningLiquidationSubscriptionsForSp(string spName);
      Task<LiquidationConfigurationDTO> GetOpeningLiquidationSubscription(int id);
      Task<LiquidationConfigurationDTO> GetOpeningLiquidationSubscriptionForStrategySPSubscriptionId(int id);
      Task UpdateOpeningLiquidationSubscription(LiquidationConfigurationDTO subscriptionDto);

      Task<LiquidationConfigurationDTO> CreateOpeningLiquidationSubscription(
         LiquidationConfigurationDTO subscriptionDto);

      Task DeleteOpeningLiquidationSubscription(int Id);

      Task<ExchangeCoinMappingsDTO> AddExchangeCoinLookup(ExchangeCoinMappingsDTO coinDTO);
      Task<List<ExchangeCoinMappingsDTO>> GetExchangeCoinLookups();
      Task<ExchangeCoinMappingsDTO> GetExchangeCoinSymbolFromGenericCoinSymbol(string exchange, string genericSymbol);
      Task<ExchangeCoinMappingsDTO> GetGenericSymbolFromExchangeCoinSymbol(string exchange, string exchangeSymbol);
      Task UpdateExchangeCoin(ExchangeCoinMappingsDTO coinDTO);
      Task DeleteExchangeCoin(int Id);
      Task<FairValueConfigForUiDTO> CreateFairValueConfigForUI(FairValueConfigForUiDTO fairValueConfigDTO);
      Task<List<FairValueConfigForUiDTO>> GetFairValueConfigForUI();
      Task UpdateFairValueConfig(FairValueConfigForUiDTO fairValueConfigDTO);
      Task DeleteFairValueConfigForUI(int id);
      Task<List<FillsInfoForInstance>> GetFillsInfoForInstance(string instance);

      Task<MakerTakerFeeDTO> CreateMakerTakerFee(MakerTakerFeeDTO makerTakerFeeDTO);
      Task<List<MakerTakerFeeDTO>> GetMakerTakerFees();
      Task UpdateMakerTakerFees(MakerTakerFeeDTO makerTakerFeeDTO);
      Task DeleteMakerTakerFees(int id);

      Task<OrderDTO> AddOrder(OrderDTO order);
      Task<List<OrderDTO>> GetOrdersForInstance(string instance);
      Task<List<OrderDTO>> GetOrdersForSP(int SpId);

      Task<TelegramAlertDTO> CreateTelegramAlerts(TelegramAlertDTO alertDTO);
      Task<List<TelegramAlertDTO>> GetTelegramAlerts();
      Task UpdateTelegramAlert(TelegramAlertDTO alertDTO);
      Task DeleteTelegramAlert(int id);
      Task<TelegramAlertCategoryDTO> CreateTelegramAlertCategory(TelegramAlertCategoryDTO alertCategoryDTO);
      Task<List<TelegramAlertCategoryDTO>> GetTelegramAlertCategories();
      Task UpdateTelegramAlertCategory(TelegramAlertCategoryDTO alertDTO);
      Task DeleteTelegramAlertCategory(int id);
      Task<TelegramAlertToChannelDTO> CreateTelegramAlertToChannel(TelegramAlertToChannelDTO alertCategoryDTO);
      Task<List<TelegramAlertToChannelDTO>> GetTelegramAlertsToChannels();
      Task UpdateTelegramAlertToChannel(TelegramAlertToChannelDTO alertDTO);
      Task DeleteTelegramAlertToChannel(int id);
      Task<TelegramChannelDTO> CreateTelegramChannel(TelegramChannelDTO channelDTO);
      Task<List<TelegramChannelDTO>> GetTelegramChannels();
      Task<List<TelegramChannelDTO>> GetCommandTelegramChannels();
      Task UpdateTelegramChannel(TelegramChannelDTO channelDTO);
      Task DeleteTelegramChannel(int id);

      Task<TelegramCommandToUserDTO> CreateTelegramCommandToUser(TelegramCommandToUserDTO commandToUserDTO);
      Task<List<TelegramCommandToUserDTO>> GetTelegramCommandToUsers();
      Task UpdateTelegramCommandToUser(TelegramCommandToUserDTO commandToUserDTO);
      Task DeleteTelegramCommandToUser(int id);

      Task<TelegramCommandTypeDTO> CreateTelegramCommandType(TelegramCommandTypeDTO commandTypeDTO);
      Task<List<TelegramCommandTypeDTO>> GetTelegramCommandTypes();
      Task UpdateTelegramCommandType(TelegramCommandTypeDTO commandTypeDTO);
      Task DeleteTelegramCommandType(int id);

      Task<TelegramCommandDTO> CreateTelegramCommand(TelegramCommandDTO commandDTO);
      Task<List<TelegramCommandDTO>> GetTelegramCommands();
      Task<List<TelegramCommandDTO>> GetTelegramLiquidationConfigurationCommands();
      Task UpdateTelegramCommand(TelegramCommandDTO commandDTO);
      Task DeleteTelegramCommand(int id);

      Task<TelegramSubscriberToChannelDTO> CreateTelegramSubscriberToChannel(TelegramSubscriberToChannelDTO subscriberToChannelDTO);
      Task<List<TelegramSubscriberToChannelDTO>> GetTelegramSubscriberToChannels();
      Task UpdateTelegramSubscriberToChannel(TelegramSubscriberToChannelDTO subscriberToChannelDTO);
      Task DeleteTelegramSubscriberToChannel(int id);

      Task<TelegramUserDTO> CreateTelegramUser(TelegramUserDTO userDTO);
      Task<List<TelegramUserDTO>> GetTelegramUsers();
      Task UpdateTelegramUser(TelegramUserDTO userDTO);
      Task DeleteTelegramUser(int id);

      Task<LiquidationOrderLoadingConfigurationDTO> CreateLiquidationOrderLoadingConfiguration(LiquidationOrderLoadingConfigurationDTO dto);
      Task<List<LiquidationOrderLoadingConfigurationDTO>> GetLiquidationOrderLoadingConfiguration();
      Task UpdateLiquidationOrderLoadingConfiguration(LiquidationOrderLoadingConfigurationDTO dto);
      Task DeleteLiquidationOrderLoadingConfiguration(int id);

      Task<LiquidationManualOrderLoadingDTO> CreateLiquidationManualOrderLoading(LiquidationManualOrderLoadingDTO dto);
      Task<List<LiquidationManualOrderLoadingDTO>> GetLiquidationManualOrderLoading();
      Task<List<LiquidationManualOrderLoadingDTO>> GetLiquidationManualOrderLoadingForInstance(int InstanceId);
      Task UpdateLiquidationManualOrderLoadingConfiguration(LiquidationManualOrderLoadingDTO dto);
      Task DeleteLiquidationManualOrderLoadingConfiguration(int id);

      Task<OpeningExchangeBalanceDTO> CreateExchangeBalance(OpeningExchangeBalanceDTO exchangeBalanceDTO);
      Task<List<OpeningExchangeBalanceDTO>> GetExchangeBalances();
      List<OpeningExchangeBalanceDTO> GetExchangeBalancesForInstance(int InstanceId);
      Task UpdateExchangeBalance(OpeningExchangeBalanceDTO dto);
      Task DeleteExchangeBalance(int id);

      Task<FundDTO> CreateFund(FundDTO fundDTO);
      Task<List<FundDTO>> GetFunds();
      Task<FundDTO> GetFundById(int id);
      Task UpdateFund(FundDTO fundDTO);
      Task DeleteFund(int id);

      Task<LocationDTO> CreateLocation(LocationDTO locationDTO);
      Task<List<LocationDTO>> GetLocations();
      Task UpdateLocation(LocationDTO locationDTO);
      Task DeleteLocation(int id);

      Task<TelegramAlertBehaviourTypeDTO> CreateTelegramAlertBehaviourType(TelegramAlertBehaviourTypeDTO dto);
      Task<List<TelegramAlertBehaviourTypeDTO>> GetTelegramAlertBehaviourTypes();
      Task UpdateTelegramAlertBehaviourType(TelegramAlertBehaviourTypeDTO dto);
      Task DeleteTelegramAlertBehaviourType(int id);

      Task<TelegramAlertBehaviourDTO> CreateTelegramAlertBehaviour(TelegramAlertBehaviourDTO dto);
      Task<List<TelegramAlertBehaviourDTO>> GetTelegramAlertBehaviours();
      Task UpdateTelegramAlertBehaviour(TelegramAlertBehaviourDTO dto);
      Task DeleteTelegramAlertBehaviour(int id);

   }

   public class PortfolioRepository : IPortfolioRepository, IDisposable
   {
      private readonly ILogger<PortfolioRepository> _logger;     
      private readonly IMapper _mapper;
      private readonly IServiceScopeFactory _scopeFactory;
      
      public PortfolioRepository(ILoggerFactory loggerFactory,                                
                                IServiceScopeFactory scopeFactory,
                                IMapper mapper)
      {
         _logger = loggerFactory.CreateLogger<PortfolioRepository>();       
         _mapper = mapper;
         _scopeFactory = scopeFactory;
      }

      public async Task<List<SPDTO>> GetPortfolios()
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var sps = await context?.SPs.Include(f => f.Fund).ToListAsync();
               var dtoList = new List<SPDTO>();
               if (sps.Count > 0)
               {
                  
                  sps.ForEach(sp => dtoList.Add(_mapper.Map<SPDTO>(sp)));
                  
               }
               return dtoList;
            }
            catch(Exception e)
            {
               _logger.LogError(e, "Error getting portfolio from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task<SPDTO> GetPortfolioByName(string name)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var sps = await context?.SPs.Where(s => s.Name.Equals(name)).ToListAsync();
               if (sps.Count() > 0)
               {
                  var sp = sps.FirstOrDefault();
                  return _mapper.Map<SPDTO>(sp);
               }
               return null;
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error getting portfolio from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task<List<SPDTO>> GetPortfoliosByFundId(int FundId)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var sps = await context?.SPs.Where(s => s.FundId == FundId).ToListAsync();
               var dtoList = new List<SPDTO>();
               if (sps.Count > 0)
               {

                  sps.ForEach(sp => dtoList.Add(_mapper.Map<SPDTO>(sp)));

               }
               return dtoList;
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error getting GetPortfoliosByFundId from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task<SPDTO> CreatePortfolio(SPDTO portfolio)
      {
       //  var exchanges = new List<ExchangeDetails>();
       //  foreach (var exchangeDto in portfolio.Exchanges)
        // {
        //    var exchangeDetails = _mapper.Map<ExchangeDetails>(exchangeDto);
         //   exchangeDetails.ApiKey = _mapper.Map<ApiKey>(exchangeDto.ApiKey);
        //    exchanges.Add(exchangeDetails);
      //   }
                
         var sp = new SP()
         {
            Name = portfolio.Name,
            Description = portfolio.Description,
            FundId = portfolio.FundId,
            DateCreated = DateTime.UtcNow
         };

         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(sp);
               await context.SaveChangesAsync();
               var justAdded = await context.SPs.Where(sp => sp.Name == portfolio.Name).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  var dto = _mapper.Map<SPDTO>(justAdded.FirstOrDefault());
                  return dto;
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing portfolio to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing portfolio to database {Error}", e.Message);
            throw;
         }
         catch(OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing portfolio to database {Error}", e.Message);
            throw;
         }
         catch(Exception e)
         {
            _logger.LogError(e, "Error writing portfolio to database {Error}", e.Message);
            throw;
         }
      }

      public async Task DeletePortfolio(int Id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var sps = await context?.SPs.Where(s => s.SPId == Id).ToListAsync();
               if (sps.Count() > 0)
               {
                  var sp = sps.First();
                  context.SPs.Remove(sp);
                  await context.SaveChangesAsync();
               }

               
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error getting portfolio from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task DeletePortfolioByName(string name)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var sps = await context?.SPs.Where(s => s.Name == name).ToListAsync();
               if (sps.Count() > 0)
               {
                  var sp = sps.First();
                  context.SPs.Remove(sp);
                  await context.SaveChangesAsync();
               }


            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error getting portfolio from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task UpdatePortfolio(SPDTO sp)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.SPs.Where(s => s.SPId == sp.SPId).ToListAsync();
               var selectedSP = res.FirstOrDefault();
               if (selectedSP != null)
               {
                  selectedSP.Description = sp.Description;
                  selectedSP.Name = sp.Name;
                  context.SPs.Update(selectedSP);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("No such Portfolio {SP}", sp.Name);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error UpdatePortfolio  {Error}", e.Message);
               throw;
               throw;
            }
         }
      }

      void IDisposable.Dispose()
      {
       
      }

      public async Task<Coin> AddCoin(CoinDTO coin)
      {
         var exchangeCoin = new Coin()
         {
            Name = coin.Name,
         };
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(exchangeCoin);
               await context.SaveChangesAsync();
               var justAdded = await context.Coins.Where(c => c.Name == exchangeCoin.Name).ToListAsync();
               if (justAdded != null && justAdded.Any())
                  return justAdded.FirstOrDefault();
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing coin to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing coin to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing coin to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing coin to database {Error}", e.Message);
            throw;
         }
      }
      
      public async Task<List<Coin>> GetCoins()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var coins = await context?.Coins?.ToListAsync();
               return coins;
            }
         }
         catch(Exception e)
         {
            _logger.LogError(e, "Error Retrieving coins from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task DeleteCoin(int Id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var coins = await context?.Coins.Where(s => s.CoinId == Id).ToListAsync();
               if (coins.Count() > 0)
               {
                  var coin = coins.First();
                  context.Coins.Remove(coin);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting coin from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task UpdateCoin(CoinDTO coin)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.Coins.Where(s => s.CoinId == coin.CoinId).ToListAsync();
               var selectedCoin = res.FirstOrDefault();
               if (selectedCoin != null)
               {
                  selectedCoin.Name = coin.Name;
                
                  context.Coins.Update(selectedCoin);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("No such Coin {Coin}", coin.Name);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error UpdateCoin  {Error}", e.Message);
               throw;
               throw;
            }
         }
      }

      public async Task<CoinPair> AddCoinPair(CoinPairDTO coinPair)
      {
        
         var exchangeCoinPair = new CoinPair()
         {
            Name = coinPair.Name,
            PCoinId = coinPair.PCoinId,
            SCoinId = coinPair.SCoinId,
         };

         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(exchangeCoinPair);
               await context.SaveChangesAsync();
               var justAdded = await context.CoinPairs.Where(c => c.Name == exchangeCoinPair.Name).ToListAsync();
               if (justAdded != null && justAdded.Any())
                  return justAdded.FirstOrDefault();
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing coinpair to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing coinpair to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing coinpair to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing coinpair to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<CoinPairDTO>> GetCoinPairs()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var coinpairs = await context?.CoinPairs?.Include(c => c.PCoin)?.Include(c => c.SCoin)
                                                                    ?.ToListAsync();
               var dtoList = new List<CoinPairDTO>();
               coinpairs.ForEach(c =>
               {
                  dtoList.Add(_mapper.Map<CoinPairDTO>(c));
               });
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving coinpairs from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task DeleteCoinPair(int Id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var pairs = await context?.CoinPairs.Where(s => s.CoinPairId == Id).ToListAsync();
               if (pairs.Count() > 0)
               {
                  var sp = pairs.First();
                  context.CoinPairs.Remove(sp);
                  await context.SaveChangesAsync();
               }


            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting coinpair from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task UpdateCoinPair(CoinPairDTO coinPair)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.CoinPairs.Where(s => s.CoinPairId == coinPair.CoinPairId).ToListAsync();
               var selectedCoinPair = res.FirstOrDefault();
               if (selectedCoinPair != null)
               {
                  selectedCoinPair.Name = coinPair.Name;
                  selectedCoinPair.PCoinId = coinPair.PCoinId;
                  selectedCoinPair.SCoinId = coinPair.SCoinId;
                  context.CoinPairs.Update(selectedCoinPair);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("No such CoinPair {CoinPair}", coinPair.Name);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error UpdateCoinPair  {Error}", e.Message);
               throw;
               throw;
            }
         }
      }
      public async Task<VenueDTO> AddVenue(VenueDTO venueDTO)
      {
         var venue = _mapper.Map<Venue>(venueDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(venue);
               await context.SaveChangesAsync();
               var justAdded = await context.Venues.Where(c => c.VenueName == venueDTO.VenueName).ToListAsync();
               if (justAdded != null && justAdded.Any())
                  return justAdded.FirstOrDefault().MapToDTO();
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing venue to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing venue to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing venue to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing venue to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<VenueDTO>> GetVenues()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var venueDTOs = new List<VenueDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var venues = await context?.Venues?.ToListAsync();
               venues.ForEach(v =>
               {
                  venueDTOs.Add(v.MapToDTO());
               });
               return venueDTOs;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving venues from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task DeleteVenue(int Id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var venues = await context?.Venues.Where(s => s.VenueId == Id).ToListAsync();
               if (venues.Count() > 0)
               {
                  var venue = venues.First();
                  context.Venues.Remove(venue);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting coin from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task UpdateVenue(VenueDTO venue)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.Venues.Where(s => s.VenueId == venue.VenueId).ToListAsync();
               var selectedVenue = res.FirstOrDefault();
               if (selectedVenue != null)
               {
                  selectedVenue.VenueName = venue.VenueName;

                  context.Venues.Update(selectedVenue);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("No such Venue {Venue}", venue.VenueName);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error UpdateVenue  {Error}", e.Message);
               throw;
               throw;
            }
         }
      }

      public async Task<StrategyDTO> AddStrategy(StrategyDTO strategyDTO)
      {
         var strategy = _mapper.Map<Strategy>(strategyDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(strategy);
               await context.SaveChangesAsync();
               var justAdded = await context.Strategies.Where(c => c.StrategyName == strategyDTO.StrategyName).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<StrategyDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing venue to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing venue to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing venue to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing venue to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<StrategyDTO>> GetStrategies()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var strategies = await context?.Strategies?.ToListAsync();
               var strategiesDTOs = new List<StrategyDTO>();
               strategies.ForEach(strategy => strategiesDTOs.Add(_mapper.Map<StrategyDTO>(strategy)));
               return strategiesDTOs;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving strategies from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateStrategy(StrategyDTO strategy)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.Strategies.Where(s => s.StrategyId == strategy.StrategyId).ToListAsync();
               var selectedStrategy = res.FirstOrDefault();
               if (selectedStrategy != null)
               {
                  selectedStrategy.StrategyName = strategy.StrategyName;

                  context.Strategies.Update(selectedStrategy);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("No such Strategy {Strategy}", strategy.StrategyName);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error Strategy  {Error}", e.Message);
               throw;
               throw;
            }
         }
      }

      public async Task DeleteStrategy(int Id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var strategies = await context?.Strategies.Where(s => s.StrategyId == Id).ToListAsync();
               if (strategies.Count() > 0)
               {
                  var strategy = strategies.First();
                  context.Strategies.Remove(strategy);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting stratgey from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task<RabbitMQSettingsDTO> CreateMQSettings(RabbitMQSettingsDTO settingsDTO)
      {
         var configSetting = settingsDTO.Map();
       
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(configSetting);
               await context.SaveChangesAsync();
               var justAdded = await context.ConfigSettings.Where(c => c.Name == RabbitMQSettingsDTO.Name).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return justAdded.FirstOrDefault().MapToMQ();
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing rabbitmq settings to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing rabbitmq settings to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing rabbitmq settings to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing rabbitmq settings to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<RabbitMQSettingsDTO> GetMQSettingsAsync()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var settings = await context?.ConfigSettings?.Where(c => c.Name.Equals(RabbitMQSettingsDTO.Name)).ToListAsync();
               var setting = settings.FirstOrDefault();
               var dto = setting.MapToMQ();
               return dto;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving MQSettings from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateMQSettings(RabbitMQSettingsDTO settingsDTO)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var settings = await context?.ConfigSettings?.Where(c => c.Name.Equals(RabbitMQSettingsDTO.Name)).ToListAsync();
               var setting = settings.FirstOrDefault();
               var updatedSetting = settingsDTO.Map();
               setting.Description = updatedSetting.Description;
               setting.Value = updatedSetting.Value;
               context.Update(setting);
               await context.SaveChangesAsync();
            }
         }
         catch(Exception e)
         {
            _logger.LogError(e, "Error updating MQSettings from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public RabbitMQSettingsDTO GetMQSettings()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var settings = context?.ConfigSettings?.Where(c => c.Name.Equals(RabbitMQSettingsDTO.Name)).ToList();
               var setting = settings.FirstOrDefault();
               var dto = setting.MapToMQ();
               return dto;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving MQSettings from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task DeleteMQSettings(int Id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.ConfigSettings.Where(s => s.ConfigSettingId == Id).ToListAsync();
               if (configs.Count() > 0)
               {
                  var config = configs.First();
                  context.ConfigSettings.Remove(config);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteMQSettings from database {Error}", e.Message);
               throw;
            }
         }
      }
   
      public async Task<StrategyExchangeConfigDTO> CreateStrategyExchangeConfigEntry(StrategyExchangeConfigDTO strategyConfig)
      {
         try
         {
            var exchanges = new List<ExchangeDetails>();
            foreach (var exchangeDto in strategyConfig.ExchangeDetails)
            {
               var exchangeDetails = exchangeDto.MapFromDTO();
               var apiKeyDto = exchangeDto.ApiKey;
               apiKeyDto.Key = apiKeyDto.Key.EncryptAES();
               apiKeyDto.Secret = apiKeyDto.Secret.EncryptAES();
               apiKeyDto.Password = apiKeyDto.Password.EncryptAES();
               apiKeyDto.PassPhrase = apiKeyDto.PassPhrase.EncryptAES();
               apiKeyDto.AccountName = apiKeyDto.AccountName.EncryptAES();
               apiKeyDto.SubAccountName = apiKeyDto.SubAccountName.EncryptAES();
               exchangeDetails.ApiKey = _mapper.Map<ApiKey>(apiKeyDto);
               exchanges.Add(exchangeDetails);
            }

            var stratConfigForDb = new StrategySPSubscriptionConfig()
            {
           //    SP = strategyConfig.SP,
               SPId = strategyConfig.SPId,
               StrategyId = strategyConfig.StrategyId,
             //  ExchangeDetails = exchanges,
               ConfigName = strategyConfig.ConfigName,
          //     Strategy = strategyConfig.Strategy
            };
           
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(stratConfigForDb);
               await context.SaveChangesAsync();
               var justAdded = await context.StrategySPSubscriptionConfigs.Where(c => c.ConfigName.Equals(strategyConfig.ConfigName))
                  .Include(c => c.ExchangeDetails).ThenInclude(c => c.ApiKey)
                  .Include(c => c.ExchangeDetails).ThenInclude(c => c.Venue)
                  .ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return justAdded.FirstOrDefault().MapToDTO();
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error CreateStrategyExchangeConfigEntry from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateStrategyExchangeConfigEntry(StrategyExchangeConfigDTO strategyConfig)
      {
         try
         {
            var exchanges = new List<ExchangeDetails>();
            foreach (var exchangeDto in strategyConfig.ExchangeDetails)
            {
               var exchangeDetails = _mapper.Map<ExchangeDetails>(exchangeDto);
               var apiKeyDto = exchangeDto.ApiKey;
               apiKeyDto.Key = apiKeyDto.Key.EncryptAES();
               apiKeyDto.Secret = apiKeyDto.Secret.EncryptAES();
               apiKeyDto.Password = apiKeyDto.Password.EncryptAES();
               apiKeyDto.PassPhrase = apiKeyDto.PassPhrase.EncryptAES();
               apiKeyDto.AccountName = apiKeyDto.AccountName.EncryptAES();
               apiKeyDto.SubAccountName = apiKeyDto.SubAccountName.EncryptAES();
               exchangeDetails.ApiKey = _mapper.Map<ApiKey>(apiKeyDto);
               
               exchanges.Add(exchangeDetails);
            }


            var stratConfigForDb = new StrategySPSubscriptionConfig()
            {
              // SP = strategyConfig.SP,
               ExchangeDetails = exchanges,
          //     Strategy = strategyConfig.Strategy
            };

            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var settings = await context?.StrategySPSubscriptionConfigs?.Where(c => c.ConfigName.Equals(strategyConfig.ConfigName)).ToListAsync();
               var setting = settings.FirstOrDefault();
               setting = stratConfigForDb;
               context.Update(setting);
               await context.SaveChangesAsync();
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating MQSettings from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<StrategyExchangeConfigDTO> GetStrategyExchangeConfigEntry(string configName)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var settings = await context?.StrategySPSubscriptionConfigs?.Where(c => c.ConfigName.Equals(configName))
                  .Include(c => c.ExchangeDetails)
                  .ThenInclude(c => c.ApiKey)
                  .Include(c => c.ExchangeDetails)
                  .ThenInclude(c => c.OpeningExchangeBalance)
                  .Include(c => c.ExchangeDetails)
                  .ThenInclude(c => c.Venue)
                  .Include(c => c.SP)
                  .Include(c => c.Strategy).ToListAsync();
               if (settings != null)
               {
                  if (settings.Count > 0)
                  {
                     var setting = settings.FirstOrDefault();
                     var dto = setting.MapToDTO();
                     foreach (var exch in dto.ExchangeDetails)
                     {
                        
                        exch.ApiKey.AccountName = exch.ApiKey.AccountName.DecryptAES();
                        exch.ApiKey.Key = exch.ApiKey.Key.DecryptAES();
                        exch.ApiKey.Secret = exch.ApiKey.Secret.DecryptAES();
                        exch.ApiKey.PassPhrase = exch.ApiKey.PassPhrase.DecryptAES();
                        exch.ApiKey.Password = exch.ApiKey.Password.DecryptAES();
                        exch.ApiKey.SubAccountName = exch.ApiKey.SubAccountName.DecryptAES();
                     }
                     return dto;
                  }

               }

               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetStrategyExchangeConfigEntry from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<StrategyExchangeConfigDTO> GetStrategyExchangeConfigEntryById(int Id)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var settings = await context?.StrategySPSubscriptionConfigs?.Where(c => c.StrategySPSubscriptionConfigId == Id)
                  .Include(c => c.ExchangeDetails).
                  ThenInclude(c => c.ApiKey).
                  Include(c => c.ExchangeDetails).
                  ThenInclude(c => c.Venue).Include(c => c.SP)
                  .Include(c => c.Strategy)
                  .Include(c => c.ExchangeDetails)
                  .ThenInclude(x => x.OpeningExchangeBalance).ToListAsync();
               var setting = settings.FirstOrDefault();
               var dto = setting.MapToDTO();
               foreach (var exch in dto.ExchangeDetails)
               {

                  exch.ApiKey.AccountName = exch.ApiKey.AccountName.DecryptAES();
                  exch.ApiKey.Key = exch.ApiKey.Key.DecryptAES();
                  exch.ApiKey.Secret = exch.ApiKey.Secret.DecryptAES();
                  exch.ApiKey.PassPhrase = exch.ApiKey.PassPhrase.DecryptAES();
                  exch.ApiKey.Password = exch.ApiKey.Password.DecryptAES();
                  exch.ApiKey.SubAccountName = exch.ApiKey.SubAccountName.DecryptAES();
               }
               return dto;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetStrategyExchangeConfigEntry from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<List<StrategyExchangeConfigDTO>> GetStrategyExchangeConfigsForSP(string accountName)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context?.StrategySPSubscriptionConfigs?.Where(c => c.SP.Name.Equals(accountName))
                  .Include(c => c.ExchangeDetails)
                  .ThenInclude(c => c.ApiKey)
                  .Include(c => c.ExchangeDetails)
                  .ThenInclude(c => c.OpeningExchangeBalance)
                  .Include(c => c.ExchangeDetails)
                  .ThenInclude(c => c.Venue).Include(c => c.SP)
                  .Include(c => c.Strategy).ToListAsync();
               var dtoList = new List<StrategyExchangeConfigDTO>();
               res.ForEach(s => dtoList.Add(s.MapToDTO()));               
               return dtoList;  
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving MQSettings from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<List<StrategyExchangeConfigDTO>> GetAllStrategyExchangeConfigs()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context?.StrategySPSubscriptionConfigs?
                  .Include(c => c.ExchangeDetails)
                  .ThenInclude(c => c.ApiKey)
                  .Include(c => c.ExchangeDetails)
                  .ThenInclude(c => c.OpeningExchangeBalance)
                  .Include(c => c.ExchangeDetails)
                  .ThenInclude(c => c.Venue)
                  .Include(c => c.SP)
                  .Include(c => c.Strategy)
                  .ToListAsync();
               var dtoList = new List<StrategyExchangeConfigDTO>();
               res.ForEach(s => dtoList.Add(s.MapToDTO()));
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving MQSettings from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task DeleteStrategyExchangeConfig(int Id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.StrategySPSubscriptionConfigs.Where(s => s.StrategySPSubscriptionConfigId == Id).ToListAsync();
               if (configs.Count() > 0)
               {
                  var config = configs.First();
                  context.StrategySPSubscriptionConfigs.Remove(config);
                  await context.SaveChangesAsync();
               }


            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteStrategyExchangeConfig from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task<ExchangeDetailsDTO> CreateExchangeDetailsEntry(ExchangeDetailsDTO entry)
      {
         try
         {
            var exchange = entry.MapFromDTO();
            if (exchange.ApiKeyId > 0)
               exchange.ApiKey = null;
            else
            {
               var apiKey = exchange.ApiKey;
               apiKey.Key = apiKey.Key.EncryptAES();
               apiKey.Secret = apiKey.Secret.EncryptAES();
               apiKey.PassPhrase = apiKey.PassPhrase.EncryptAES();
               apiKey.Password = apiKey.Password.EncryptAES();
               apiKey.AccountName = apiKey.AccountName.EncryptAES();
               apiKey.SubAccountName = apiKey.SubAccountName.EncryptAES();
            }

            if (exchange.OpeningExchangeBalanceId > 0)
               exchange.OpeningExchangeBalance = null;

            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(exchange);
               await context.SaveChangesAsync();
               var justAdded = await context.ExchangeDetails.Where(x => x.ExchangeDetailsId == exchange.ExchangeDetailsId) 
                  .Include(x => x.ApiKey)
                  .Include(x => x.Venue)
                  .Include(x => x.OpeningExchangeBalance)
                  .ToListAsync();
               if (justAdded.Any())
               {
                  return justAdded.FirstOrDefault().Map();
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error CreateExchangeDetailsEntry from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<List<ExchangeDetailsDTO>> GetExchangeDetailsForStrategyConfigId(int ConfigId)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {

               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.ExchangeDetails.Where(e => e.StrategySPSubscriptionConfigId == ConfigId)
                  .Include(x => x.ApiKey)
                  .Include(x => x.Venue)
                  .Include(x => x.OpeningExchangeBalance)
                  ?.ToListAsync();
               if (configs != null)
               {
                  var dtoList = new List<ExchangeDetailsDTO>();
                  configs.ForEach(c =>
                  {
                     dtoList.Add(c.Map());
                  });
                  return dtoList;
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetLiquidationStrategyConfigs from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateExchangeDetails(ExchangeDetailsDTO exchangeDetailsDTO)
      {
         var exchange = exchangeDetailsDTO.MapFromDTO();
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var currentList = await context?.ExchangeDetails?.Where(c => c.ExchangeDetailsId == exchange.ExchangeDetailsId).ToListAsync();
               var current = currentList.FirstOrDefault();
               current = exchange;
               context.Update(current);
               await context.SaveChangesAsync();
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating UpdateExchangeDetails from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task DeleteExchangeDetails(int Id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var exchanges = await context?.ExchangeDetails.Where(s => s.ExchangeDetailsId == Id).ToListAsync();
               if (exchanges.Count() > 0)
               {
                  var exchange = exchanges.First();
                  context.ExchangeDetails.Remove(exchange);
                  await context.SaveChangesAsync();
               }


            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting ExchangeDetails from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task<ExchangeCoinpairMappingDTO> AddExchangeCoinPairLookup(ExchangeCoinpairMappingDTO coinPairDTO)
      {
         var exchangeCoinPair = _mapper.Map<ExchangeCoinpairMapping>(coinPairDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(exchangeCoinPair);
               await context.SaveChangesAsync();
               var justAdded = await context.ExchangeCoinPairMappings.Where(c => c.ExchangeCoinpairName == exchangeCoinPair.ExchangeCoinpairName).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                   return _mapper.Map<ExchangeCoinpairMappingDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing coinpair to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing coinpair to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing coinpair to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing coinpair to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<ExchangeCoinpairMappingDTO>> GetExchangeCoinPairLookups()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<ExchangeCoinpairMappingDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var coinpairs = await context?.ExchangeCoinPairMappings.Include(c => c.Venue).Include(c => c.CoinPair)?.ToListAsync();
               if (coinpairs != null)
               {                  
                  coinpairs.ForEach(p => dtoList.Add(_mapper.Map<ExchangeCoinpairMappingDTO>(p)));
               }              
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving coinpairs from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<ExchangeCoinpairMappingDTO> GetExchangeCoinPairSymbolFromGenericCoinPairSymbol(string exchange, string genericSymbol)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<ExchangeCoinpairMappingDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var coinpairs = await context?.ExchangeCoinPairMappings.Where(s => s.Venue.VenueName.Equals(exchange) && s.CoinPair.Name.Equals(genericSymbol))
                                            ?.ToListAsync();
               if (coinpairs != null)
               {
                  return _mapper.Map < ExchangeCoinpairMappingDTO > (coinpairs.FirstOrDefault());                
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving exchange coinpair lookups from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<ExchangeCoinpairMappingDTO> GetGenericSymbolFromExchangeCoinPairSymbol(string exchange, string exchangeSymbol)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<ExchangeCoinpairMappingDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var coinpairs = await context?.ExchangeCoinPairMappings.Where(s => s.Venue.VenueName.Equals(exchange) && s.CoinPair.Name.Equals(exchangeSymbol))
                                            ?.ToListAsync();
               if (coinpairs != null)
               {
                  return _mapper.Map<ExchangeCoinpairMappingDTO>(coinpairs.FirstOrDefault());
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetGenericSymbolFromExchangeCoinPairSymbol exchange coinpair from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateExchangeCoinPair(ExchangeCoinpairMappingDTO coinPairDTO)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.ExchangeCoinPairMappings.Where(s => s.ExchangeCoinpairLookupId == coinPairDTO.ExchangeCoinpairLookupId).ToListAsync();
               var selectedSP = res.FirstOrDefault();
               if (selectedSP != null)
               {
                  selectedSP.ExchangeCoinpairName = coinPairDTO.ExchangeCoinpairName;
                  selectedSP.VenueId = coinPairDTO.VenueId;
                  selectedSP.CoinPairId = coinPairDTO.CoinPairId;
                  context.ExchangeCoinPairMappings.Update(selectedSP);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("No such Exchange CoinPair {SP}", coinPairDTO.ExchangeCoinpairName);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating Exchange CoinPair  {Error}", e.Message);
               throw;
               throw;
            }
         }
      }

      public async Task DeleteExchangeCoinPair(int Id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var exchanges = await context?.ExchangeCoinPairMappings.Where(s => s.ExchangeCoinpairLookupId == Id).ToListAsync();
               if (exchanges.Count() > 0)
               {
                  var exchange = exchanges.First();
                  context.ExchangeCoinPairMappings.Remove(exchange);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteExchangeCoinPair from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task<LiquidationStrategyConfigDTO> GetLiquidationStrategyConfigByStrategyConfigId(int id)
      {
         try
         {

            using (var scope = _scopeFactory.CreateScope())
            {

               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.LiquidationStrategyConfigs.Where(x => x.StrategySPSubscriptionConfigId == id)
                  .Include(s => s.CoinPair).ThenInclude(s => s.PCoin)
                  .Include(s => s.CoinPair).ThenInclude(s => s.SCoin)
                  .Include(s => s.StrategySPSubscriptionConfig).ThenInclude(c => c.ExchangeDetails).ThenInclude(c => c.ApiKey)
                  .Include(s => s.StrategySPSubscriptionConfig).ThenInclude(p => p.SP)
                  .Include(s => s.StrategySPSubscriptionConfig).ThenInclude(c => c.Strategy)
                  .Include(c => c.StrategySPSubscriptionConfig).ThenInclude(p => p.ExchangeDetails).ThenInclude(p => p.Venue)
                  .ToListAsync();
               if (configs != null)
               {
                  var dtoList = new List<LiquidationStrategyConfigDTO>();
                  configs.ForEach(c =>
                  {
                     dtoList.Add(c.MapToDTO());
                  });

                  return dtoList.FirstOrDefault();
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetLiquidationStrategyConfigByStrategyConfigId from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<List<LiquidationStrategyConfigDTO>> GetLiquidationStrategyConfigs()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.LiquidationStrategyConfigs.Include(s => s.CoinPair).ThenInclude(s => s.PCoin)
                  .Include(s => s.CoinPair).ThenInclude(s => s.SCoin)
                  .Include(s => s.StrategySPSubscriptionConfig).ThenInclude(c => c.ExchangeDetails).ThenInclude(c => c.ApiKey)                                
                                            ?.ToListAsync();
               if (configs != null)
               {
                  var dtoList = new List<LiquidationStrategyConfigDTO>();
                  configs.ForEach(c =>
                  {
                     dtoList.Add(c.MapToDTO());
                  });
                  return dtoList;
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetLiquidationStrategyConfigs from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<LiquidationStrategyConfigDTO> CreateLiquidationStrategyConfig(LiquidationStrategyConfigDTO configDTO)
      {
         //  var config = _mapper.Map<LiquidationStrategyConfig>(configDTO);
         var config = configDTO.MapFromDTO();
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(config);
               await context.SaveChangesAsync();
               var justAdded = await context.LiquidationStrategyConfigs.Where(c => c.DateCreated == config.DateCreated).Include(c => c.StrategySPSubscriptionConfig)
                  .ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return justAdded.FirstOrDefault().MapToDTO();
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateLiquidationStrategyConfig to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing coinpair to CreateLiquidationStrategyConfig {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateLiquidationStrategyConfig to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateLiquidationStrategyConfig to database {Error}", e.Message);
            throw;
         }
      }

      public async Task UpdateLiquidationStrategyConfig(LiquidationStrategyConfigDTO configDTO)
      {
         var config = configDTO.MapFromDTO();
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var settings = await context?.LiquidationStrategyConfigs?.Where(c => c.Id == config.Id).ToListAsync();
               var setting = settings.FirstOrDefault();
               var co = _mapper.Map<LiquidationStrategyConfig>(config);
               setting = co;
               context.Update(setting);
               await context.SaveChangesAsync();
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating UpdateLiquidationStrategyConfig from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task DeleteLiquidationStrategyConfig(int Id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.LiquidationStrategyConfigs.Where(s => s.Id == Id).ToListAsync();
               if (configs.Any())
               {
                  var config = configs.First();
                  context.LiquidationStrategyConfigs.Remove(config);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting ExchangeDetails from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task<ApiKeyDTO> CreateApiKey(ApiKeyDTO apiKeyDto)
      {
         apiKeyDto.Key = apiKeyDto.Key.EncryptAES();
         apiKeyDto.Secret = apiKeyDto.Secret.EncryptAES();
         apiKeyDto.PassPhrase = apiKeyDto.PassPhrase.EncryptAES();
         apiKeyDto.Password = apiKeyDto.Password.EncryptAES();
         apiKeyDto.AccountName = apiKeyDto.AccountName.EncryptAES();
         apiKeyDto.SubAccountName = apiKeyDto.SubAccountName.EncryptAES();
         var apiKey = _mapper.Map<ApiKey>(apiKeyDto);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               

               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(apiKey);
               await context.SaveChangesAsync();
               var justAdded = await context.ApiKeys.Where(c => c.DateCreated == apiKey.DateCreated).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<ApiKeyDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateApiKey to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing  to CreateApiKey {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateApiKey to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateApiKey to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<ApiKeyDTO>> GetApiKeys()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.ApiKeys?.ToListAsync();
               if (configs != null)
               {
                  var dtoList = new List<ApiKeyDTO>();
                  configs.ForEach(c =>
                  {
                     c.AccountName = c.AccountName.DecryptAES();
                     c.Key = c.Key.DecryptAES();
                     c.Secret = c.Secret.DecryptAES();
                     c.PassPhrase = c.PassPhrase.DecryptAES();
                     c.Password = c.Password.DecryptAES();
                     c.SubAccountName = c.SubAccountName.DecryptAES();

                     dtoList.Add(_mapper.Map<ApiKeyDTO>(c));
                  });
                  return dtoList;
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetApiKeys from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateApiKey(ApiKeyDTO apiKeyDto)
      {
         apiKeyDto.Key = apiKeyDto.Key.EncryptAES();
         apiKeyDto.Secret = apiKeyDto.Secret.EncryptAES();
         apiKeyDto.PassPhrase = apiKeyDto.PassPhrase.EncryptAES();
         apiKeyDto.Password = apiKeyDto.Password.EncryptAES();
         apiKeyDto.AccountName = apiKeyDto.AccountName.EncryptAES();
         apiKeyDto.SubAccountName = apiKeyDto.SubAccountName.EncryptAES();
         var apiKey = _mapper.Map<ApiKey>(apiKeyDto);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var currentList = await context?.ApiKeys?.Where(c => c.ApiKeyId == apiKey.ApiKeyId).ToListAsync();
               var current = currentList.FirstOrDefault();
               current = apiKey;
               context.Update(current);
               await context.SaveChangesAsync();
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating UpdateApiKey from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task DeleteApiKey(int Id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var apiKeys = await context?.ApiKeys.Where(s => s.ApiKeyId == Id).ToListAsync();
               if (apiKeys.Any())
               {
                  var apiKey = apiKeys.First();
                  context.ApiKeys.Remove(apiKey);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting ExchangeDetails from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task<TradeDTO> AddTrade(TradeDTO tradeDTO)
      {
         var trade = _mapper.Map<Trade>(tradeDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(trade);
               await context.SaveChangesAsync();
               var justAdded = await context.Trades.Where(c => c.TradeId == trade.TradeId).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<TradeDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing AddTrade to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing  to AddTrade {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing Trade to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateApiKey to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<TradeDTO>> GetLatestTrades(string InstanceName, int numberToTake=15)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               List<Trade> trades = null;
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               if (numberToTake == 0)
               {
                  trades = await context.Trades.Where(x => x.InstanceName == InstanceName)
                     .Include(t => t.CoinPair)
                     .Include(t => t.SP)
                     .Include(t => t.Venue)
                     .OrderByDescending(x => x.DateCreated).ToListAsync();
               }
               else
               {
                  trades = await context.Trades.Where(x => x.InstanceName == InstanceName)
                     .Include(t => t.CoinPair)
                     .Include(t => t.SP)
                     .Include(t => t.Venue)
                     .OrderByDescending(x => x.DateCreated).Take(numberToTake).ToListAsync();
               }

               if (trades != null)
               {
                  var dtoList = new List<TradeDTO>();
                  trades.ForEach(c =>
                  {
                     dtoList.Add(_mapper.Map<TradeDTO>(c));
                  });

              
                  return dtoList;
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error GetLatestTrades from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<List<TradeDTO>> GetLatestTradesForSP(int SpId)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               List<Trade> trades = null;
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
              
               trades = await context.Trades.Where(x => x.SPId == SpId)
                     .Include(t => t.CoinPair)
                     .Include(t => t.SP)
                     .Include(t => t.Venue)
                     .OrderByDescending(x => x.DateCreated).ToListAsync();
               
               

               if (trades != null)
               {
                  var dtoList = new List<TradeDTO>();
                  trades.ForEach(c =>
                  {
                     dtoList.Add(_mapper.Map<TradeDTO>(c));
                  });


                  return dtoList;
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error GetLatestTrades from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<List<TradeDTO>> GetTradesOnDateForInstance(string instance, DateTime date)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               List<Trade> trades = null;
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
             
               trades = await context.Trades.Where(x => x.InstanceName == instance && x.DateCreated.Date == date.Date)
                  .Include(t => t.CoinPair)
                  .Include(t => t.SP)
                  .Include(t => t.Venue)
                  .OrderByDescending(x => x.DateCreated).ToListAsync();

               if (trades != null)
               {
                  var dtoList = new List<TradeDTO>();
                  trades.ForEach(c =>
                  {
                     dtoList.Add(_mapper.Map<TradeDTO>(c));
                  });


                  return dtoList;
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error GetTradesOnDateForInstance from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }
      public async Task<List<LiquidationConfigurationDTO>> GetOpeningLiquidationSubscriptions()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {

               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var subs = await context.LiquidationConfigurations.Include(t => t.CoinPair)
                  .Include(t => t.SP)
                  .Include(t => t.StrategySPSubscriptionConfig)
                  .ThenInclude(c => c.ExchangeDetails).ThenInclude(c => c.ApiKey)
                  .Include(t => t.StrategySPSubscriptionConfig)
                  .ThenInclude(c => c.ExchangeDetails).ThenInclude(c => c.OpeningExchangeBalance)
                  .Include(c => c.StrategySPSubscriptionConfig).ThenInclude(c => c.ExchangeDetails).ThenInclude(c => c.Venue)
                  .Include(t => t.CoinPair).ThenInclude(t => t.PCoin)
                  .Include(t => t.CoinPair).ThenInclude(t => t.SCoin)
                  .Include(t => t.Strategy)
                  .Include(t => t.LiquidationOrderLoadingConfiguration)
                  .ToListAsync();
               if (subs != null)
               {
                  var dtoList = new List<LiquidationConfigurationDTO>();
                  subs.ForEach(c => { dtoList.Add(c.MapToDTO()); });


                  return dtoList;
               }

               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in GetOpeningLiquidationSubscriptions - {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<LiquidationConfigurationDTO>> GetOpeningLiquidationSubscriptionsForSp(string spName)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {

               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var subs = await context.LiquidationConfigurations
                  .Where(t => t.SP.Name == spName)
                  .Include(t => t.SP)
                  .Include(t => t.StrategySPSubscriptionConfig)
                  .ThenInclude(c => c.ExchangeDetails).ThenInclude(c => c.ApiKey)
                  .Include(t => t.StrategySPSubscriptionConfig)
                  .ThenInclude(c => c.ExchangeDetails).ThenInclude(c => c.OpeningExchangeBalance)

                  .Include(c => c.StrategySPSubscriptionConfig).ThenInclude(c => c.ExchangeDetails).ThenInclude(c => c.Venue)
                  .Include(t => t.CoinPair).ThenInclude(t => t.PCoin)
                  .Include(t => t.CoinPair).ThenInclude(t => t.SCoin)
                  .Include(t => t.Strategy)
                  .Include(t => t.LiquidationOrderLoadingConfiguration)
                  .ToListAsync();
               if (subs != null)
               {
                  var dtoList = new List<LiquidationConfigurationDTO>();
                  subs.ForEach(c => { dtoList.Add(c.MapToDTO()); });


                  return dtoList;
               }

               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in GetOpeningLiquidationSubscriptionsForSp {Name}- {Error}", spName, e.Message);
            throw;
         }
      }

      public async Task<LiquidationConfigurationDTO> GetOpeningLiquidationSubscriptionsForInstance(string instance)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var subs = await context.LiquidationConfigurations
                  .Where(t => t.StrategySPSubscriptionConfig.ConfigName == instance)
                  .Include(t => t.SP)
                  .Include(t => t.StrategySPSubscriptionConfig)
                  .ThenInclude(c => c.ExchangeDetails).ThenInclude(c => c.ApiKey)
                  .Include(c => c.StrategySPSubscriptionConfig)
                  .ThenInclude(c => c.ExchangeDetails)
                  .ThenInclude(c => c.Venue)
                  .Include(c => c.StrategySPSubscriptionConfig)
                  .ThenInclude(c => c.ExchangeDetails)
                  .ThenInclude(c => c.OpeningExchangeBalance)
                  .Include(t => t.CoinPair).
                  ThenInclude(t => t.PCoin)
                  .Include(t => t.CoinPair).
                  ThenInclude(t => t.SCoin)
                  .Include(t => t.Strategy)
                  .Include(t => t.LiquidationOrderLoadingConfiguration)
                  .ToListAsync();
               if (subs.Any())
               {
                  var dtoList = new List<LiquidationConfigurationDTO>();
                  var entry = subs[0];
                  var dto = entry.MapToDTO();
                  return dto;
               }

               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in GetOpeningLiquidationSubscriptionsForInstance {Name}- {Error}", instance, e.Message);
            throw;
         }
      }

      public async Task<LiquidationConfigurationDTO> GetOpeningLiquidationSubscription(int id)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var subs = await context.LiquidationConfigurations.Where(t => t.Id == id).Include(t => t.CoinPair)
                  .Include(t => t.CoinPair).ThenInclude(c => c.PCoin)
                  .Include(t => t.CoinPair).ThenInclude(c => c.SCoin)
                  .Include(t => t.SP)
                  .Include(t => t.StrategySPSubscriptionConfig).ThenInclude(t => t.Strategy)
                  .Include(t => t.StrategySPSubscriptionConfig).ThenInclude(t => t.ExchangeDetails).ThenInclude(x => x.ApiKey)
                  .Include(t => t.StrategySPSubscriptionConfig).ThenInclude(t => t.ExchangeDetails).ThenInclude(x => x.OpeningExchangeBalance)
                  .Include(t => t.StrategySPSubscriptionConfig).ThenInclude(t => t.ExchangeDetails).ThenInclude(x => x.Venue)
                  .Include(t => t.LiquidationOrderLoadingConfiguration)
                  .ToListAsync();
               if (subs != null)
               {
                  return  subs.FirstOrDefault().MapToDTO();
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in GetOpeningLiquidationSubscriptions - {Error}", e.Message);
            throw;
         }
      }

      public async Task<LiquidationConfigurationDTO> GetOpeningLiquidationSubscriptionForStrategySPSubscriptionId(int id)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var subs = await context.LiquidationConfigurations.Where(t => t.StrategySPSubscriptionConfigId == id)
                  .Include(t => t.CoinPair).ThenInclude(c => c.PCoin)
                  .Include(t => t.CoinPair).ThenInclude(c => c.SCoin)
                  .Include(t => t.SP)
                  .Include(t => t.StrategySPSubscriptionConfig).ThenInclude(t => t.Strategy)
                  .Include(t => t.StrategySPSubscriptionConfig).ThenInclude(t => t.ExchangeDetails).ThenInclude(x => x.ApiKey)
                  .Include(t => t.StrategySPSubscriptionConfig).ThenInclude(t => t.ExchangeDetails).ThenInclude(x => x.OpeningExchangeBalance)
                  .Include(t => t.StrategySPSubscriptionConfig).ThenInclude(t => t.ExchangeDetails).ThenInclude(x => x.Venue)
                  .Include(t => t.LiquidationOrderLoadingConfiguration)
                  .ToListAsync();
               if (subs != null)
               {
                  return subs.FirstOrDefault().MapToDTO();
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in GetOpeningLiquidationSubscriptions - {Error}", e.Message);
            throw;
         }
      }

      public async Task UpdateOpeningLiquidationSubscription(LiquidationConfigurationDTO subscriptionDto)
      {
         try
         {
            var balances =  GetExchangeBalancesForInstance(subscriptionDto.StrategySPSubscriptionConfigId);
            using (var scope = _scopeFactory.CreateScope())
            {
               
               foreach (var bal in balances)
                  subscriptionDto.CoinAmount += bal.AmountToBeLiquidated;
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var subs = await context?.LiquidationConfigurations?.Where(c => c.Id == subscriptionDto.Id)
                  .ToListAsync();
               var chosen = subs.FirstOrDefault();
               var co = subscriptionDto.MapFromDTO();

               chosen = co;
               context?.Update(chosen);
               await context?.SaveChangesAsync();
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in UpdateOpeningLiquidationSubscription - {Error}", e.Message);
            throw;
         }
      }

      public async Task<LiquidationConfigurationDTO> CreateOpeningLiquidationSubscription(
         LiquidationConfigurationDTO subscriptionDto)
      {
         try
         {
            var balances =  GetExchangeBalancesForInstance(subscriptionDto.StrategySPSubscriptionConfigId);
            var sub = subscriptionDto.MapFromDTO();
            sub.CoinAmount = 0;

           
            foreach (var bal in balances)
               sub.CoinAmount += bal.AmountToBeLiquidated;

            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(sub);
               await context.SaveChangesAsync();
               var justAdded = await context.LiquidationConfigurations.Where(c => c.Id == sub.Id).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<LiquidationConfigurationDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in CreateOpeningLiquidationSubscription - {Error}", e.Message);
            throw;
         }
      }

      public async Task DeleteOpeningLiquidationSubscription(int Id)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var subs = await context?.LiquidationConfigurations.Where(s => s.Id == Id).ToListAsync();
               if (subs.Any())
               {
                  var sub = subs.First();
                  context.LiquidationConfigurations.Remove(sub);
                  await context.SaveChangesAsync();
               }
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in DeleteOpeningLiquidationSubscription - {Error}", e.Message);
            throw;
         }
      }

      public async Task<ExchangeCoinMappingsDTO> AddExchangeCoinLookup(ExchangeCoinMappingsDTO coinDTO)
      {
         var exchangeCoin = _mapper.Map<ExchangeCoinMappings>(coinDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(exchangeCoin);
               await context.SaveChangesAsync();
               var justAdded = await context.ExchangeCoinMappings.Where(c => c.ExchangeCoinName == exchangeCoin.ExchangeCoinName).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<ExchangeCoinMappingsDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing exchange coin to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing exchange coin to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing exchange coin to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing exchange coin to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<ExchangeCoinMappingsDTO>> GetExchangeCoinLookups()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<ExchangeCoinMappingsDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var coins = await context?.ExchangeCoinMappings.Include(c => c.Venue).Include(c => c.Coin)?.ToListAsync();
               if (coins != null)
               {
                  coins.ForEach(p => dtoList.Add(_mapper.Map<ExchangeCoinMappingsDTO>(p)));
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving exchange coins from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<ExchangeCoinMappingsDTO> GetExchangeCoinSymbolFromGenericCoinSymbol(string exchange, string genericSymbol)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<ExchangeCoinpairMappingDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var coins = await context?.ExchangeCoinMappings.Where(s => s.Venue.VenueName.Equals(exchange) && s.Coin.Name.Equals(genericSymbol))
                                            ?.ToListAsync();
               if (coins != null)
               {
                  return _mapper.Map<ExchangeCoinMappingsDTO>(coins.FirstOrDefault());
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving exchange coin lookups from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<ExchangeCoinMappingsDTO> GetGenericSymbolFromExchangeCoinSymbol(string exchange, string exchangeSymbol)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<ExchangeCoinpairMappingDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var coins = await context?.ExchangeCoinMappings.Where(s => s.Venue.VenueName.Equals(exchange) && s.Coin.Name.Equals(exchangeSymbol))
                                            ?.ToListAsync();
               if (coins != null)
               {
                  return _mapper.Map<ExchangeCoinMappingsDTO>(coins.FirstOrDefault());
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetGenericSymbolFromExchangeCoinSymbol exchange coin from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateExchangeCoin(ExchangeCoinMappingsDTO coinDTO)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.ExchangeCoinMappings.Where(s => s.Id == coinDTO.Id).ToListAsync();
               var selectedSP = res.FirstOrDefault();
               if (selectedSP != null)
               {
                  selectedSP.ExchangeCoinName = coinDTO.ExchangeCoinName;
                  selectedSP.VenueId = coinDTO.VenueId;
                  selectedSP.CoinId = coinDTO.CoinId;
                  context.ExchangeCoinMappings.Update(selectedSP);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("No such Exchange Coin {SP}", coinDTO.ExchangeCoinName);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating Exchange Coin {Coin}  {Error}", coinDTO.ExchangeCoinName, e.Message);
               throw;
               throw;
            }
         }
      }

      public async Task DeleteExchangeCoin(int Id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var exchanges = await context?.ExchangeCoinMappings.Where(s => s.Id == Id).ToListAsync();
               if (exchanges.Count() > 0)
               {
                  var exchange = exchanges.First();
                  context.ExchangeCoinMappings.Remove(exchange);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteExchangeCoin from database {Error}", e.Message);
               throw;
            }
         }
      }

      #region fairvalueconfig
      public async Task<FairValueConfigForUiDTO> CreateFairValueConfigForUI(FairValueConfigForUiDTO fairValueConfigDTO)
      {
         var exchangeCoin = _mapper.Map<FairValueConfigForUI>(fairValueConfigDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(exchangeCoin);
               await context.SaveChangesAsync();
               var justAdded = await context.FairValueConfigForUI.Where(c => c.CoinPairId == fairValueConfigDTO.CoinPairId && c.VenueId == fairValueConfigDTO.VenueId).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<FairValueConfigForUiDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing exchange coin to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing exchange coin to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing exchange coin to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing exchange coin to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<FairValueConfigForUiDTO>> GetFairValueConfigForUI()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<FairValueConfigForUiDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.FairValueConfigForUI.Include(c => c.Venue).Include(c => c.CoinPair)?.ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p => dtoList.Add(_mapper.Map<FairValueConfigForUiDTO>(p)));
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving exchange coins from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateFairValueConfig(FairValueConfigForUiDTO fairValueConfigDTO)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.FairValueConfigForUI.Where(s => s.Id == fairValueConfigDTO.Id).ToListAsync();
               var selectedSP = res.FirstOrDefault();
               if (selectedSP != null)
               {
                  selectedSP.CoinPairId = fairValueConfigDTO.CoinPairId;
                  selectedSP.VenueId = fairValueConfigDTO.VenueId;
                  selectedSP.UpdateIntervalSecs = fairValueConfigDTO.UpdateIntervalSecs;
                  context.FairValueConfigForUI.Update(selectedSP);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateFairValueConfig - No entry for fa");
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateFairValueConfig {Coin} {Venue} {Error}", fairValueConfigDTO.CoinPair.Name, fairValueConfigDTO.Venue.VenueName, e.Message);
               throw;
            }
         }
      }

      public async Task DeleteFairValueConfigForUI(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.FairValueConfigForUI.Where(s => s.Id == id).ToListAsync();
               if (configs.Count() > 0)
               {
                  var config = configs.First();
                  context.FairValueConfigForUI.Remove(config);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteFairValueConfigForUI from database {Error}", e.Message);
               throw;
            }
         }
      }
      #endregion

      #region fills
      public async Task<List<FillsInfoForInstance>> GetFillsInfoForInstance(string instance)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var fillsList = new List<FillsInfoForInstance>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var trades = await context.Trades.Where(t => t.InstanceName.Equals(instance) && !t.IsBuy).Include(t => t.Venue).ToListAsync();
               var groupedTrades =  trades.GroupBy(t => t.VenueId, t => t, (key, g) => new {VenueId = key, Trades = g.ToList()});
               foreach (var xx in groupedTrades)
               {
                  var venueFills = new FillsInfoForInstance()
                  {
                     VenueId = xx.VenueId,
                     TotalFills = xx.Trades.Count,
                     DailyFills = xx.Trades.Where(t => t.DateCreated.Date == DateTime.UtcNow.Date).Count(),
                     TotalLiquidated = xx.Trades.Sum(t => t.Quantity),
                     LiquidatedToday = xx.Trades.Where(t => t.DateCreated.Date == DateTime.UtcNow.Date).Sum(t => t.Quantity),
                     TotalStableEarned = xx.Trades.Sum(t => t.Quantity * t.Price),
                     TotalStableEarnedToday = xx.Trades.Where(t => t.DateCreated.Date == DateTime.UtcNow.Date).Sum(t => t.Quantity * t.Price)
                  };
                  fillsList.Add(venueFills);
               }
               return fillsList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error GetFillsInfoForInstance from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }
      #endregion

      #region makertakerfees

      

      
      public async Task<MakerTakerFeeDTO> CreateMakerTakerFee(MakerTakerFeeDTO makerTakerFeeDTO)
      {
         var exchangeCoin = _mapper.Map<MakerTakerFee>(makerTakerFeeDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(exchangeCoin);
               await context.SaveChangesAsync();
               var justAdded = await context.MakerTakerFees.Where(c => c.VenueId == makerTakerFeeDTO.VenueId && c.Mode == makerTakerFeeDTO.Mode).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<MakerTakerFeeDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateMakerTakerFee to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateMakerTakerFee to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateMakerTakerFee to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateMakerTakerFee to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<MakerTakerFeeDTO>> GetMakerTakerFees()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<MakerTakerFeeDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.MakerTakerFees.Include(c => c.Venue)?.ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p => dtoList.Add(_mapper.Map<MakerTakerFeeDTO>(p)));
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving exchange coins from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateMakerTakerFees(MakerTakerFeeDTO makerTakerFeeDTO)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.MakerTakerFees.Where(s => s.Id == makerTakerFeeDTO.Id).ToListAsync();
               var makerTaker = res.FirstOrDefault();
               if (makerTaker != null)
               {
                  makerTaker.VenueId = makerTakerFeeDTO.VenueId;
                  makerTaker.Mode = makerTakerFeeDTO.Mode;
                  makerTaker.MakerPercentage = makerTakerFeeDTO.MakerPercentage;
                  makerTaker.TakerPercentage = makerTakerFeeDTO.TakerPercentage;
                  context.MakerTakerFees.Update(makerTaker);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateMakerTakerFees - No entry for Id={Id}", makerTakerFeeDTO.Id);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateMakerTakerFees {Venue} {Error}",  makerTakerFeeDTO.Venue.VenueName, e.Message);
               throw;
            }
         }
      }

      public async Task DeleteMakerTakerFees(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var makerTaker = await context?.MakerTakerFees.Where(s => s.Id == id).ToListAsync();
               if (makerTaker.Count() > 0)
               {
                  var fees = makerTaker.First();
                  context.MakerTakerFees.Remove(fees);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteFairValueConfigForUI from database {Error}", e.Message);
               throw;
            }
         }
      }
      #endregion

      #region orders
      public async Task<OrderDTO> AddOrder(OrderDTO orderDTO)
      {
         var order = _mapper.Map<Order>(orderDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(order);
               await context.SaveChangesAsync();
               var justAdded = await context.Orders.Where(c => c.OrderId == order.OrderId).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<OrderDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Adding Order to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<OrderDTO>> GetOrdersForInstance(string instance)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               List<Order> orders = null;
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();

               orders = await context.Orders.Where(x => x.Instance == instance)
                  .Include(t => t.CoinPair)
                  .Include(t => t.SP)
                  .Include(t => t.Venue)
                  .OrderByDescending(x => x.OrderTime).ToListAsync();

               if (orders != null)
               {
                  var dtoList = new List<OrderDTO>();
                  orders.ForEach(c =>
                  {
                     dtoList.Add(_mapper.Map<OrderDTO>(c));
                  });
                  return dtoList;
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error GetOrdersForSP from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<List<OrderDTO>> GetOrdersForSP(int SpId)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               List<Order> orders = null;
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();

               orders = await context.Orders.Where(x => x.SPId == SpId)
                  .Include(t => t.CoinPair)
                  .Include(t => t.SP)
                  .Include(t => t.Venue)
                  .OrderByDescending(x => x.OrderTime).ToListAsync();

               if (orders != null)
               {
                  var dtoList = new List<OrderDTO>();
                  orders.ForEach(c =>
                  {
                     dtoList.Add(_mapper.Map<OrderDTO>(c));
                  });
                  return dtoList;
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error GetOrdersForSP from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }
      #endregion

      #region telegram

      #region telegramalerts
      public async Task<TelegramAlertDTO> CreateTelegramAlerts(TelegramAlertDTO alertDTO)
      {
         alertDTO.DateCreated = DateTime.UtcNow;
         var alert = _mapper.Map<TelegramAlert>(alertDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(alert);
               await context.SaveChangesAsync();
               var justAdded = await context.TelegramAlerts.Where(c => c.AlertName == alertDTO.AlertName).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<TelegramAlertDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateTelegramAlerts to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateTelegramAlerts to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateTelegramAlerts to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateTelegramAlerts to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<TelegramAlertDTO>> GetTelegramAlerts()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<TelegramAlertDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.TelegramAlerts.Include(x => x.AlertCategory)?.ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p => dtoList.Add(_mapper.Map<TelegramAlertDTO>(p)));
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetTelegramAlerts from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateTelegramAlert(TelegramAlertDTO alertDTO)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.TelegramAlerts.Where(s => s.TelegramAlertId == alertDTO.TelegramAlertId).ToListAsync();
               var alert = res.FirstOrDefault();
               if (alert != null)
               {
                  alert.AlertName = alertDTO.AlertName;
                  alert.Message = alertDTO.Message;
                  alert.AlertCategoryId = alertDTO.AlertCategoryId;
                  alert.AlertEnumId = alertDTO.AlertEnumId;
                  context.TelegramAlerts.Update(alert);
                 
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateTelegramAlert - No entry for Id={Id}", alertDTO.TelegramAlertId);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateTelegramAlert {AlertName} {Error}", alertDTO.AlertName, e.Message);
               throw;
            }
         }
      }

      public async Task DeleteTelegramAlert(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               if (context != null)
               {
                  var alerts = await context?.TelegramAlerts.Where(s => s.TelegramAlertId == id).ToListAsync();
                  if (alerts.Count() > 0)
                  {
                     var alert = alerts.First();
                     context.TelegramAlerts.Remove(alert);
                     await context.SaveChangesAsync();
                  }
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteTelegramAlert from database {Error}", e.Message);
               throw;
            }
         }
      }
      #endregion

      #region telegramalertcategories
      public async Task<TelegramAlertCategoryDTO> CreateTelegramAlertCategory(TelegramAlertCategoryDTO alertCategoryDTO)
      {
         
         var alert = _mapper.Map<TelegramAlertCategory>(alertCategoryDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(alert);
               await context.SaveChangesAsync();
               var justAdded = await context.TelegramAlertCategories.Where(c => c.Category == alertCategoryDTO.Category).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<TelegramAlertCategoryDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateTelegramAlertCategory to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateTelegramAlertCategory to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateTelegramAlertCategory to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateTelegramAlertCategory to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<TelegramAlertCategoryDTO>> GetTelegramAlertCategories()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<TelegramAlertCategoryDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.TelegramAlertCategories?.ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p => dtoList.Add(_mapper.Map<TelegramAlertCategoryDTO>(p)));
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetTelegramAlertCategories from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateTelegramAlertCategory(TelegramAlertCategoryDTO alertDTO)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.TelegramAlertCategories.Where(s => s.Id == alertDTO.Id).ToListAsync();
               var alert = res.FirstOrDefault();
               if (alert != null)
               {
                  alert.Category = alertDTO.Category;
                  

                  context.TelegramAlertCategories.Update(alert);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateTelegramAlertCategory - No entry for Id={Id}", alertDTO.Id);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateTelegramAlertCategory {AlertName} {Error}", alertDTO.Category, e.Message);
               throw;
            }
         }
      }

      public async Task DeleteTelegramAlertCategory(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var alerts = await context?.TelegramAlertCategories.Where(s => s.Id == id).ToListAsync();
               if (alerts.Count() > 0)
               {
                  var alert = alerts.First();
                  context.TelegramAlertCategories.Remove(alert);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteTelegramAlertCategory from database {Error}", e.Message);
               throw;
            }
         }
      }
      #endregion

      #region telegramalertstochannels
      public async Task<TelegramAlertToChannelDTO> CreateTelegramAlertToChannel(TelegramAlertToChannelDTO alertCategoryDTO)
      {
         alertCategoryDTO.DateCreated = DateTime.UtcNow;
         var alert = _mapper.Map<TelegramAlertToChannel>(alertCategoryDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(alert);
               await context.SaveChangesAsync();
               var justAdded = await context.TelegramAlertsToChannels.Where(c => c.TelegramAlertId == alertCategoryDTO.TelegramAlertId).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<TelegramAlertToChannelDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateTelegramAlertToChannel to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateTelegramAlertToChannel to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateTelegramAlertToChannel to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateTelegramAlertToChannel to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<TelegramAlertToChannelDTO>> GetTelegramAlertsToChannels()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<TelegramAlertToChannelDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.TelegramAlertsToChannels?.Include(x => x.TelegramAlert)
                  .Include(x => x.TelegramChannel)
                  .ToListAsync();
               if (configs != null)
               {
                  foreach (var alertThing in configs)
                  {
                     var dto = _mapper.Map<TelegramAlertToChannelDTO>(alertThing);
                     dto.TelegramChannel.ChannelName = dto.TelegramChannel.ChannelName.DecryptAES();
                     dto.TelegramChannel.TokenId = dto.TelegramChannel.TokenId.DecryptAES();
                     dtoList.Add(dto);
                  }
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetTelegramAlertsToChannels from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateTelegramAlertToChannel(TelegramAlertToChannelDTO alertDTO)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.TelegramAlertsToChannels.Where(s => s.TelegramAlertToChannelId == alertDTO.TelegramAlertToChannelId).ToListAsync();
               var alert = res.FirstOrDefault();
               if (alert != null)
               {
                  alert.TelegramAlertId = alertDTO.TelegramAlertId;
                  alert.TelegramChannelId = alertDTO.TelegramChannelId;


                  context.TelegramAlertsToChannels.Update(alert);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateTelegramAlertToChannel - No entry for Id={Id}", alertDTO.TelegramAlertToChannelId);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateTelegramAlertCategory {AlertId} {Error}", alertDTO.TelegramAlertId, e.Message);
               throw;
            }
         }
      }

      public async Task DeleteTelegramAlertToChannel(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var alerts = await context?.TelegramAlertsToChannels.Where(s => s.TelegramAlertToChannelId == id).ToListAsync();
               if (alerts.Count() > 0)
               {
                  var alert = alerts.First();
                  context.TelegramAlertsToChannels.Remove(alert);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteTelegramAlertToChannel from database {Error}", e.Message);
               throw;
            }
         }
      }
      #endregion

      #region telegramchannels
      public async Task<TelegramChannelDTO> CreateTelegramChannel(TelegramChannelDTO channelDTO)
      {
         channelDTO.ChannelName = channelDTO.ChannelName.EncryptAES();
         channelDTO.TokenId = channelDTO.TokenId.EncryptAES();

         channelDTO.DateCreated = DateTime.UtcNow;
         var alert = _mapper.Map<TelegramChannel>(channelDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(alert);
               await context.SaveChangesAsync();
               var justAdded = await context.TelegramChannels.Where(c => c.ChannelName == channelDTO.ChannelName).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<TelegramChannelDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateTelegramChannel to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateTelegramChannel to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateTelegramChannel to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateTelegramChannel to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<TelegramChannelDTO>> GetTelegramChannels()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<TelegramChannelDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.TelegramChannels?.ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p =>
                  {
                     p.ChannelName = p.ChannelName.DecryptAES();
                     p.TokenId = p.TokenId.DecryptAES();
                     dtoList.Add(_mapper.Map<TelegramChannelDTO>(p));
                  });
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetTelegramChannels from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<List<TelegramChannelDTO>> GetCommandTelegramChannels()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<TelegramChannelDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.TelegramChannels?.ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p =>
                  {
                     p.ChannelName = p.ChannelName.DecryptAES();
                     p.TokenId = p.TokenId.DecryptAES();
                     if (p.ChannelName.Contains("Command"))
                        dtoList.Add(_mapper.Map<TelegramChannelDTO>(p));
                  });
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetTelegramChannels from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateTelegramChannel(TelegramChannelDTO channelDTO)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.TelegramChannels.Where(s => s.TelegramChannelId == channelDTO.TelegramChannelId).ToListAsync();
               var channel = res.FirstOrDefault();
               if (channel != null)
               {
                  channel.ChannelName = channelDTO.ChannelName;
                  context.TelegramChannels.Update(channel);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateTelegramChannel - No entry for Id={Id}", channelDTO.TelegramChannelId);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateTelegramChannel {ChannelName} {Error}", channelDTO.ChannelName, e.Message);
               throw;
            }
         }
      }

      public async Task DeleteTelegramChannel(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var channels = await context?.TelegramChannels.Where(s => s.TelegramChannelId == id).ToListAsync();
               if (channels.Count() > 0)
               {
                  var channel = channels.First();
                  context.TelegramChannels.Remove(channel);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteTelegramChannel from database {Error}", e.Message);
               throw;
            }
         }
      }
      #endregion

      #region telegramcommandstousers
      public async Task<TelegramCommandToUserDTO> CreateTelegramCommandToUser(TelegramCommandToUserDTO commandToUserDTO)
      {
         commandToUserDTO.DateCreated = DateTime.UtcNow;
         var commandToUser = new TelegramCommandToUser();
         commandToUser.TelegramCommandId = commandToUserDTO.TelegramCommandId;
         commandToUser.TelegramUserId = commandToUserDTO.TelegramUserId;
         commandToUser.DateCreated = commandToUserDTO.DateCreated;
         commandToUser.IsAuthorised = commandToUserDTO.IsAuthorised;

         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(commandToUser);
               await context.SaveChangesAsync();
               var justAdded = await context.TelegramCommandToUsers.Where(c => c.TelegramCommandId == commandToUserDTO.TelegramCommandId && c.TelegramUserId == commandToUserDTO.TelegramUserId).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map <TelegramCommandToUserDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateTelegramCommandToUser to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateTelegramCommandToUser to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateTelegramCommandToUser to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateTelegramCommandToUser to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<TelegramCommandToUserDTO>> GetTelegramCommandToUsers()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<TelegramCommandToUserDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.TelegramCommandToUsers?.Include(x => x.TelegramCommand)
                  .Include(x => x.TelegramUser).ToListAsync();
               if (configs != null)
               {
                  foreach (var config in configs)
                  {
                     var dto = new TelegramCommandToUserDTO();
                     dto.TelegramUser.UserName = config.TelegramUser.UserName.DecryptAES();
                     dto.TelegramUser.UserToken = config.TelegramUser.UserToken.DecryptAES();
                     dto.TelegramUserId = config.TelegramUserId;
                     dto.IsAuthorised = config.IsAuthorised;
                     dto.DateCreated = config.DateCreated;
                     dto.TelegramCommandId = config.TelegramCommandId;
                     dto.TelegramCommandToUserId = config.TelegramCommandToUserId;
                     dto.TelegramCommand.TelegramCommandText = config.TelegramCommand.TelegramCommandText;
                     dtoList.Add(dto);
                  }

                 //
                 // configs.ForEach(p => dtoList.Add(_mapper.Map<TelegramCommandToUserDTO>(p)));
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetTelegramCommandToUsers from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateTelegramCommandToUser(TelegramCommandToUserDTO commandToUserDTO)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.TelegramCommandToUsers.Where(s => s.TelegramCommandToUserId == commandToUserDTO.TelegramCommandToUserId).ToListAsync();
               var channel = res.FirstOrDefault();
               if (channel != null)
               {
                  channel.TelegramCommandId = commandToUserDTO.TelegramCommandId;
                  channel.TelegramUserId = commandToUserDTO.TelegramUserId;
                  channel.IsAuthorised = commandToUserDTO.IsAuthorised;
                  context.TelegramCommandToUsers.Update(channel);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateTelegramCommandToUser - No entry for Id={Id}", commandToUserDTO.TelegramCommandToUserId);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateTelegramCommandToUser {AlertName} {Error}", commandToUserDTO.TelegramCommandToUserId, e.Message);
               throw;
            }
         }
      }

      public async Task DeleteTelegramCommandToUser(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var channels = await context?.TelegramCommandToUsers.Where(s => s.TelegramCommandToUserId == id).ToListAsync();
               if (channels.Count() > 0)
               {
                  var channel = channels.First();
                  context.TelegramCommandToUsers.Remove(channel);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteTelegramCommandToUser from database {Error}", e.Message);
               throw;
            }
         }
      }
      #endregion
      #region telegramcommandtypes

      public async Task<TelegramCommandTypeDTO> CreateTelegramCommandType(TelegramCommandTypeDTO commandTypeDTO)
      {
        
         var commandType = _mapper.Map<TelegramCommandType>(commandTypeDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(commandType);
               await context.SaveChangesAsync();
               var justAdded = await context.TelegramCommandTypes.Where(c => c.Category == commandTypeDTO.Category).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<TelegramCommandTypeDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateTelegramCommandType to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateTelegramCommandType to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateTelegramCommandType to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateTelegramCommandType to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<TelegramCommandTypeDTO>> GetTelegramCommandTypes()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<TelegramCommandTypeDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.TelegramCommandTypes?.ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p => dtoList.Add(_mapper.Map<TelegramCommandTypeDTO>(p)));
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetTelegramCommandTypes from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateTelegramCommandType(TelegramCommandTypeDTO commandTypeDTO)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.TelegramCommandTypes.Where(s => s.TelegramCommandTypeId == commandTypeDTO.TelegramCommandTypeId).ToListAsync();
               var commandType = res.FirstOrDefault();
               if (commandType != null)
               {
                  commandType.Category = commandTypeDTO.Category;


                  context.TelegramCommandTypes.Update(commandType);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateTelegramCommandType - No entry for Id={Id}", commandTypeDTO.TelegramCommandTypeId);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateTelegramCommandType {AlertName} {Error}", commandTypeDTO.Category, e.Message);
               throw;
            }
         }
      }

      public async Task DeleteTelegramCommandType(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var commandTypes = await context?.TelegramCommandTypes.Where(s => s.TelegramCommandTypeId == id).ToListAsync();
               if (commandTypes.Count() > 0)
               {
                  var commandType = commandTypes.First();
                  context.TelegramCommandTypes.Remove(commandType);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteTelegramCommandType from database {Error}", e.Message);
               throw;
            }
         }
      }

      #endregion

      #region telegramcommands
      public async Task<TelegramCommandDTO> CreateTelegramCommand(TelegramCommandDTO commandDTO)
      {
         commandDTO.DateCreated = DateTime.UtcNow;
         var command = _mapper.Map<TelegramCommand>(commandDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(command);
               await context.SaveChangesAsync();
               var justAdded = await context.TelegramCommands.Where(c => c.TelegramCommandText == commandDTO.TelegramCommandText).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<TelegramCommandDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateTelegramCommand to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateTelegramCommand to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateTelegramCommand to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateTelegramCommand to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<TelegramCommandDTO>> GetTelegramCommands()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<TelegramCommandDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.TelegramCommands.Include(x => x.TelegramCommandType)?.ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p => dtoList.Add(_mapper.Map<TelegramCommandDTO>(p)));
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetTelegramCommandss from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<List<TelegramCommandDTO>> GetTelegramLiquidationConfigurationCommands()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<TelegramCommandDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.TelegramCommands.Where(x => x.TelegramCommandType.Category == "Configuration").Include(x => x.TelegramCommandType)?.ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p => dtoList.Add(_mapper.Map<TelegramCommandDTO>(p)));
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetTelegramLiquidationConfigurationCommands from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }
      public async Task UpdateTelegramCommand(TelegramCommandDTO commandDTO)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.TelegramCommands.Where(s => s.TelegramCommandId == commandDTO.TelegramCommandId).ToListAsync();
               var command = res.FirstOrDefault();
               if (command != null)
               {
                  command.TelegramCommandText = commandDTO.TelegramCommandText;
                  command.TelegramCommandTypeId = commandDTO.TelegramCommandTypeId;
                  context.TelegramCommands.Update(command);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateTelegramCommand - No entry for Id={Id}", commandDTO.TelegramCommandId);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateTelegramCommand {AlertName} {Error}", commandDTO.TelegramCommandText, e.Message);
               throw;
            }
         }
      }

      public async Task DeleteTelegramCommand(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var commands = await context?.TelegramCommands.Where(s => s.TelegramCommandId == id).ToListAsync();
               if (commands.Count() > 0)
               {
                  var command = commands.First();
                  context.TelegramCommands.Remove(command);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteTelegramCommand from database {Error}", e.Message);
               throw;
            }
         }
      }
      #endregion

      #region telegramsubscriberstochannels
      public async Task<TelegramSubscriberToChannelDTO> CreateTelegramSubscriberToChannel(TelegramSubscriberToChannelDTO subscriberToChannelDTO)
      {
         subscriberToChannelDTO.DateCreated = DateTime.UtcNow;
         var subscriberToChannel = _mapper.Map<TelegramSubscriberToChannel>(subscriberToChannelDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(subscriberToChannel);
               await context.SaveChangesAsync();
               var justAdded = await context.TelegramSubscriberToChannels.Where(c => c.TelegramChannelId == subscriberToChannelDTO.TelegramChannelId && c.TelegramUserId == subscriberToChannelDTO.TelegramUserId)
                  .Include(x => x.TelegramChannel)
                  .Include(x => x.TelegramUser)
                  .ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  var dto =  _mapper.Map<TelegramSubscriberToChannelDTO>(justAdded.FirstOrDefault());
                  dto.TelegramChannel.ChannelName = dto.TelegramChannel.ChannelName.DecryptAES();
                  dto.TelegramChannel.TokenId = dto.TelegramChannel.TokenId.DecryptAES();
                  return dto;
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateTelegramSubscriberToChannel to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateTelegramSubscriberToChannel to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateTelegramSubscriberToChannel to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateTelegramSubscriberToChannel to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<TelegramSubscriberToChannelDTO>> GetTelegramSubscriberToChannels()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<TelegramSubscriberToChannelDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.TelegramSubscriberToChannels.Include(x => x.TelegramChannel)
                  .Include(x => x.TelegramUser)?.ToListAsync();
               if (configs != null)
               {
                  foreach (var config in configs)
                  {
                     var dto = _mapper.Map<TelegramSubscriberToChannelDTO>(config);
                   //  var dto = new TelegramSubscriberToChannelDTO();
                    // dto.DateCreated = config.DateCreated;
                 //    dto.IsAuthorised = config.IsAuthorised;
                  //   dto.TelegramSubscriberToChannelId = config.TelegramSubscriberToChannelId;
                     dto.TelegramChannel.ChannelName = config.TelegramChannel.ChannelName.DecryptAES();
                //     dto.TelegramChannelId = config.TelegramChannelId;
                 //    dto.TelegramUserId = config.TelegramUserId;
                     dto.TelegramUser.UserName = config.TelegramUser.UserName.DecryptAES();
                     dto.TelegramUser.UserToken = config.TelegramUser.UserToken.DecryptAES();
                     dtoList.Add(dto);
                  }
                  //configs.ForEach(p => dtoList.Add(_mapper.Map<TelegramSubscriberToChannelDTO>(p)));
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetTelegramSubscriberToChannels from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateTelegramSubscriberToChannel(TelegramSubscriberToChannelDTO subscriberToChannelDTO)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.TelegramSubscriberToChannels.Where(s => s.TelegramSubscriberToChannelId == subscriberToChannelDTO.TelegramSubscriberToChannelId).ToListAsync();
               var subscriberToChannel = res.FirstOrDefault();
               if (subscriberToChannel != null)
               {
                  subscriberToChannel.TelegramChannelId = subscriberToChannelDTO.TelegramChannelId;
                  subscriberToChannel.TelegramUserId = subscriberToChannelDTO.TelegramUserId;
                  subscriberToChannel.IsAuthorised = subscriberToChannelDTO.IsAuthorised;
                  context.TelegramSubscriberToChannels.Update(subscriberToChannel);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateTelegramSubscriberToChannel - No entry for Id={Id}", subscriberToChannelDTO.TelegramSubscriberToChannelId);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateTelegramSubscriberToChannel {AlertName} {Error}", subscriberToChannelDTO.TelegramSubscriberToChannelId, e.Message);
               throw;
            }
         }
      }

      public async Task DeleteTelegramSubscriberToChannel(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var subscriberToChannels = await context?.TelegramSubscriberToChannels.Where(s => s.TelegramSubscriberToChannelId == id).ToListAsync();
               if (subscriberToChannels.Count() > 0)
               {
                  var subscriberToChannel = subscriberToChannels.First();
                  context.TelegramSubscriberToChannels.Remove(subscriberToChannel);
                  await context.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteTelegramSubscriberToChannel from database {Error}", e.Message);
               throw;
            }
         }
      }
      #endregion

      #region telegramusers
      public async Task<TelegramUserDTO> CreateTelegramUser(TelegramUserDTO userDTO)
      {
         userDTO.DateCreated = DateTime.UtcNow;
         userDTO.UserName = userDTO.UserName.EncryptAES();
         userDTO.UserToken = userDTO.UserToken.EncryptAES();

         var user = _mapper.Map<TelegramUser>(userDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(user);
               await context.SaveChangesAsync();
               var justAdded = await context.TelegramUsers.Where(c => c.UserName == userDTO.UserName).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<TelegramUserDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateTelegramUser to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateTelegramUser to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateTelegramUser to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateTelegramUser to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<TelegramUserDTO>> GetTelegramUsers()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<TelegramUserDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.TelegramUsers?.ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p =>
                  {
                     p.UserName = p.UserName.DecryptAES();
                     p.UserToken = p.UserToken.DecryptAES();
                     dtoList.Add(_mapper.Map<TelegramUserDTO>(p));
                  });
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetTelegramUsers from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateTelegramUser(TelegramUserDTO userDTO)
      {
         userDTO.UserName = userDTO.UserName.EncryptAES();
         userDTO.UserToken = userDTO.UserToken.EncryptAES();
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.TelegramUsers.Where(s => s.Id == userDTO.Id).ToListAsync();
               var user = res.FirstOrDefault();
               if (user != null)
               {
                  user.UserName = userDTO.UserName;
                  user.UserToken = userDTO.UserToken;
                  context.TelegramUsers.Update(user);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateTelegramUser - No entry for Id={Id}", userDTO.Id);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateTelegramUser {AlertName} {Error}", userDTO.UserName, e.Message);
               throw;
            }
         }
      }

      public async Task DeleteTelegramUser(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var users = await context?.TelegramUsers.Where(s => s.Id == id).ToListAsync();
               if (users.Count() > 0)
               {
                  var user = users.First();
                  context?.TelegramUsers.Remove(user);
                  await context?.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteTelegramUser from database {Error}", e.Message);
               throw;
            }
         }
      }
      #endregion

      #region liquidationorderloadingconfiguration
      public async Task<LiquidationOrderLoadingConfigurationDTO> CreateLiquidationOrderLoadingConfiguration(LiquidationOrderLoadingConfigurationDTO 
                        dto)
      {
         var config = _mapper.Map<LiquidationOrderLoadingConfiguration>(dto);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(config);
               await context.SaveChangesAsync();
               var justAdded = await context.LiquidationOrderLoadingConfigurations.Where(c => c.StartPercentage == dto.StartPercentage && c.ScalingFactor == dto.ScalingFactor).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<LiquidationOrderLoadingConfigurationDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateLiquidationOrderLoadingConfiguration to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateLiquidationOrderLoadingConfiguration to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateLiquidationOrderLoadingConfiguration to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateLiquidationOrderLoadingConfiguration to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<LiquidationOrderLoadingConfigurationDTO>> GetLiquidationOrderLoadingConfiguration()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<LiquidationOrderLoadingConfigurationDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.LiquidationOrderLoadingConfigurations?.ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p =>
                  {
                  
                     dtoList.Add(_mapper.Map<LiquidationOrderLoadingConfigurationDTO>(p));
                  });
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetLiquidationOrderLoadingConfiguration from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateLiquidationOrderLoadingConfiguration(LiquidationOrderLoadingConfigurationDTO dto)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.LiquidationOrderLoadingConfigurations.Where(s => s.Id == dto.Id).ToListAsync();
               var config = res.FirstOrDefault();
               if (config != null)
               {
                  
                  context.LiquidationOrderLoadingConfigurations.Update(config);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateLiquidationOrderLoadingConfiguration - No entry for Id={Id}", dto.Id);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateLiquidationOrderLoadingConfiguration {Error} ", e.Message);
               throw;
            }
         }
      }

      public async Task DeleteLiquidationOrderLoadingConfiguration(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.LiquidationOrderLoadingConfigurations.Where(s => s.Id == id).ToListAsync();
               if (configs.Any())
               {
                  var config = configs.First();
                  context?.LiquidationOrderLoadingConfigurations.Remove(config);
                  await context?.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteLiquidationOrderLoadingConfiguration from database {Error}", e.Message);
               throw;
            }
         }
      }

      public async Task<LiquidationManualOrderLoadingDTO> CreateLiquidationManualOrderLoading(LiquidationManualOrderLoadingDTO dto)
      {
         var config = _mapper.Map<LiquidationManualOrderLoading>(dto);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(config);
               await context.SaveChangesAsync();
               var justAdded = await context.LiquidationManualOrderLoadings.Where(c => c.StrategySPSubscriptionConfigId == dto.StrategySPSubscriptionConfigId ).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<LiquidationManualOrderLoadingDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateLiquidationManualOrderLoading to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateLiquidationManualOrderLoading to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateLiquidationManualOrderLoading to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateLiquidationManualOrderLoading to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<LiquidationManualOrderLoadingDTO>> GetLiquidationManualOrderLoading()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<LiquidationManualOrderLoadingDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.LiquidationManualOrderLoadings?.
                  Include(x => x.StrategyInstance)
                  .ThenInclude(y => y.SP)
                  .ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p =>
                  {

                     dtoList.Add(_mapper.Map<LiquidationManualOrderLoadingDTO>(p));
                  });
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetLiquidationManualOrderLoading from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<List<LiquidationManualOrderLoadingDTO>> GetLiquidationManualOrderLoadingForInstance(int InstanceId)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<LiquidationManualOrderLoadingDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.LiquidationManualOrderLoadings?.
                  Where(x => x.StrategySPSubscriptionConfigId == InstanceId)
                  .Include(x => x.StrategyInstance)
                  .ThenInclude(y => y.SP)
                  .ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p =>
                  {

                     dtoList.Add(_mapper.Map<LiquidationManualOrderLoadingDTO>(p));
                  });
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetLiquidationManualOrderLoading from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }


      public async Task UpdateLiquidationManualOrderLoadingConfiguration(LiquidationManualOrderLoadingDTO dto)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.LiquidationManualOrderLoadings.Where(s => s.Id == dto.Id).ToListAsync();
               var config = res.FirstOrDefault();
               if (config != null)
               {
                  config.OrderNo = dto.OrderNo;
                  config.Percentage = dto.Percentage;
                  config.StrategySPSubscriptionConfigId = dto.StrategySPSubscriptionConfigId;
                  context.LiquidationManualOrderLoadings.Update(config);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateLiquidationManualOrderLoadingConfiguration - No entry for Id={Id}", dto.Id);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateLiquidationManualOrderLoadingConfiguration {Error} ", e.Message);
               throw;
            }
         }
      }

      public async Task DeleteLiquidationManualOrderLoadingConfiguration(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.LiquidationManualOrderLoadings.Where(s => s.Id == id).ToListAsync();
               if (configs.Any())
               {
                  var config = configs.First();
                  context?.LiquidationManualOrderLoadings.Remove(config);
                  await context?.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteLiquidationManualOrderLoadingConfiguration from database {Error}", e.Message);
               throw;
            }
         }
      }
      #endregion
      #region exchangebalances

      public async Task<OpeningExchangeBalanceDTO> CreateExchangeBalance(OpeningExchangeBalanceDTO dto)
      {
         dto.Created = DateTime.UtcNow;
         var config = _mapper.Map<OpeningExchangeBalance>(dto);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(config);
               await context.SaveChangesAsync();
               var justAdded =  context.OpeningExchangeBalances.OrderByDescending(c => c.OpeningExchangeBalanceId).FirstOrDefault();
               if (justAdded != null )
               {
                  return _mapper.Map<OpeningExchangeBalanceDTO>(justAdded);
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateExchangeBalance to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateExchangeBalance to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateExchangeBalance to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateExchangeBalance to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<OpeningExchangeBalanceDTO>> GetExchangeBalances()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<OpeningExchangeBalanceDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.OpeningExchangeBalances?
                  .ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p =>
                  {

                     dtoList.Add(_mapper.Map<OpeningExchangeBalanceDTO>(p));
                  });
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetExchangeBalances from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public  List<OpeningExchangeBalanceDTO> GetExchangeBalancesForInstance(int InstanceId)
      {
         // InstanceId will refer to the SubscriptionConfigId 
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<OpeningExchangeBalanceDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var exchangeDetails = context?.ExchangeDetails?.Where(x => x.StrategySPSubscriptionConfigId == InstanceId);
               
               if (exchangeDetails != null)
               {
                  foreach (var exchange in exchangeDetails)
                  {
                     var exchangeBalances = context?.OpeningExchangeBalances
                        ?.Where(x => x.OpeningExchangeBalanceId == exchange.OpeningExchangeBalanceId)
                        .ToList();
                     if (exchangeBalances != null)
                     {
                        exchangeBalances.ForEach(ex =>
                        {
                           dtoList.Add(_mapper.Map<OpeningExchangeBalanceDTO>(ex));
                        });
                     }
                  }
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError("Error in GetExchangeBalancesForInstance {Error}", e.Message);
            throw e;
         }
      }

      public async Task UpdateExchangeBalance(OpeningExchangeBalanceDTO dto)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.OpeningExchangeBalances.Where(s => s.OpeningExchangeBalanceId == dto.OpeningExchangeBalanceId).ToListAsync();
               var config = res.FirstOrDefault();
               if (config != null)
               {
                  config.Description = dto.Description;
                  config.LiquidatingFromCurrency = dto.LiquidatingFromCurrency;
                  config.LiquidatingToCurrency = dto.LiquidatingToCurrency;
                  config.AmountToBeLiquidated = dto.AmountToBeLiquidated;
                  config.LiquidatingFromOpeningBalance = dto.LiquidatingFromOpeningBalance;
                  config.LiquidatingToOpeningBalance = dto.LiquidatingToOpeningBalance;
                  config.Created = DateTime.UtcNow;
                //  config.ExchangeDetailsId = dto.ExchangeDetailsId;
                  context.OpeningExchangeBalances.Update(config);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateExchangeBalance - No entry for Id={Id}", dto.OpeningExchangeBalanceId);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateExchangeBalance {Error} ", e.Message);
               throw;
            }
         }
      }

      public async Task DeleteExchangeBalance(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.OpeningExchangeBalances.Where(s => s.OpeningExchangeBalanceId == id).ToListAsync();
               if (configs.Any())
               {
                  var config = configs.First();
                  context?.OpeningExchangeBalances.Remove(config);
                  await context?.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteExchangeBalance from database {Error}", e.Message);
               throw;
            }
         }
      }

      #endregion
      #endregion
       
      #region funds
      public async Task<FundDTO> CreateFund(FundDTO fundDTO)
      {
         fundDTO.DateCreated = DateTime.UtcNow;
         var fund = _mapper.Map<Fund>(fundDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(fund);
               await context.SaveChangesAsync();
               var justAdded = await context.Funds.Where(c => c.FundName == fundDTO.FundName).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<FundDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateFund to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateFund to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateFund to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateFund to database {Error}", e.Message);
            throw;
         }
      }
      public async Task<List<FundDTO>> GetFunds()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<FundDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.Funds?.Include(f => f.Location).ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p =>
                  {
                    
                     dtoList.Add(_mapper.Map<FundDTO>(p));
                  });
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetFunds from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task<FundDTO> GetFundById(int id)
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.Funds?.Where(x => x.FundId == id)?.Include(f => f.Location).ToListAsync();
               if (configs != null)
               {
                  if (configs.Any())
                  {
                     return _mapper.Map<FundDTO>(configs.FirstOrDefault());
                  }
               }
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetFundById from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }
      public async Task UpdateFund(FundDTO fundDTO)
      {
        
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.Funds.Where(s => s.FundId == fundDTO.FundId).ToListAsync();
               var fund = res.FirstOrDefault();
               if (fund != null)
               {
                  fund.FundName = fundDTO.FundName;
                
                  context.Funds.Update(fund);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateFund - No entry for Id={Id}", fundDTO.FundId);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateFund {AlertName} {Error}", fundDTO.FundName, e.Message);
               throw;
            }
         }
      }
      public async Task DeleteFund(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var funds = await context?.Funds.Where(s => s.FundId == id).ToListAsync();
               if (funds.Count() > 0)
               {
                  var fund = funds.First();
                  context?.Funds.Remove(fund);
                  await context?.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteFund from database {Error}", e.Message);
               throw;
            }
         }
      }

      #endregion

      #region locations
      public async Task<LocationDTO> CreateLocation(LocationDTO locationDTO)
      {
         locationDTO.DateCreated = DateTime.UtcNow;
         var location = _mapper.Map<Location>(locationDTO);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(location);
               await context.SaveChangesAsync();
               var justAdded = await context.Locations.Where(c => c.LocationName == locationDTO.LocationName).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<LocationDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateLocation to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateLocation to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateLocation to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateLocation to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<LocationDTO>> GetLocations()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<LocationDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.Locations?.ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p =>
                  {

                     dtoList.Add(_mapper.Map<LocationDTO>(p));
                  });
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetLocations from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateLocation(LocationDTO locationDTO)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.Locations.Where(s => s.LocationId == locationDTO.LocationId).ToListAsync();
               var location = res.FirstOrDefault();
               if (location != null)
               {
                  location.LocationName = locationDTO.LocationName;

                  context.Locations.Update(location);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateLocation - No entry for Id={Id}", locationDTO.LocationId);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateLocation {AlertName} {Error}", locationDTO.LocationName, e.Message);
               throw;
            }
         }
      }

      public async Task DeleteLocation(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var locations = await context?.Locations.Where(s => s.LocationId == id).ToListAsync();
               if (locations.Count() > 0)
               {
                  var location = locations.First();
                  context?.Locations.Remove(location);
                  await context?.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteFund from database {Error}", e.Message);
               throw;
            }
         }
      }
      #endregion

      #region telegramalertbehaviortypes

      public async Task<TelegramAlertBehaviourTypeDTO> CreateTelegramAlertBehaviourType(TelegramAlertBehaviourTypeDTO dto)
      {
        
         var behaviour = _mapper.Map<TelegramAlertBehaviourType>(dto);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(behaviour);
               await context.SaveChangesAsync();
               var justAdded = await context.TelegramAlertBehaviourTypes.Where(c => c.Name == dto.Name).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<TelegramAlertBehaviourTypeDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateTelegramAlertBehaviourType to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateTelegramAlertBehaviourType to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateTelegramAlertBehaviourType to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateTelegramAlertBehaviourType to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<TelegramAlertBehaviourTypeDTO>> GetTelegramAlertBehaviourTypes()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<TelegramAlertBehaviourTypeDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.TelegramAlertBehaviourTypes?.ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p =>
                  {

                     dtoList.Add(_mapper.Map<TelegramAlertBehaviourTypeDTO>(p));
                  });
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetTelegramAlertBehaviourTypes from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateTelegramAlertBehaviourType(TelegramAlertBehaviourTypeDTO dto)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.TelegramAlertBehaviourTypes.Where(s => s.Id == dto.Id).ToListAsync();
               var behaviourType = res.FirstOrDefault();
               if (behaviourType != null)
               {
                  behaviourType.Name = dto.Name;
                  behaviourType.EnumId = dto.EnumId;

                  context.TelegramAlertBehaviourTypes.Update(behaviourType);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateTelegramAlertBehaviourType - No entry for Id={Id}", dto.Id);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateTelegramAlertBehaviourType {AlertName} {Error}", dto.Name, e.Message);
               throw;
            }
         }
      }

      public async Task DeleteTelegramAlertBehaviourType(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var behaviourTypes = await context?.TelegramAlertBehaviourTypes.Where(s => s.Id == id).ToListAsync();
               if (behaviourTypes.Count() > 0)
               {
                  var behaviourType = behaviourTypes.First();
                  context?.TelegramAlertBehaviourTypes.Remove(behaviourType);
                  await context?.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteTelegramAlertBehaviourType from database {Error}", e.Message);
               throw;
            }
         }
      }

      #endregion

      #region telegramalertbehaviours

      public async Task<TelegramAlertBehaviourDTO> CreateTelegramAlertBehaviour(TelegramAlertBehaviourDTO dto)
      {
         var behaviour = _mapper.Map<TelegramAlertBehaviour>(dto);
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               context.Add(behaviour);
               await context.SaveChangesAsync();
               var justAdded = await context.TelegramAlertBehaviours.Where(c => c.TelegramAlertId == dto.TelegramAlertId).ToListAsync();
               if (justAdded != null && justAdded.Any())
               {
                  return _mapper.Map<TelegramAlertBehaviourDTO>(justAdded.FirstOrDefault());
               }
               return null;
            }
         }
         catch (DbUpdateConcurrencyException e)
         {
            _logger.LogError(e, "DbUpdateConcurrencyException - Error writing CreateTelegramAlertBehaviour to database {Error}", e.Message);
            throw;
         }
         catch (DbUpdateException e)
         {
            _logger.LogError(e, "DbUpdateException - Error writing CreateTelegramAlertBehaviour to database {Error}", e.Message);
            throw;
         }
         catch (OperationCanceledException e)
         {
            _logger.LogError(e, "OperationCanceledException - Error writing CreateTelegramAlertBehaviour to database {Error}", e.Message);
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error writing CreateTelegramAlertBehaviour to database {Error}", e.Message);
            throw;
         }
      }

      public async Task<List<TelegramAlertBehaviourDTO>> GetTelegramAlertBehaviours()
      {
         try
         {
            using (var scope = _scopeFactory.CreateScope())
            {
               var dtoList = new List<TelegramAlertBehaviourDTO>();
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var configs = await context?.TelegramAlertBehaviours?.ToListAsync();
               if (configs != null)
               {
                  configs.ForEach(p =>
                  {

                     dtoList.Add(_mapper.Map<TelegramAlertBehaviourDTO>(p));
                  });
               }
               return dtoList;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Retrieving GetTelegramAlertBehaviours from db {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task UpdateTelegramAlertBehaviour(TelegramAlertBehaviourDTO dto)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var res = await context.TelegramAlertBehaviours.Where(s => s.Id == dto.Id).ToListAsync();
               var behaviour = res.FirstOrDefault();
               if (behaviour != null)
               {
                  behaviour.TelegramAlertId = dto.TelegramAlertId;
                  behaviour.TelegramAlertBehaviourTypeId = dto.TelegramAlertBehaviourTypeId;
                  context.TelegramAlertBehaviours.Update(behaviour);
                  await context.SaveChangesAsync();
               }
               else
               {
                  _logger.LogWarning("UpdateTelegramAlertBehaviour - No entry for Id={Id}", dto.Id);
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error updating UpdateTelegramAlertBehaviour {AlertId} {Error}", dto.Id, e.Message);
               throw;
            }
         }
      }

      public async Task DeleteTelegramAlertBehaviour(int id)
      {
         using (var scope = _scopeFactory.CreateScope())
         {
            try
            {
               var context = scope.ServiceProvider.GetRequiredService<LiquidityDbContext>();
               var behaviours = await context?.TelegramAlertBehaviours.Where(s => s.Id == id).ToListAsync();
               if (behaviours.Count() > 0)
               {
                  var behaviour = behaviours.First();
                  context?.TelegramAlertBehaviours.Remove(behaviour);
                  await context?.SaveChangesAsync();
               }
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error deleting DeleteTelegramAlertBehaviour from database {Error}", e.Message);
               throw;
            }
         }
      }
      #endregion
   }
}
