using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
   public static class Keys
   {
      private static string _masterKey = "";
      private static string _secondaryKey = "";
      private static string _tertiaryKey = "";
      
      public static string MasterKey
      {
         get => _masterKey;
         set
         {
            if (string.IsNullOrEmpty(_masterKey))
            {
               _masterKey = value;
            }
            else
            {
               if (value != _masterKey)
               {
                  throw new System.Exception("Variable can only be set once!");
               }
            }
         }
      }

      public static string SecondaryKey
      {
         get => _secondaryKey;
         set
         {
            if (string.IsNullOrEmpty(_secondaryKey))
            {
               _secondaryKey = value;
            }
            else
            {
               if (value != _secondaryKey)
               {
                  throw new System.Exception("Variable can only be set once!");
               }
            }
         }
      }

      public static string TertiaryKey
      {
         get => _tertiaryKey;
         set
         {
            if (string.IsNullOrEmpty(_tertiaryKey))
            {
               _tertiaryKey = value;
            }
            else
            {
               if (value != _tertiaryKey)
               {
                  throw new System.Exception("Variable can only be set once!");
               }
            }
         }
      }
   }
}
