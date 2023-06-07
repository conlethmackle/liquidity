using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models.Entities
{
   public class ExchangeDetails
   {
      [Key]
      public int ExchangeDetailsId { get; set; }
      public string Name { get; set; }
      [ForeignKey("SPId")]
      public int SPId { get; set; }
      public SP SP { get; set; }
   //   [ForeignKey("VenueId")]
      public int VenueId { get; set; }  
      public Venue Venue { get; set; }  
      public int StrategySPSubscriptionConfigId { get; set; }
      public bool IsEnabled { get; set; }
      public DateTime DateCreated { get; set; }
      public int ApiKeyId { get; set; }
      public ApiKey ApiKey { get; set; }
      public string CoinPairs { get; set; }
      public int OpeningExchangeBalanceId { get; set; }
      public OpeningExchangeBalance OpeningExchangeBalance { get; set; }

   }
}
