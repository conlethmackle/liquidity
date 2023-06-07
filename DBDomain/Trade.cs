using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBDomain
{
   public class Trade
   {
      public int Id { get; set; }
      public string Venue { get; set; }      
      public string Symbol { get; set; }     
      public bool IsBuy { get; set; }    
      public string OrderId { get; set; } 
      public DateTime OrderTime { get; set; }    
      public decimal Quantity { get; set; }     
      public decimal FilledQuantity { get; set; }    
      public decimal Price { get; set; }    
      public decimal MatchedPrice { get; set; }   
      public decimal MatchedSize { get; set; }   
      public string ClientOid { get; set; }   
      public decimal RemainingQuantity { get; set; }         
      public bool IsTaker { get; set; }     
      public DateTime TS { get; set; }
   }
}
