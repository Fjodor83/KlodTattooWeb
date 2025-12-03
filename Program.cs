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
// 🛑 CONFIGURAZIONE MANUALE (HARDCODED PER DEBUG)
// ---------------------------------------------------------------------
// Ho inserito qui i dati del tuo DB Railway che mi hai fornito.
// In futuro rimetteremo le variabili, ma ora DEVE funzionare.
var connectionString = "Host=postgres.railway.internal;" +
                       "Port=5432;" +
                       "Database=railway;" +
                       "Username=postgres;" +
                       "Password=dHqHqMSTztlmgJNxfKpxRWGGCJSRxPSe;" +
                       "SSL Mode=Require;" +
                       "Trust Server Certificate=true";

Console.WriteLine("🐘 [FORCE] Usando Configurazione Manuale Railway");

// Configurazione DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
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

// 🔥 FORZIAMO LA PAGINA DI ERRORE DETTAGLIATA (Per vedere se ci sono altri problemi)
app.UseDeveloperExceptionPage();

// ---------------------------------------------------------------------
// Apply Migrations + Seed
// ---------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        Console.WriteLine("🔄 Tentativo migrazione...");
        await db.Database.MigrateAsync();
        logger.LogInformation("📦 Database migrato correttamente");

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@klodtattoo.com";
        var adminPass = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123";

        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(admin, adminPass);
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        string[] tattooStyles = { "Realistic", "Fine line", "Black Art", "Lettering", "Small Tattoos", "Cartoons", "Animals" };
        foreach (var t in tattooStyles)
            if (!db.TattooStyles.Any(s => s.Name == t))
                db.TattooStyles.Add(new TattooStyle { Name = t });

        await db.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ ERRORE CRITICO DATABASE");
        // Non blocchiamo l'app, così puoi leggere l'errore a video
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();