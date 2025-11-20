using KlodTattooWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace KlodTattooWeb.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PortfolioItem> PortfolioItems { get; set; }
    public DbSet<BookingRequest> BookingRequests { get; set; }
}