using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models.Entities;

namespace Common.Models.DTOs
{
   public class ExchangeCoinMappingsDTO
   {
      public int Id { get; set; }
      [Required]
      public int VenueId { get; set; }
      public Venue Venue { get; set; }
      [Required]
      public int CoinId { get; set; }
      public Coin Coin { get; set; }
      [Required]
      public string ExchangeCoinName { get; set; }
   }
}
