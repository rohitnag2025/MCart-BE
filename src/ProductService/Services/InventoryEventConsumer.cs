using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductService.Models;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Services
{
    public class InventoryEventConsumer : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _scopeFactory;
        public InventoryEventConsumer(IConfiguration config, IServiceScopeFactory scopeFactory)
        {
            var connStr = config["ServiceBus:ConnectionString"];
            var topic = config["ServiceBus:InventoryTopic"] ?? "inventory-events";
            var subscription = config["ServiceBus:ProductSubscription"] ?? "product-service";
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
            var eventObj = JsonSerializer.Deserialize<InventoryEvent>(body);
            if (eventObj?.eventType == "InventoryReduceRequested")
            {
                var payload = JsonSerializer.Deserialize<InventoryReducePayload>(eventObj.payload.ToString());
                if (payload != null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
                    foreach (var item in payload.Items)
                    {
                        var product = await db.Products.FindAsync(item.ProductId);
                        if (product != null && product.Stock >= item.Quantity)
                        {
                            product.Stock -= item.Quantity;
                        }
                    }
                    await db.SaveChangesAsync();
                    // Publish InventoryReduced event (not shown here)
                }
            }
            await args.CompleteMessageAsync(args.Message);
        }

        private class InventoryEvent
        {
            public string eventType { get; set; }
            public JsonElement payload { get; set; }
        }
        private class InventoryReducePayload
        {
            public List<InventoryItem> Items { get; set; }
        }
        private class InventoryItem
        {
            public Guid ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
