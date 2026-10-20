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
                BootstrapServers = "kafka:9092",
                GroupId = "calculator-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Null, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Ждем немного, чтобы Kafka успела полностью инициализироваться
            await Task.Delay(5000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _consumer.Subscribe("grishin");
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var result = _consumer.Consume(stoppingToken);
                            if (result != null && result.Message != null)
                            {
                                var data = JsonSerializer.Deserialize<DataInputVariant>(result.Message.Value);
                                if (data != null)
                                {
                                    using var scope = _serviceProvider.CreateScope();
                                    var context = scope.ServiceProvider.GetRequiredService<CalculatorContext>();
                                    context.DataInputVariants.Add(data);
                                    await context.SaveChangesAsync();
                                }
                            }
                        }
                        catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                        {
                            // Топика еще нет - ждем, пока Producer его создаст
                            await Task.Delay(2000, stoppingToken);
                        }
                        catch (OperationCanceledException) { break; }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка Consumer: {ex.Message}");
                            await Task.Delay(5000, stoppingToken); // Пауза перед повтором
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Критическая ошибка KafkaConsumerService: {ex.Message}");
                    await Task.Delay(10000, stoppingToken); // Ждем перед переподключением
                }
            }
            _consumer.Close();
        }
    }
}