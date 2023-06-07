namespace BlazorLiquidity.Client
{
   public class ConnectorStatus
   {
      public ConnectorStatus()
      {
         Color = "black";
      }
      public string Name { get; set; }
      public string Status { get; set; }
      public string Color { get; set; }
   }
}
