using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// 1. Добавляем MVC и Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 2. Настройка мониторинга OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        // Включаем сбор метрик для HTTP-запросов ASP.NET Core
        metrics.AddAspNetCoreInstrumentation();

        // Явно указываем источники метрик, чтобы Prometheus их увидел
        metrics.AddMeter("Microsoft.AspNetCore.Hosting");
        metrics.AddMeter("Microsoft.AspNetCore.Server.Kestrel");

        // Включаем экспорт в формате Prometheus
        metrics.AddPrometheusExporter();
    });

var app = builder.Build();

// 3. Конфигурация пайплайна запросов
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// 4. Открываем эндпоинт /metrics для Prometheus
app.MapPrometheusScrapingEndpoint();

// 5. Маршрут по умолчанию на калькулятор
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calculator}/{action=Index}/{id?}");

app.Run();