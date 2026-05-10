using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShippingService.Models;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ShippingService.Services
{
    public class ShippingEventConsumer : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _scopeFactory;
        public ShippingEventConsumer(IConfiguration config, IServiceScopeFactory scopeFactory)
        {
            var connStr = config["ServiceBus:ConnectionString"];
            var topic = config["ServiceBus:ShippingTopic"] ?? "shipping-events";
            var subscription = config["ServiceBus:Subscription"] ?? "shipping-service";
            var client = new ServiceBusClient(connStr);
            _processor = client.CreateProcessor(topic, subscription);
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += args => Task.CompletedTask;
            await _processor.StartProcessingAsync(stoppingToken);
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            var eventObj = JsonSerializer.Deserialize<ShippingEvent>(body);
            if (eventObj?.eventType == "ShippingRequested")
            {
                var payload = JsonSerializer.Deserialize<ShippingRequestedPayload>(eventObj.payload.ToString());
                if (payload != null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();
                    var shipment = new Shipment
                    {
                        ShipmentId = Guid.NewGuid(),
                        OrderId = payload.OrderId,
                        UserId = payload.UserId,
                        ShippingAddress = payload.ShippingAddress,
                        Status = "Pending"
                    };
                    db.Shipments.Add(shipment);
                    await db.SaveChangesAsync();
                }
            }
            await args.CompleteMessageAsync(args.Message);
        }

        private class ShippingEvent
        {
            public string eventType { get; set; }
            public JsonElement payload { get; set; }
        }
        private class ShippingRequestedPayload
        {
            public Guid OrderId { get; set; }
            public Guid UserId { get; set; }
            public string ShippingAddress { get; set; }
        }
    }
}
