using MessageBroker.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataStore;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBroker
{
   public class RabbitMQBroker : IMessageBroker
   {
      private readonly ILogger<RabbitMQBroker> _logger;
     // private readonly MessageBrokerConfig _config;
      private static ConcurrentDictionary<string, IModel> _fanoutMap { get; set; } = new ConcurrentDictionary<string, IModel>();
      private static ConcurrentDictionary<string, IModel> _topicMap { get; set; } = new ConcurrentDictionary<string, IModel>();

      private static Dictionary<string, Action<string, byte[]>> _fanoutSubscribers =
         new Dictionary<string, Action<string, byte[]>>();
      private static Dictionary<string, Action<string, byte[]>> _topicSubscribers =
         new Dictionary<string, Action<string, byte[]>>();
      private IConnection _connection { get; set; }
      private readonly IPortfolioRepository _repository;      

      public RabbitMQBroker(ILoggerFactory loggerFactory, IPortfolioRepository repository)
      {
         _logger = loggerFactory.CreateLogger<RabbitMQBroker>();
        // _config = config.Value;
         _repository = repository;
         //var mqSettings = await _repository.GetMQSettings();
         _fanoutSubscribers = new Dictionary<string, Action<string, byte[]>>();
         _topicSubscribers = new Dictionary<string, Action<string, byte[]>>();
         Connect(true);
      }

      public void  Connect(bool rethrow)
      {        
         var mqSettings =  _repository.GetMQSettings();        
         var factory = new ConnectionFactory()
         {
            HostName = mqSettings.Url, //_config.Endpoint, // "localhost", //
            UserName = mqSettings.UserName, // "guest", //
            Password = mqSettings.Password, // "guest", //
            Port     = mqSettings.Port, // 5672, //
            VirtualHost = mqSettings.VirtualHost // "/" //
         };

         try
         {
            _connection = factory.CreateConnection();
         }
         catch (Exception e)
         {
            _logger.LogError("RabbitConnection - error connecting to RabbitMQ Server {Message}", e.Message);
            if (rethrow)
               throw;
         }
       }

       public void PublishToSubject(string subject, byte[] data)
       {
         IModel channel;
         if (_fanoutMap is null)
            _fanoutMap = new ConcurrentDictionary<string, IModel>();

         if (_fanoutMap.ContainsKey(subject))
         {
            channel = _fanoutMap[subject];
            if (channel.IsClosed)
            {
               channel = _connection?.CreateModel();
               channel?.ExchangeDeclare(subject, ExchangeType.Fanout, autoDelete: false, durable: false);
               _fanoutMap[subject] = channel;
               _logger.LogError("Channel for {Subject} has closed - reconnecting", subject);
            }
         }
         else
         {
            channel = _connection?.CreateModel();
            channel?.ExchangeDeclare(subject, ExchangeType.Fanout, autoDelete: false, durable: false);
            _fanoutMap[subject] = channel;
         }

         try
         {
            channel?.BasicPublish(exchange: subject,
                routingKey: "",
                basicProperties: null,
                body: data);
         }
         catch (Exception e)
         {
            _logger.LogError("RabbitMQ Error - {Message}", e.Message);
            throw;
         }
       }

       public void SubscribeToSubject(string subject, Action<string, byte[]> action)
       {
         try
         {          
            if (!_fanoutSubscribers.ContainsKey(subject))
            {
               _fanoutSubscribers[subject] = action;
            }

            IModel channel = _connection?.CreateModel();
            channel?.ExchangeDeclare(subject, ExchangeType.Fanout, autoDelete: false, durable: false);
            _logger.LogInformation("Creating Exchange for {Subject}", subject);
            var queueName = channel?.QueueDeclare(
                durable: false,
                exclusive: false,
                autoDelete: true).QueueName;
            channel?.QueueBind(queue: queueName,
                  exchange: subject,
                  routingKey: "");

            
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
               var body = ea.Body.ToArray();
               action(subject, body);
            };
            channel?.BasicConsume(queue: queueName,
                  autoAck: true,
                  consumer: consumer);
         }
         catch (Exception e)
         {
            _logger.LogError("Error in SubscribeToSubject - Exception {Message}", e.Message);
            throw;
         }
       }

      public void PublishToTopicSubject(string routingKey, byte[] data)
      {
         IModel channel;
        
         if (_topicMap.ContainsKey(routingKey))
         {
            channel = _topicMap[routingKey];
            if (channel.IsClosed)
            {
               channel = _connection?.CreateModel();
               channel?.ExchangeDeclare(Common.Constants.TOPIC_EXCHANGE, ExchangeType.Topic, autoDelete: false, durable: false);
               _topicMap[routingKey] = channel;
               _logger.LogError("Channel for {Subject} has closed - reconnecting", Common.Constants.TOPIC_EXCHANGE);
            }
         }
         else
         {
            channel = _connection?.CreateModel();
            channel?.ExchangeDeclare(Common.Constants.TOPIC_EXCHANGE, ExchangeType.Topic, autoDelete: false, durable: false);
            _topicMap[routingKey] = channel;
         }

         try
         {
            channel?.BasicPublish(exchange: Common.Constants.TOPIC_EXCHANGE,
                routingKey: routingKey,
                basicProperties: null,
                body: data);
         }
         catch (Exception e)
         {
            _logger.LogError("RabbitMQ Error - {Message}", e.Message);
            throw;
         }
      }

      public void SubscribeToTopicSubject(string bindingKey, Action<string, byte[]> action)
      {
         try
         {
            SubscribeToSubject(bindingKey, action);
         /*   if (!_topicSubscribers.ContainsKey(bindingKey))
            {
               _topicSubscribers[bindingKey] = action;
            }

            IModel channel = _connection?.CreateModel();
            channel?.ExchangeDeclare(Common.Constants.TOPIC_EXCHANGE, ExchangeType.Topic, autoDelete: false, durable: false);
            _logger.LogInformation("Creating Exchange for {Subject}", bindingKey);
            var queueName = channel?.QueueDeclare(
                durable: false,
                exclusive: false,
                autoDelete: true).QueueName;
            channel?.QueueBind(queue: queueName,
                  exchange: Common.Constants.TOPIC_EXCHANGE,
                  routingKey: bindingKey);

            //var consumer = new AsyncEventingBasicConsumer(channel);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
               //await Task.Yield();
               var body = ea.Body.ToArray();
               action(bindingKey, body);
               
            };
            channel?.BasicConsume(queue: queueName,
                  autoAck: true,
                  consumer: consumer); */
         }
         catch (Exception e)
         {
            _logger.LogError("Error in SubscribeToSubject - Exception {Message}", e.Message);
            throw;
         }
      }


      public void ResubscribeToAllSubjects()
      {
         if (_fanoutMap is null)
            _fanoutMap = new ConcurrentDictionary<string, IModel>();

         // Close all previous channels
         foreach (var pair in _fanoutMap)
         {
            _logger.LogInformation("Closing channel to subject {Subject}", pair.Key);
            var channel = pair.Value;
            channel.Close();
         }

         _fanoutMap.Clear();

         foreach (var pair in _fanoutSubscribers)
         {
            SubscribeToSubject(pair.Key, pair.Value);
         }

         if (_topicMap is null)
            _topicMap = new ConcurrentDictionary<string, IModel>();

         // Close all previous channels
         foreach (var pair in _topicMap)
         {
            _logger.LogInformation("Closing channel to subject {Subject}", pair.Key);
            var channel = pair.Value;
            channel.Close();
         }

         _topicMap.Clear();

         foreach (var pair in _topicSubscribers)
         {
            SubscribeToTopicSubject(pair.Key, pair.Value);
         }

      }



    /*  private void PublishToRabbit(string engine, byte[] message)
      {
         _channel.BasicPublish(exchange: _exchangeName,
               routingKey: engine,
               basicProperties: null,
               body: message);
      }*/
   }
}
