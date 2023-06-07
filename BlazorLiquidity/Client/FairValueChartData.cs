namespace BlazorLiquidity.Client
{

   public class FairValueChartData
   {
      public DateTime Time { get; set; }
      public decimal Value { get; set; }
      //public decimal BitfinexValue { get; set; }

    //  public FairValueChartData(decimal binancePrice, decimal bitfinexPrice)
     // {
     //    BinanceValue = binancePrice;
     //    BitfinexValue = bitfinexPrice;
      //   Time = DateTime.UtcNow;
     // }

      public FairValueChartData()
      {
            
      }
   }
}
