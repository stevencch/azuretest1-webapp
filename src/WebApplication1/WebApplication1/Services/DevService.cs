
using Azure.Storage.Queues.Models;
using Azure.Storage.Queues;
using System.Text;

namespace WebApplication1.Services
{
    public class DevService :IHostedService, IDisposable
    {
        private readonly ILogger<DevService> _logger;
        private readonly QueueService _queueService;
        private System.Timers.Timer? _timer = null;
        public DevService(IConfiguration configuration, ILogger<DevService> logger, QueueService queueService)
        {
            _logger = logger;
            _queueService = queueService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");
            _timer = new System.Timers.Timer(10000);
            _timer.Elapsed += async (sender, e) => await DoWork();

            // Start the timer
            _timer.Start();

        }
        public async Task DoWork()
        {
            _logger.LogInformation("ProcessMessagesAsync");
            await _queueService.ProcessMessagesAsync();

        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Stop();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
