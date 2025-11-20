using KlodTattooWeb.Data;
using KlodTattooWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace KlodTattooWeb.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Booking/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientName,Email,BodyPart,IdeaDescription,PreferredDate")] BookingRequest bookingRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bookingRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Success));
            }
            return View(bookingRequest);
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
