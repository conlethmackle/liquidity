using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Common.Utilities
{
   public static class KeyInitialiser
   {
      public static void Initialize(IConfigurationRoot config)
      {
         string str = config["Liquidity:EncryptionKeys:MasterKey"];
         if (!string.IsNullOrEmpty(str))
         {
            Keys.MasterKey = str;
         }

         str = config["Liquidity:EncryptionKeys:SecondaryKey"];
         if (!string.IsNullOrEmpty(str))
         {
            Keys.SecondaryKey = str;
         }

         str = config["Liquidity:EncryptionKeys:TertiaryKey"];
         if (!string.IsNullOrEmpty(str))
         {
            Keys.TertiaryKey = str;
         }
      }
   }
}
