using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Models;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService.ServiceBus
{
    public class NotificationBusConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private ServiceBusProcessor? _processor;

        public NotificationBusConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var client = new ServiceBusClient(_configuration["ServiceBus:ConnectionString"]);
            _processor = client.CreateProcessor(_configuration["ServiceBus:QueueName"], new ServiceBusProcessorOptions());
            _processor.ProcessMessageAsync += ProcessMessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;
            await _processor.StartProcessingAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            var notification = JsonSerializer.Deserialize<Notification>(body);
            if (notification != null)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
                notification.Status = "Pending";
                db.Notifications.Add(notification);
                await db.SaveChangesAsync();
                // TODO: Trigger actual sending (email/SMS/push)
            }
            await args.CompleteMessageAsync(args.Message);
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            // Log error
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_processor != null)
                await _processor.StopProcessingAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
    }
}
