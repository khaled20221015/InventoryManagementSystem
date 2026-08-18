using InventoryManagementSystem.Business.Services;
using InventoryManagementSystem.DataAccess.Extensions;
using InventoryManagementSystem.DataAccess.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagementSystem.Business.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusiness(this IServiceCollection services,IConfiguration configuration)
        {
           
            services.AddDataAccess(configuration);

            services.AddScoped<ProductRepository>();
            services.AddScoped<ProductService>();

            return services;
        }
    }
}