using Microsoft.AspNetCore.Mvc;

namespace grievance.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Login", "Account");
        }
    }
}