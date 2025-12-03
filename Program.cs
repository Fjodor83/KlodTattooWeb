using KlodTattooWeb.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using KlodTattooWeb.Services;
using KlodTattooWeb.Models;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// FIX per compatibilità PostgreSQL timestamp
// ---------------------------------------------------------------------
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// ---------------------------------------------------------------------
// Configurazione Porta per Railway
// ---------------------------------------------------------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// ---------------------------------------------------------------------
// DATABASE STRATEGY (Auto-Detect)
// ---------------------------------------------------------------------
var databaseProvider = "Sqlite"; // Default di partenza
string? connectionString = null;

// 1. Cerca variabili d'ambiente (Railway/Produzione)
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? Environment.GetEnvironmentVariable("RAILWAY_DATABASE_URL");

if (!string.IsNullOrEmpty(dbUrl))
{
    // Railway fornisce l'URL. Convertiamo da formato URI a Connection String per Npgsql
    try
    {
        // Normalizza lo schema se necessario (alcuni provider usano postgres://)
        if (dbUrl.StartsWith("postgres://"))
            dbUrl = dbUrl.Replace("postgres://", "postgresql://");

        var uri = new Uri(dbUrl);
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";

        connectionString =
            $"Host={uri.Host};" +
            $"Port={uri.Port};" +
            $"Database={uri.AbsolutePath.TrimStart('/')};" +
            $"Username={username};" +
            $"Password={password};" +
            $"SSL Mode=Require;Trust Server Certificate=true";

        databaseProvider = "PostgreSQL";
        Console.WriteLine("🐘 Usando PostgreSQL (da Variabili Ambiente)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Errore parsing DATABASE_URL: {ex.Message}. Fallback su appsettings.");
    }
}

// 2. Se non abbiamo trovato nulla nelle variabili, usiamo appsettings.json
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // ANALISI INTELLIGENTE: Se la stringa contiene "Host=", è sicuramente Postgres!
    if (!string.IsNullOrEmpty(connectionString) &&
       (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("postgres", StringComparison.OrdinalIgnoreCase)))
    {
        databaseProvider = "PostgreSQL";
        Console.WriteLine("🐘 Usando PostgreSQL (da appsettings.json)");
    }
    else
    {
        databaseProvider = "Sqlite";
        Console.WriteLine("🗄️ Usando SQLite (default)");
    }
}

// 3. Configurazione DbContext in base al provider rilevato
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (databaseProvider == "PostgreSQL")
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString ?? "Data Source=klodtattoo.db");
    }
});

// ---------------------------------------------------------------------
// Identity
// ---------------------------------------------------------------------
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// MVC + Razor
builder.Services.AddControllersWithViews().AddViewLocalization();
builder.Services.AddRazorPages();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[] { "de-DE", "it-IT" };
    options.DefaultRequestCulture = new RequestCulture("de-DE");
    options.SupportedCultures = cultures.Select(c => new CultureInfo(c)).ToList();
    options.SupportedUICultures = cultures.Select(c => new CultureInfo(c)).ToList();
});

// Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

// ---------------------------------------------------------------------
// BUILD APP
// ---------------------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------------------
// Apply Migrations + Seed
// ---------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    try
    {
        await db.Database.MigrateAsync();
        logger.LogInformation("📦 Database migrato correttamente");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Errore durante la migration del database.");
        // Non rilanciamo l'eccezione per evitare crash loop continui su Railway se il DB è temporaneamente giù,
        // ma in produzione è meglio indagare.
    }

    // Ruoli
    string[] roles = { "Admin", "User" };
    foreach (var role in roles)
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    // Admin
    var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@klodtattoo.com";
    var adminPass = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123";

    if (await userManager.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(admin, adminPass);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }

    // Seed Tattoo Styles
    string[] tattooStyles = { "Realistic", "Fine line", "Black Art", "Lettering", "Small Tattoos", "Cartoons", "Animals" };
    foreach (var t in tattooStyles)
        if (!db.TattooStyles.Any(s => s.Name == t))
            db.TattooStyles.Add(new TattooStyle { Name = t });

    await db.SaveChangesAsync();
}

// ---------------------------------------------------------------------
// Middleware
// ---------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Areas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();