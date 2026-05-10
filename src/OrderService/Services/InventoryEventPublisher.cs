using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public class InventoryEventPublisher
    {
        private readonly ServiceBusClient _client;
        private readonly string _topicName;
        public InventoryEventPublisher(IConfiguration config)
        {
            var connStr = config["ServiceBus:ConnectionString"];
            _topicName = config["ServiceBus:InventoryTopic"] ?? "inventory-events";
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
