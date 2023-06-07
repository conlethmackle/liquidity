using System.Text.Json.Serialization;
using ProtoBuf;

namespace Common.Messages
{
   [ProtoContract]
   public class MessageBusReponse
   {
      [ProtoMember(1)]
      [JsonPropertyName("responseType")]
      public ResponseTypeEnums ResponseType { get; set; }
      [ProtoMember(2)]
      [JsonPropertyName("fromVenue")]
      public string FromVenue { get; set; }
      [ProtoMember(3)]
      [JsonPropertyName("isPrivate")]
      public bool IsPrivate { get; set; }
      [ProtoMember(4)]
      [JsonPropertyName("accountName")]
      public string AccountName { get; set; }
      [ProtoMember(5)]
      [JsonPropertyName("originatingSource")]
      public string OriginatingSource { get; set; }
      [ProtoMember(6)]
      [JsonPropertyName("jobNo")]
      public int JobNo { get; set; }
      [ProtoMember(7)]
      [JsonPropertyName("data")]
      public string Data { get; set; }
      [ProtoMember(8)]
      [JsonPropertyName("success")]
      public bool Success { get; set; }

      public MessageBusReponse()
      {
         Success = true;
      }
   }
}
