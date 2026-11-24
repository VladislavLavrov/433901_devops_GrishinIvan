using Calculator.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics; // <-- Добавьте это

namespace Calculator.Controllers
{
    public class CalculatorController : Controller
    {
        // Создаем Meter и Counter для наших расчетов
        private static readonly Meter MyMeter = new Meter("Calculator.Metrics", "1.0.0");
        private static readonly Counter<int> CalculationCounter = MyMeter.CreateCounter<int>(
            name: "calculator_calculations_total",
            description: "Total number of calculations performed"
        );

        public CalculatorController() { }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(double num1, double num2, Operation operation)
        {
            // Увеличиваем счетчик при каждом расчете
            CalculationCounter.Add(1);

            ViewBag.Result = CalculatorLibrary.CalculateOperation(num1, num2, operation).ToString();
            return View();
        }
    }
}
