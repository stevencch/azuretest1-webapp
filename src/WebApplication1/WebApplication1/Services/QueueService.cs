using Azure.Storage.Queues.Models;
using Azure.Storage.Queues;
using System.Text;

namespace WebApplication1.Services
{
    public class QueueService
    {
        private readonly string _connectionString = "";
        private readonly string _queueName = "devqueue";
        private readonly ILogger<QueueService> _logger;

        public QueueService(IConfiguration configuration, ILogger<QueueService> logger)
        {
            _connectionString = configuration.GetConnectionString("StorageService");
            _logger = logger;
        }
        public async Task ProcessMessagesAsync()
        {
            // Instantiate a QueueClient which will be used to create and manipulate the queue
            QueueClient queueClient = new QueueClient(_connectionString, _queueName);
            if (await queueClient.ExistsAsync())
            {
                // Get the next message
                QueueMessage[] retrievedMessage = await queueClient.ReceiveMessagesAsync();

                if (retrievedMessage != null)
                {
                    foreach (var msg in retrievedMessage)
                    {
                        // Process (i.e. print) the message in less than 30 seconds
                        _logger.LogInformation($"Received: {Base64Decode(msg.MessageText)}");

                        // Let the service know we're finished with the message and it can be safely deleted
                        await queueClient.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
                    }
                }
            }
        }
        public async Task SendMessageAsync(string message)
        {
            // Instantiate a QueueClient which will be used to create and manipulate the queue
            QueueClient queueClient = new QueueClient(_connectionString, _queueName);

            // Create the queue if it doesn't already exist
            await queueClient.CreateIfNotExistsAsync();

            if (await queueClient.ExistsAsync())
            {
                await queueClient.SendMessageAsync(Base64Encode(message));
                _logger.LogInformation($"Sent: {message}");
            }
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
