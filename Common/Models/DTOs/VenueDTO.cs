using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models.Entities;

namespace Common.Models.DTOs
{
   public class VenueDTO
   {
      public int VenueId { get; set; }
      public string VenueName { get; set; }
      public VenueType VenueType { get; set; }
   }
}
