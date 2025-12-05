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
// DATABASE CONFIG (LOCALE o RAILWAY)
// ----------------------------------------------------------
var dbEnvVar = Environment.GetEnvironmentVariable("DATABASE_URL");
var connectionString = ConnectionHelper.GetConnectionString(builder.Configuration);

if (!string.IsNullOrEmpty(dbEnvVar))
{
    // Railway
    try
    {
        var validUrl = dbEnvVar.StartsWith("postgres://")
            ? dbEnvVar.Replace("postgres://", "postgresql://")
            : dbEnvVar;

        var uri = new Uri(validUrl);
        var userInfo = uri.UserInfo.Split(':');

        connectionString =
            $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
            $"Username={userInfo[0]};Password={(userInfo.Length > 1 ? userInfo[1] : "")};" +
            $"SSL Mode=Require;Trust Server Certificate=true";

        Console.WriteLine($"🐘 Railway DB: {uri.Host}");
    }
    catch
    {
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
    }
}
else
{
    // Locale
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
    Console.WriteLine("🐘 PostgreSQL Locale");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Fix timestamp PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// ----------------------------------------------------------
// IDENTITY + ROLES (CONFIGURAZIONE CORRETTA E UNICA)
// ----------------------------------------------------------
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ----------------------------------------------------------
// DATA PROTECTION su DB (obbligatorio su Railway)
// ----------------------------------------------------------
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>();

// ----------------------------------------------------------
// LOCALIZATION
// ----------------------------------------------------------
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[] { "de-DE", "it-IT" };
    options.DefaultRequestCulture = new RequestCulture("de-DE");
    options.SupportedCultures = cultures.Select(c => new CultureInfo(c)).ToList();
    options.SupportedUICultures = cultures.Select(c => new CultureInfo(c)).ToList();
});

// ----------------------------------------------------------
// SERVICES
// ----------------------------------------------------------
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

// MVC + Razor Pages
builder.Services.AddControllersWithViews().AddViewLocalization();
builder.Services.AddRazorPages();

// ----------------------------------------------------------
// AVVIO APP
// ----------------------------------------------------------
var app = builder.Build();

// Dettagli errori solo in dev
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// ----------------------------------------------------------
// MIGRAZIONI + SEED AUTOMATICO
// ----------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        Console.WriteLine("🔄 Applico migrazioni...");
        await db.Database.MigrateAsync();
        Console.WriteLine("✅ Migrazioni OK");

        // Ruoli
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "Admin", "User" };

        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // Admin
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@klodtattoo.com";
        var adminPass = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123";

        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, adminPass);
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Tattoo styles
        string[] tattooStyles =
        {
            "Realistic", "Fine line", "Black Art",
            "Lettering", "Small Tattoos", "Cartoons", "Animals"
        };

        foreach (var t in tattooStyles)
            if (!db.TattooStyles.Any(s => s.Name == t))
                db.TattooStyles.Add(new TattooStyle { Name = t });

        await db.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Errore migrazioni/seed");
        Console.WriteLine(ex);
    }
}

// ----------------------------------------------------------
// MIDDLEWARE
// ----------------------------------------------------------
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseRequestLocalization(
    app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.UseAuthentication();
app.UseAuthorization();

// ----------------------------------------------------------
// ROUTES
// ----------------------------------------------------------
app.MapRazorPages();
app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
