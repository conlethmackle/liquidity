using System;
using System.Threading.Tasks;

namespace MessageBroker
{
   public interface IMessageBroker
   {
      void Connect(bool rethrow);
      void PublishToSubject(string subject, byte[] data);
      void SubscribeToSubject(string subject, Action<string, byte[]> action);
      void PublishToTopicSubject(string routingKey, byte[] data);
      void SubscribeToTopicSubject(string bindingKey, Action<string, byte[]> action);
   }
}
