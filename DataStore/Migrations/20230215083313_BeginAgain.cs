using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataStore.Migrations
{
    public partial class BeginAgain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    ApiKeyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Secret = table.Column<string>(type: "text", nullable: true),
                    PassPhrase = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AccountName = table.Column<string>(type: "text", nullable: true),
                    IsSubAccount = table.Column<bool>(type: "boolean", nullable: false),
                    SubAccountName = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.ApiKeyId);
                });

            migrationBuilder.CreateTable(
                name: "Coins",
                columns: table => new
                {
                    CoinId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coins", x => x.CoinId);
                });

            migrationBuilder.CreateTable(
                name: "ConfigSettings",
                columns: table => new
                {
                    ConfigSettingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigSettings", x => x.ConfigSettingId);
                });

            migrationBuilder.CreateTable(
                name: "LiquidationOrderLoadingConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsAuto = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    StartPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    ScalingFactor = table.Column<decimal>(type: "numeric", nullable: false),
                    IsHighestFirst = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidationOrderLoadingConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SPs",
                columns: table => new
                {
                    SPId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SPs", x => x.SPId);
                });

            migrationBuilder.CreateTable(
                name: "Strategies",
                columns: table => new
                {
                    StrategyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StrategyName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strategies", x => x.StrategyId);
                });

            migrationBuilder.CreateTable(
                name: "TelegramAlertCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramAlertCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelegramAlerts",
                columns: table => new
                {
                    TelegramAlertId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlertName = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramAlerts", x => x.TelegramAlertId);
                });

            migrationBuilder.CreateTable(
                name: "TelegramChannels",
                columns: table => new
                {
                    TelegramChannelId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelName = table.Column<string>(type: "text", nullable: true),
                    TokenId = table.Column<string>(type: "text", nullable: true),
                    TelegramAlertCategoryId = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramChannels", x => x.TelegramChannelId);
                });

            migrationBuilder.CreateTable(
                name: "TelegramCommandTypes",
                columns: table => new
                {
                    TelegramCommandTypeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramCommandTypes", x => x.TelegramCommandTypeId);
                });

            migrationBuilder.CreateTable(
                name: "TelegramUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    UserToken = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    VenueId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VenueName = table.Column<string>(type: "text", nullable: true),
                    VenueType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.VenueId);
                });

            migrationBuilder.CreateTable(
                name: "CoinPairs",
                columns: table => new
                {
                    CoinPairId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PCoinId = table.Column<int>(type: "integer", nullable: false),
                    SCoinId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinPairs", x => x.CoinPairId);
                    table.ForeignKey(
                        name: "FK_CoinPairs_Coins_PCoinId",
                        column: x => x.PCoinId,
                        principalTable: "Coins",
                        principalColumn: "CoinId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoinPairs_Coins_SCoinId",
                        column: x => x.SCoinId,
                        principalTable: "Coins",
                        principalColumn: "CoinId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrategySPSubscriptionConfigs",
                columns: table => new
                {
                    StrategySPSubscriptionConfigId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConfigName = table.Column<string>(type: "text", nullable: true),
                    SPId = table.Column<int>(type: "integer", nullable: false),
                    StrategyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategySPSubscriptionConfigs", x => x.StrategySPSubscriptionConfigId);
                    table.ForeignKey(
                        name: "FK_StrategySPSubscriptionConfigs_SPs_SPId",
                        column: x => x.SPId,
                        principalTable: "SPs",
                        principalColumn: "SPId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StrategySPSubscriptionConfigs_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "StrategyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TelegramAlertsToChannels",
                columns: table => new
                {
                    TelegramAlertToChannelId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TelegramAlertId = table.Column<int>(type: "integer", nullable: false),
                    TelegramChannelId = table.Column<int>(type: "integer", nullable: false),
                    IsAuthorised = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramAlertsToChannels", x => x.TelegramAlertToChannelId);
                    table.ForeignKey(
                        name: "FK_TelegramAlertsToChannels_TelegramAlerts_TelegramAlertId",
                        column: x => x.TelegramAlertId,
                        principalTable: "TelegramAlerts",
                        principalColumn: "TelegramAlertId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TelegramAlertsToChannels_TelegramChannels_TelegramChannelId",
                        column: x => x.TelegramChannelId,
                        principalTable: "TelegramChannels",
                        principalColumn: "TelegramChannelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TelegramCommands",
                columns: table => new
                {
                    TelegramCommandId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TelegramCommandText = table.Column<string>(type: "text", nullable: true),
                    TelegramCommandTypeId = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramCommands", x => x.TelegramCommandId);
                    table.ForeignKey(
                        name: "FK_TelegramCommands_TelegramCommandTypes_TelegramCommandTypeId",
                        column: x => x.TelegramCommandTypeId,
                        principalTable: "TelegramCommandTypes",
                        principalColumn: "TelegramCommandTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TelegramSubscriberToChannels",
                columns: table => new
                {
                    TelegramSubscriberToChannelId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TelegramChannelId = table.Column<int>(type: "integer", nullable: false),
                    TelegramUserId = table.Column<int>(type: "integer", nullable: false),
                    IsAuthorised = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramSubscriberToChannels", x => x.TelegramSubscriberToChannelId);
                    table.ForeignKey(
                        name: "FK_TelegramSubscriberToChannels_TelegramChannels_TelegramChann~",
                        column: x => x.TelegramChannelId,
                        principalTable: "TelegramChannels",
                        principalColumn: "TelegramChannelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TelegramSubscriberToChannels_TelegramUsers_TelegramUserId",
                        column: x => x.TelegramUserId,
                        principalTable: "TelegramUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeCoinMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VenueId = table.Column<int>(type: "integer", nullable: false),
                    CoinId = table.Column<int>(type: "integer", nullable: false),
                    ExchangeCoinName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeCoinMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeCoinMappings_Coins_CoinId",
                        column: x => x.CoinId,
                        principalTable: "Coins",
                        principalColumn: "CoinId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExchangeCoinMappings_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MakerTakerFees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VenueId = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    MakerPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    TakerPercentage = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MakerTakerFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MakerTakerFees_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeCoinPairMappings",
                columns: table => new
                {
                    ExchangeCoinpairLookupId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VenueId = table.Column<int>(type: "integer", nullable: false),
                    CoinPairId = table.Column<int>(type: "integer", nullable: false),
                    ExchangeCoinpairName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeCoinPairMappings", x => x.ExchangeCoinpairLookupId);
                    table.ForeignKey(
                        name: "FK_ExchangeCoinPairMappings_CoinPairs_CoinPairId",
                        column: x => x.CoinPairId,
                        principalTable: "CoinPairs",
                        principalColumn: "CoinPairId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExchangeCoinPairMappings_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FairValueConfigForUI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VenueId = table.Column<int>(type: "integer", nullable: false),
                    CoinPairId = table.Column<int>(type: "integer", nullable: false),
                    UpdateIntervalSecs = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FairValueConfigForUI", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FairValueConfigForUI_CoinPairs_CoinPairId",
                        column: x => x.CoinPairId,
                        principalTable: "CoinPairs",
                        principalColumn: "CoinPairId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FairValueConfigForUI_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SPId = table.Column<int>(type: "integer", nullable: false),
                    VenueId = table.Column<int>(type: "integer", nullable: false),
                    CoinPairId = table.Column<int>(type: "integer", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: true),
                    OrderType = table.Column<int>(type: "integer", nullable: false),
                    IsBuy = table.Column<bool>(type: "boolean", nullable: false),
                    OrderId = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    OrderTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    FilledQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    ClientOid = table.Column<string>(type: "text", nullable: true),
                    RemainingQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Account = table.Column<string>(type: "text", nullable: true),
                    Instance = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_CoinPairs_CoinPairId",
                        column: x => x.CoinPairId,
                        principalTable: "CoinPairs",
                        principalColumn: "CoinPairId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_SPs_SPId",
                        column: x => x.SPId,
                        principalTable: "SPs",
                        principalColumn: "SPId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    TradeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExchangeTradeId = table.Column<string>(type: "text", nullable: true),
                    VenueId = table.Column<int>(type: "integer", nullable: false),
                    CoinPairId = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SPId = table.Column<int>(type: "integer", nullable: false),
                    InstanceName = table.Column<string>(type: "text", nullable: true),
                    IsBuy = table.Column<bool>(type: "boolean", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    LeaveQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    Fee = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.TradeId);
                    table.ForeignKey(
                        name: "FK_Trades_CoinPairs_CoinPairId",
                        column: x => x.CoinPairId,
                        principalTable: "CoinPairs",
                        principalColumn: "CoinPairId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trades_SPs_SPId",
                        column: x => x.SPId,
                        principalTable: "SPs",
                        principalColumn: "SPId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trades_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeDetails",
                columns: table => new
                {
                    ExchangeDetailsId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    SPId = table.Column<int>(type: "integer", nullable: false),
                    VenueId = table.Column<int>(type: "integer", nullable: false),
                    StrategySPSubscriptionConfigId = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApiKeyId = table.Column<int>(type: "integer", nullable: false),
                    CoinPairs = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeDetails", x => x.ExchangeDetailsId);
                    table.ForeignKey(
                        name: "FK_ExchangeDetails_ApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "ApiKeys",
                        principalColumn: "ApiKeyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExchangeDetails_SPs_SPId",
                        column: x => x.SPId,
                        principalTable: "SPs",
                        principalColumn: "SPId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExchangeDetails_StrategySPSubscriptionConfigs_StrategySPSub~",
                        column: x => x.StrategySPSubscriptionConfigId,
                        principalTable: "StrategySPSubscriptionConfigs",
                        principalColumn: "StrategySPSubscriptionConfigId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExchangeDetails_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiquidationConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SPId = table.Column<int>(type: "integer", nullable: false),
                    StrategyId = table.Column<int>(type: "integer", nullable: false),
                    StrategySPSubscriptionConfigId = table.Column<int>(type: "integer", nullable: false),
                    CoinPairId = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    CoinAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderSize = table.Column<decimal>(type: "numeric", nullable: false),
                    DailyLiquidationTarget = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxOrderSize = table.Column<decimal>(type: "numeric", nullable: false),
                    PercentageSpreadFromFV = table.Column<decimal>(type: "numeric", nullable: false),
                    PercentageSpreadLowerThreshold = table.Column<decimal>(type: "numeric", nullable: false),
                    ShortTimeInterval = table.Column<int>(type: "integer", nullable: false),
                    LongTimeInterval = table.Column<int>(type: "integer", nullable: false),
                    CancelTimerInterval = table.Column<int>(type: "integer", nullable: false),
                    TakerModeTimeInterval = table.Column<int>(type: "integer", nullable: false),
                    BatchSize = table.Column<int>(type: "integer", nullable: false),
                    PriceDecimals = table.Column<int>(type: "integer", nullable: false),
                    AmountDecimals = table.Column<int>(type: "integer", nullable: false),
                    LiquidationOrderLoadingConfigurationId = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StrategyState = table.Column<bool>(type: "boolean", nullable: false),
                    MakerMode = table.Column<int>(type: "integer", nullable: false),
                    StopOnDailyTargetReached = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidationConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiquidationConfigurations_CoinPairs_CoinPairId",
                        column: x => x.CoinPairId,
                        principalTable: "CoinPairs",
                        principalColumn: "CoinPairId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiquidationConfigurations_LiquidationOrderLoadingConfigurat~",
                        column: x => x.LiquidationOrderLoadingConfigurationId,
                        principalTable: "LiquidationOrderLoadingConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiquidationConfigurations_SPs_SPId",
                        column: x => x.SPId,
                        principalTable: "SPs",
                        principalColumn: "SPId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiquidationConfigurations_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "StrategyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiquidationConfigurations_StrategySPSubscriptionConfigs_Str~",
                        column: x => x.StrategySPSubscriptionConfigId,
                        principalTable: "StrategySPSubscriptionConfigs",
                        principalColumn: "StrategySPSubscriptionConfigId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiquidationManualOrderLoadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StrategySPSubscriptionConfigId = table.Column<int>(type: "integer", nullable: false),
                    OrderNo = table.Column<int>(type: "integer", nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidationManualOrderLoadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiquidationManualOrderLoadings_StrategySPSubscriptionConfig~",
                        column: x => x.StrategySPSubscriptionConfigId,
                        principalTable: "StrategySPSubscriptionConfigs",
                        principalColumn: "StrategySPSubscriptionConfigId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiquidationStrategyConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StrategySPSubscriptionConfigId = table.Column<int>(type: "integer", nullable: false),
                    CoinPairId = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    NumberOfCoins = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderSize = table.Column<decimal>(type: "numeric", nullable: false),
                    PercentageSpreadFromFV = table.Column<decimal>(type: "numeric", nullable: false),
                    PercentageSpreadLowerThreshold = table.Column<decimal>(type: "numeric", nullable: false),
                    CancellationPolicyOnStart = table.Column<int>(type: "integer", nullable: false),
                    Venue = table.Column<string>(type: "text", nullable: true),
                    ShortTimeInterval = table.Column<int>(type: "integer", nullable: false),
                    LongTimeInterval = table.Column<int>(type: "integer", nullable: false),
                    BatchSize = table.Column<int>(type: "integer", nullable: false),
                    PriceDecimals = table.Column<int>(type: "integer", nullable: false),
                    AmountDecimals = table.Column<int>(type: "integer", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidationStrategyConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiquidationStrategyConfigs_CoinPairs_CoinPairId",
                        column: x => x.CoinPairId,
                        principalTable: "CoinPairs",
                        principalColumn: "CoinPairId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiquidationStrategyConfigs_StrategySPSubscriptionConfigs_St~",
                        column: x => x.StrategySPSubscriptionConfigId,
                        principalTable: "StrategySPSubscriptionConfigs",
                        principalColumn: "StrategySPSubscriptionConfigId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiquidationTrackers",
                columns: table => new
                {
                    LiquidationTrackerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StrategySubscriptionConfigId = table.Column<int>(type: "integer", nullable: false),
                    StrategySPSubscriptionConfigId = table.Column<int>(type: "integer", nullable: true),
                    CoinId = table.Column<int>(type: "integer", nullable: false),
                    RunId = table.Column<int>(type: "integer", nullable: false),
                    DateStarted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalDaysToRun = table.Column<int>(type: "integer", nullable: false),
                    CurrentDayNo = table.Column<int>(type: "integer", nullable: false),
                    NumberOfFillsInDay = table.Column<int>(type: "integer", nullable: false),
                    TotalNumberOfFillsIn = table.Column<int>(type: "integer", nullable: false),
                    CoinQtyLiquidatedInDay = table.Column<decimal>(type: "numeric", nullable: false),
                    DollarQtyForDay = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalCoinQtyLiquidated = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalDollarQtyLiquidated = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidationTrackers", x => x.LiquidationTrackerId);
                    table.ForeignKey(
                        name: "FK_LiquidationTrackers_StrategySPSubscriptionConfigs_StrategyS~",
                        column: x => x.StrategySPSubscriptionConfigId,
                        principalTable: "StrategySPSubscriptionConfigs",
                        principalColumn: "StrategySPSubscriptionConfigId");
                });

            migrationBuilder.CreateTable(
                name: "TelegramCommandToUsers",
                columns: table => new
                {
                    TelegramCommandToUserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TelegramCommandId = table.Column<int>(type: "integer", nullable: false),
                    TelegramUserId = table.Column<int>(type: "integer", nullable: false),
                    IsAuthorised = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramCommandToUsers", x => x.TelegramCommandToUserId);
                    table.ForeignKey(
                        name: "FK_TelegramCommandToUsers_TelegramCommands_TelegramCommandId",
                        column: x => x.TelegramCommandId,
                        principalTable: "TelegramCommands",
                        principalColumn: "TelegramCommandId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TelegramCommandToUsers_TelegramUsers_TelegramUserId",
                        column: x => x.TelegramUserId,
                        principalTable: "TelegramUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Balances",
                columns: table => new
                {
                    BalanceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SPId = table.Column<int>(type: "integer", nullable: false),
                    ExchangeDetailsId = table.Column<int>(type: "integer", nullable: false),
                    CoinId = table.Column<int>(type: "integer", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Balances", x => x.BalanceId);
                    table.ForeignKey(
                        name: "FK_Balances_Coins_CoinId",
                        column: x => x.CoinId,
                        principalTable: "Coins",
                        principalColumn: "CoinId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Balances_ExchangeDetails_ExchangeDetailsId",
                        column: x => x.ExchangeDetailsId,
                        principalTable: "ExchangeDetails",
                        principalColumn: "ExchangeDetailsId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Balances_SPs_SPId",
                        column: x => x.SPId,
                        principalTable: "SPs",
                        principalColumn: "SPId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpeningExchangeBalances",
                columns: table => new
                {
                    OpeningExchangeBalanceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExchangeDetailsId = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpeningExchangeBalances", x => x.OpeningExchangeBalanceId);
                    table.ForeignKey(
                        name: "FK_OpeningExchangeBalances_ExchangeDetails_ExchangeDetailsId",
                        column: x => x.ExchangeDetailsId,
                        principalTable: "ExchangeDetails",
                        principalColumn: "ExchangeDetailsId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpeningSubscription",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiquidationTrackerId = table.Column<int>(type: "integer", nullable: false),
                    VenueId = table.Column<int>(type: "integer", nullable: false),
                    CoinId = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    InitialCoinAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ProjectedNominal = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpeningSubscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpeningSubscription_Coins_CoinId",
                        column: x => x.CoinId,
                        principalTable: "Coins",
                        principalColumn: "CoinId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpeningSubscription_LiquidationTrackers_LiquidationTrackerId",
                        column: x => x.LiquidationTrackerId,
                        principalTable: "LiquidationTrackers",
                        principalColumn: "LiquidationTrackerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpeningSubscription_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Balances_CoinId",
                table: "Balances",
                column: "CoinId");

            migrationBuilder.CreateIndex(
                name: "IX_Balances_ExchangeDetailsId",
                table: "Balances",
                column: "ExchangeDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Balances_SPId",
                table: "Balances",
                column: "SPId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinPairs_PCoinId",
                table: "CoinPairs",
                column: "PCoinId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinPairs_SCoinId",
                table: "CoinPairs",
                column: "SCoinId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigSettings_Name",
                table: "ConfigSettings",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeCoinMappings_CoinId",
                table: "ExchangeCoinMappings",
                column: "CoinId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeCoinMappings_VenueId",
                table: "ExchangeCoinMappings",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeCoinPairMappings_CoinPairId",
                table: "ExchangeCoinPairMappings",
                column: "CoinPairId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeCoinPairMappings_VenueId",
                table: "ExchangeCoinPairMappings",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeDetails_ApiKeyId",
                table: "ExchangeDetails",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeDetails_SPId",
                table: "ExchangeDetails",
                column: "SPId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeDetails_StrategySPSubscriptionConfigId",
                table: "ExchangeDetails",
                column: "StrategySPSubscriptionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeDetails_VenueId",
                table: "ExchangeDetails",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_FairValueConfigForUI_CoinPairId",
                table: "FairValueConfigForUI",
                column: "CoinPairId");

            migrationBuilder.CreateIndex(
                name: "IX_FairValueConfigForUI_VenueId",
                table: "FairValueConfigForUI",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationConfigurations_CoinPairId",
                table: "LiquidationConfigurations",
                column: "CoinPairId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationConfigurations_LiquidationOrderLoadingConfigurat~",
                table: "LiquidationConfigurations",
                column: "LiquidationOrderLoadingConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationConfigurations_SPId",
                table: "LiquidationConfigurations",
                column: "SPId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationConfigurations_StrategyId",
                table: "LiquidationConfigurations",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationConfigurations_StrategySPSubscriptionConfigId",
                table: "LiquidationConfigurations",
                column: "StrategySPSubscriptionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationManualOrderLoadings_StrategySPSubscriptionConfig~",
                table: "LiquidationManualOrderLoadings",
                column: "StrategySPSubscriptionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationStrategyConfigs_CoinPairId",
                table: "LiquidationStrategyConfigs",
                column: "CoinPairId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationStrategyConfigs_StrategySPSubscriptionConfigId",
                table: "LiquidationStrategyConfigs",
                column: "StrategySPSubscriptionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationTrackers_StrategySPSubscriptionConfigId",
                table: "LiquidationTrackers",
                column: "StrategySPSubscriptionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_MakerTakerFees_VenueId",
                table: "MakerTakerFees",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningExchangeBalances_ExchangeDetailsId",
                table: "OpeningExchangeBalances",
                column: "ExchangeDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningSubscription_CoinId",
                table: "OpeningSubscription",
                column: "CoinId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningSubscription_LiquidationTrackerId",
                table: "OpeningSubscription",
                column: "LiquidationTrackerId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningSubscription_VenueId",
                table: "OpeningSubscription",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CoinPairId",
                table: "Orders",
                column: "CoinPairId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SPId",
                table: "Orders",
                column: "SPId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_VenueId",
                table: "Orders",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategySPSubscriptionConfigs_ConfigName",
                table: "StrategySPSubscriptionConfigs",
                column: "ConfigName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StrategySPSubscriptionConfigs_SPId",
                table: "StrategySPSubscriptionConfigs",
                column: "SPId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategySPSubscriptionConfigs_StrategyId",
                table: "StrategySPSubscriptionConfigs",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramAlertCategories_Category",
                table: "TelegramAlertCategories",
                column: "Category",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramAlerts_AlertName",
                table: "TelegramAlerts",
                column: "AlertName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramAlertsToChannels_TelegramAlertId",
                table: "TelegramAlertsToChannels",
                column: "TelegramAlertId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramAlertsToChannels_TelegramChannelId",
                table: "TelegramAlertsToChannels",
                column: "TelegramChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramChannels_ChannelName",
                table: "TelegramChannels",
                column: "ChannelName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramCommands_TelegramCommandText",
                table: "TelegramCommands",
                column: "TelegramCommandText",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramCommands_TelegramCommandTypeId",
                table: "TelegramCommands",
                column: "TelegramCommandTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramCommandToUsers_TelegramCommandId",
                table: "TelegramCommandToUsers",
                column: "TelegramCommandId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramCommandToUsers_TelegramUserId",
                table: "TelegramCommandToUsers",
                column: "TelegramUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramSubscriberToChannels_TelegramChannelId",
                table: "TelegramSubscriberToChannels",
                column: "TelegramChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramSubscriberToChannels_TelegramUserId",
                table: "TelegramSubscriberToChannels",
                column: "TelegramUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUsers_UserName",
                table: "TelegramUsers",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trades_CoinPairId",
                table: "Trades",
                column: "CoinPairId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_SPId",
                table: "Trades",
                column: "SPId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_VenueId",
                table: "Trades",
                column: "VenueId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Balances");

            migrationBuilder.DropTable(
                name: "ConfigSettings");

            migrationBuilder.DropTable(
                name: "ExchangeCoinMappings");

            migrationBuilder.DropTable(
                name: "ExchangeCoinPairMappings");

            migrationBuilder.DropTable(
                name: "FairValueConfigForUI");

            migrationBuilder.DropTable(
                name: "LiquidationConfigurations");

            migrationBuilder.DropTable(
                name: "LiquidationManualOrderLoadings");

            migrationBuilder.DropTable(
                name: "LiquidationStrategyConfigs");

            migrationBuilder.DropTable(
                name: "MakerTakerFees");

            migrationBuilder.DropTable(
                name: "OpeningExchangeBalances");

            migrationBuilder.DropTable(
                name: "OpeningSubscription");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "TelegramAlertCategories");

            migrationBuilder.DropTable(
                name: "TelegramAlertsToChannels");

            migrationBuilder.DropTable(
                name: "TelegramCommandToUsers");

            migrationBuilder.DropTable(
                name: "TelegramSubscriberToChannels");

            migrationBuilder.DropTable(
                name: "Trades");

            migrationBuilder.DropTable(
                name: "LiquidationOrderLoadingConfigurations");

            migrationBuilder.DropTable(
                name: "ExchangeDetails");

            migrationBuilder.DropTable(
                name: "LiquidationTrackers");

            migrationBuilder.DropTable(
                name: "TelegramAlerts");

            migrationBuilder.DropTable(
                name: "TelegramCommands");

            migrationBuilder.DropTable(
                name: "TelegramChannels");

            migrationBuilder.DropTable(
                name: "TelegramUsers");

            migrationBuilder.DropTable(
                name: "CoinPairs");

            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "Venues");

            migrationBuilder.DropTable(
                name: "StrategySPSubscriptionConfigs");

            migrationBuilder.DropTable(
                name: "TelegramCommandTypes");

            migrationBuilder.DropTable(
                name: "Coins");

            migrationBuilder.DropTable(
                name: "SPs");

            migrationBuilder.DropTable(
                name: "Strategies");
        }
    }
}
