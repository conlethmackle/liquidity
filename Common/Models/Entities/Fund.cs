using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class Fund
   {
      public int FundId { get; set; }
      public string FundName { get; set; }
      public int LocationId { get; set; }
      public Location Location { get; set; }
      public DateTime DateCreated { get; set; }
       
   }
}
