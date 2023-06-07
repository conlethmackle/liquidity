using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker
{
   public static class PublishHelper
   {
      public static void Publish<T>(string subject, T data, IMessageBroker broker)
      {
         using (var stream = new MemoryStream())
         {
            Serializer.Serialize(stream, data);
            var bytes = stream.ToArray();
            broker.PublishToSubject(subject, bytes);
         }
      }

      public static void PublishToTopic<T>(string routingKey, T data, IMessageBroker broker)
      {
         //using (var stream = new MemoryStream())
         //  {
         //    Serializer.Serialize(stream, data);
         //     var bytes = stream.ToArray();
         ///     broker.PublishToTopicSubject(routingKey, bytes);
         //}
         using (var stream = new MemoryStream())
         {
            Serializer.Serialize(stream, data);
            var bytes = stream.ToArray();
            broker.PublishToSubject(routingKey, bytes);
         }
      }
   }
}
