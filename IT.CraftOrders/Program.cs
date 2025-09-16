using IT.CraftOrders.Data;
using IT.CraftOrders.Services;
using IT.CraftOrders.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IT.CraftOrders
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CraftFactoryDbContext>();
                await db.Database.MigrateAsync();
                await db.InitializeDataAsync();

                var app = scope.ServiceProvider.GetRequiredService<Menu>();
                await app.RunAsync();
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
             .ConfigureAppConfiguration((hostingContext, config) =>
             {
                 config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
             })
             .ConfigureServices((context, services) =>
             {
                 services.AddDbContext<CraftFactoryDbContext>();
                 services.AddScoped<ProductService>();
                 services.AddScoped<OrderService>();
                 services.AddScoped<AuthService>();
                 services.AddScoped<IncidentService>();
                 services.AddScoped<Menu>();
             });
    }
}
