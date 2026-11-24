using Calculator.Models; // <-- Подключаем Models
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Controllers
{
    public class CalculatorController : Controller
    {
        public CalculatorController() { }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(double num1, double num2, Operation operation)
        {
            // Теперь Operation берется из Calculator.Models
            ViewBag.Result = CalculatorLibrary.CalculateOperation(num1, num2, operation).ToString();
            return View();
        }
    }
}