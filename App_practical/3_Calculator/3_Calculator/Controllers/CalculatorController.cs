using Calculator.Data;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading;

namespace Calculator.Controllers
{
    public enum Operation { Add, Subtract, Multiply, Divide }

    public class CalculatorController : Controller
    {
        // _context больше не нужен в контроллере, но оставим, если он нужен для страницы History
        private readonly CalculatorContext _context;
        private readonly IProducer<Null, string> _producer;

        public CalculatorController(CalculatorContext context, IProducer<Null, string> producer)
        {
            _context = context;
            _producer = producer;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(double num1, double num2, Operation operation)
        {
            double result = 0;
            switch (operation)
            {
                case Operation.Add: result = num1 + num2; break;
                case Operation.Subtract: result = num1 - num2; break;
                case Operation.Multiply: result = num1 * num2; break;
                case Operation.Divide: result = num1 / num2; break;
            }

            ViewBag.Result = result;

            // Формируем объект для БД
            var dataInputVariant = new DataInputVariant
            {
                Operand_1 = num1.ToString(),
                Operand_2 = num2.ToString(),
                Type_operation = operation.ToString(),
                Result = result.ToString()
            };

            // Отправляем в Kafka
            string jsonMessage = JsonSerializer.Serialize(dataInputVariant);
            try
            {
                // Добавляем CancellationToken с тайм-аутом в 5 секунд
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await _producer.ProduceAsync("grishin", new Message<Null, string> { Value = jsonMessage }, cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки в Kafka: {ex.Message}");
            }

            return View("Index");
        }

        [HttpGet]
        public IActionResult History()
        {
            var items = _context.DataInputVariants.OrderByDescending(x => x.ID_DataInputVariant).ToList();
            return View(items);
        }
    }
}