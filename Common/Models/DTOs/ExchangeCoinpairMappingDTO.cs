using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models.Entities;

namespace Common.Models.DTOs
{
   public class ExchangeCoinpairMappingDTO
   {
      public int ExchangeCoinpairLookupId { get; set; }
      [Required]
      public int VenueId { get; set; }
      public Venue Venue { get; set; }
      [Required]
      public int CoinPairId { get; set; }
      public CoinPair CoinPair { get; set; }
      [Required]
      public string ExchangeCoinpairName { get; set; }
   }
}
