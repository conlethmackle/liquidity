using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Common.Models.Entities
{
   public class ExchangeCoinpairMapping
   {
      [Key]
      public int ExchangeCoinpairLookupId { get; set; }
      [Required, NotNull]
      public int VenueId { get; set; }
      public Venue Venue { get; set; }
      [Required, NotNull]
      public int CoinPairId { get; set; }
      public CoinPair CoinPair { get; set; }
      [Required, NotNull]
      public string ExchangeCoinpairName { get; set; }
   }
}
