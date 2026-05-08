using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Threading.Tasks;
using OrderService.Models;

namespace OrderService.Services
{
    public class OrderEventPublisher
    {
        private readonly ServiceBusClient _client;
        private readonly string _topicName;
        public OrderEventPublisher(IConfiguration config)
        {
            var connStr = config["ServiceBus:ConnectionString"];
            _topicName = config["ServiceBus:OrderTopic"] ?? "order-events";
            _client = new ServiceBusClient(connStr);
        }

        public async Task PublishEventAsync(string eventType, object payload)
        {
            var sender = _client.CreateSender(_topicName);
            var message = new ServiceBusMessage(JsonSerializer.Serialize(new { eventType, payload }))
            {
                Subject = eventType
            };
            await sender.SendMessageAsync(message);
        }
    }
}
