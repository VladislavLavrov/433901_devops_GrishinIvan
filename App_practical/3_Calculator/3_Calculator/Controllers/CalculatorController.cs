using Calculator.Data;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Controllers
{
    public enum Operation { Add, Subtract, Multiply, Divide }

    public class CalculatorController : Controller
    {
        private readonly CalculatorContext _context;

        public CalculatorController(CalculatorContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

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
                        ViewBag.Result = "Деление на ноль!";
                        return View("Index");
                    }
                    result = num1 / num2;
                    break;
            }

            ViewBag.Result = result;

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
    }
}