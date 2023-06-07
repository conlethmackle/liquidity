using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Entities
{
   public class ApiKey
   {
      public int ApiKeyId { get; set; }   
      public string Key { get; set; }
      public string Secret { get; set; }
      public string PassPhrase { get; set; }
      public string Description { get; set; }
      public string AccountName { get; set; }
      public bool IsSubAccount { get; set; }
      public string SubAccountName { get; set; }
      public string Password { get; set; }
      public DateTime DateCreated { get; set; } = DateTime.UtcNow;
   }
}
