using KlodTattooWeb.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KlodTattooWeb.Data;

public class AppDbContext : IdentityDbContext, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Tabelle per Data Protection
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    // Tabelle custom della tua applicazione
    public DbSet<PortfolioItem> PortfolioItems { get; set; }
    public DbSet<BookingRequest> BookingRequests { get; set; }
    public DbSet<TattooStyle> TattooStyles { get; set; }
}
