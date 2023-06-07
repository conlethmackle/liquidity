using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public enum VenueType
   {
      EXCHANGE = 1,
      MASTER = 2,
   }

   public class Venue
   {
      public int VenueId { get; set; }
      public string VenueName { get; set; }
      public VenueType VenueType { get; set; }
   }
}
