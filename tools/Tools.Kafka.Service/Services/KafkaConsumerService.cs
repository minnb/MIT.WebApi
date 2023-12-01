using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tools.Kafka.Service.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ConsumerConfig _consumerConfig;

        public KafkaConsumerService(ILogger<KafkaConsumerService> logger)
        {
            // Setup the consumer configuration.
            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "my-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000,
                MaxPollIntervalMs = 300000,
                //MaxPollRecords = 1000,
                // Add any other consumer configuration settings needed.
            };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).SetValueDeserializer((_, msg) => msg).Build();

            //consumer.Subscribe("test-topic");

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    try
            //    {
            //        var consumeResult = consumer.Consume(stoppingToken);
            //        Console.Write($"Received message: {consumeResult.Message.Value} at offset {consumeResult.Offset}");
            //    }
            //    catch (OperationCanceledException)
            //    {
            //        // Ignore the exception when the cancellation token is triggered.
            //    }
            //    catch (ConsumeException ex)
            //    {
            //        Console.Write($"Error occurred while consuming message: {ex.Error.Reason}");
            //    }
            //}

            //// Close the consumer when the service is stopped.
            //consumer.Close();
            return Task.CompletedTask;
        }
    }
}
