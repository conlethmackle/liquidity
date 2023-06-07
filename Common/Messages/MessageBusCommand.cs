using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Common.Messages
{
   [ProtoContract]
   public class MessageBusCommand
   {
      [ProtoMember(1)]
      public CommandTypesEnum CommandType { get; set; }
      [ProtoMember(2)]
      public string Exchange { get; set; }
      [ProtoMember(3)]
      public bool IsPrivate { get; set; }
      [ProtoMember(4)]
      public string AccountName { get; set; }
      [ProtoMember(5)]
      public string InstanceName { get; set; }
      [ProtoMember(6)]
      public int JobNo { get; set; }
      [ProtoMember(7)]
      public string Data { get; set; }
    //  public T Data { get; set; }

      public static byte[] ProtoSerialize<T>(T record) where T : class
      {
         if (null == record) return null;

         try
         {
            using (var stream = new MemoryStream())
            {
               Serializer.Serialize(stream, record);
               return stream.ToArray();
            }
         }
         catch
         {
            throw;
         }
      }

      public static T ProtoDeserialize<T>(byte[] data) where T : class
      {
         if (null == data) return null;

         try
         {
            using (var stream = new MemoryStream(data))
            {
               return Serializer.Deserialize<T>(stream);
            }
         }
         catch
         {
            // Log error
            throw;
         }
      }
   }
}
