
using Azure.Messaging.ServiceBus;
using System.Diagnostics;
using System.Text;

namespace WebApplication1.Services
{
    public class ServiceBusHostedService : IHostedService, IDisposable
    {

        private readonly string _connectionString = "";
        private readonly string _queueName = "sbq-azuretest1";
        private const string topicName = "sbt-azuretest1";
        private const string subscriptionName = "sbts-azuretest1";
        private readonly ILogger<ServiceBusHostedService> _logger;
        private ServiceBusClient _client = null;
        ServiceBusProcessor processor = null;
        ServiceBusProcessor processorTopic = null;
        public ServiceBusHostedService(IConfiguration configuration, ILogger<ServiceBusHostedService> logger)
        {
            _connectionString = configuration.GetConnectionString("ServiceBusService");
            _logger = logger;
        }
        public void Dispose()
        {
            _client?.DisposeAsync();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client = new ServiceBusClient(_connectionString);
            processor = _client.CreateProcessor(_queueName, new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;
            await processor.StartProcessingAsync();

            processorTopic = _client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions());
            processorTopic.ProcessMessageAsync += MessageHandler;
            processorTopic.ProcessErrorAsync += ErrorHandler;
            await processorTopic.StartProcessingAsync();
        }

        Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = Encoding.UTF8.GetString(args.Message.Body);
            _logger.LogInformation($"Received: {body}");
            return Task.CompletedTask;
        }

        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogInformation(args.Exception.ToString());
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await processor.StopProcessingAsync();
        }
    }
}
