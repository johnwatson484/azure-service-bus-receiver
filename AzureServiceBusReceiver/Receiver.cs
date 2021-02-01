using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace AzureServiceBusSender
{
    class Receiver : BackgroundService
    {
        ServiceBusConfig config;
        string connectionString;

        public Receiver(ServiceBusConfig config)
        {
            this.config = config;
            connectionString = CreateConnectionString(config);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var client = new ServiceBusClient(connectionString);

            await using var queueProcessor = client.CreateProcessor(config.Queue);
            queueProcessor.ProcessMessageAsync += MessageHandler;
            queueProcessor.ProcessErrorAsync += ErrorHandler;

            await using var subscriptionProcessor = client.CreateProcessor(config.Topic, config.Subscription);
            subscriptionProcessor.ProcessMessageAsync += MessageHandler;
            subscriptionProcessor.ProcessErrorAsync += ErrorHandler;

            await queueProcessor.StartProcessingAsync();
            await subscriptionProcessor.StartProcessingAsync();

            while(!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1);
            }
        }

        private string CreateConnectionString(ServiceBusConfig config)
        {
            return $"Endpoint=sb://{config.Host}/;SharedAccessKeyName={config.Username};SharedAccessKey={config.Password}";
        }

        private Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Message received - {body}");
            return Task.CompletedTask;
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
