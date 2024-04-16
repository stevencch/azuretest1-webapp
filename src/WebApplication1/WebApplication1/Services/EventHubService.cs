using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using System.Collections.Concurrent;
using System.Text.Json;

namespace WebApplication1.Services
{
    public class EventHubService
    {
        private readonly string _connectionString = "";
        private readonly string _eventHubName = "evh-azuretest1";
        public EventHubService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("EventHubService");
        }

        public async Task SendToRandomPartition()
        {
            Random rand = new Random();
            await using var producerClient =
                new EventHubProducerClient(_connectionString, _eventHubName);
            using EventDataBatch eventBatch =
                await producerClient.CreateBatchAsync();
            for (int i = 0; i < 100; i++)
            {
                double waterTemp = (rand.NextDouble()) * 100;
                int coffeeTypeIndex = rand.Next(2);

                var coffeeMachineData = new CoffeeData
                {
                    WaterTemperature = waterTemp,
                    ReadingTime = DateTime.Now,
                    CoffeeType = CoffeeData.AllCoffeeTypes[coffeeTypeIndex]
                };

                var coffeeMachineDataBytes =
                    JsonSerializer.SerializeToUtf8Bytes(coffeeMachineData);

                var eventData = new EventData(coffeeMachineDataBytes);

                if (!eventBatch.TryAdd(eventData))
                {
                    throw new Exception("Cannot add coffee machine data to random batch");
                }
            }
            await producerClient.SendAsync(eventBatch);
        }

        public async Task SendToSamePartition(string partitionKey)
        {
            Random rand = new Random();
            await using var producerClient =
                new EventHubProducerClient(_connectionString, _eventHubName);

            // can also do this with EventDataBatch - but showing another way

            List<EventData> eventBatch = new List<EventData>();

            for (int i = 0; i < 100; i++)
            {
                double waterTemp = (rand.NextDouble()) * 100;
                int coffeeTypeIndex = rand.Next(2);

                var coffeeMachineData = new CoffeeData
                {
                    WaterTemperature = waterTemp,
                    ReadingTime = DateTime.Now,
                    CoffeeType = CoffeeData.AllCoffeeTypes[coffeeTypeIndex]
                };

                var coffeeMachineDataBytes =
                    JsonSerializer.SerializeToUtf8Bytes(coffeeMachineData);

                var eventData = new EventData(coffeeMachineDataBytes);

                eventBatch.Add(eventData);
            }

            var options = new SendEventOptions();
            options.PartitionKey = partitionKey;

            await producerClient.SendAsync(eventBatch, options);

        }

        public async IAsyncEnumerable<string> GetPartitionInfo()
        {
            await using var consumerClient =
                new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, _connectionString, _eventHubName);

            var partitionIds = await consumerClient.GetPartitionIdsAsync();

            foreach (var id in partitionIds)
            {
                var partitionInfo = await
                    consumerClient.GetPartitionPropertiesAsync(id);

                yield return $"Partition Id: {partitionInfo.Id}{Environment.NewLine}Empty? {partitionInfo.IsEmpty}{Environment.NewLine}Last Sequence: {partitionInfo.LastEnqueuedSequenceNumber}";
            }
        }

        public async Task<List<string>> ReadFromPartition(string partitionNumber)
        {
            var list = new List<string>();
            var cancelToken = new CancellationTokenSource();
            cancelToken.CancelAfter(TimeSpan.FromSeconds(20));

            await using (var consumerClient =
                new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, _connectionString, _eventHubName))
            {
                try
                {
                    PartitionProperties props =
                        await consumerClient
                            .GetPartitionPropertiesAsync(partitionNumber);

                    EventPosition startingPosition =
                        EventPosition.FromSequenceNumber(
                             //props.LastEnqueuedSequenceNumber
                             props.BeginningSequenceNumber
                        );

                    await foreach (PartitionEvent partitionEvent in consumerClient
                        .ReadEventsFromPartitionAsync(partitionNumber, startingPosition, cancelToken.Token))
                    {
                        string partitionId = partitionEvent.Partition.PartitionId;
                        var sequenceNumber = partitionEvent.Data.SequenceNumber;
                        var key = partitionEvent.Data.PartitionKey;

                        list.Add($"Partition Id: {partitionId}{Environment.NewLine}Sequence Number: {sequenceNumber}{Environment.NewLine}Partition Key: {key}");

                        var coffee = JsonSerializer
                            .Deserialize<CoffeeData>(partitionEvent.Data.Body.Span);

                        list.Add($"Type: {coffee.CoffeeType}{Environment.NewLine}Temp: {coffee.WaterTemperature}{Environment.NewLine}Date: {coffee.ReadingTime.ToShortDateString()}");
                    }
                }
                catch (Exception ex)
                {
                    list.Add(ex.Message);
                }
                finally
                {
                    await consumerClient.CloseAsync();
                }
               
            }
            return list;
        }
    }

    public class CoffeeData
    {
        public static readonly string[] AllCoffeeTypes =
            { "Sumatra", "Columbian", "French" };

        public double WaterTemperature { get; set; }
        public DateTime ReadingTime { get; set; }
        public string CoffeeType { get; set; }
    }
}
