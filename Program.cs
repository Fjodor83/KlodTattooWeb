using KlodTattoo.Data.Helper;
using KlodTattooWeb.Data;
using KlodTattooWeb.Models;
using KlodTattooWeb.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------
// LOGGING
// ----------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// ----------------------------------------------------------
// DATABASE CONFIG
// ----------------------------------------------------------
// In produzione (Azure) usa SEMPRE DefaultConnection
// In locale puoi usare MssqlConnection o SqliteConnection
string connectionString;

if (builder.Environment.IsProduction())
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new Exception("❌ DefaultConnection non trovata nella configurazione Azure.");
    Console.WriteLine("🌐 Production mode → Using DefaultConnection");
}
else
{
    // Usa DatabaseProvider solo in locale
    var dbProvider = builder.Configuration["ConnectionStrings:DatabaseProvider"] ?? "Mssql";

    if (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        connectionString = builder.Configuration.GetConnectionString("SqliteConnection")
            ?? "Data Source=klodtattoo.db";
        Console.WriteLine("📂 SQLite (local)");
    }
    else if (dbProvider.Equals("Mssql", StringComparison.OrdinalIgnoreCase))
    {
        connectionString = builder.Configuration.GetConnectionString("MssqlConnection")
            ?? throw new Exception("❌ MssqlConnection non trovata in locale.");
        Console.WriteLine("🗄️ MSSQL (local)");
    }
    else
    {
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new Exception("❌ DefaultConnection non trovata.");
        Console.WriteLine("🐘 Using DefaultConnection (fallback)");
    }
}

// ----------------------------------------------------------
// DB CONTEXT
// ----------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsProduction())
    {
        // Azure usa SQL Server
        options.UseSqlServer(connectionString);
    }
    else
    {
        var dbProvider = builder.Configuration["ConnectionStrings:DatabaseProvider"] ?? "Mssql";

        if (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            options.UseSqlite(connectionString);
        else
            options.UseSqlServer(connectionString);
    }

    options.EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information);
});

// ----------------------------------------------------------
// IDENTITY
// ----------------------------------------------------------
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>();

// ----------------------------------------------------------
// LOCALIZATION
// ----------------------------------------------------------
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[] { "de-DE", "it-IT", "en" };
    options.DefaultRequestCulture = new RequestCulture("de-DE");
    options.SupportedCultures = cultures.Select(c => new CultureInfo(c)).ToList();
    options.SupportedUICultures = cultures.Select(c => new CultureInfo(c)).ToList();
});

// ----------------------------------------------------------
// EMAIL
// ----------------------------------------------------------
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<EmailSender>();

// ----------------------------------------------------------
// MVC + RAZOR
// ----------------------------------------------------------
builder.Services.AddControllersWithViews().AddViewLocalization();
builder.Services.AddRazorPages();

var app = builder.Build();

// ----------------------------------------------------------
// MIDDLEWARE
// ----------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
app.UseAuthentication();
app.UseAuthorization();

// ----------------------------------------------------------
// DATABASE SEEDING
// ----------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        await DatabaseSeeder.SeedAsync(services, logger);
    }
    catch (Exception ex)
    {
        logger.LogError($"❌ ERRORE CRITICO NEL SEEDING: {ex}");
        throw;
    }
}

// ----------------------------------------------------------
// ROUTING
// ----------------------------------------------------------
app.MapRazorPages();
app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
