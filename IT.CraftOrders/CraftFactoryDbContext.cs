using IT.CraftOrders.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.CraftOrders
{
    public class CraftFactoryDbContext : DbContext
    {
        public DbSet <Order> Orders { get; set; }
        public DbSet <Product> Products { get; set; }
        public DbSet <Customer> Customers { get; set; }
        public DbSet <OrderLine> OrderLines { get; set; }
        public DbSet <Incident> Incidents { get; set; }
        public DbSet <Employee> Employees { get; set; }

        public CraftFactoryDbContext(DbContextOptions<CraftFactoryDbContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
                optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            }
        }
    }
}
