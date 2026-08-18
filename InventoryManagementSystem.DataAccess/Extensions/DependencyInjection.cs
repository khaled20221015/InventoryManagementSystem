using InventoryManagementSystem.DataAccess.Data;
using InventoryManagementSystem.DataAccess.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagementSystem.DataAccess.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services,IConfiguration configuration)
        {
            var connectionString =configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}