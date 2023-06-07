using System;

namespace Common.Models.DTOs
{
   public class ApiKeyDTO
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
      public DateTime DateCreated { get; set; }
   }
}