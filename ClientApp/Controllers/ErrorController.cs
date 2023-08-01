using Microsoft.AspNetCore.Mvc;

namespace ClientApp.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
