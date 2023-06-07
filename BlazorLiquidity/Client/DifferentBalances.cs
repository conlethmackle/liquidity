namespace BlazorLiquidity.Client
{
   public class DifferentBalances
   {
      public int Id { get; set; }
      public string Account { get; set; }
      public string Instance { get; set; }
      public string Venue { get; set; }
      public decimal TotalLiqudationFrom { get; set; }
      public string FromCoinName { get; set; }
      public decimal TotalLiqudationTo { get; set; }
      public string ToStableCoinName { get; set; }
      public decimal DayLiquidationFrom { get; set; }
      public decimal DayLiquidationTo { get; set; }
      public decimal StartOfDayBalanceForLiquidationCoin { get; set; }
      public decimal StartOfDayBalanceForLiquidatedToCurrency { get; set; }
      public decimal OpeningBalanceForLiquidatedCoin { get; set; }
      public decimal OpeningBalanceForLiquidatedToCurrency { get; set; }
      public decimal CurrentAvailableCoinBalance { get; set; }
      public decimal CurrentAvailableStableBalance { get; set; }
      public decimal AmountToBeLiquidated { get; set; }
      public decimal TotalFees { get; set; }
      public decimal DailyFees { get; set; }
      
   }
}
