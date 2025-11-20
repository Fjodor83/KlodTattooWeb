using Microsoft.AspNetCore.Mvc;

namespace KlodTattooWeb.Controllers
{
    public class ContactsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
