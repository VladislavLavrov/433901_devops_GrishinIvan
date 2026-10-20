using Microsoft.EntityFrameworkCore;
using Calculator.Data;
using Calculator.Services;    // Для KafkaConsumerService
using Confluent.Kafka;        // Для IProducer и ProducerConfig

var builder = WebApplication.CreateBuilder(args);

// 1. MVC (контроллеры + представления)
builder.Services.AddControllersWithViews();

// 2. Регистрация контекста БД с MariaDB
string mariadbCS = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CalculatorContext>(options =>
    options.UseMySql(mariadbCS, new MySqlServerVersion(new Version(10, 5, 15))));

// ==========================================================
// 3. НАСТРОЙКА KAFKA (добавляется для новой практики)
// ==========================================================

// 3.1. Producer (отправитель) — контроллер отправляет сюда данные
builder.Services.AddSingleton<IProducer<Null, string>>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = "kafka:9092"
    };
    return new ProducerBuilder<Null, string>(config).Build();
});

// 3.2. Consumer (фоновый сервис) — читает из Kafka и сохраняет в БД
builder.Services.AddHostedService<KafkaConsumerService>();

// ==========================================================

builder.Services.AddRazorPages();

var app = builder.Build();

// 4. Пайплайн обработки запросов
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// 5. Маршрут по умолчанию — контроллер Calculator
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calculator}/{action=Index}/{id?}");

app.Run();