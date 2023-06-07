
using Common.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
//using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore
{

   public class LiquidityDbContext : DbContext
   {
      public DbSet<Trade> Trades { get; set; }
      public DbSet<Balance> Balances { get; set; }
      public DbSet<Coin> Coins { get; set; }
      public DbSet<CoinPair> CoinPairs { get; set; }
      public DbSet<SP> SPs { get; set; }
      public DbSet<ExchangeDetails> ExchangeDetails { get; set; }
      public DbSet<ApiKey> ApiKeys { get; set; }
      public DbSet<Venue> Venues { get; set; }
      public DbSet<ConfigSetting> ConfigSettings { get; set; }
      public DbSet<Strategy> Strategies { get; set; }
      public DbSet<StrategySPSubscriptionConfig> StrategySPSubscriptionConfigs { get; set; }
      public DbSet<ExchangeCoinpairMapping> ExchangeCoinPairMappings { get; set; }
      public DbSet<ExchangeCoinMappings> ExchangeCoinMappings { get; set; }
      public DbSet<LiquidationStrategyConfig> LiquidationStrategyConfigs { get; set; }
      public DbSet<LiquidationTracker> LiquidationTrackers { get; set; }
      public DbSet<LiquidationConfiguration> LiquidationConfigurations { get; set; }
      public DbSet<FairValueConfigForUI> FairValueConfigForUI { get; set; }
      public DbSet<MakerTakerFee> MakerTakerFees { get; set; }
      public DbSet<Order> Orders { get; set; }
      public DbSet<TelegramAlert> TelegramAlerts { get; set; }
      public DbSet<TelegramChannel> TelegramChannels { get; set; }
      public DbSet<TelegramCommandToUser> TelegramCommandToUsers { get; set; }
      public DbSet<TelegramCommandType> TelegramCommandTypes { get; set; }
      public DbSet<TelegramAlertCategory> TelegramAlertCategories { get; set; }
      public DbSet<TelegramSubscriberToChannel> TelegramSubscriberToChannels { get; set; }
      public DbSet<TelegramCommand> TelegramCommands { get; set; }
      public DbSet<TelegramAlertToChannel> TelegramAlertsToChannels { get; set; }
      public DbSet<TelegramUser> TelegramUsers { get; set; }
      public DbSet<LiquidationOrderLoadingConfiguration> LiquidationOrderLoadingConfigurations { get; set; }
      public DbSet<LiquidationManualOrderLoading> LiquidationManualOrderLoadings { get; set; }
      public DbSet<OpeningExchangeBalance> OpeningExchangeBalances { get; set; }
      public DbSet<Fund> Funds { get; set; }
      public DbSet<Location> Locations { get; set; }
      public DbSet<TelegramAlertBehaviour> TelegramAlertBehaviours { get; set; }
      public DbSet<TelegramAlertBehaviourType> TelegramAlertBehaviourTypes { get; set; }
      public LiquidityDbContext()
      {

      }
      public LiquidityDbContext(DbContextOptions<LiquidityDbContext> options) : base(options)
      {
         
      }
      protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
      {
         optionsBuilder.EnableSensitiveDataLogging();
         optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
         base.OnConfiguring(optionsBuilder);
      }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         modelBuilder.Entity<TelegramChannel>()
            .HasIndex(b => b.ChannelName)
            .IsUnique();

         modelBuilder.Entity<TelegramAlert>()
            .HasIndex(b => b.AlertName)
            .IsUnique();

         modelBuilder.Entity<TelegramAlertCategory>()
            .HasIndex(b => b.Category)
            .IsUnique();

         modelBuilder.Entity<TelegramCommand>()
            .HasIndex(b => b.TelegramCommandText)
            .IsUnique();

         modelBuilder.Entity<TelegramUser>()
            .HasIndex(b => b.UserName)
            .IsUnique();
      }
   }
}
