using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTOs
{
   public class LocationDTO
   {
      public int LocationId { get; set; }
      public string LocationName { get; set; }
      public DateTime DateCreated { get; set; }
   }
}
