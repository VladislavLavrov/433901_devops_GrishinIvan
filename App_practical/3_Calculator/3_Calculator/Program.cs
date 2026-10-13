using Microsoft.EntityFrameworkCore;
using Calculator.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC (контроллеры + представления)
builder.Services.AddControllersWithViews();

// 2. Регистрация контекста БД с MariaDB
string mariadbCS = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CalculatorContext>(options =>
    options.UseMySql(mariadbCS, new MySqlServerVersion(new Version(10, 5, 15))));

builder.Services.AddRazorPages();

var app = builder.Build();

// 3. Пайплайн обработки запросов
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// 4. Маршрут по умолчанию — контроллер Calculator
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calculator}/{action=Index}/{id?}");

app.Run();