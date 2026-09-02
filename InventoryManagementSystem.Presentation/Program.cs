using InventoryManagementSystem.Business.Extensions;
using InventoryManagementSystem.DataAccess.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Swagger is only used to try out the REST API while developing.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// One call registers the Business layer, which registers the Data Access layer.
builder.Services.AddBusiness(builder.Configuration);

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

// Sends 404, 403, ... to the same friendly error page.
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

// Creates/updates the database, then the roles and the default admin account.
await DbSeeder.SeedAsync(app.Services);

app.Run();
