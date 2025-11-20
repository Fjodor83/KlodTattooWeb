using Microsoft.AspNetCore.Mvc;
using KlodTattooWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace KlodTattooWeb.Controllers;

public class PortfolioController : Controller
{
    private readonly AppDbContext _context;

    public PortfolioController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Portfolio
    public async Task<IActionResult> Index()
    {
        // Recupera tutti i tatuaggi dal DB
        var items = await _context.PortfolioItems.OrderByDescending(x => x.CreatedAt).ToListAsync();
        return View(items);
    }

    // GET: Portfolio/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.PortfolioItems.FirstOrDefaultAsync(m => m.Id == id);
        if (item == null) return NotFound();

        return View(item);
    }
}