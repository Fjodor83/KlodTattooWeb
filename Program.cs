using KlodTattoo.Data.Helper;
using KlodTattooWeb.Data;
using KlodTattooWeb.Models;
using KlodTattooWeb.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------
// HOSTING (Railway / containers)
// ----------------------------------------------------------
var portEnv = Environment.GetEnvironmentVariable("PORT");

if (int.TryParse(portEnv, out var port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
else if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
}

// ----------------------------------------------------------
// LOGGING
// ----------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// ----------------------------------------------------------
// FORWARDED HEADERS (Railway / reverse proxy)
// ----------------------------------------------------------
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// ----------------------------------------------------------
// DATABASE CONFIG
// ----------------------------------------------------------
var databaseUrl =
    Environment.GetEnvironmentVariable("DATABASE_URL") ??
    Environment.GetEnvironmentVariable("DATABASE_PRIVATE_URL");

var dbProvider = builder.Configuration["ConnectionStrings:DatabaseProvider"];

if (!string.IsNullOrWhiteSpace(databaseUrl) && !(
    dbProvider?.Equals("Postgres", StringComparison.OrdinalIgnoreCase) == true ||
    dbProvider?.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase) == true ||
    dbProvider?.Equals("Npgsql", StringComparison.OrdinalIgnoreCase) == true))
{
    dbProvider = "Postgres";
}
else if (string.IsNullOrWhiteSpace(dbProvider))
{
    dbProvider = "Mssql";
}

dbProvider = dbProvider.Trim();

string connectionString;
if (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
{
    connectionString = builder.Configuration.GetConnectionString("SqliteConnection")
        ?? "Data Source=klodtattoo.db";
}
else if (
    dbProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase) ||
    dbProvider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase) ||
    dbProvider.Equals("Npgsql", StringComparison.OrdinalIgnoreCase))
{
    connectionString = ConnectionHelper.GetConnectionString(builder.Configuration);
}
else if (
    dbProvider.Equals("Mssql", StringComparison.OrdinalIgnoreCase) ||
    dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    connectionString =
        (builder.Environment.IsProduction()
            ? builder.Configuration.GetConnectionString("DefaultConnection")
            : builder.Configuration.GetConnectionString("MssqlConnection"))
        ?? builder.Configuration.GetConnectionString("MssqlConnection")
        ?? builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new Exception("❌ Connection string per MSSQL non trovata (MssqlConnection/DefaultConnection).");
}
else
{
    throw new Exception($"❌ DatabaseProvider '{dbProvider}' non supportato. Usa: Postgres | Mssql | Sqlite");
}

Console.WriteLine($"🗄️ DB Provider → {dbProvider}");

// ----------------------------------------------------------
// DB CONTEXT
// ----------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        options.UseSqlite(connectionString);
    else if (
        dbProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase) ||
        dbProvider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase) ||
        dbProvider.Equals("Npgsql", StringComparison.OrdinalIgnoreCase))
        options.UseNpgsql(connectionString);
    else
        options.UseSqlServer(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .LogTo(Console.WriteLine, LogLevel.Information);
    }
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
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);

var app = builder.Build();

// ----------------------------------------------------------
// MIDDLEWARE
// ----------------------------------------------------------
app.UseForwardedHeaders();

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
