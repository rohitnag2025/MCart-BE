using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderService.Models;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public class PaymentEventConsumer : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _scopeFactory;
        public PaymentEventConsumer(IConfiguration config, IServiceScopeFactory scopeFactory)
        {
            var connStr = config["ServiceBus:ConnectionString"];
            var topic = config["ServiceBus:PaymentTopic"] ?? "payment-events";
            var subscription = config["ServiceBus:OrderSubscription"] ?? "order-service";
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
            var eventObj = JsonSerializer.Deserialize<PaymentEvent>(body);
            if (eventObj?.eventType == "PaymentSucceeded")
            {
                var payment = JsonSerializer.Deserialize<PaymentPayload>(eventObj.payload.ToString());
                if (payment != null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                    var inventoryPublisher = scope.ServiceProvider.GetRequiredService<InventoryEventPublisher>();
                    var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == payment.OrderId);
                    if (order != null)
                    {
                        order.Status = "Paid";
                        await db.SaveChangesAsync();

                        // Publish InventoryReduceRequested event
                        await inventoryPublisher.PublishEventAsync("InventoryReduceRequested", new
                        {
                            Items = order.Items.Select(i => new { i.ProductId, i.Quantity }).ToList()
                        });

                        // Publish ShippingRequested event
                        await inventoryPublisher.PublishEventAsync("ShippingRequested", new
                        {
                            OrderId = order.OrderId,
                            UserId = order.UserId,
                            ShippingAddress = order.ShippingAddress,
                            Items = order.Items.Select(i => new { i.ProductId, i.ProductName, i.Quantity }).ToList()
                        });
                    }
                }
            }
            else if (eventObj?.eventType == "PaymentFailed")
            {
                var payment = JsonSerializer.Deserialize<PaymentPayload>(eventObj.payload.ToString());
                if (payment != null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                    var order = await db.Orders.FindAsync(payment.OrderId);
                    if (order != null)
                    {
                        order.Status = "PaymentFailed";
                        await db.SaveChangesAsync();
                    }
                }
            }
            await args.CompleteMessageAsync(args.Message);
        }

        private class PaymentEvent
        {
            public string eventType { get; set; }
            public JsonElement payload { get; set; }
        }
        private class PaymentPayload
        {
            public Guid PaymentId { get; set; }
            public Guid OrderId { get; set; }
            public Guid UserId { get; set; }
            public decimal Amount { get; set; }
            public string Status { get; set; }
        }
    }
}
