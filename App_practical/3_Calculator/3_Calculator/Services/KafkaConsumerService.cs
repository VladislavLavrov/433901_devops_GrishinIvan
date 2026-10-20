using Confluent.Kafka;
using Calculator.Data;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Calculator.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<Null, string> _consumer;
        private readonly IServiceProvider _serviceProvider;

        public KafkaConsumerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var config = new ConsumerConfig
            {
                BootstrapServers = "kafka:9092",
                GroupId = "calculator-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Null, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe("calculator-topic");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    if (result != null && result.Message != null)
                    {
                        // Десериализуем сообщение
                        var data = JsonSerializer.Deserialize<DataInputVariant>(result.Message.Value);

                        if (data != null)
                        {
                            // Создаем scope для получения контекста БД
                            using var scope = _serviceProvider.CreateScope();
                            var context = scope.ServiceProvider.GetRequiredService<CalculatorContext>();

                            context.DataInputVariants.Add(data);
                            await context.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при чтении из Kafka: {ex.Message}");
                }
            }
            _consumer.Close();
        }
    }
}