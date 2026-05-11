using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentService.Models;

namespace PaymentService.Services
{
    public class OrderEventConsumer : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly PaymentEventPublisher _eventPublisher;
        public OrderEventConsumer(IConfiguration config, IServiceScopeFactory scopeFactory, PaymentEventPublisher eventPublisher)
        {
            var connStr = config["ServiceBus:ConnectionString"];
            var topic = config["ServiceBus:OrderTopic"] ?? "order-events";
            var subscription = config["ServiceBus:Subscription"] ?? "payment-service";
            var client = new ServiceBusClient(connStr);
            _processor = client.CreateProcessor(topic, subscription);
            _scopeFactory = scopeFactory;
            _eventPublisher = eventPublisher;
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
            var eventObj = JsonSerializer.Deserialize<OrderEvent>(body);
            if (eventObj?.eventType == "OrderPlaced")
            {
                var order = JsonSerializer.Deserialize<OrderPayload>(eventObj.payload.ToString());
                if (order != null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
                    var payment = new Payment
                    {
                        PaymentId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        UserId = order.UserId,
                        Amount = order.TotalAmount,
                        Status = "Pending"
                    };
                    db.Payments.Add(payment);
                    await db.SaveChangesAsync();

                    // No external payment provider configured here; mark payment as Initiated and set a placeholder external id
                    payment.Status = "Initiated";
                    payment.StripePaymentIntentId = Guid.NewGuid().ToString(); // placeholder external payment id
                    await db.SaveChangesAsync();

                    // Publish PaymentInitiated event
                    await _eventPublisher.PublishEventAsync("PaymentInitiated", new
                    {
                        payment.PaymentId,
                        payment.OrderId,
                        payment.UserId,
                        payment.Amount,
                        payment.Status,
                        payment.StripePaymentIntentId
                    });
                }
            }
            await args.CompleteMessageAsync(args.Message);
        }

        private class OrderEvent
        {
            public string eventType { get; set; }
            public JsonElement payload { get; set; }
        }
        private class OrderPayload
        {
            public Guid OrderId { get; set; }
            public Guid UserId { get; set; }
            public decimal TotalAmount { get; set; }
        }
    }
}
