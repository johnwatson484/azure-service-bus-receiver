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

            var options = new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false
            };

            await using var queueProcessor = client.CreateProcessor(config.Queue, options);
            queueProcessor.ProcessMessageAsync += MessageHandler;
            queueProcessor.ProcessErrorAsync += ErrorHandler;


            await using var subscriptionProcessor = client.CreateProcessor(config.Topic, config.Subscription, options);
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

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            try
            {
                Console.WriteLine($"Message received - {body}");
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to process message {ex}");
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
