using Microsoft.AspNetCore.Mvc;

namespace KlodTattooWeb.Controllers
{
    public class InfoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
