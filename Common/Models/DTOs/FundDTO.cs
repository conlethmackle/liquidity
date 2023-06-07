using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class FundDTO
   {
      public int FundId { get; set; }
      public string FundName { get; set; }
      public int LocationId { get; set; }
      public LocationDTO Location { get; set; }
      public DateTime DateCreated { get; set; }
   }
}
