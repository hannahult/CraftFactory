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
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
                optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            }
        }
        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Product>().HasIndex(x => x.Sku).IsUnique();
            b.Entity<Customer>().HasIndex(x => x.Email).IsUnique();

            b.Entity<OrderLine>()
                .HasOne(ol => ol.Order)
                .WithMany(o => o.OrderLines)
                .HasForeignKey(ol => ol.OrderId);

            b.Entity<OrderLine>()
                .HasOne(ol => ol.Product)
                .WithMany(p => p.OrderLines)
                .HasForeignKey(ol => ol.ProductId);
        }
    }
}
