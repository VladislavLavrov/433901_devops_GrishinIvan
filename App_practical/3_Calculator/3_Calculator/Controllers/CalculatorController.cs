using Calculator.Models;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Controllers
{
    public class CalculatorController : Controller
    {
        public CalculatorController() { }

        public IActionResult Index() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(double num1, double num2, Operation operation)
        {
            ViewBag.Result = CalculatorLibrary.CalculateOperation(num1, num2, operation).ToString();
            return View();
        }
    }
}