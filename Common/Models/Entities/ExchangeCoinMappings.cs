using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class ExchangeCoinMappings
   {
      public int Id { get; set; }
      public int VenueId { get; set; }
      public Venue Venue { get; set; }
      [Required, NotNull]
      public int CoinId { get; set; }
      public Coin Coin { get; set; }
      [Required, NotNull]
      public string ExchangeCoinName { get; set; }
   }
}
