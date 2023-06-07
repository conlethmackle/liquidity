using BlazorLiquidity.Shared;
using System.Text.Json;

namespace BlazorLiquidity.Client
{
   
   public partial class App
   {
      private string _fairValueSymbol { get; set; }
      private decimal _fairValuePrice { get; set; }
      private string _fairPriceDisplay { get; set; } = "text-success";
      private decimal _previousPrice = 0;
      private decimal _btcBalance { get; set; }
      private decimal _usdtBalance { get; set; }
      private decimal _btcOpeningBalance { get; set; }
      private decimal _usdtOpeningBalance { get; set; }

      private void HandleMessages(MessageQueueData msg)
      {
         switch(msg.MessageType)
         {
            case QueueMsgTypes.FAIRVALUEUPDATE:
               var fairValueData = JsonSerializer.Deserialize<FairValueData>(msg.Data);
               HandleFairValueUpdate(fairValueData);
               break;
            case QueueMsgTypes.BALANCEUPDATE:
               var balanceUpdate = JsonSerializer.Deserialize<BalanceUpdate>(msg.Data);
               HandleBalanceUpdate(balanceUpdate);
               break;
            case QueueMsgTypes.OPENINGBALANCE:
               var openingBalance = JsonSerializer.Deserialize<BalanceUpdate>(msg.Data);
               HandleOpeningBalance(openingBalance);
               break;
            default:
               break;
         }
      }

      private void HandleBalanceUpdate(BalanceUpdate? balanceUpdate)
      {
         if (balanceUpdate != null)
         {
            if (balanceUpdate.Currency.Equals("BTC"))
            {
               _btcBalance = balanceUpdate.Balance.Available;
            }
            else if (balanceUpdate.Currency.Equals("USDT"))
            {
               _usdtBalance = balanceUpdate.Balance.Available;
            }
            StateHasChanged();
         }
      }

      private void HandleOpeningBalance(BalanceUpdate? balanceUpdate)
      {
         if (balanceUpdate != null)
         {
            if (balanceUpdate.Currency.Equals("BTC"))
            {
               _btcOpeningBalance = balanceUpdate.Balance.Available;
            }
            else if (balanceUpdate.Currency.Equals("USDT"))
            {
               _usdtOpeningBalance = balanceUpdate.Balance.Available;
            }
            StateHasChanged();
         }
      }

      private void HandleFairValueUpdate(FairValueData? fairValueData)
      {
         if (fairValueData != null)
         {
            _fairValueSymbol = fairValueData.Symbol;
            _fairValuePrice = fairValueData.Price;
            if (_fairValuePrice != _previousPrice)
            {
               _fairValuePrice = Math.Round(fairValueData.Price, 2, MidpointRounding.ToEven);
               if (_fairValuePrice > _previousPrice)
                  _fairPriceDisplay = "text-success";
               else if (_fairValuePrice < _previousPrice)
                  _fairPriceDisplay = "text-danger";
               else
                  _fairPriceDisplay = "text-dark";
               _previousPrice = _fairValuePrice;
               StateHasChanged();
            }
         }
      }
   }
}
