using Confluent.Kafka;
using Calculator.Data;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

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
                BootstrapServers = "kafka:9092", // Обращаемся к серверной Kafka
                GroupId = "calculator-group-grishin", // Уникальная группа, чтобы не конфликтовать с другими
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Null, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(5000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _consumer.Subscribe("grishin"); // Ваш топик
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var result = _consumer.Consume(stoppingToken);
                            if (result != null && result.Message != null)
                            {
                                // 1. Добавляем PropertyNameCaseInsensitive для правильного парсинга
                                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                                var dataFromKafka = JsonSerializer.Deserialize<DataInputVariant>(result.Message.Value, options);

                                if (dataFromKafka != null)
                                {
                                    // 2. Создаем копию без ID, чтобы EF Core сгенерировал его сам
                                    var dbRecord = new DataInputVariant
                                    {
                                        Operand_1 = dataFromKafka.Operand_1,
                                        Operand_2 = dataFromKafka.Operand_2,
                                        Type_operation = dataFromKafka.Type_operation,
                                        Result = dataFromKafka.Result
                                    };

                                    using var scope = _serviceProvider.CreateScope();
                                    var context = scope.ServiceProvider.GetRequiredService<CalculatorContext>();
                                    context.DataInputVariants.Add(dbRecord);
                                    await context.SaveChangesAsync();
                                }
                            }
                        }
                        catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                        {
                            await Task.Delay(2000, stoppingToken);
                        }
                        catch (OperationCanceledException) { break; }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка Consumer: {ex.Message}");
                            await Task.Delay(5000, stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Критическая ошибка KafkaConsumerService: {ex.Message}");
                    await Task.Delay(10000, stoppingToken);
                }
            }
            _consumer.Close();
        }
    }
}