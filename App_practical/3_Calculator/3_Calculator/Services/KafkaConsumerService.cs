using Confluent.Kafka;
using Calculator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Calculator.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Calculator.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly string _topic;
        private readonly IConsumer<Null, string> _kafkaConsumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _clientFactory;

        public KafkaConsumerService(IConfiguration config, IServiceProvider serviceProvider, IHttpClientFactory clientFactory)
        {
            var consumerConfig = new ConsumerConfig();
            config.GetSection("Kafka:ConsumerSettings").Bind(consumerConfig);
            _topic = config.GetValue<string>("Kafka:TopicName");
            _kafkaConsumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
            _serviceProvider = serviceProvider;
            _clientFactory = clientFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        private async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            _kafkaConsumer.Subscribe(_topic);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _kafkaConsumer.Consume(cancellationToken);
                    var inputData = JsonSerializer.Deserialize<DataInputVariant>(cr.Message.Value);

                    var result = CalculatorLibrary.CalculateOperation(inputData.Operand_1, inputData.Operand_2, inputData.Type_operation);
                    inputData.Result = result.ToString();

                    var httpClient = _clientFactory.CreateClient();

                    // Отправляем результат обратно в контроллер
                    await httpClient.PostAsJsonAsync($"http://localhost:5003/Calculator/Callback", inputData);

                    Console.WriteLine($"Message key: {cr.Message.Key}, value: {cr.Message.Value}");
                }
                catch (OperationCanceledException) { break; }
                catch (ConsumeException e)
                {
                    if (e.Error.IsFatal) break;
                }
                catch (Exception e) { break; }
            }
        }

        public override void Dispose()
        {
            _kafkaConsumer.Close();
            _kafkaConsumer.Dispose();
            base.Dispose();
        }
    }
}