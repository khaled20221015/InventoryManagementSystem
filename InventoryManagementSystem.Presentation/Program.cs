using Hangfire;
using Hangfire.SqlServer;
using InventoryManagementSystem.Business.Services;
using InventoryManagementSystem.DataAccess.Data;
using InventoryManagementSystem.DataAccess.Identity;
using InventoryManagementSystem.DataAccess.Repositories;
using InventoryManagementSystem.Presentation.Hubs;
using InventoryManagementSystem.Presentation.Jobs;
using InventoryManagementSystem.Presentation.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();
builder.Services.AddSingleton<ChatPresenceTracker>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(1);
});

builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<StockTransactionRepository>();
builder.Services.AddScoped<ChatMessageRepository>();

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<StockService>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<EmailSender>();
builder.Services.AddScoped<ExpiryAlertJob>();

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        PrepareSchemaIfNecessary = true,
        QueuePollInterval = TimeSpan.FromSeconds(15)
    }));

builder.Services.AddHangfireServer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var headers = context.Response.Headers;

        headers.CacheControl = "no-store, no-cache, must-revalidate";
        headers.Pragma = "no-cache";
        headers.Expires = "0";

        return Task.CompletedTask;
    });

    await next();
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AdminOnlyDashboardFilter() }
});

// Must run before anything touches Hangfire storage. On a machine where the
// database does not exist yet, this is what creates it: MigrateAsync builds
// the schema, then the roles, the administrator account and the demo data are
// seeded. Hangfire can create its own tables but not the database itself, so
// calling it first would crash a fresh clone with "Cannot open database".
await DbSeeder.SeedAsync(app.Services);

// Expiry alert emails are switched off. Uncomment to turn them back on.
// The cron field order is: minute hour day-of-month month day-of-week
//   "*/2 * * * *"  -> every 2 minutes   (demo)
//   "0 8 * * *"    -> once a day at 08:00 (sensible for real use)
// Note: "* * * * */2" is NOT every 2 minutes. It runs every single minute on
// alternate days of the week, which is why the mailbox filled up.
//
// RecurringJob.AddOrUpdate<ExpiryAlertJob>(
//     ExpiryAlertJob.RecurringJobId,
//     job => job.RunAsync(),
//     "*/2 * * * *");

// Nothing registers the job, so Hangfire never schedules it. Note that a
// recurring job already stored in the database keeps running even after this
// block is commented out, because Hangfire schedules from stored state rather
// than from code. To clear one, delete it from the /hangfire dashboard, or run:
//     RecurringJob.RemoveIfExists(ExpiryAlertJob.RecurringJobId);
// once, after the database exists.

app.Run();
