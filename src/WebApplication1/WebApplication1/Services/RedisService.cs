using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Text;

namespace WebApplication1.Services
{
    public class RedisService
    {
        private readonly string _connectionString = "";
        private readonly string _queueName = "sbq-azuretest1";
        private const string topicName = "sbt-azuretest1";
        private readonly ILogger<RedisService> _logger;

        public RedisService(IConfiguration configuration, ILogger<RedisService> logger)
        {
            _connectionString = configuration.GetConnectionString("RedisService");
            _logger = logger;
        }

        public async Task<string> Test()
        {
            var lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(_connectionString);
            });

            IDatabase cache = lazyConnection.Value.GetDatabase();


            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Running the PING command");
            builder.AppendLine($"Response: {cache.Execute("PING").ToString()}");
            builder.AppendLine();

            builder.AppendLine("Running the FLUSHALL command");
            builder.AppendLine($"Response: {cache.Execute("FLUSHALL").ToString()}");
            builder.AppendLine();

            builder.AppendLine("GET the KEY: Message");
            builder.AppendLine($"Response: {cache.StringGet("Message").ToString()}");
            builder.AppendLine();

            builder.AppendLine("SET a KEY with value: Hello from ASP.NET");
            builder.AppendLine($"Response: {cache.StringSet("Message", "Hello from ASP.NET").ToString()}");
            builder.AppendLine();


            builder.AppendLine("GET the KEY: Message");
            builder.AppendLine($"Response: {cache.StringGet("Message").ToString()}");
            builder.AppendLine();

            builder.AppendLine("SET a KEY with value an Expiry");
            cache.StringSet("ExpiringMessage", "Hi, I expire", TimeSpan.FromSeconds(3));

            builder.AppendLine("GET the KEY: ExpiringMessage");
            builder.AppendLine($"ExpiringMessage: {cache.StringGet("Message").ToString()}");
            builder.AppendLine();


            builder.AppendLine("Wait for 30 seconds and then get the KEY ExpiryMessage");
            await Task.Delay(4000);

            builder.AppendLine($"ExpiringMessage: {(await cache.StringGetAsync("ExpiringMessage")).ToString()}");
            builder.AppendLine();



            // Get the client list

            builder.AppendLine(cache.Execute("CLIENT", "LIST").ToString().Replace(
                                                                    "id=", "\rid="));
            lazyConnection.Value.Dispose();
            return builder.ToString();
        }
    }
}
