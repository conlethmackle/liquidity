using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class ExchangeDetailsDTO
   {
      public int ExchangeDetailsId { get; set; }

      
      [Required(AllowEmptyStrings = false)]
      public string Name { get; set; }      
      public bool IsEnabled { get; set; }
      [Required]
      public int SPId { get; set; }
      [Required]
      [Range(1, int.MaxValue, ErrorMessage = "Please select an exchange from list")]
      public int VenueId { get; set; }
      public VenueDTO Venue { get; set; }
      [Required]
      [Range(1, int.MaxValue, ErrorMessage = "Please select an apikey or create one ")]
      public int ApiKeyId { get; set; }
      [Required]
      public ApiKeyDTO ApiKey { get; set; }
     // [Required]
     // [Range(1, int.MaxValue, ErrorMessage = "Please select an Strategy Instance Id  ")]
      public int StrategySPSubscriptionConfigId { get; set; }
      [Required]
      public string CoinPairs { get; set; }
      
      public int OpeningExchangeBalanceId { get; set; }
      public OpeningExchangeBalanceDTO OpeningExchangeBalance { get; set; }
      public DateTime DateCreated { get; set; }

      public ExchangeDetailsDTO()
      {
            Venue = new VenueDTO();
            ApiKey = new ApiKeyDTO();
            OpeningExchangeBalance = new OpeningExchangeBalanceDTO();
      }
   }
}
