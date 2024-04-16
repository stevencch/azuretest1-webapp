using Azure.Messaging.ServiceBus;
using System.Text;

namespace WebApplication1.Services
{
    public class ServiceBusService
    {
        private readonly string _connectionString = "";
        private readonly string _queueName = "sbq-azuretest1";
        private readonly ILogger<ServiceBusService> _logger;

        public ServiceBusService(IConfiguration configuration, ILogger<ServiceBusService> logger)
        {
            _connectionString = configuration.GetConnectionString("ServiceBusService");
            _logger = logger;
        }

        public async Task SendMessageAsync(string message)
        {
            await using (ServiceBusClient client = new ServiceBusClient(_connectionString))
            {
                ServiceBusSender sender = client.CreateSender(_queueName);
                ServiceBusMessage busMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(message));
                await sender.SendMessageAsync(busMessage);
                _logger.LogInformation($"Sent: {message}");
            }
        }


    }
}
