using Microsoft.AspNetCore.Mvc;

namespace KlodTattooWeb.Controllers
{
    public class ServicesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
