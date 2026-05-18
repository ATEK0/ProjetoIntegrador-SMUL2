using Microsoft.AspNetCore.Mvc;

namespace Portal.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Admin()
        {
            return View();
        }

        public IActionResult Aluno()
        {
            return View();
        }

        public IActionResult Professor()
        {
            return View();
        }
    }
}
