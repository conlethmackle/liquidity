using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
   public class Signing
   {
      public const string SectionName = "Liquidity:EncryptionKeys";
      public string MasterKey { get; set; }
      public string SecondaryKey { get; set; }
      public string TertiaryKey { get; set; }
   }
}
