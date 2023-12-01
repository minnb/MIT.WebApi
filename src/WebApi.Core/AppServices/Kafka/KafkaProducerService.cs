using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace WebApi.Core.AppServices.Kafka
{
    public interface IKafkaProducerService
    {
        Task SendMessageAsync(string topicName, string message);
    }
    public class KafkaProducerService: IKafkaProducerService
    {
        public readonly ProducerConfig _producerConfig;

        public KafkaProducerService(ProducerConfig producerConfig)
        {
            _producerConfig = producerConfig;
        }
        public async Task SendMessageAsync(string topicName, string message)
        {
            try
            {

                using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();
                var result = await producer.ProduceAsync(topicName, new Message<Null, string>
                {
                    Value = message
                });

                Console.WriteLine($"Delivered '{result.Value}' to '{result.TopicPartition}'");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception SendMessageAsync:" + ex.Message.ToString());
            }
        }

        public string SetTopicKafka()
        {
            throw new NotImplementedException();
        }
    }
}
