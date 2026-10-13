using Calculator.Data;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Controllers
{
    // Перечисление операций
    public enum Operation { Add, Subtract, Multiply, Divide }

    public class CalculatorController : Controller
    {
        // Ссылка на контекст БД (внедряется через DI)
        private readonly CalculatorContext _context;

        // Конструктор — ASP.NET сама передаст сюда CalculatorContext
        public CalculatorController(CalculatorContext context)
        {
            _context = context;
        }

        // GET: /Calculator/Index — показать форму
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Calculator/Calculate — посчитать и сохранить
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Calculate(double num1, double num2, Operation operation)
        {
            double result = 0;

            switch (operation)
            {
                case Operation.Add:
                    result = num1 + num2;
                    break;
                case Operation.Subtract:
                    result = num1 - num2;
                    break;
                case Operation.Multiply:
                    result = num1 * num2;
                    break;
                case Operation.Divide:
                    if (num2 == 0)
                    {
                        ViewBag.Result = "Ошибка: деление на ноль!";
                        return View("Index");
                    }
                    result = num1 / num2;
                    break;
            }

            ViewBag.Result = result;

            // Сохраняем операцию в БД
            var dataInputVariant = new DataInputVariant
            {
                Operand_1 = num1.ToString(),
                Operand_2 = num2.ToString(),
                Type_operation = operation.ToString(),
                Result = result.ToString()
            };

            _context.DataInputVariants.Add(dataInputVariant);
            _context.SaveChanges();

            return View("Index");
        }

        // GET: /Calculator/History — показать историю операций
        [HttpGet]
        public IActionResult History()
        {
            var items = _context.DataInputVariants
                .OrderByDescending(x => x.ID_DataInputVariant)
                .ToList();
            return View(items);
        }
    }
}