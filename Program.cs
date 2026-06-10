using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SentinelAI.Data;
using SentinelAI.Models;
using SentinelAI.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ──────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ── HttpClient & Services ──────────────────────────────────────────────────────
builder.Services.AddHttpClient<GeminiService>();
builder.Services.AddScoped<GeminiService>();
builder.Services.AddScoped<FileUploadService>();

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// ── Migrate & Seed ────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // Seed Roles
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole("User"));

        // Seed Admin user
        const string adminEmail = "admin@sentinel.mil";
        const string adminPassword = "Admin@123";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Seed demo complaints linked to the admin user (runs only if DB is empty)
        if (!context.Complaints.Any())
        {
            var seedUserId = adminUser?.Id ?? "seed-user-001";
            var seedComplaints = new List<Complaint>
            {
                new Complaint
                {
                    Title = "Suspicious OTP SMS",
                    Description = "Received an OTP SMS from an unknown number asking to verify bank account.",
                    IncidentType = "Phishing",
                    RiskLevel = "High",
                    Status = "Pending",
                    MitigationSteps = "Report to your unit CERT. Do not click suspicious links. Change credentials immediately.",
                    CreatedDate = new DateTime(2025, 1, 10, 9, 0, 0, DateTimeKind.Utc),
                    UserId = seedUserId
                },
                new Complaint
                {
                    Title = "Bank Account Compromised",
                    Description = "Unauthorized transactions detected on the unit's bank account linked to defence payroll.",
                    IncidentType = "Fraud",
                    RiskLevel = "Medium",
                    Status = "Investigating",
                    MitigationSteps = "Freeze the account immediately. Contact bank fraud department. Report to financial officer.",
                    CreatedDate = new DateTime(2025, 1, 15, 11, 30, 0, DateTimeKind.Utc),
                    UserId = seedUserId
                },
                new Complaint
                {
                    Title = "Malware on Army Laptop",
                    Description = "Malware virus detected on an army-issued laptop used for classified operations.",
                    IncidentType = "Malware",
                    RiskLevel = "Critical",
                    Status = "Resolved",
                    MitigationSteps = "Isolate the device immediately. Run full antivirus scan. Reimage the system. Report to CERT.",
                    CreatedDate = new DateTime(2025, 1, 20, 14, 0, 0, DateTimeKind.Utc),
                    UserId = seedUserId
                },
                new Complaint
                {
                    Title = "Classified Info Leak",
                    Description = "Espionage suspected — classified military posting information leaked to unauthorized personnel.",
                    IncidentType = "Espionage",
                    RiskLevel = "Critical",
                    Status = "Investigating",
                    MitigationSteps = "Initiate counterintelligence protocol. Lock down access. Notify commanding officer and CERT immediately.",
                    CreatedDate = new DateTime(2025, 1, 25, 8, 0, 0, DateTimeKind.Utc),
                    UserId = seedUserId
                }
            };
            context.Complaints.AddRange(seedComplaints);
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during migration/seeding.");
    }
}

// ── Middleware ────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ── Routes ────────────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
