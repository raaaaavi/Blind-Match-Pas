using BlindMatchPAS.Web.Data;
using BlindMatchPAS.Web.Data.Seed;
using BlindMatchPAS.Web.Repositories;
using BlindMatchPAS.Web.Repositories.Interfaces;
using BlindMatchPAS.Web.Services;
using BlindMatchPAS.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// ============================================================================
// BLIND-MATCH PAS APPLICATION STARTUP CONFIGURATION
// ============================================================================
// This Program.cs configures the ASP.NET Core application with:
// - Database provider selection (SQL Server or SQLite)
// - Identity and authentication configuration
// - Dependency injection setup for services and repositories
// - Database migration and seeding
// - Application middleware pipeline
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// Determine database provider: SQL Server for production, SQLite for testing
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";

// CONFIGURE DATABASE CONTEXT
// Supports both SQL Server (production) and SQLite (testing)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection"));
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

// CONFIGURE IDENTITY AND AUTHENTICATION
// Sets up user management, password policies, and cookie-based authentication
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password policy: 8+ chars, digit, uppercase, lowercase, special character
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure authentication cookie behavior and redirect paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();

// REGISTER DEPENDENCY INJECTION (Repositories and Services)
// These will be injected into controllers via constructor dependency injection
builder.Services.AddScoped<IProposalRepository, ProposalRepository>();
builder.Services.AddScoped<IResearchAreaRepository, ResearchAreaRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IMatchingService, MatchingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<DatabaseSeeder>();

var app = builder.Build();

// CONFIGURE MIDDLEWARE PIPELINE
// The order matters: Exception handling → HTTPS → Static files → Routing → Auth → Endpoints

if (!app.Environment.IsDevelopment())
{
    // Production: Use generic error page and HSTS for security
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
// IMPORTANT: Middleware order - Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// DATABASE INITIALIZATION AND SEEDING
// Run only in non-Testing environments to populate demo data
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        // Apply any pending Entity Framework migrations to SQL Server
        await db.Database.MigrateAsync();
    }

    // Seed demo data (research areas, users, sample proposals)
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program;
