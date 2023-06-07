using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Timers;
using AccountBalanceManager;
using Common;
using Common.Extensions;
using Common.Messages;
using Common.Models;
using Common.Models.DTOs;
using DataStore;
using MessageBroker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StrategyMessageListener;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Timer = System.Timers.Timer;

namespace TelegramCommandServer.Services
{
   public enum MessageHandlingState
   {
      INITIAL = 1,
      CATEGORY_CHOICES = 2,
      LIQUIDITY_SELECTED = 3,
      INITIAL_LIQUIDITY_CHOICES = 4,
      CONFIG_ACCOUNT_CHOSEN = 5,
      CONFIG_INSTANCE_CHOSEN = 6,
      CONFIG_MENU_SELECTION_CHOSEN = 7,
      SHOW_CONFIG_SUMMARY_DISPLAYED = 8,
      ORDER_BATCH_SIZE_CHANGE = 9,
      ORDER_SIZE_CHANGE = 10,
      UPPER_THRESHOLD_SIZE_CHANGE = 11,
      LOWER_THRESHOLD_SIZE_CHANGE = 12,
      END_DATE_CHANGE = 13,
      MAKER_TAKER_MODE_CHANGE = 14,

      // Position/State states
      POSITION_ACCOUNT_CHOSEN = 15,
      POSITION_INSTANCE_CHOSEN = 16,
      POSITION_BALANCE_DISPLAYED = 17,
      POSITION_MENU_SELECTION_CHOSEN = 18
   }
   public interface ITelegramCommandListener
   {
      Task Init(TelegramChannelDTO channel);
      Task ListenForMessagesAsync();
   }
   public class TelegramCommandListener : ITelegramCommandListener
   {
      private readonly ILogger<TelegramCommandListener> _logger;
      private readonly IMessageBroker _messageBroker;
      private readonly IPortfolioRepository _repository;
      private readonly ITelegramMessageReceiver _receiver;
      private readonly IInventoryManager _inventoryMgr;
      private TelegramBotClient _botClient = null;
      private Dictionary<string, SPDTO> _accountNameToIdLookup = new Dictionary<string, SPDTO>();
      private MessageHandlingState _state = MessageHandlingState.INITIAL;
      
      private Dictionary<string,TelegramSubscriberToChannelDTO> _subscribersToChannels = new();
      private Dictionary<string, TelegramUserDTO> _users = new();
      private Dictionary<int, TelegramUserDTO> _usersById = new();
      private Dictionary<int, Dictionary<string, TelegramCommandToUserDTO>> _userCommandTable = new();
      private bool _initialised = false;
      private TelegramChannelDTO _telegramChannel;
      //private string? _chosenPositionInstance;
   //   private string _chosenPositionAccount;


      private Dictionary<long, string> _chosenPositionInstanceTable = new();
      private Dictionary<long, string> _chosenPositionAccountTable = new();
      private Dictionary<long, string> _chosenConfigInstance = new();
      private Dictionary<long, LiquidationConfigurationDTO> _latestConfigPerUser = new();
      //private Dictionary<>

      private int _jobNo { get; set; }
      private Timer _balancesTimer = null;

      private Dictionary<int, Dictionary<string, Dictionary<string, ExchangeBalance>>> _exchangeBalancesByJobNo = new();
      private Dictionary<int, Message> _jobNoChatIdTable = new();
      private Dictionary<int, VenueDTO> _venueTable = new();

      public TelegramCommandListener(ILoggerFactory loggerFactory, 
                                     IPortfolioRepository repository,
                                     IMessageBroker messageBroker,
                                     ITelegramMessageReceiver messageReceiver,
                                     IInventoryManager inventoryMgr
                                      )
      {
            _logger = loggerFactory.CreateLogger<TelegramCommandListener>();
            _messageBroker = messageBroker;
            _repository = repository;
            _receiver = messageReceiver;
            _inventoryMgr = inventoryMgr;
            _inventoryMgr.OnOpeningBalance += OnOpeningBalance;
            _inventoryMgr.OnBalanceUpdateForTelegram += OnBalanceUpdateForTelegram;
            _receiver.OnTelegramConfigChange += OnTelegramConfigChange;
            _jobNo = 1;
            
      }

      private void OnBalanceUpdateForTelegram(string venue, string coin, int jobNo, ExchangeBalance balance)
      {
         Dictionary<string, ExchangeBalance> balByCoin = null;
         Dictionary<string, Dictionary<string, ExchangeBalance>> venueTable = null;
         Console.WriteLine($"Received Balance update from {venue} for coin {coin} with {balance.Available}");
         if (_exchangeBalancesByJobNo.ContainsKey(jobNo))
         {
            venueTable = _exchangeBalancesByJobNo[jobNo];
            if (venueTable.ContainsKey(venue))
            {
               balByCoin = venueTable[venue];
            }
            else
            {
               balByCoin = new Dictionary<string, ExchangeBalance>();
               venueTable.Add(venue, balByCoin);
            }

            if (balByCoin.ContainsKey(coin))
            {
                balByCoin[coin] = balance;
            }
            else
               balByCoin.Add(coin, balance);
         }
         else
         {
            venueTable = new Dictionary<string, Dictionary<string, ExchangeBalance>>();
            balByCoin = new Dictionary<string, ExchangeBalance>();
            venueTable.Add(venue, balByCoin);
            _exchangeBalancesByJobNo.Add(jobNo, venueTable);
            balByCoin.Add(coin, balance);
         }
      }

      private void OnOpeningBalance(string venue, string coin, ExchangeBalance balance)
      {
         Console.WriteLine($"Received Opening Balance update from {venue} for coin {coin} with {balance.Available}");
      }

      private async Task OnTelegramConfigChange(TelegramConfigChange configChange)
      {
         if (configChange != null && _initialised)
         {

            switch (configChange.ChangeType)
            {
               case TelegramConfigChangeType.CHANNEL_CHANGE:
               case TelegramConfigChangeType.COMMAND_CHANGE:
               case TelegramConfigChangeType.USER_CHANGE:
               case TelegramConfigChangeType.USER_TO_CHANNEL:
               case TelegramConfigChangeType.USER_TO_COMMAND:
                  await InitTables(_telegramChannel);
                  break;
            }
         }
      }

      public async Task Init(TelegramChannelDTO channel)
      {
         await InitTables(channel);
         _botClient = new TelegramBotClient(channel.TokenId);
         if (_botClient == null)
         {
            _logger.LogInformation("Unable to create Telegram Bot for {ChannelName}", channel.ChannelName);
            throw new Exception($"Unable to create Telegram Bot for {channel.ChannelName}");
         }
         var venues = await _repository.GetVenues();
         _venueTable = venues.ToDictionary(x => x.VenueId, x => x);
         _initialised = true;
      }

      private async Task InitTables(TelegramChannelDTO channel)
      {
         var subscribersToChannels = await _repository.GetTelegramSubscriberToChannels();
         subscribersToChannels = subscribersToChannels.Where(x => x.TelegramChannelId == channel.TelegramChannelId).ToList();
         if (subscribersToChannels.Any())
         {
            _telegramChannel = subscribersToChannels[0].TelegramChannel;
         }
         _subscribersToChannels = subscribersToChannels.ToDictionary(x => x.TelegramUser.UserToken, x => x);
         var userList = await _repository.GetTelegramUsers();
         _users = userList.ToDictionary(x => x.UserToken, x => x);
         _usersById = userList.ToDictionary(x => x.Id, x => x);

         var ccc = await _repository.GetTelegramCommandToUsers();
         var ggg = ccc.GroupBy(x => x.TelegramUserId, x => x, (key, g) => new { UserId = key, Commands = g.ToDictionary(x => x.TelegramCommand.TelegramCommandText, x => x) });
         _userCommandTable = ggg.ToDictionary(x => x.UserId, x => x.Commands);
      }

      public async Task ListenForMessagesAsync()
      {
         using var cts = new CancellationTokenSource();

         // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool
         var receiverOptions = new ReceiverOptions
         {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
         };
         _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
         );

         var me = await _botClient.GetMeAsync();
         _logger.LogInformation("Start Listening for {User}", me.Username);

         while (true)
         {
            var mre = new ManualResetEvent(false);
            mre.WaitOne(3600 * 1000);
         }
      }

      private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
      {
         if (update.Message is not { } message)
         {
            return;
         }
         
         // Only process text messages
         if (message.Text is not { } messageText)
         {
            return;
         }

         // Check that user is allowed on this channel
         if (_subscribersToChannels.ContainsKey(message.Chat.Id.ToString()))
         {
            var details = _subscribersToChannels[message.Chat.Id.ToString()];
            if (details != null)
            {
               if (!details.IsAuthorised)
               {
                  await SendNotAuthorisedMessage(botClient, update.Message, cancellationToken);
                  return;
               }
            }
         }
         else
         {
            // Send unauthorised message
            await SendNotAuthorisedMessage(botClient, update.Message, cancellationToken);
            return;
         }

         switch (_state)
         {
            case MessageHandlingState.INITIAL:
               await InitialStateHandling(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.CATEGORY_CHOICES:
               await CategoryChoicesStateHandling(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.LIQUIDITY_SELECTED:
               await LiquiditySelectedStateHandling(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.INITIAL_LIQUIDITY_CHOICES:
               await InitialLiquidityChoicesStateHandling(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.CONFIG_ACCOUNT_CHOSEN:
               await ConfigAccountChosen(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.CONFIG_INSTANCE_CHOSEN:
               await ConfigInstanceChosen(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.CONFIG_MENU_SELECTION_CHOSEN:
               await ConfigMenuSelectionChosen(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.SHOW_CONFIG_SUMMARY_DISPLAYED:
               await ShowConfigSummaryDisplayed(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.ORDER_BATCH_SIZE_CHANGE:
               await ChangeOrderBatchSize(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.ORDER_SIZE_CHANGE:
               await ChangeOrderSize(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.UPPER_THRESHOLD_SIZE_CHANGE:
               await ChangeUpperThresholdSize(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.LOWER_THRESHOLD_SIZE_CHANGE:
               await ChangeLowerThresholdSize(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.END_DATE_CHANGE:
               await ChangeEndDate(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.MAKER_TAKER_MODE_CHANGE:
               await ChangeMakerTakerMode(botClient, message, cancellationToken);
               break;

            case MessageHandlingState.POSITION_ACCOUNT_CHOSEN:
               await PositionAccountChosen(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.POSITION_INSTANCE_CHOSEN:
               await PositionInstanceChosen(botClient, message, cancellationToken);
               break;
            case MessageHandlingState.POSITION_MENU_SELECTION_CHOSEN:
               await PositionInstanceChosen(botClient, message, cancellationToken);
               break;
            default:
               await SendErrorMessage(botClient, message, cancellationToken);
               break;
         }
      }
      private async Task LiquiditySelectedStateHandling(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         var result = await CheckForBeginButtonPress(botClient, message, cancellationToken);
         if (result) return;

         switch (message.Text)
         {
            case "Strategy Config Control":
               await SendStrategyConfigControlMenuMessageAsync(botClient, message, cancellationToken);
               break;
            case "Strategy Instance Control":
               break;
            case "Current State Information":
               await SendCurrentStateInformation(botClient, message, cancellationToken);
               break;
            case "/begin":
               _state = MessageHandlingState.INITIAL;
               await SendCategoryChoiceMessageAsync(botClient, message, cancellationToken);
               break;
         }
      }

      private async Task SendPortfolioMenuAsync(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken, MessageHandlingState state)
      {
         var accounts = await _repository.GetPortfolios();

         _accountNameToIdLookup = accounts.ToDictionary(a => a.Name, a => a);
         if (accounts.Any())
         {
            var k = new KeyboardButton[1, accounts.Count];
            int i = 0;
            //  var rows = new KeyboardButton[accounts.Count];
            var rows1 = new List<List<KeyboardButton>>();
            foreach (var account in accounts)
            {
               var kk = new KeyboardButton(account.Name);
               var l = new List<KeyboardButton>();
               l.Add(kk);
               rows1.Add(l);
               i++;
            }
            AddBeginButton(rows1);

            _state = state;
            ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(rows1);
            Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
               text: "Please choose a Portfolio:",
               replyMarkup: keyboard,
               cancellationToken: cancellationToken);
         }
      }

      private async Task SendStrategyConfigControlMenuMessageAsync(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         await SendPortfolioMenuAsync(botClient, message, cancellationToken, MessageHandlingState.CONFIG_ACCOUNT_CHOSEN);
      }

      private async Task SendCurrentStateInformation(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         var chosenPositionAccount = "";
         if (!_chosenPositionAccountTable.ContainsKey(message.Chat.Id))
         {
            _chosenPositionAccountTable.Add(message.Chat.Id, message.Text);
         }
         await SendPortfolioMenuAsync(botClient, message, cancellationToken, MessageHandlingState.POSITION_ACCOUNT_CHOSEN);
      }

      private async Task ShowConfigSummaryDisplayed(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
      {
         if (message.Text == "/begin")
         {
            await InitialStateHandling(botClient, message, cancellationToken);
         }
         else if (message.Text == "/back")
         {
            if (_chosenConfigInstance.ContainsKey(message.Chat.Id))
            {
               message.Text = _chosenConfigInstance[message.Chat.Id];
               await ConfigInstanceChosen(botClient, message, cancellationToken);
            }
            else
            {
               _chosenConfigInstance.Add(message.Chat.Id, message.Text);
               await ConfigInstanceChosen(botClient, message, cancellationToken);
            }
         }
         
      }

      private async Task ConfigInstanceChosen(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         var result = await CheckForBeginButtonPress(botClient, message, cancellationToken);
         if (result)
            return;
         if (_chosenConfigInstance.ContainsKey(message.Chat.Id))
         {
            _chosenConfigInstance[message.Chat.Id] = message.Text;
         }
         else
         {
            _chosenConfigInstance.Add(message.Chat.Id, message.Text);
         }
         

         int i = 0;

         var rows = new List<List<KeyboardButton>>();
         var commands = await _repository.GetTelegramLiquidationConfigurationCommands();
         foreach (var command in commands)
         {
            var r = new List<KeyboardButton>();
            var b = new KeyboardButton(command.TelegramCommandText);
            r.Add(b);
            rows.Add(r);
         }
         
         AddBeginButton(rows);
         _state = MessageHandlingState.CONFIG_MENU_SELECTION_CHOSEN;
         ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(rows);
         Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Please choose a menu item:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
      }

      private async Task ChangeOrderBatchSizeMenuDisplay(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
      {
         int i = 0;

         var rows1 = new List<List<KeyboardButton>>();
         for (int j = 1; j < 7; j++)
         {
            var kk = new KeyboardButton(j.ToString());
            var l = new List<KeyboardButton>();
            l.Add(kk);
            rows1.Add(l);
         }
         AddBeginAndBackButtons(rows1);
         _state = MessageHandlingState.ORDER_BATCH_SIZE_CHANGE;
         ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(rows1);
         Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Please Number of Orders:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
      }

      private async Task ChangeOrderBatchSize(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         var result = await CheckForBeginAndBackButtonPresses(botClient, message, cancellationToken);
         if (result)
            return;

         LiquidationConfigurationDTO? latestConfig = await GetLatestLiquidityConfig(message);
         if (latestConfig == null)
            return;

         var size = Int16.Parse(message.Text);
         latestConfig.BatchSize = size;
         await _repository.UpdateOpeningLiquidationSubscription(latestConfig);
         BroadcastConfigChange(latestConfig);
         await HandleShowConfigSummary(botClient, message, cancellationToken);
      }

      private async Task ChangeOrderSizeMenuDisplay(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
      {
         int i = 0;

         int startRange = -20;
         int endRange = 20;

         var rows1 = new List<List<KeyboardButton>>();
         for (int j = startRange; j <= endRange; j += 5)
         {
            var kk = new KeyboardButton($"{j.ToString()}%");
            var l = new List<KeyboardButton>();
            l.Add(kk);
            rows1.Add(l);
         }
         AddBeginAndBackButtons(rows1);
         _state = MessageHandlingState.ORDER_SIZE_CHANGE;
         ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(rows1);
         Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Please select size of in terms of % change:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

      }

      private async Task ChangeOrderSize(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         var result = await CheckForBeginAndBackButtonPresses(botClient, message, cancellationToken);
         if (result)
            return;

         LiquidationConfigurationDTO? latestConfig = await GetLatestLiquidityConfig(message);
         if (latestConfig == null)
            return;

         var text = message.Text;
         if (text.Contains("%"))
         {
            var changes = text.Split("%");
            if (changes.Length == 2)
            {
               var res = Int32.TryParse(changes[0], out var change);
               if (res)
               {
                  latestConfig.OrderSize = latestConfig.OrderSize + latestConfig.OrderSize * change / 100;
                  await _repository.UpdateOpeningLiquidationSubscription(latestConfig);
                  BroadcastConfigChange(latestConfig);
                  await HandleShowConfigSummary(botClient, message, cancellationToken);
               }
            }
         }
         else
            await SendErrorMessage(botClient, message, cancellationToken);
      }

      private async Task ChangeUpperThrsholdSizeMenuDisplay(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
      {
         int i = 0;

         int startRange = -20;
         int endRange = 20;

         var rows1 = new List<List<KeyboardButton>>();
         for (int j = startRange; j <= endRange; j += 5)
         {
            var kk = new KeyboardButton($"{j.ToString()}%");
            var l = new List<KeyboardButton>();
            l.Add(kk);
            rows1.Add(l);
         }
         AddBeginAndBackButtons(rows1);
         _state = MessageHandlingState.UPPER_THRESHOLD_SIZE_CHANGE;
         ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(rows1);
         Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Please select size of in terms of % change:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

      }

      private async Task ChangeUpperThresholdSize(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         var result = await CheckForBeginAndBackButtonPresses(botClient, message, cancellationToken);
         if (result)
            return;

         LiquidationConfigurationDTO? latestConfig = await GetLatestLiquidityConfig(message);
         if (latestConfig == null)
            return;

         var text = message.Text;
         if (text.Contains("%"))
         {
            var changes = text.Split("%");
            if (changes.Length == 2)
            {
               var res = Int32.TryParse(changes[0], out var change);
               if (res)
               {
                  latestConfig.PercentageSpreadFromFV = latestConfig.PercentageSpreadFromFV + latestConfig.PercentageSpreadFromFV * change / 100;
                  await _repository.UpdateOpeningLiquidationSubscription(latestConfig);
                  BroadcastConfigChange(latestConfig);
                  await HandleShowConfigSummary(botClient, message, cancellationToken);
               }
            }
         }
         else
            await SendErrorMessage(botClient, message, cancellationToken);
      }

      private async Task ChangeLowerThrsholdSizeMenuDisplay(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
      {
         int i = 0;

         int startRange = -20;
         int endRange = 20;

         var rows1 = new List<List<KeyboardButton>>();
         for (int j = startRange; j <= endRange; j += 5)
         {
            var kk = new KeyboardButton($"{j.ToString()}%");
            var l = new List<KeyboardButton>();
            l.Add(kk);
            rows1.Add(l);
         }
         AddBeginAndBackButtons(rows1);
         _state = MessageHandlingState.LOWER_THRESHOLD_SIZE_CHANGE;
         ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(rows1);
         Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Please select size of in terms of % change:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

      }

      private async Task ChangeLowerThresholdSize(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         var result = await CheckForBeginAndBackButtonPresses(botClient, message, cancellationToken);
         if (result)
            return;

         LiquidationConfigurationDTO? latestConfig = await GetLatestLiquidityConfig(message);
         if (latestConfig == null)
            return;

         var text = message.Text;
         if (text.Contains("%"))
         {
            var changes = text.Split("%");
            if (changes.Length == 2)
            {
               var res = Int32.TryParse(changes[0], out var change);
               if (res)
               {
                  latestConfig.PercentageSpreadLowerThreshold = latestConfig.PercentageSpreadLowerThreshold + latestConfig.PercentageSpreadLowerThreshold * change / 100;
                  await _repository.UpdateOpeningLiquidationSubscription(latestConfig);
                  BroadcastConfigChange(latestConfig);
                  await HandleShowConfigSummary(botClient, message, cancellationToken);
               }
            }
         }
         else
            await SendErrorMessage(botClient, message, cancellationToken);
      }

      private async Task ChangeEndDateMenuDisplay(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
      {
         int i = 0;

         int startRange;
         int endRange;


         LiquidationConfigurationDTO? latestConfig = await GetLatestLiquidityConfig(message);
         if (latestConfig == null)
            return;


         // Get the end date
         var endDate = latestConfig.EndDate;
         var currentDate = DateTime.UtcNow;
         var msg = "";

         var days = (endDate - currentDate).Days;
         if (days >= 0) // end date after current date or same date
         {
            startRange = days * -1;
            endRange = 5;
            msg = "Please select number of days forward or back from end date";
         }
         else // end date before current date
         {
            startRange = 1;
            endRange = 10;
            msg = "Please select number of days forward from current date:";
         }

         var rows = new List<List<KeyboardButton>>();
         for (int j = startRange; j <= endRange; j++)
         {
            
            var kk = new KeyboardButton($"{j.ToString()}");
            var l = new List<KeyboardButton>();
            l.Add(kk);
            rows.Add(l);
         }

         AddBeginAndBackButtons(rows);

         _state = MessageHandlingState.END_DATE_CHANGE;
         ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(rows);
         Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: msg,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
         
      }

      private async Task ChangeEndDate(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         var result = await CheckForBeginAndBackButtonPresses(botClient, message, cancellationToken);
         if (result)
            return;

         LiquidationConfigurationDTO? latestConfig = await GetLatestLiquidityConfig(message);
         if (latestConfig == null)
            return;

         var endDate = latestConfig.EndDate;
         var currentDate = DateTime.UtcNow;
         
         var days = (endDate - currentDate).Days;

         var res = Int32.TryParse(message.Text, out var diff);
         if (res)
         {
            if (days >= 0)
            { 
               latestConfig.EndDate = endDate.AddDays(diff);
            }
            else
            {
               latestConfig.EndDate = currentDate.AddDays(diff);
            }
            await _repository.UpdateOpeningLiquidationSubscription(latestConfig);
            BroadcastConfigChange(latestConfig);
            await HandleShowConfigSummary(botClient, message, cancellationToken);
         }
         else
            await SendErrorMessage(botClient, message, cancellationToken);
      }

      private async Task ChangeMakerTakerModeMenuDisplay(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
      {
         var rows = new List<List<KeyboardButton>>();

         var makerKeyboard = new KeyboardButton("Maker");
         var m = new List<KeyboardButton>();
         m.Add(makerKeyboard);
         rows.Add(m);

         var takerKeyboard = new KeyboardButton("Taker");
         var t = new List<KeyboardButton>();
         t.Add(takerKeyboard);
         rows.Add(t);

         AddBeginAndBackButtons(rows);

         _state = MessageHandlingState.MAKER_TAKER_MODE_CHANGE;
         ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(rows);
         Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Choose Maker or Taker Mode",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
      }

      private async Task ChangeMakerTakerMode(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         var res = await CheckForBeginAndBackButtonPresses(botClient, message, cancellationToken);
         if (res)
            return;

         LiquidationConfigurationDTO? latestConfig = await GetLatestLiquidityConfig(message);
         if (latestConfig == null)
            return;

         switch (message.Text)
         {
            case "Maker":
               latestConfig.MakerMode = StratgeyMode.MAKER;
               break;
            case "Taker":
               latestConfig.MakerMode = StratgeyMode.TAKER;
               break;
         }

         await _repository.UpdateOpeningLiquidationSubscription(latestConfig);
         BroadcastConfigChange(latestConfig);
         await HandleShowConfigSummary(botClient, message, cancellationToken);
      }

      private async Task<LiquidationConfigurationDTO?> GetLatestLiquidityConfig(Message message)
      {
         try
         {
            if (_chosenConfigInstance.ContainsKey(message.Chat.Id))
            {
               var chosenInstance = _chosenConfigInstance[message.Chat.Id];
               var latestConfig = await _repository.GetOpeningLiquidationSubscriptionsForInstance(chosenInstance);
               if (latestConfig == null)
               {
                  _logger.LogError("No liquidation config for {Instance}", chosenInstance);
                  return null;
               }

               if (_latestConfigPerUser.ContainsKey(message.Chat.Id))
               {
                  // replace
                  _latestConfigPerUser[message.Chat.Id] = latestConfig;
               }
               else
               {
                  _latestConfigPerUser.Add(message.Chat.Id, latestConfig);
               }
               return latestConfig;
            }
            else
            {
               await SendNotAuthorisedCommandMessage(_botClient, message, new CancellationToken());
               return null;
            }
         }
         catch (Exception e)
         {
            _logger.LogError("Error in getting liquidation config {Error} for id = {Id}", e.Message, message.Chat.Id);
            return null;
         }
      }

      private void BroadcastConfigChange(LiquidationConfigurationDTO liquidationStrategyConfig)
      {
         try
         {
            var strategyConfigData = new StrategyConfigChangeData()
            {
               StrategyConfigChangeType = StrategyConfigChangeType.LIQUIDATION,
               InstanceName = liquidationStrategyConfig.StrategySPSubscriptionConfig.ConfigName
            };

            var configChangeUpdate = new ConfigChangeUpdate()
            {
               ConfigChangeType = ConfigChangeType.STRATEGY,
               Data = JsonSerializer.Serialize(strategyConfigData)

            };
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.CONFIG_UPDATE_STATUS,
               Data = JsonSerializer.Serialize(configChangeUpdate)

            };
            PublishHelper.PublishToTopic(Constants.CONFIG_UPDATE_TOPIC, response, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogError("Error cancelling Order {Error}", e.Message);
            throw;
         }
      }
      private async Task InitialStateHandling(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         if (message.Text == "/begin")
         {
            _state = MessageHandlingState.CATEGORY_CHOICES;
            await SendCategoryChoiceMessageAsync(botClient, message, cancellationToken);
         }
      }

      private async Task CategoryChoicesStateHandling(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         if (message.Text == "/begin")
         {
            _state = MessageHandlingState.CATEGORY_CHOICES;
            await SendCategoryChoiceMessageAsync(botClient, message, cancellationToken);
            return;
         }

         if (message.Text == "Liquidity")
         {
            _state = MessageHandlingState.LIQUIDITY_SELECTED;
            await SendLiquidityInitialMenuMessageAsync(botClient, message, cancellationToken);
         }
      }

      private async Task InitialLiquidityChoicesStateHandling(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
      {
         
      }

      private async Task ConfigAccountChosen(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
      {
         await SendInstanceMenu(botClient, message, cancellationToken, MessageHandlingState.CONFIG_INSTANCE_CHOSEN);
      }

      private async Task PositionAccountChosen(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
      {
         var chosenPositionAccount = "";
         if (string.IsNullOrEmpty(message.Text))
         {
            _logger.LogError("Error choosing position account - text is empty or null {UserName}",
               message.Chat.Username);
            await SendErrorMessage(botClient, message, cancellationToken);
            return;
         }
         if (!_chosenPositionAccountTable.ContainsKey(message.Chat.Id))
         {
            _chosenPositionAccountTable.Add(message.Chat.Id, message.Text);
         }
         else
         {
            _chosenPositionAccountTable[message.Chat.Id] = message.Text;
         }
         
         await SendInstanceMenu(botClient, message, cancellationToken, MessageHandlingState.POSITION_INSTANCE_CHOSEN);
      }

      private async Task SendInstanceMenu(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken, MessageHandlingState state)
      {
         var result = await CheckForBeginAndBackButtonPresses(botClient, message, cancellationToken);
         //var instances = await _repository.GetOpeningLiquidationSubscriptionsForSp(message.Text);
         if (_accountNameToIdLookup.ContainsKey(message.Text))
         {
            var id = _accountNameToIdLookup[message.Text];
            var configs = await _repository.GetStrategyExchangeConfigsForSP(message.Text);

            if (configs.Any())
            {
               int i = 0;
               //  var rows = new KeyboardButton[accounts.Count];
               var rows1 = new List<List<KeyboardButton>>();
               foreach (var instance in configs)
               {
                  var kk = new KeyboardButton(instance.ConfigName);
                  var l = new List<KeyboardButton>();
                  l.Add(kk);
                  rows1.Add(l);
                  i++;
               }

               AddBeginButton(rows1);
               _state = state;
               ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(rows1);
               Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                  text: "Please choose a Strategy Instance:",
                  replyMarkup: keyboard,
                  cancellationToken: cancellationToken);
            }
         }
      }

      private async Task ConfigMenuSelectionChosen(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
      {
         var result = await CheckForBeginButtonPress(botClient, message, cancellationToken);
         if (result) return;

         // Get the user id
         int userId = 0;
         bool accessGranted = false;
         if (_users.ContainsKey(message.Chat.Id.ToString()))
         {
            userId = _users[message.Chat.Id.ToString()].Id;
            if (_userCommandTable.ContainsKey(userId))
            {
               var availableCommands = _userCommandTable[userId];
               if (availableCommands.ContainsKey(message.Text))
               {
                  var commandUserPermissions = availableCommands[message.Text];
                  if (commandUserPermissions.IsAuthorised)
                  {
                     accessGranted = true;
                  }
               }
            }
            
         }

         if (accessGranted)
         {
            switch (message.Text)
            {
               case "Show Config Summary":
                  await HandleShowConfigSummary(botClient, message, cancellationToken);
                  break;
               case "Change Order batch Size":
                  await ChangeOrderBatchSizeMenuDisplay(botClient, message, cancellationToken);
                  break;
               case "Change Order Size":
                  await ChangeOrderSizeMenuDisplay(botClient, message, cancellationToken);
                  break;
               case "Change Upper Threshold":
                  await ChangeUpperThrsholdSizeMenuDisplay(botClient, message, cancellationToken);
                  break;
               case "Change Lower Threshold":
                  await ChangeLowerThrsholdSizeMenuDisplay(botClient, message, cancellationToken);
                  break;
               case "Change End Date":
                  await ChangeEndDateMenuDisplay(botClient, message, cancellationToken);
                  break;
               case "Change Maker/Taker Mode":
                  await ChangeMakerTakerModeMenuDisplay(botClient, message, cancellationToken);
                  break;
               default:
                  _state = MessageHandlingState.INITIAL;
                  break;
            }
         }
         else
         {
            await SendNotAuthorisedCommandMessage(botClient, message, cancellationToken);
         }
      }

      private async Task HandleShowConfigSummary(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
      {
         var chosenInstance = "";
         if (_chosenConfigInstance.ContainsKey(message.Chat.Id))
         {
            chosenInstance = _chosenConfigInstance[message.Chat.Id];
         }
         else
         {
            _logger.LogError("No entry in ");
            await SendErrorMessage(botClient, message, cancellationToken);
         }
         var configData = await _repository.GetOpeningLiquidationSubscriptionsForInstance(chosenInstance);

         var msg = "";

         msg += $"Liquidation Config Summary for {chosenInstance}\n";
         msg += $"Batch Size : {configData.BatchSize}\n";
         msg += $"Max Order Size : {configData.OrderSize}\n";
         msg += $"Upper Threshold % from FV : {configData.PercentageSpreadFromFV}\n";
         msg += $"Lower Threshold % from FV : {configData.PercentageSpreadLowerThreshold}\n";
         msg += $"End Date : {configData.EndDate.ToString("dd/MM/yyyy")}\n";
        
         msg += $"Maker/Taker Mode : {configData.MakerMode.ToString()}\n";

         var rows = new List<List<KeyboardButton>>();
         AddBeginAndBackButtons(rows);
         
         _state = MessageHandlingState.SHOW_CONFIG_SUMMARY_DISPLAYED;
         ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(rows);
         Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: msg,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
      }

      private async Task SendCategoryChoiceMessageAsync(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         var rows = new List<List<KeyboardButton>>();

         var liquidityKeyboard = new KeyboardButton("Liquidity");
         var m = new List<KeyboardButton>();
         m.Add(liquidityKeyboard);
         rows.Add(m);

         var marketMakingKeyboard = new KeyboardButton("MarketMaking");
         var t = new List<KeyboardButton>();
         t.Add(marketMakingKeyboard);
         rows.Add(t);

         ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(rows);
         Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Please choose an option:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
      }

      private async Task SendLiquidityInitialMenuMessageAsync(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         var rows = new List<List<KeyboardButton>>();

         var keyboard1 = new KeyboardButton("Strategy Config Control");
         var m = new List<KeyboardButton>();
         m.Add(keyboard1);
         rows.Add(m);

         var keyboard2 = new KeyboardButton("Strategy Instance Control");
         var t = new List<KeyboardButton>();
         t.Add(keyboard2);
         rows.Add(t);

         var keyboard3 = new KeyboardButton("Current State Information");
         var x = new List<KeyboardButton>();
         x.Add(keyboard3);
         rows.Add(x);

         AddBeginButton(rows);

         ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(rows);
         Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Please choose an option:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
      }

      private async Task PositionInstanceChosen(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {

         var result = await CheckForBeginButtonPress(botClient, message, cancellationToken);
         if (result)
            return;
         var chosenPositionInstance = "";
         if (_chosenPositionInstanceTable.ContainsKey(message.Chat.Id))
         {
            if (!string.IsNullOrEmpty(message.Text))
            {
               _chosenPositionInstanceTable[message.Chat.Id] = message.Text;
               chosenPositionInstance = message.Text;
            }
            else
            {
               _logger.LogWarning("Empty Text message for {User} in getting position instance", message.Chat.Username);
               await SendErrorMessage(botClient, message, cancellationToken);
               return;
            }
         }
         else
         {
            if (!string.IsNullOrEmpty(message.Text))
            {
               _chosenPositionInstanceTable.Add(message.Chat.Id, message.Text);
               chosenPositionInstance = message.Text;
            }
            else
            {
               _logger.LogWarning("Empty Text message for {User} in getting position instance", message.Chat.Username);
               await SendErrorMessage(botClient, message, cancellationToken);
               return;
            }
         }

         var chosenPositionAccount = "";
         if (_chosenPositionAccountTable.ContainsKey(message.Chat.Id))
         {
            chosenPositionAccount = _chosenPositionAccountTable[message.Chat.Id];
         }
         else
         {
            _logger.LogWarning("No Account chosen for {Instance} ", chosenPositionInstance);
            await SendErrorMessage(botClient, message, cancellationToken);
            return;
         }
         
         _inventoryMgr.InitConfig(chosenPositionAccount, chosenPositionInstance);
         var exchangeConfig = await _repository.GetStrategyExchangeConfigEntry(chosenPositionInstance);
         var liquidConfig = await _repository.GetOpeningLiquidationSubscriptionsForInstance(chosenPositionInstance);
         if (liquidConfig != null)
         {
            _inventoryMgr.InitCoins(liquidConfig.CoinPair.Name);
         }

         _jobNo++;

         _balancesTimer = new Timer();
         _balancesTimer.Interval = 1000;
         _balancesTimer.Elapsed += async (sender, e) => await OnBalanceTimerExpired(_jobNo);
         
         _jobNoChatIdTable.Add(_jobNo, message);
         foreach (var exchange in exchangeConfig.ExchangeDetails)
         {
            try
            {
               await GetBalances(exchange, _jobNo, message);
               
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Error in logon to connectors {Error}", e.Message);
            }
         }
         _balancesTimer.Start();

         int i = 0;

         //AddBeginButton(rows);
         _state = MessageHandlingState.POSITION_MENU_SELECTION_CHOSEN;
     //    ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(rows);
     //    Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
       //     text: "Please choose a menu item:",
       //     replyMarkup: keyboard,
       //     cancellationToken: cancellationToken);
      }

      private async Task OnBalanceTimerExpired(int jobNo)
      {
         _balancesTimer.Stop();
         var chosenPositionInstance = "";
         var msg = "";
         msg += "=============== Balance Info ==============\n";
         Message storedMessage = null;
         if (_exchangeBalancesByJobNo.ContainsKey(jobNo))
         {
            var val = _exchangeBalancesByJobNo[jobNo];

            foreach (var exchange in val)
            {
               var balList = exchange.Value;
               var venue = exchange.Key;
               msg += $"Balances for {venue}\n";
               foreach (var bal in balList)
               {
                  var coin = bal.Key;
                  var balance = bal.Value.Available;
                  balance = Math.Round(balance, 2, MidpointRounding.ToEven);
                  msg += $"{coin} Amount = {balance}\n";
               }

               msg += "----------------------\n";
               val.Remove(venue);

               

               if (_jobNoChatIdTable.ContainsKey(jobNo))
                  storedMessage = _jobNoChatIdTable[jobNo];
               else
               {
                  _logger.LogError("Can't associate jobNo {JobNo} to any chat id", jobNo);
                  return;
               }

               
               if (_chosenPositionInstanceTable.ContainsKey(storedMessage.Chat.Id))
               {
                  chosenPositionInstance = _chosenPositionInstanceTable[storedMessage.Chat.Id];
               }
               else
               {
                  _logger.LogError("No entry in _chosenPositionInstanceTable for chat Id {ChatId} username {UserName}",
                     storedMessage.Chat.Id, storedMessage.Chat.Username);
                  return;
               }
            }

            msg += "=============== Fill Info =================\n";
            var fillInfo = await _repository.GetFillsInfoForInstance(chosenPositionInstance);
            foreach (var fill in fillInfo)
            {
               var fillVenue = _venueTable[fill.VenueId].VenueName;
               var totalFills = fill.TotalFills;
               var dailyFills = fill.DailyFills;
               msg += $"{fillVenue}\n";
               msg += $"Total Fills   = {totalFills}\n";
               msg += $"Today's Fills = {dailyFills}\n";
               msg += "----------------------\n";

            }
            _state = MessageHandlingState.POSITION_MENU_SELECTION_CHOSEN;

            _exchangeBalancesByJobNo.Remove(jobNo);
            // _state = MessageHandlingState.POSITION_BALANCE_DISPLAYED;
            Message keyBoard = await _botClient.SendTextMessageAsync(chatId: storedMessage.Chat.Id,
               text: msg,
               // replyMarkup: keyboard,
               cancellationToken: new CancellationToken());
         }
         else
         {
            _logger.LogError("No entry for {JobNo} in _jobNoChatIdTable", jobNo);
            _balancesTimer.Dispose();
            return;
         }

         if (storedMessage != null)
         {
            storedMessage.Text = "Current State Information";
            await SendCurrentStateInformation(_botClient, storedMessage, new CancellationToken());
            _balancesTimer.Dispose();
         }
      }

      private async Task GetBalances(ExchangeDetailsDTO exchange, int jobNumber, Message message)
      {
         var chosenPositionAccount = "";
         var chosenPositionInstance = "";

         if (_chosenPositionInstanceTable.ContainsKey(message.Chat.Id))
         {
            chosenPositionInstance = _chosenPositionInstanceTable[message.Chat.Id];
         }
         else
         {
            _logger.LogError("No entry in _chosenPositionInstanceTable for Chat Id {ChatId} and Username {UserName}",
                    message.Chat.Id, message.Chat.Username);
            await SendErrorMessage(_botClient, message, new CancellationToken());
            return;
         }

         if (_chosenPositionAccountTable.ContainsKey(message.Chat.Id))
         {
            chosenPositionAccount = _chosenPositionAccountTable[message.Chat.Id];
         }
         else
         {
            _logger.LogError("No entry in _chosenPositionAccountTable for Chat Id {ChatId} and Username {UserName}",
               message.Chat.Id, message.Chat.Username);
            await SendErrorMessage(_botClient, message, new CancellationToken());
            return;
         }

         var topic = chosenPositionAccount + "." + chosenPositionInstance + Constants.BALANCES_TOPIC;
         _receiver.Subscribe(topic);
         _inventoryMgr.GetOpeningBalancesDirect(exchange.Venue.VenueName, _jobNo);
      }

      private async Task SendErrorMessage(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         _state = MessageHandlingState.INITIAL;
         Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: $"{message.Text} is not a valid option:",
            cancellationToken: cancellationToken);
      }

      private async Task SendNotAuthorisedMessage(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         var rows = new List<List<KeyboardButton>>();
         AddBeginAndBackButtons(rows);
         _state = MessageHandlingState.CONFIG_MENU_SELECTION_CHOSEN;
         if (_users.ContainsKey(message.Chat.Id.ToString()))
         {
            var user = _users[message.Chat.Id.ToString()];
            Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
               text: $"{user.UserName} is not authorized to use this channel:",
               cancellationToken: cancellationToken);
         }
         else
         {
            Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
               text: $"{message.Text} is not authorized to use this channel:",
               cancellationToken: cancellationToken);
         }
      }

      private async Task SendNotAuthorisedCommandMessage(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         var rows = new List<List<KeyboardButton>>();
         AddBeginAndBackButtons(rows);
         _state = MessageHandlingState.CONFIG_MENU_SELECTION_CHOSEN;
         if (_users.ContainsKey(message.Chat.Id.ToString()))
         {
            var user = _users[message.Chat.Id.ToString()];
            Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
               text: $"{user.UserName} is not authorized to execute this command:",
               cancellationToken: cancellationToken);
         }
         else
         {
            Message keyBoardOne = await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
               text: $"{message.Text} is not authorized to execute this command:",
               cancellationToken: cancellationToken);
         }
      }

      private async Task<bool> CheckForBeginAndBackButtonPresses(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         if (message.Text == "/begin")
         {
            _state = MessageHandlingState.CATEGORY_CHOICES;
            await SendCategoryChoiceMessageAsync(botClient, message, cancellationToken);
            return true;

         }
         else if (message.Text == "/back")
         {
            var chosenInstance = "";
            if (_chosenConfigInstance.ContainsKey(message.Chat.Id))
            {
               chosenInstance = _chosenConfigInstance[message.Chat.Id];
            }
            else
            {
               _logger.LogError("No entry in ");
               await SendErrorMessage(botClient, message, cancellationToken);
            }
            message.Text = chosenInstance;
            await ConfigInstanceChosen(botClient, message, cancellationToken);
            return true;
         }

         return false;

      }

      private async Task<bool> CheckForBeginButtonPress(ITelegramBotClient botClient, Message message,
         CancellationToken cancellationToken)
      {
         if (message.Text == "/begin")
         {
            _state = MessageHandlingState.CATEGORY_CHOICES;
            await SendCategoryChoiceMessageAsync(botClient, message, cancellationToken);
            return true;

         }
         return false;
      }

      private void AddBeginAndBackButtons(List<List<KeyboardButton>> rows)
      {
         var row1 = new List<KeyboardButton>();
         var button1 = new KeyboardButton("/back");
         row1.Add(button1);
         rows.Add(row1);

         var row2 = new List<KeyboardButton>();
         var button2 = new KeyboardButton("/begin");
         row2.Add(button2);
         rows.Add(row2);
      }

      private void AddBeginButton(List<List<KeyboardButton>> rows)
      {
         var row2 = new List<KeyboardButton>();
         var button2 = new KeyboardButton("/begin");
         row2.Add(button2);
         rows.Add(row2);
      }

      private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
      {
         var ErrorMessage = exception switch
         {
            ApiRequestException apiRequestException
               => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
         };

         _logger.LogError(ErrorMessage);
         return Task.CompletedTask;
      }
   }
}
